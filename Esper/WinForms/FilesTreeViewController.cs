using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Utilities;
using Esper.Model;

namespace Esper.WinForms
{
    internal sealed class FilesTreeViewController : IFileOpen
    {
        public event EventHandler<NodeEventArgs> FocusedNodeChanged;
        public event EventHandler<NodeEventArgs> NodeDoubleClick;
        public event EventHandler BeforeInitialize;

        private readonly TreeView _treeView;
        private EsperProject _project;
        private FileStore.Directory _root;

        private readonly BackgroundWorker2 _worker;

        public EsperProject Project
        {
            get { return _project; }
        }

        public FilesTreeViewController(TreeView treeView)
        {
            _treeView = treeView;
            //_fileStore = fileStore;

            _treeView.MouseDown += treeView_MouseDown;
            _treeView.BeforeCollapse += treeView_BeforeCollapse;
            _treeView.AfterSelect += treeView_AfterSelect;
            _treeView.NodeMouseDoubleClick += treeView_NodeMouseDoubleClick;
            _treeView.ContextMenuStrip.ItemClicked += contextMenuStrip_ItemClicked;
            _treeView.ContextMenuStrip.Opening += contextMenuStrip_Opening;

            _worker = new BackgroundWorker2(treeView);
        }

        public FileStore.File FindFile(string path)
        {
            return FileStore.Directory.FindFile(_root, f => f.GetPath() == path);
        }

        private void ChangeFileName(string oldFullFilePath, string newFullFilePath)
        {
            var node = _FindNode(_treeView.Nodes, n =>
            {
                var f = n.Tag as FileStore.File;
                return f != null && f.GetFullSystemPath() == oldFullFilePath;
            });
            if (node != null)
            {
                var f = node.Tag as FileStore.File;
                f.Name = System.IO.Path.GetFileName(newFullFilePath);
                f.Type = FileStore.File.GetFileType(System.IO.Path.GetExtension(newFullFilePath));
                node.Name = f.Name;
            }
        }

        private TreeNode _FindNode(TreeNodeCollection nodes, Func<TreeNode,bool> compare)
        {
            if (nodes != null)
            {
                foreach(TreeNode node in nodes)
                {
                    if (compare(node)) return node;
                    var n = _FindNode(node.Nodes, compare);
                    if (n != null) return node;
                }
            }
            return null;
        }

        private void contextMenuStrip_Opening(object sender, CancelEventArgs e)
        {
            var node = _treeView.SelectedNode;
            if (node == null || node.Tag == null)
            {
                e.Cancel = true;
            }
            else
            {
                FindContextMenuItem("CREATE_NEW_FOLDER").Enabled = node.Tag is FileStore.Directory;
                FindContextMenuItem("CREATE_NEW_FILE").Enabled = node.Tag is FileStore.Directory;
                FindContextMenuItem("ADD_EXISTING_FILE").Enabled = node.Tag is FileStore.Directory;
                FindContextMenuItem("RENAME").Enabled = node.Tag is FileStore.Directory || node.Tag is FileStore.File;
            }
        }

        private void contextMenuStrip_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            var node = _treeView.SelectedNode;
            if (node == null || node.Tag == null)
            {
                if (e.ClickedItem?.Tag?.ToString() == "CREATE_NEW_FOLDER")
                {
                    //...
                }
                else if (e.ClickedItem?.Tag?.ToString() == "CREATE_NEW_FILE")
                {
                    //...
                }
                else if (e.ClickedItem?.Tag?.ToString() == "ADD_EXISTING_FILE")
                {
                    //...
                }
                else if (e.ClickedItem?.Tag?.ToString() == "RENAME")
                {
                    //...
                }
            }
        }

        private ToolStripItem FindContextMenuItem(string tag)
        {
            foreach(ToolStripItem it in _treeView.ContextMenuStrip.Items)
            {
                if (it.Tag!=null && string.Equals(it.Tag as string, tag))
                {
                    return it;
                }
            }
            return null;
        }

        private void treeView_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            var args = GetNodeEventArgs(e.Node);
            if (args != null)
            {
                NodeDoubleClick?.Invoke(this, args);
            }
        }

        private void treeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            var args = GetNodeEventArgs(e.Node);
            if (args != null)
            {
                FocusedNodeChanged?.Invoke(this, args);
            }
        }

        private void treeView_BeforeCollapse(object sender, TreeViewCancelEventArgs e)
        {
            if (e.Node.Tag == _root)
            {
                e.Cancel = true;
            }
        }

        private void treeView_MouseDown(object sender, MouseEventArgs e)
        {
            var ht = _treeView.HitTest(e.Location);
            _treeView.SelectedNode = ht.Node;
        }

        private void Init()
        {
            _treeView.Nodes.Clear();
            if (_root != null && _project != null)
            {
                var node = new TreeNode(_project.Name);
                node.Tag = _root;
                _treeView.Nodes.Add(node);
                _Init(_root, node.Nodes);
                node.Expand();
            }
        }

        public void CreateFile()
        {
            CreateFile(null);
        }

        public void CreateFile(Action<Exception> result)
        {
            var dlg = new CreateProjectForm();
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                OpenProject(dlg.FullFileName, true, result);
            }
        }

        public void OpenFile(string fileName)
        {
            OpenFile(fileName, null);
        }

        public void OpenFile(string fileName, Action<Exception> result)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                var dlg = new OpenFileDialog();
                dlg.Filter = "Project Files (*.esper)|*.esper";
                dlg.DefaultExt = ".esper";
                dlg.AddExtension = true;
                dlg.CheckFileExists = true;
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    fileName = dlg.FileName;
                }
                else
                {
                    return;
                }
            }
            OpenProject(fileName, true, result);
        }

        private void OpenProject(string fileName, bool create, Action<Exception> result)
        {
            EsperProject p = null;
            FileStore.Directory r = null;
            _worker.Do(() =>
            {
                p = new EsperProject(fileName, create);
                r = p.FileStore.GetFullTree();
            },
            (e) =>
            {
                if (e != null)
                {
                    MessageBox.Show("FileStore error occured:\n" + e.Message ?? e.ToString(),
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
                else
                {
                    BeforeInitialize?.Invoke(this, EventArgs.Empty);
                    _project = p;
                    _root = r;
                    Init();
                }
                result?.Invoke(e);
            });
        }

        private void _Init(FileStore.Directory root, TreeNodeCollection nodes)
        {
            foreach(var dir in root.Directories)
            {
                var node = new TreeNode(dir.Name);
                node.Tag = dir;
                nodes.Add(node);
                _Init(dir, node.Nodes);
            }

            foreach(var file in root.Files)
            {
                if (file.GetFullSystemPath() != _project.FullFileName)
                {
                    var node = new TreeNode(file.Name);
                    node.Tag = file;
                    nodes.Add(node);
                }
            }
        }

        private NodeEventArgs GetNodeEventArgs(TreeNode node)
        {
            NodeEventArgs args = null;
            if (node?.Tag is FileStore.Directory)
            {
                args = new NodeEventArgs() { Directory = node.Tag as FileStore.Directory };
            }
            else if (node?.Tag is FileStore.File)
            {
                args = new NodeEventArgs() { File = node.Tag as FileStore.File };
            }
            return args;
        }

        public sealed class NodeEventArgs : EventArgs
        {
            public FileStore.Directory Directory { get; set; }
            public FileStore.File File { get; set; }
        }
    }
}

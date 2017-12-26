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

            _worker = new BackgroundWorker2(treeView);
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
            var dlg = new CreateProjectForm();
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                OpenProject(dlg.FullFileName, true);
            }
        }

        public void OpenFile(string fileName)
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
            }
            OpenProject(fileName, true);
        }

        private void OpenProject(string fileName, bool create)
        {
            _worker.Do(() =>
            {
                _project = new EsperProject(fileName, create);
                _root = _project.FileStore.GetFullTree();
            },
            (e) =>
            {
                if (e != null)
                {
                    MessageBox.Show("FileStore error occured:\n" + e.Message ?? e.ToString(),
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    _root = null;
                }
                Init();
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

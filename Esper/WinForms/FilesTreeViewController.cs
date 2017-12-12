using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Esper.Model;

namespace Esper.WinForms
{
    internal sealed class FilesTreeViewController
    {
        public event EventHandler<NodeEventArgs> FocusedNodeChanged;
        public event EventHandler<NodeEventArgs> NodeDoubleClick;

        private readonly TreeView _treeView;
        private readonly FileStore _fileStore;
        private FileStore.Directory _root;

        public FilesTreeViewController(TreeView treeView, FileStore fileStore)
        {
            _treeView = treeView;
            _fileStore = fileStore;

            _treeView.MouseDown += treeView_MouseDown;
            _treeView.BeforeCollapse += treeView_BeforeCollapse;
            _treeView.AfterSelect += treeView_AfterSelect;
            _treeView.NodeMouseDoubleClick += treeView_NodeMouseDoubleClick;
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

        public void Init()
        {
            _root = _fileStore.GetFullTree();
            _treeView.Nodes.Clear();

            var node = new TreeNode(_root.Name);
            node.Tag = _root;
            _treeView.Nodes.Add(node);
            _Init(_root, node.Nodes);
            node.Expand();
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
                var node = new TreeNode(file.Name);
                node.Tag = file;
                nodes.Add(node);
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

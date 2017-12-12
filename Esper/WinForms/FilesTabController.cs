using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using ScintillaNET;
using Esper.Model;

namespace Esper.WinForms
{
    internal sealed class FilesTabController
    {
        private readonly TabControl _tabControl;
        private readonly FileStore _fileStore;
        private readonly FilesTreeViewController _treeViewController;
        private readonly List<TabItem> _items = new List<TabItem>();

        public FilesTabController(TabControl tabControl, FileStore fileStore, FilesTreeViewController treeViewController)
        {
            _tabControl = tabControl;
            _fileStore = fileStore;
            _treeViewController = treeViewController;

            _tabControl.TabPages.Clear();
            _tabControl.Visible = false;
            _tabControl.HotTrack = true;
            _tabControl.ContextMenuStrip.ItemClicked += contextMenuStrip_ItemClicked;
            _tabControl.MouseDown += tabControl_MouseDown;
            _treeViewController.NodeDoubleClick += treeViewController_NodeDoubleClick;
        }

        private void tabControl_MouseDown(object sender, MouseEventArgs e)
        {
            for (int i = 0; i < _tabControl.TabPages.Count; i++)
            {
                if (_tabControl.GetTabRect(i).Contains(e.Location))
                {
                    _tabControl.SelectedIndex = i;
                    break;
                }
            }
        }

        private void contextMenuStrip_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            if (e.ClickedItem?.Tag?.ToString() == "CLOSE_TAB")
            {
                var page = _tabControl.SelectedTab;
                _tabControl.TabPages.Remove(page);
                DeleteItem(page);
            }
            else if (e.ClickedItem?.Tag?.ToString() == "CLOSE_ALL_TAB")
            {
                _tabControl.TabPages.Clear();
                _items.Clear();
            }
            _tabControl.Visible = _tabControl.TabPages.Count > 0;
        }

        private void AddOrActivatePage(FileStore.File file)
        {
            var item = _items.FirstOrDefault(it => Equals(it.File, file));
            if (item == null)
            {
                item = new TabItem()
                {
                    Page = new TabPage(),
                    Editor = new Scintilla(),
                    File = file
                };
                item.Page.Text = file.Name;
                item.Page.Controls.Add(item.Editor);
                item.Editor.Dock = DockStyle.Fill;
                item.Editor.BorderStyle = BorderStyle.None;
                item.Editor.EolMode = Eol.CrLf;
                item.Editor.TextChanged += editor_TextChanged;

                switch (file.Type)
                {
                    case FileStore.FileType.Lua:
                        item.Editor.Lexer = Lexer.Cpp;
                        break;
                    default:
                        item.Editor.Lexer = Lexer.Container;
                        break;
                }

                item.Editor.Text = string.Join("\n", _fileStore.ReadFile(file));
                item.Editor.SetSavePoint();
                item.Editor.EmptyUndoBuffer();

                _tabControl.TabPages.Add(item.Page);
                _items.Add(item);
            }
            _tabControl.SelectedTab = item.Page;
            item.Editor.Focus();
            _tabControl.Visible = true;
        }

        private void editor_TextChanged(object sender, EventArgs e)
        {
            var item = _items.FirstOrDefault(it => it.Editor == sender);
            if (item != null)
            {
                item.Page.Text = item.File.Name;
            }
            if (item.Editor.Modified)
            {
                item.Page.Text += "*";
            }
        }

        private void DeleteItem(TabPage page)
        {
            var item = _items.FirstOrDefault(it => it.Page == page);
            if (item != null) _items.Remove(item);
        }

        private void treeViewController_NodeDoubleClick(object sender, FilesTreeViewController.NodeEventArgs e)
        {
            if (e.File != null)
            {
                AddOrActivatePage(e.File);
            }
        }

        private sealed class TabItem
        {
            public TabPage Page { get; set; }
            public Scintilla Editor { get; set; }
            public FileStore.File File { get; set; }
            //public bool IsChanged { get; set; }
        }
    }
}

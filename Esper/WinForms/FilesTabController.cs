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
    internal sealed class FilesTabController : ICopyPaste, IFileSave
    {
        private readonly TabControl _tabControl;
        private readonly EsperProject _project;
        private readonly FilesTreeViewController _treeViewController;
        private readonly List<TabItem> _items = new List<TabItem>();

        public bool CanCut
        {
            get
            {
                var item = GetSelectedItem();
                return item != null && !string.IsNullOrEmpty(item.Editor.SelectedText);
            }
        }

        public bool CanCopy
        {
            get
            {
                var item = GetSelectedItem();
                return item != null && !string.IsNullOrEmpty(item.Editor.SelectedText);
            }
        }

        public bool CanPaste
        {
            get
            {
                var item = GetSelectedItem();
                return item != null && item.Editor.CanPaste;
            }
        }

        public bool CanSelectAll
        {
            get
            {
                var item = GetSelectedItem();
                return item != null && item.Editor.CanSelect;
            }
        }

        public bool CanUndo
        {
            get
            {
                var item = GetSelectedItem();
                return item != null && item.Editor.CanUndo;
            }
        }

        public bool CanRedo
        {
            get
            {
                var item = GetSelectedItem();
                return item != null && item.Editor.CanRedo;
            }
        }

        public bool CanSaveFile
        {
            get
            {
                var item = GetSelectedItem();
                return item != null;
            }
        }

        public bool CanSaveAsFile
        {
            get
            {
                var item = GetSelectedItem();
                return item != null;
            }
        }

        public bool CanPrintFile
        {
            get
            {
                return false;
                //var item = GetSelectedItem();
                //return item != null;
            }
        }

        public bool CanPrintPreviewFile
        {
            get
            {
                return false;
                //var item = GetSelectedItem();
                //return item != null;
            }
        }

        public FilesTabController(TabControl tabControl, EsperProject project, FilesTreeViewController treeViewController)
        {
            _tabControl = tabControl;
            _project = project;
            _treeViewController = treeViewController;

            _tabControl.TabPages.Clear();
            _tabControl.Visible = false;
            _tabControl.HotTrack = true;
            _tabControl.ContextMenuStrip.ItemClicked += contextMenuStrip_ItemClicked;
            _tabControl.MouseDown += tabControl_MouseDown;
            _treeViewController.NodeDoubleClick += treeViewController_NodeDoubleClick;
        }

        public void Cut()
        {
            var item = GetSelectedItem();
            if (item != null) item.Editor.Cut();
        }

        public void Copy()
        {
            var item = GetSelectedItem();
            if (item != null) item.Editor.Copy();
        }

        public void Paste()
        {
            var item = GetSelectedItem();
            if (item != null) item.Editor.Paste();
        }

        public void SelectAll()
        {
            var item = GetSelectedItem();
            if (item != null) item.Editor.SelectAll();
        }

        public void Undo()
        {
            var item = GetSelectedItem();
            if (item != null) item.Editor.Undo();
        }

        public void Redo()
        {
            var item = GetSelectedItem();
            if (item != null) item.Editor.Redo();
        }

        public void SaveFile()
        {
            var item = GetSelectedItem();
            if (item != null)
            {
                _project.FileStore.SaveAllText(item.File, item.Editor.Text);//!!!! thread?
                SetSavePoint(item);
            }
        }

        public void SaveAsFile()
        {
            //...
        }

        public void PrintFile()
        {
            //...
        }

        public void PrintPreviewFile()
        {
            //...
        }

        public FileStore.File GetSelectedFile()
        {
            return _items.FirstOrDefault(it => it.Page == _tabControl.SelectedTab)?.File;
        }

        public void CloseAllTab()
        {
            _tabControl.TabPages.Clear();
            _items.Clear();
        }

        private TabItem GetSelectedItem()
        {
            return _items.FirstOrDefault(it => it.Page == _tabControl.SelectedTab);
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
                CloseAllTab();
            }
            _tabControl.Visible = _tabControl.TabPages.Count > 0;
        }

        private void editor_TextChanged(object sender, EventArgs e)
        {
            CheckTextModified(_items.FirstOrDefault(it => it.Editor == sender));
        }

        private void treeViewController_NodeDoubleClick(object sender, FilesTreeViewController.NodeEventArgs e)
        {
            if (e.File != null)
            {
                AddOrActivatePage(e.File);
            }
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
                //item.Page.Text = file.GetPath();
                item.Page.Controls.Add(item.Editor);

                ScintillaHelper.InitScintilla(item.Editor);

                switch (file.Type)
                {
                    case FileStore.FileType.Lua:
                        item.Editor.Lexer = Lexer.Cpp;
                        break;
                    default:
                        item.Editor.Lexer = Lexer.Container;
                        break;
                }

                item.Editor.TextChanged += editor_TextChanged;
                item.Editor.Text = _project.FileStore.ReadAllText(file); //!!!! thread?
                SetSavePoint(item);

                _tabControl.TabPages.Add(item.Page);
                _items.Add(item);
            }
            _tabControl.SelectedTab = item.Page;
            item.Editor.Focus();
            _tabControl.Visible = true;
        }

        private void SetSavePoint(TabItem item)
        {
            item.Editor.SetSavePoint();
            item.Editor.EmptyUndoBuffer();
            CheckTextModified(item);
        }

        private void CheckTextModified(TabItem item)
        {
            if (item != null)
            {
                var s = item.File.GetPath();
                if (item.Editor.Modified) s += "*";
                if (item.Page.Text != s)
                {
                    item.Page.Text = s;
                }
            }
        }

        private void DeleteItem(TabPage page)
        {
            var item = _items.FirstOrDefault(it => it.Page == page);
            if (item != null) _items.Remove(item);
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

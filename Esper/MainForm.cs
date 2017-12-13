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
using Esper.WinForms;

namespace Esper
{
    public partial class MainForm : Form
    {
        private FileStore _fileStore;
        private FilesTreeViewController _filesTreeController;
        private FilesTabController _filesTabController;

        public MainForm()
        {
            InitializeComponent();

            _fileStore = new FileStore("d:/TestCases");
            _filesTreeController = new FilesTreeViewController(filesTreeView, _fileStore);
            _filesTabController = new FilesTabController(filesTabControl, _fileStore, _filesTreeController);
            _filesTreeController.Init();
            //_filesTabController.
        }

        private void cutToolStripButton_Click(object sender, EventArgs e)
        {
            _filesTabController.Cut();
        }

        private void copyToolStripButton_Click(object sender, EventArgs e)
        {
            _filesTabController.Copy();
        }

        private void pasteToolStripButton_Click(object sender, EventArgs e)
        {
            _filesTabController.Paste();
        }

        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _filesTabController.Undo();
        }

        private void redoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _filesTabController.Redo();
        }

        private void cutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _filesTabController.Cut();
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _filesTabController.Copy();
        }

        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _filesTabController.Paste();
        }

        private void selectAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _filesTabController.SelectAll();
        }

        private void saveToolStripButton_Click(object sender, EventArgs e)
        {
            _filesTabController.SaveFile();
        }

        private void printToolStripButton_Click(object sender, EventArgs e)
        {
            _filesTabController.PrintFile();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _filesTabController.SaveFile();
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _filesTabController.SaveAsFile();
        }

        private void printToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _filesTabController.PrintFile();
        }

        private void printPreviewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _filesTabController.PrintPreviewFile();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void updateTimer_Tick(object sender, EventArgs e)
        {
            // Edit
            cutToolStripButton.Enabled = _filesTabController.CanCut;
            copyToolStripButton.Enabled = _filesTabController.CanCopy;
            pasteToolStripButton.Enabled = _filesTabController.CanPaste;

            cutToolStripMenuItem.Enabled = _filesTabController.CanCut;
            copyToolStripMenuItem.Enabled = _filesTabController.CanCopy;
            pasteToolStripMenuItem.Enabled = _filesTabController.CanPaste;
            selectAllToolStripMenuItem.Enabled = _filesTabController.CanSelectAll;
            undoToolStripMenuItem.Enabled = _filesTabController.CanUndo;
            redoToolStripMenuItem.Enabled = _filesTabController.CanRedo;

            // File
            saveToolStripButton.Enabled = _filesTabController.CanSaveFile;
            printToolStripButton.Enabled = _filesTabController.CanPrintFile;

            saveToolStripMenuItem.Enabled = _filesTabController.CanSaveFile;
            saveAsToolStripMenuItem.Enabled = _filesTabController.CanSaveAsFile;
            printToolStripMenuItem.Enabled = _filesTabController.CanPrintFile;
            printPreviewToolStripMenuItem.Enabled = _filesTabController.CanPrintPreviewFile;

            // Tools
            customizeToolStripMenuItem.Enabled = false;
            optionsToolStripMenuItem.Enabled = false;

            // Help
            helpToolStripButton.Enabled = false;

            contentsToolStripMenuItem.Enabled = false;
            indexToolStripMenuItem.Enabled = false;
            searchToolStripMenuItem.Enabled = false;
            aboutToolStripMenuItem.Enabled = false;
        }
    }
}

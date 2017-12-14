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
        private ComEspConnector _connector;
        private FilesTreeViewController _filesTreeController;
        private FilesTabController _filesTabController;
        private ConsoleController _consoleController;

        public MainForm()
        {
            InitializeComponent();

            splitContainer2_Panel1_Resize(this, EventArgs.Empty);

            _fileStore = new FileStore("d:/TestCases");
            _filesTreeController = new FilesTreeViewController(filesTreeView, _fileStore);
            _filesTabController = new FilesTabController(filesTabControl, _fileStore, _filesTreeController);
            _filesTreeController.Init();

            _connector = new ComEspConnector();
            _connector.PortName = "COM5";
            _connector.BaudRate = ComEspConnector.SerialBaudRate.BR_115200;
            _connector.Connect();

            _consoleController = new ConsoleController(_connector, consoleTextBox, sendConsoleTextBox);
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

        private void connectToolStripButton_Click(object sender, EventArgs e)
        {
            _connector.Connect();
        }

        private void disconnectToolStripButton_Click(object sender, EventArgs e)
        {
            _connector.Disconnect();
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

            // Right Tabs
            var ik = _connector.IsConnected ? "connect_established.png" : "connect_no.png";
            if (consoleTabPage.ImageKey != ik) consoleTabPage.ImageKey = ik;
            var t = _connector.IsConnected ? "Connected" : "Disconnected";
            if (consoleTabPage.ToolTipText != t) consoleTabPage.ToolTipText = t;

            // Connection
            connectToolStripButton.Enabled = !_connector.IsConnected;
            disconnectToolStripButton.Enabled = _connector.IsConnected;
            uploadToolStripButton.Enabled = false;
        }

        private void splitContainer2_Panel1_Resize(object sender, EventArgs e)
        {
            int pw = splitContainer2.Panel1.Width;
            int ph = splitContainer2.Panel1.Height;
            selectFileLabel.Location = new Point(
                (pw - selectFileLabel.Width) / 2, (ph - selectFileLabel.Height) / 2);
        }
    }
}

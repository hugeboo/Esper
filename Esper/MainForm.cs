using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using Utilities;
using Esper.Model;
using Esper.WinForms;

namespace Esper
{
    public partial class MainForm : Form
    {
        private EspComConnector _connector;
        private FilesTreeViewController _filesTreeController;
        private FilesTabController _filesTabController;
        private ConsoleController _consoleController;

        private EsperOptions _options;

        private readonly BackgroundWorker2 _worker;

        public MainForm()
        {
            InitializeComponent();

            splitContainer2_Panel1_Resize(this, EventArgs.Empty);

            _filesTreeController = new FilesTreeViewController(filesTreeView);
            _filesTabController = new FilesTabController(filesTabControl, _filesTreeController);
            _connector = new EspComConnector();
            _consoleController = new ConsoleController(_connector, consoleTextBox, sendConsoleTextBox);

            _worker = new BackgroundWorker2(this);
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

        private void newToolStripButton_Click(object sender, EventArgs e)
        {
            _filesTreeController.CreateFile();
        }

        private void openToolStripButton_Click(object sender, EventArgs e)
        {
            _filesTreeController.OpenFile(null);
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _filesTreeController.CreateFile();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _filesTreeController.OpenFile(null);
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

        private void saveAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _filesTabController.SaveAllFiles();
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
            DoConnector(() => { _connector.Connect(); }, false);
            //if (!_connector.IsConnected) _connector.Connect();
        }

        private void disconnectToolStripButton_Click(object sender, EventArgs e)
        {
            DoConnector(() => { _connector.Disconnect(); });
            //if (_connector.IsConnected) _connector.Disconnect();
        }

        private void restartToolStripButton_Click(object sender, EventArgs e)
        {
            DoConnector(() => { _connector.WriteLine("node.restart()"); });
            //if (_connector.IsConnected) _connector.WriteLine("node.restart()");
        }

        private void uploadToolStripButton_Click(object sender, EventArgs e)
        {
            if (_connector.IsConnected && _filesTabController.CanSaveFile)
            {
                _filesTabController.SaveFile((ee) =>
                {
                    if (ee == null)
                    {
                        var file = _filesTabController.GetSelectedFile();
                        var dst = file.GetPath();
                        DoConnector(() =>
                        {
                            EspExecutor.DoWriteFile(_connector, file.GetFullSystemPath(), dst);
                        });
                    }
                });
            }
        }

        private void optionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var dlg = new OptionsForm();
            dlg.SetOptions(_options);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                _options = dlg.GetOptions();
                ApplyOptions();
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            try
            {
                _options = EsperOptions.Load();
            }
            catch
            {
                _options = new EsperOptions();
            }
            ApplyOptions();
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(_options.LastProjectFileName))
            {
                _filesTreeController.OpenFile(_options.LastProjectFileName, (ee) =>
                {
                    if (ee == null)
                    {
                        if (_options.LastOpenedFiles != null)
                        {
                            foreach (var p in _options.LastOpenedFiles)
                            {
                                var f = _filesTreeController.FindFile(p);
                                if (f != null)
                                {
                                    _filesTabController.AddOrActivatePage(f);
                                }
                            }
                            if (_options.LastActiveFile != null)
                            {
                                _filesTabController.AddOrActivatePage(_filesTreeController.FindFile(_options.LastActiveFile));
                            }
                        }
                    }
                });
            }
            else
            {
#warning нужно какое-нибудь цивильное окно для такого случая
                _filesTreeController.OpenFile("projects\\default\\default.esper");
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                _options.LastProjectFileName = _filesTreeController?.Project?.FullFileName;
                _options.LastOpenedFiles = _filesTabController?.GetAllFiles().Select(f => f.GetPath()).ToArray();
                _options.LastActiveFile = _filesTabController.GetSelectedFile()?.GetPath();
                _options.Save();
            }
            catch { }
        }

        private void ApplyOptions()
        {
            DoConnector(() =>
            {
                _connector.Disconnect();
            });
            _connector.Options = (EsperOptions.ComPortOptions)_options.ComPort.Clone();
        }

        private void DoConnector(Action doWork, bool requiredConnection = true)
        {
            _worker.Do(() =>
            {
                if ((requiredConnection && _connector.IsConnected) ||
                    (!requiredConnection && !_connector.IsConnected))
                {
                    doWork();
                }
            },
            (e) =>
            {
                if (e != null)
                {
                    MessageBox.Show("Connector error occured:\n" + e.Message ?? e.ToString(),
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            });
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
            saveAllToolStripMenuItem.Enabled = _filesTabController.CanSaveAllFiles;
            printToolStripMenuItem.Enabled = _filesTabController.CanPrintFile;
            printPreviewToolStripMenuItem.Enabled = _filesTabController.CanPrintPreviewFile;

            // Tools
            customizeToolStripMenuItem.Enabled = false;
            optionsToolStripMenuItem.Enabled = true;

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
            var tt = _connector.IsConnected ? $"Console: {_options.ComPort.Name}" : "Console: disconnected";
            if (consoleTabPage.Text != tt) consoleTabPage.Text = tt;

            // Connection
            connectToolStripButton.Enabled = !_connector.IsConnected;
            disconnectToolStripButton.Enabled = _connector.IsConnected;
            restartToolStripButton.Enabled = _connector.IsConnected;
            uploadToolStripButton.Enabled = _connector.IsConnected && _filesTabController.CanSaveFile;
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

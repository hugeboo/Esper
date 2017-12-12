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

        private void saveToolStripButton_Click(object sender, EventArgs e)
        {
            _filesTabController.SaveCurrentFile();
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

using Esper.Model;

namespace Esper.WinForms
{
    public partial class CreateProjectForm : Form
    {
        public string FullFileName
        {
            get
            {
                return Path.Combine(dirTextBox.Text ?? "", 
                    Path.GetFileNameWithoutExtension(fileTextBox.Text ?? "") + EsperProject.EsperExtension);
            }
        }

        public CreateProjectForm()
        {
            InitializeComponent();
        }

        private void dirTextBox_TextChanged(object sender, EventArgs e)
        {
            okButton.Enabled = !string.IsNullOrWhiteSpace(FullFileName);
        }

        private void fileTextBox_TextChanged(object sender, EventArgs e)
        {
            okButton.Enabled = !string.IsNullOrWhiteSpace(FullFileName);
        }

        private void chooseDirButton_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog.ShowDialog()== DialogResult.OK)
            {
                dirTextBox.Text = folderBrowserDialog.SelectedPath;
            }
            okButton.Enabled = !string.IsNullOrWhiteSpace(FullFileName);
        }
    }
}

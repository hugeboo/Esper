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
    public partial class OptionsForm : Form
    {
        public OptionsForm()
        {
            InitializeComponent();

            var ports = EspComConnector.GetAvailablePortNames();
            portComboBox.Items.AddRange(ports);
            if (ports.Length == 0)
            {
                portComboBox.Enabled = false;
            }

            var bauds = Enum.GetValues(typeof(EspComConnector.SerialBaudRate)).
                Cast<EspComConnector.SerialBaudRate>().
                Select(b => (object)(int)b).ToArray();
            baudComboBox.Items.AddRange(bauds);
        }

        public void SetOptions(EsperOptions options)
        {
            portComboBox.SelectedItem = options.ComPort.Name;
            baudComboBox.SelectedItem = (int)options.ComPort.BaudRate;
            firstSymbolDelayNumericUpDown.Value = options.ComPort.AfterFirstSymbolDelay;
            symbolDelayNumericUpDown.Value = options.ComPort.AfterSymbolDelay;
        }

        public EsperOptions GetOptions()
        {
            var e = new EsperOptions();
            e.ComPort.Name = (string)portComboBox.SelectedItem;
            e.ComPort.BaudRate = (EspComConnector.SerialBaudRate)(int)baudComboBox.SelectedItem;
            e.ComPort.AfterFirstSymbolDelay = (int)firstSymbolDelayNumericUpDown.Value;
            e.ComPort.AfterSymbolDelay = (int)symbolDelayNumericUpDown.Value;
            return e;
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            okButton.Enabled = !string.IsNullOrEmpty((string)portComboBox.SelectedItem);
        }
    }
}

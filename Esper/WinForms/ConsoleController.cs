using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Esper.Model;

namespace Esper.WinForms
{
    internal sealed class ConsoleController
    {
        private IEspConnector _connector;
        private TextBox _consoleTextBox;
        private TextBox _sendTextBox;

        private readonly List<string> _lstHistory = new List<string>();
        private int _historyIndex = 0;

        public bool IsConnected
        {
            get { return _connector.IsConnected; }
        }

        public ConsoleController(IEspConnector connector, TextBox consoleTextBox, TextBox sendTextBox)
        {
            _connector = connector;
            _consoleTextBox = consoleTextBox;
            _sendTextBox = sendTextBox;

            _connector.LineReceived += connector_LineReceived;
            _sendTextBox.KeyPress += sendTextBox_KeyPress;
            _sendTextBox.KeyDown += sendTextBox_KeyDown;
        }

        public void Send(string command)
        {
            _connector.WriteLine(command);
        }

        public void Clear()
        {
            _consoleTextBox.Text = null;
        }

        private void connector_LineReceived(object sender, LineReceivedEventArgs e)
        {
            _consoleTextBox.BeginInvoke(new Action(() =>
            {
                _consoleTextBox.AppendText(e.Line);
                if (e.Type == LineReceivedEventArgs.LineType.General)
                {
                    _consoleTextBox.AppendText("\r\n");
                }
            }));
        }

        private void sendTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (int)Keys.Enter)
            {
                if (_connector.IsConnected) _connector.WriteLine(_sendTextBox.Text);
                if (!string.IsNullOrEmpty(_sendTextBox.Text))
                {
                    _lstHistory.Remove(_sendTextBox.Text);
                    _lstHistory.Add(_sendTextBox.Text);
                    _historyIndex = _lstHistory.Count - 1;
                }
                _sendTextBox.Text = null;
                e.Handled = true;
            }
            else if (e.KeyChar == (int)Keys.Escape)
            {
                if (_connector.IsConnected) _connector.WriteLine("\x1b");
                _sendTextBox.Text = null;
                e.Handled = true;
            }
        }

        private void sendTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (_lstHistory.Count > 0)
            {
                if (e.KeyCode == Keys.Down)
                {
                        _sendTextBox.Text = _lstHistory[_historyIndex];
                        _historyIndex -= 1;
                        if (_historyIndex < 0) _historyIndex = _lstHistory.Count - 1;
                        _sendTextBox.SelectionStart = 0;
                        _sendTextBox.SelectionLength = _sendTextBox.Text.Length;
                }
                else if (e.KeyCode == Keys.Up)
                {
                    _sendTextBox.Text = _lstHistory[_historyIndex];
                    _historyIndex += 1;
                    if (_historyIndex >= _lstHistory.Count) _historyIndex = 0;
                    _sendTextBox.SelectionStart = 0;
                    _sendTextBox.SelectionLength = _sendTextBox.Text.Length;
                }
            }
        }
    }
}

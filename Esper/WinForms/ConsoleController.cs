﻿using System;
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

        private readonly List<string> _sendHistory = new List<string>();

        public bool IsConnected
        {
            get { return _connector.IsConnected; }
        }

        public ConsoleController(IEspConnector connector, TextBox consoleTextBox, TextBox sendTextBox)
        {
            _connector = connector;
            _consoleTextBox = consoleTextBox;
            _sendTextBox = sendTextBox;

            _connector.RawLineReceived += connector_RawLineReceived;
            _connector.LineReceived += connector_LineReceived;
            _sendTextBox.KeyPress += sendTextBox_KeyPress;
        }

        private void connector_RawLineReceived(object sender, string e)
        {
            //_consoleTextBox.BeginInvoke(new Action(() =>
            //{
            //    _consoleTextBox.AppendText(e);
            //    if (e != "> " && e != ">> ")
            //    {
            //        _consoleTextBox.AppendText("\r\n");
            //    }
            //}));
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
                if (!string.IsNullOrEmpty(_sendTextBox.Text)) _sendHistory.Add(_sendTextBox.Text);
                _sendTextBox.Text = null;
            }
            else if (e.KeyChar == (int)Keys.Escape)
            {
                if (_connector.IsConnected) _connector.WriteLine("\x1b");
                _sendTextBox.Text = null;
            }
            //else if (e.KeyChar == (int)Keys.Down) // ??????
            //{
            //    _sendTextBox.Text = _sendHistory.LastOrDefault();
            //}
        }
    }
}

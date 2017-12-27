using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO.Ports;

namespace Esper.Model
{
    public sealed class EspComConnector : IEspConnector
    {
        public event EventHandler<byte> RawByteReceived;
        public event EventHandler<string> RawLineReceived;
        public event EventHandler<LineReceivedEventArgs> LineReceived;

        private bool _isDisposed;
        private SerialPort _port;
        private Thread _threadReader;
        private EsperOptions.ComPortOptions _options;

        private readonly List<byte> _buffer = new List<byte>();
        private bool _0dRecv;

        public bool IsConnected
        {
            get { return _port != null && _port.IsOpen; }
        }

        public EsperOptions.ComPortOptions Options
        {
            get { return _options; }
            set { if (value != null) _options = value; }
        }

        public EspComConnector()
        {
            _options = new EsperOptions.ComPortOptions();
        }

        public static string[] GetAvailablePortNames()
        {
            return SerialPort.GetPortNames();
        }

        public void Connect()
        {
            if (_isDisposed) throw new ObjectDisposedException(nameof(EspComConnector));
            if (IsConnected) throw new InvalidOperationException("Already connected");

            try
            {
                _port = new SerialPort(Options.Name);
                _port.BaudRate = (int)Options.BaudRate;
                _port.Parity = Parity.None;
                _port.StopBits = StopBits.One;
                _port.DataBits = 8;
                _port.Handshake = Handshake.None;
                //_port.NewLine = "\r\n";
                _port.DtrEnable = true;
                _port.RtsEnable = true;
                //_port.Encoding = Encoding.GetEncoding(1251);

                _port.WriteTimeout = 5000;
                _port.ReadTimeout = Timeout.Infinite;
                _port.WriteBufferSize = 4096;
                _port.ReadBufferSize = 4096;

                _port.Open();
                //WriteLine(null);
                //_port.DiscardInBuffer();
                //_port.DiscardOutBuffer();
            }
            catch
            {
                try { _port.Dispose(); } catch { }
                throw;
            }

            try
            {
                _threadReader = new Thread(ThreadReaderProc);
                _threadReader.Start();
            }
            catch
            {
                try { _port.Dispose(); } catch { }
                try { _threadReader.Abort(); } catch { }
                throw;
            }
        }

        public void Disconnect()
        {
            if (_isDisposed) throw new ObjectDisposedException(nameof(EspComConnector));
            if (IsConnected)
            {
                try { _port.Dispose(); } catch { }
                try { _threadReader.Abort(); } catch { }
            }
        }

        public void Dispose()
        {
            if (!_isDisposed)
            {
                Disconnect();
                _isDisposed = true;
            }
        }

        public void WriteLine(string line)
        {
            if (_isDisposed) throw new ObjectDisposedException(nameof(EspComConnector));
            if (!IsConnected) throw new InvalidOperationException("Not connected");
            var arr = Encoding.GetEncoding(1251).GetBytes(line ?? "");
            for (int i = 0; i < arr.Length; i++)
            {
                _port.Write(arr, i, 1);
                Thread.Sleep(i == 0 ? Options.AfterFirstSymbolDelay : Options.AfterSymbolDelay);
            }
            _port.Write(new byte[] { 0x0d }, 0, 1);
            Thread.Sleep(Options.AfterSymbolDelay);
        }

        private void ThreadReaderProc()
        {
            try
            {
                while (true)
                {
                    int b = _port.ReadByte();
                    if (b != -1)
                    {
                        RawByteReceived?.Invoke(this, (byte)b);

                        if (b == 0x0d)
                        {
                            _0dRecv = true;
                        }
                        else if (b == 0x0a)
                        {
                            if (_0dRecv)
                            {
                                // строка
                                var s = GetStringFromBuffer();
                                RawLineReceived?.Invoke(this, s);
                                LineReceived?.Invoke(this, new LineReceivedEventArgs() { Line = s, Type = LineReceivedEventArgs.LineType.General });
                                _0dRecv = false;
                                _buffer.Clear();
                            }
                        }
                        else
                        {
                            _buffer.Add((byte)b);
                            if (_buffer.Count == 2 && _buffer[0] == 0x3e && _buffer[1] == 0x20)
                            {
                                // приглашение >
                                LineReceived?.Invoke(this, new LineReceivedEventArgs() { Line = "> ", Type = LineReceivedEventArgs.LineType.Ask });
                                _0dRecv = false;
                                _buffer.Clear();
                            }
                            else if (_buffer.Count == 3 && _buffer[0] == 0x3e && _buffer[1] == 0x3e && _buffer[2] == 0x20)
                            {
                                // приглашение >>
                                LineReceived?.Invoke(this, new LineReceivedEventArgs() { Line = ">> ", Type = LineReceivedEventArgs.LineType.DoubleAsk });
                                _0dRecv = false;
                                _buffer.Clear();
                            }
                        }
                    }
                }
            }
            catch //(Exception ex)
            {
                //...
            }
            finally
            {
                try { _port.Dispose(); } catch { }
            }
        }

        private string GetStringFromBuffer()
        {
            var sb = new StringBuilder();
            for (int i = 0; i < _buffer.Count; i++)
            {
                if ((_buffer[i] >= 32 && _buffer[i] <= 127) || _buffer[i] == 9)
                {
                    sb.Append((char)_buffer[i]);
                }
                else
                {
                    sb.Append("<").Append(_buffer[i].ToString("X00")).Append(">");
                }
            }
            return sb.ToString();
        }
    }
}

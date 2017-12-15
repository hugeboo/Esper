using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Esper.Model
{
    public interface IEspDataStream
    {
        event EventHandler<byte> RawByteReceived;
        event EventHandler<string> RawLineReceived;
        event EventHandler<LineReceivedEventArgs> LineReceived;

        void WriteLine(string line);
    }

    public sealed class LineReceivedEventArgs : EventArgs
    {
        public enum LineType
        {
            General, Ask, DoubleAsk
        }

        public string Line { get; set; }
        public LineType Type { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Esper.Model
{
    public sealed class EsperOptions : ICloneable
    {
        public ComPortOptions ComPort { get; set; } = new ComPortOptions();

        public object Clone()
        {
            var e = new EsperOptions();
            e.ComPort = (ComPortOptions)this.ComPort.Clone();
            return e;
        }

        public sealed class ComPortOptions : ICloneable
        {
            public string Name { get; set; }
            public EspComConnector.SerialBaudRate BaudRate { get; set; } = EspComConnector.SerialBaudRate.BR_115200;
            public int AfterFirstSymbolDelay { get; set; } = 100;
            public int AfterSymbolDelay { get; set; } = 10;

            public object Clone()
            {
                return this.MemberwiseClone();
            }
        }
    }
}

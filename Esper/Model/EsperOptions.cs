using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml.Serialization;
using System.Reflection;

namespace Esper.Model
{
    public sealed class EsperOptions : ICloneable
    {
        public ComPortOptions ComPort { get; set; } = new ComPortOptions();

        // State
        public string LastProjectFileName { get; set; }
        public string[] LastOpenedFiles { get; set; }
        public string LastActiveFile { get; set; }

        public object Clone()
        {
            var e = new EsperOptions();
            e.ComPort = (ComPortOptions)this.ComPort.Clone();
            return e;
        }

        public static EsperOptions Load()
        {
            using (var sr = new StreamReader(GetFilePath()))
            {
                var xml = new XmlSerializer(typeof(EsperOptions));
                return xml.Deserialize(sr) as EsperOptions;
            }
        }

        public void Save()
        {
            using(var sw = new StreamWriter(GetFilePath()))
            {
                var xml = new XmlSerializer(typeof(EsperOptions));
                xml.Serialize(sw, this);
            }
        }

        public static string GetFilePath()
        {
            var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            return Path.Combine(dir, "esper.config.xml");
        }

        public sealed class ComPortOptions : ICloneable
        {
            public string Name { get; set; }
            public SerialBaudRate BaudRate { get; set; } = SerialBaudRate.BR_115200;
            public int AfterFirstSymbolDelay { get; set; } = 100;
            public int AfterSymbolDelay { get; set; } = 10;

            public object Clone()
            {
                return this.MemberwiseClone();
            }
        }
    }

    public enum SerialBaudRate
    {
        BR_4800 = 4800,
        BR_9600 = 9600,
        BR_19200 = 19200,
        BR_57600 = 57600,
        BR_115200 = 115200
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;

namespace Esper.Model
{
    internal static class EspExecutor
    {
        public static bool DoWriteFile(IEspDataStream stream, string localPath, string dstFileName)
        {
            try
            {
                var lines = File.ReadAllLines(localPath);
                Do(stream, $"file.open(\"{dstFileName}\", \"w\")", 5000);
                Thread.Sleep(50);
                foreach (var line in lines)
                {
                    var trim = line.Trim();
                    if (!string.IsNullOrEmpty(trim) && !trim.StartsWith("--"))
                    {
                        Do(stream, $"file.writeline([[{trim}]])", 5000);
                        Thread.Sleep(50);
                    }
                }
                Do(stream, "file.close()", 5000);
                Thread.Sleep(50);
                return true;
            }
            catch
            {
                try { Do(stream, "file.close()", 5000); } catch { }
                return false;
            }
        }

        public static void Do(IEspDataStream stream, string command, int timeoutMSec)
        {
            AutoResetEvent ev = null;
            bool ok = true;
            var h = new EventHandler<LineReceivedEventArgs>((o, e) =>
            {
                if (e.Type == LineReceivedEventArgs.LineType.Ask)
                {
                    //ok = true;
                    ev.Set();
                }
                else if (e.Type == LineReceivedEventArgs.LineType.DoubleAsk)
                {
                    ok = false;
                    ev.Set();
                }
                else if (e.Type == LineReceivedEventArgs.LineType.General)
                {
                    if (e.Line.StartsWith("stdin:")) ok = false;
                }
            });

            try
            {
                ev = new AutoResetEvent(false);
                stream.LineReceived += h;
                stream.WriteLine(command);
                var w = ev.WaitOne(timeoutMSec);
                if (!w || !ok) throw new InvalidOperationException();
            }
            finally
            {
                stream.LineReceived -= h;
                if (ev != null) ev.Dispose();
            }
        }
    }
}

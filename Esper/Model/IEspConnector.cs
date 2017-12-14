using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Esper.Model
{
    internal interface IEspConnector : IEspDataStream, IDisposable
    {
        bool IsConnected { get; }
        void Connect();
        void Disconnect();
    }
}

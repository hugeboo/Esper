using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Esper.WinForms
{
    internal interface IFileSave
    {
        bool CanSaveFile { get; }
        bool CanSaveAsFile { get; }
        bool CanSaveAllFiles { get; }
        bool CanPrintFile { get; }
        bool CanPrintPreviewFile { get; }

        void SaveFile();
        void SaveAsFile();
        void SaveAllFiles();
        void PrintFile();
        void PrintPreviewFile();
    }
}

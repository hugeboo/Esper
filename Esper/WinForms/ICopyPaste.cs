using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Esper.WinForms
{
    internal interface ICopyPaste
    {
        bool CanCut { get; }
        bool CanCopy { get; }
        bool CanPaste { get; }
        bool CanSelectAll { get; }
        bool CanUndo { get; }
        bool CanRedo { get; }

        void Cut();
        void Copy();
        void Paste();
        void SelectAll();
        void Undo();
        void Redo();
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

using ScintillaNET;

namespace Esper.WinForms
{
    internal static class ScintillaHelper
    {
        public static void InitScintilla(Scintilla editor)
        {
            // INITIAL VIEW CONFIG
            editor.Dock = DockStyle.Fill;
            editor.BorderStyle = BorderStyle.None;
            editor.EolMode = Eol.CrLf;
            editor.WrapMode = WrapMode.None;
            editor.IndentationGuides = IndentView.LookBoth;

            // STYLING
            //InitColors(editor);
            InitSyntaxColoring2(editor);

            // NUMBER MARGIN
            InitNumberMargin(editor);

            // BOOKMARK MARGIN
            //InitBookmarkMargin(editor);

            // CODE FOLDING MARGIN
            //InitCodeFolding(editor);
        }

        private static void InitColors(Scintilla editor)
        {
            editor.SetSelectionBackColor(true, IntToColor(0x114D9C));
        }

        private static void InitSyntaxColoring2(Scintilla editor)
        {
            // Configuring the default style with properties
            // we have common to every lexer style saves time.
            editor.StyleResetDefault();
            editor.Styles[Style.Default].Font = "Consolas";
            editor.Styles[Style.Default].Size = 10;
            editor.StyleClearAll();

            // Configure the CPP (C#) lexer styles
            editor.Styles[Style.Cpp.Default].ForeColor = Color.Black;
            editor.Styles[Style.Cpp.Comment].ForeColor = Color.FromArgb(0, 128, 0); // Green
            editor.Styles[Style.Cpp.CommentLine].ForeColor = Color.FromArgb(0, 128, 0); // Green
            editor.Styles[Style.Cpp.CommentLineDoc].ForeColor = Color.FromArgb(128, 128, 128); // Gray
            editor.Styles[Style.Cpp.Number].ForeColor = Color.Olive;
            editor.Styles[Style.Cpp.Word].ForeColor = Color.Blue;
            editor.Styles[Style.Cpp.Word2].ForeColor = Color.SlateBlue;
            editor.Styles[Style.Cpp.String].ForeColor = Color.FromArgb(163, 21, 21); // Red
            editor.Styles[Style.Cpp.Character].ForeColor = Color.FromArgb(163, 21, 21); // Red
            editor.Styles[Style.Cpp.Verbatim].ForeColor = Color.FromArgb(163, 21, 21); // Red
            editor.Styles[Style.Cpp.StringEol].BackColor = Color.Pink;
            editor.Styles[Style.Cpp.Operator].ForeColor = Color.Purple;
            editor.Styles[Style.Cpp.Preprocessor].ForeColor = Color.Maroon;

            editor.Lexer = Lexer.Cpp;

            // Set the keywords
            editor.SetKeywords(0, "local nil function end then abstract as base break case catch checked continue default delegate do else event explicit extern false finally fixed for foreach goto if implicit in interface internal is lock namespace new null object operator out override params private protected public readonly ref return sealed sizeof stackalloc switch this throw true try typeof unchecked unsafe using virtual while");
            editor.SetKeywords(1, "collectgarbage wifi print sntp adc node bool byte char class const decimal double enum float int long sbyte short static string struct uint ulong ushort void");
        }

        private static void InitSyntaxColoring(Scintilla editor)
        {
            // Configure the default style
            editor.StyleResetDefault();
            editor.Styles[Style.Default].Font = "Consolas";
            editor.Styles[Style.Default].Size = 10;
            editor.Styles[Style.Default].BackColor = IntToColor(0x212121);
            editor.Styles[Style.Default].ForeColor = IntToColor(0xFFFFFF);
            editor.StyleClearAll();

            // Configure the CPP (C#) lexer styles
            editor.Styles[Style.Cpp.Identifier].ForeColor = IntToColor(0xD0DAE2);
            editor.Styles[Style.Cpp.Comment].ForeColor = IntToColor(0xBD758B);
            editor.Styles[Style.Cpp.CommentLine].ForeColor = IntToColor(0x40BF57);
            editor.Styles[Style.Cpp.CommentDoc].ForeColor = IntToColor(0x2FAE35);
            editor.Styles[Style.Cpp.Number].ForeColor = IntToColor(0xFFFF00);
            editor.Styles[Style.Cpp.String].ForeColor = IntToColor(0xFFFF00);
            editor.Styles[Style.Cpp.Character].ForeColor = IntToColor(0xE95454);
            editor.Styles[Style.Cpp.Preprocessor].ForeColor = IntToColor(0x8AAFEE);
            editor.Styles[Style.Cpp.Operator].ForeColor = IntToColor(0xE0E0E0);
            editor.Styles[Style.Cpp.Regex].ForeColor = IntToColor(0xff00ff);
            editor.Styles[Style.Cpp.CommentLineDoc].ForeColor = IntToColor(0x77A7DB);
            editor.Styles[Style.Cpp.Word].ForeColor = IntToColor(0x48A8EE);
            editor.Styles[Style.Cpp.Word2].ForeColor = IntToColor(0xF98906);
            editor.Styles[Style.Cpp.CommentDocKeyword].ForeColor = IntToColor(0xB3D991);
            editor.Styles[Style.Cpp.CommentDocKeywordError].ForeColor = IntToColor(0xFF0000);
            editor.Styles[Style.Cpp.GlobalClass].ForeColor = IntToColor(0x48A8EE);

            editor.Lexer = Lexer.Cpp;

            editor.SetKeywords(0, "class extends implements import interface new case do while else if for in switch throw get set function var try catch finally while with default break continue delete return each const namespace package include use is as instanceof typeof author copy default deprecated eventType example exampleText exception haxe inheritDoc internal link mtasc mxmlc param private return see serial serialData serialField since throws usage version langversion playerversion productversion dynamic private public partial static intrinsic internal native override protected AS3 final super this arguments null Infinity NaN undefined true false abstract as base bool break by byte case catch char checked class const continue decimal default delegate do double descending explicit event extern else enum false finally fixed float for foreach from goto group if implicit in int interface internal into is lock long new null namespace object operator out override orderby params private protected public readonly ref return switch struct sbyte sealed short sizeof stackalloc static string select this throw true try typeof uint ulong unchecked unsafe ushort using var virtual volatile void while where yield");
            editor.SetKeywords(1, "void Null ArgumentError arguments Array Boolean Class Date DefinitionError Error EvalError Function int Math Namespace Number Object RangeError ReferenceError RegExp SecurityError String SyntaxError TypeError uint XML XMLList Boolean Byte Char DateTime Decimal Double Int16 Int32 Int64 IntPtr SByte Single UInt16 UInt32 UInt64 UIntPtr Void Path File System Windows Forms ScintillaNET");
        }

        #region Numbers, Bookmarks, Code Folding

        /// <summary>
        /// the background color of the text area
        /// </summary>
        private const int BACK_COLOR = 0x2A211C;

        /// <summary>
        /// default text color of the text area
        /// </summary>
        private const int FORE_COLOR = 0xB7B7B7;

        /// <summary>
        /// change this to whatever margin you want the line numbers to show in
        /// </summary>
        private const int NUMBER_MARGIN = 1;

        /// <summary>
        /// change this to whatever margin you want the bookmarks/breakpoints to show in
        /// </summary>
        private const int BOOKMARK_MARGIN = 2;
        private const int BOOKMARK_MARKER = 2;

        /// <summary>
        /// change this to whatever margin you want the code folding tree (+/-) to show in
        /// </summary>
        private const int FOLDING_MARGIN = 3;

        /// <summary>
        /// set this true to show circular buttons for code folding (the [+] and [-] buttons on the margin)
        /// </summary>
        private const bool CODEFOLDING_CIRCULAR = true;

        private static void InitNumberMargin(Scintilla editor)
        {
            //editor.Styles[Style.LineNumber].BackColor = IntToColor(BACK_COLOR);
            editor.Styles[Style.LineNumber].ForeColor = Color.Silver;// IntToColor(FORE_COLOR);
            editor.Styles[Style.IndentGuide].ForeColor = Color.Silver;// IntToColor(FORE_COLOR);
            //editor.Styles[Style.IndentGuide].BackColor = IntToColor(BACK_COLOR);

            var nums = editor.Margins[NUMBER_MARGIN];
            nums.Width = 30;
            nums.Type = MarginType.Number;
            nums.Sensitive = true;
            nums.Mask = 0;

            //editor.MarginClick += TextArea_MarginClick;
        }

        private static void InitBookmarkMargin(Scintilla editor)
        {
            var margin = editor.Margins[BOOKMARK_MARGIN];
            margin.Width = 20;
            margin.Sensitive = true;
            margin.Type = MarginType.Symbol;
            margin.Mask = (1 << BOOKMARK_MARKER);
            //margin.Cursor = MarginCursor.Arrow;

            var marker = editor.Markers[BOOKMARK_MARKER];
            marker.Symbol = MarkerSymbol.Circle;
            marker.SetBackColor(IntToColor(0xFF003B));
            marker.SetForeColor(IntToColor(0x000000));
            marker.SetAlpha(100);
        }

        private static void InitCodeFolding(Scintilla editor)
        {
            editor.SetFoldMarginColor(true, IntToColor(BACK_COLOR));
            editor.SetFoldMarginHighlightColor(true, IntToColor(BACK_COLOR));

            // Enable code folding
            editor.SetProperty("fold", "1");
            editor.SetProperty("fold.compact", "1");

            // Configure a margin to display folding symbols
            editor.Margins[FOLDING_MARGIN].Type = MarginType.Symbol;
            editor.Margins[FOLDING_MARGIN].Mask = Marker.MaskFolders;
            editor.Margins[FOLDING_MARGIN].Sensitive = true;
            editor.Margins[FOLDING_MARGIN].Width = 20;

            // Set colors for all folding markers
            for (int i = 25; i <= 31; i++)
            {
                editor.Markers[i].SetForeColor(IntToColor(BACK_COLOR)); // styles for [+] and [-]
                editor.Markers[i].SetBackColor(IntToColor(FORE_COLOR)); // styles for [+] and [-]
            }

            // Configure folding markers with respective symbols
            editor.Markers[Marker.Folder].Symbol = CODEFOLDING_CIRCULAR ? MarkerSymbol.CirclePlus : MarkerSymbol.BoxPlus;
            editor.Markers[Marker.FolderOpen].Symbol = CODEFOLDING_CIRCULAR ? MarkerSymbol.CircleMinus : MarkerSymbol.BoxMinus;
            editor.Markers[Marker.FolderEnd].Symbol = CODEFOLDING_CIRCULAR ? MarkerSymbol.CirclePlusConnected : MarkerSymbol.BoxPlusConnected;
            editor.Markers[Marker.FolderMidTail].Symbol = MarkerSymbol.TCorner;
            editor.Markers[Marker.FolderOpenMid].Symbol = CODEFOLDING_CIRCULAR ? MarkerSymbol.CircleMinusConnected : MarkerSymbol.BoxMinusConnected;
            editor.Markers[Marker.FolderSub].Symbol = MarkerSymbol.VLine;
            editor.Markers[Marker.FolderTail].Symbol = MarkerSymbol.LCorner;

            // Enable automatic folding
            editor.AutomaticFold = (AutomaticFold.Show | AutomaticFold.Click | AutomaticFold.Change);
        }

        //private void TextArea_MarginClick(object sender, MarginClickEventArgs e)
        //{
        //    if (e.Margin == BOOKMARK_MARGIN)
        //    {
        //        // Do we have a marker for this line?
        //        const uint mask = (1 << BOOKMARK_MARKER);
        //        var line = editor.Lines[editor.LineFromPosition(e.Position)];
        //        if ((line.MarkerGet() & mask) > 0)
        //        {
        //            // Remove existing bookmark
        //            line.MarkerDelete(BOOKMARK_MARKER);
        //        }
        //        else
        //        {
        //            // Add bookmark
        //            line.MarkerAdd(BOOKMARK_MARKER);
        //        }
        //    }
        //}

        #endregion

        #region Indent / Outdent

        private static void Indent(Scintilla editor)
        {
            // we use this hack to send "Shift+Tab" to scintilla, since there is no known API to indent,
            // although the indentation function exists. Pressing TAB with the editor focused confirms this.
            GenerateKeystrokes(editor, "{TAB}");
        }

        private static void Outdent(Scintilla editor)
        {
            // we use this hack to send "Shift+Tab" to scintilla, since there is no known API to outdent,
            // although the indentation function exists. Pressing Shift+Tab with the editor focused confirms this.
            GenerateKeystrokes(editor, "+{TAB}");
        }

        private static void GenerateKeystrokes(Scintilla editor, string keys)
        {
            HotKeyManager.Enable = false;
            editor.Focus();
            SendKeys.Send(keys);
            HotKeyManager.Enable = true;
        }

        #endregion

        #region Utils

        private static Color IntToColor(int rgb)
        {
            return Color.FromArgb(255, (byte)(rgb >> 16), (byte)(rgb >> 8), (byte)rgb);
        }

        #endregion
    }

    internal class HotKeyManager
    {
        public static bool Enable = true;

        public static void AddHotKey(Form form, Action function, Keys key, bool ctrl = false, bool shift = false, bool alt = false)
        {
            form.KeyPreview = true;

            form.KeyDown += delegate (object sender, KeyEventArgs e)
            {
                if (IsHotkey(e, key, ctrl, shift, alt))
                {
                    function();
                }
            };
        }

        public static bool IsHotkey(KeyEventArgs eventData, Keys key, bool ctrl = false, bool shift = false, bool alt = false)
        {
            return eventData.KeyCode == key && eventData.Control == ctrl && eventData.Shift == shift && eventData.Alt == alt;
        }
    }
}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Eto.Forms;
using Eto;
using Eto.CodeEditor;
using Eto.CodeEditor.XamMac2;
using AppKit;
using ScintillaNET;
using Foundation;
using System.IO;
using ObjCRuntime;
using System.Runtime.InteropServices;

[assembly: ExportHandler(typeof(CodeEditor), typeof(CodeEditorHandler))]

namespace Eto.CodeEditor.XamMac2
{
    public class CodeEditorHandler : Eto.Mac.Forms.MacView<ScintillaView, CodeEditor, CodeEditor.ICallback>, CodeEditor.IHandler
    {
        static CodeEditorHandler()
        {
            var path = Path.Combine(NSBundle.MainBundle.PrivateFrameworksPath, "Scintilla.framework", "Scintilla");
            Dlfcn.dlopen(path, 4);
        }

        private EtoScintillaNotificationProtocol notificationProtocol;
        public CodeEditorHandler()
        {
            var sv = new ScintillaView();
            notificationProtocol = new EtoScintillaNotificationProtocol();
            notificationProtocol.Notify += NotificationProtocol_Notify;
            sv.WeakDelegate = notificationProtocol;
            Control = sv;

            FontName = "Menlo";
            FontSize = 14;
            LineNumberColumnWidth = 40;
            TabWidth = 4;
            ReplaceTabsWithSpaces = true;
            ShowIndentationGuides();
            Control.Message(NativeMethods.SCI_AUTOCSETMAXHEIGHT, new IntPtr(10), IntPtr.Zero);
        }

        public string Text
        {
            get => Control.Text;
            set
            {
                Control.Text = value ?? string.Empty;
            }
        }

        public override NSView ContainerControl => Control;

        public override bool Enabled { get; set; }

        public void SetKeywords(int set, string keywords)
        {
            Control.SetKeywords(set, keywords);
        }

        public void SetProgrammingLanguage(ProgrammingLanguage language, string[] keywordSets)
        {
            int which = ScintillaNET.NativeMethods.SCLEX_CPP;
            switch (language)
            {
                case ProgrammingLanguage.CSharp:
                case ProgrammingLanguage.GLSL:
                    which = ScintillaNET.NativeMethods.SCLEX_CPP;
                    break;
                case ProgrammingLanguage.VB:
                    which = ScintillaNET.NativeMethods.SCLEX_VB;
                    break;
                case ProgrammingLanguage.Python:
                    which = ScintillaNET.NativeMethods.SCLEX_PYTHON;
                    break;
            }
            Control.SetGeneralProperty(ScintillaNET.NativeMethods.SCI_SETLEXER, which, 0);

            if (keywordSets != null)
            {
                for (int i = 0; i < keywordSets.Length; i++)
                {
                    SetKeywords(i, keywordSets[i]);
                }
            }
        }

        public bool IsWhitespaceVisible => Control.GetGeneralProperty(NativeMethods.SCI_GETVIEWWS) == 1;

        public void ShowWhitespace()
        {
            Control.SetGeneralProperty(NativeMethods.SCI_SETVIEWWS, NativeMethods.SCWS_VISIBLEALWAYS);
        }

        public void HideWhitespace()
        {
            Control.SetGeneralProperty(NativeMethods.SCI_SETVIEWWS, 0);
        }

        public void ShowWhitespaceWithColor(Eto.Drawing.Color color)
        {
            ShowWhitespace();
            Control.SetColorProperty(NativeMethods.SCI_SETWHITESPACEBACK, NativeMethods.SCWS_VISIBLEALWAYS, color.ToHex());
        }

        public bool AreIndentationGuidesVisible => Control.GetGeneralProperty(NativeMethods.SCI_GETINDENTATIONGUIDES) != NativeMethods.SC_IV_NONE;

        public void ShowIndentationGuides()
        {
            Control.SetGeneralProperty(NativeMethods.SCI_SETINDENTATIONGUIDES, NativeMethods.SC_IV_LOOKBOTH);
        }

        public void HideIndentationGuides()
        {
            Control.SetGeneralProperty(NativeMethods.SCI_SETINDENTATIONGUIDES, NativeMethods.SC_IV_NONE);
        }

        public string FontName
        {
            get
            {
                return Control.GetStringProperty(ScintillaNET.NativeMethods.SCI_STYLEGETFONT, ScintillaNET.NativeMethods.STYLE_DEFAULT);
            }
            set
            {
                Control.SetStringProperty(ScintillaNET.NativeMethods.SCI_STYLEGETFONT, ScintillaNET.NativeMethods.STYLE_DEFAULT, value);
            }
        }

        public int FontSize
        {
            get
            {
                return (int)Control.GetGeneralProperty(ScintillaNET.NativeMethods.SCI_STYLEGETSIZE);
            }
            set
            {
                Control.SetGeneralProperty(ScintillaNET.NativeMethods.SCI_STYLESETSIZE, ScintillaNET.NativeMethods.STYLE_DEFAULT, value);
            }
        }

        public int TabWidth
        {
            get
            {
                return (int)Control.GetGeneralProperty(NativeMethods.SCI_GETTABWIDTH);
            }
            set
            {
                Control.SetGeneralProperty(NativeMethods.SCI_SETTABWIDTH, value);
            }
        }

        public bool ReplaceTabsWithSpaces
        {
            get
            {
                return (int)Control.GetGeneralProperty(NativeMethods.SCI_GETUSETABS) == 0;
            }
            set
            {
                Control.SetGeneralProperty(NativeMethods.SCI_SETUSETABS, value ? 0 : 1);
            }
        }

        public int LineNumberColumnWidth
        {
            get
            {
                return (int)Control.GetGeneralProperty(NativeMethods.SCI_GETMARGINWIDTHN);
            }
            set
            {
                Control.SetGeneralProperty(NativeMethods.SCI_SETMARGINWIDTHN, 0, value);
            }
        }

        public void SetColor(Section section, Eto.Drawing.Color foreground, Eto.Drawing.Color background)
        {
            string fg = foreground.ToHex(false);
            string bg = background.ToHex(false);
            //NSColor fg = NSColor.FromRgba(foreground.R, foreground.G, foreground.B, foreground.A);
            //NSColor bg = NSColor.FromRgba(background.R, background.G, background.B, background.A);
            if (section == Section.Comment)
            {
                if (foreground != Eto.Drawing.Colors.Transparent)
                {
                    Control.SetColorProperty(NativeMethods.SCI_STYLESETFORE, NativeMethods.SCE_PROPS_COMMENT, fg);
                    Control.SetColorProperty(NativeMethods.SCI_STYLESETFORE, NativeMethods.SCE_C_COMMENTLINE, fg);
                }
                if (background != Eto.Drawing.Colors.Transparent)
                {
                    Control.SetColorProperty(NativeMethods.SCI_STYLESETBACK, NativeMethods.SCE_PROPS_COMMENT, bg);
                    Control.SetColorProperty(NativeMethods.SCI_STYLESETBACK, NativeMethods.SCE_C_COMMENTLINE, bg);
                }
            }

            if (section == Section.Keyword)
            {
                if (foreground != Eto.Drawing.Colors.Transparent)
                {
                    Control.SetColorProperty(NativeMethods.SCI_STYLESETFORE, NativeMethods.SCE_C_WORD, fg);
                    Control.SetColorProperty(NativeMethods.SCI_STYLESETFORE, NativeMethods.SCE_C_WORD2, fg);
                }
                if (background != Eto.Drawing.Colors.Transparent)
                {
                    Control.SetColorProperty(NativeMethods.SCI_STYLESETBACK, NativeMethods.SCE_C_WORD, bg);
                    Control.SetColorProperty(NativeMethods.SCI_STYLESETBACK, NativeMethods.SCE_C_WORD2, bg);
                }
            }

            if (section == Section.LineNumber)
            {
                if (foreground != Eto.Drawing.Colors.Transparent)
                {
                    Control.SetColorProperty(NativeMethods.SCI_STYLESETFORE, NativeMethods.STYLE_LINENUMBER, fg);
                }
                if (background != Eto.Drawing.Colors.Transparent)
                {
                    Control.SetColorProperty(NativeMethods.SCI_STYLESETBACK, NativeMethods.STYLE_LINENUMBER, bg);
                }
            }
        }

        public int CurrentPosition
        {
            get => (int)Control.GetGeneralProperty(NativeMethods.SCI_GETCURRENTPOS);
            set => Control.SetGeneralProperty(NativeMethods.SCI_GOTOPOS, value);
        }

        public int CurrentPositionInLine =>
            CurrentPosition - (int)Control.GetGeneralProperty(NativeMethods.SCI_POSITIONFROMLINE, CurrentLineNumber);

        public int CurrentLineNumber => (int)Control.GetGeneralProperty(NativeMethods.SCI_LINEFROMPOSITION, CurrentPosition);

        public int GetLineIndentation(int lineNumber) => (int)Control.GetGeneralProperty(NativeMethods.SCI_GETLINEINDENTATION, lineNumber);

        public void SetLineIndentation(int lineNumber, int indentation) => Control.SetGeneralProperty(NativeMethods.SCI_SETLINEINDENTATION, lineNumber, indentation);

        public char GetLineLastChar(int lineNumber)
        {
            var lineEndPos = Control.GetGeneralProperty(NativeMethods.SCI_GETLINEENDPOSITION, lineNumber);
            var lineStartPos = Control.GetGeneralProperty(NativeMethods.SCI_POSITIONFROMLINE, lineNumber);
            char lineLastChar;
            do
            {
                lineLastChar = (char)Control.GetGeneralProperty(NativeMethods.SCI_GETCHARAT, lineEndPos--);
            }
            while (lineEndPos >= lineStartPos && (lineLastChar == '\n' || lineLastChar == '\r'));
            return lineLastChar;
        }

        public string GetLineText(int lineNumber)
        {
            var start = Control.GetGeneralProperty(NativeMethods.SCI_POSITIONFROMLINE, lineNumber);
            var length = Control.GetGeneralProperty(NativeMethods.SCI_LINELENGTH, lineNumber);
            var ptr = Control.GetGeneralProperty(NativeMethods.SCI_GETRANGEPOINTER, start, length);
            if (ptr == 0)
                return string.Empty;
            var text = Mac.Helpers.GetString(new IntPtr(ptr), (int)length, Encoding); // new string((sbyte*)ptr, 0, length.ToInt32(), scintilla.Encoding);
            return text;
        }
        // private because it's a test. To see if it works like GetLineText
        private string GetLineText2(int lineNumber)
        {
            // look at SCI_GETLINE
            return "not implemented";
        }

        public int GetLineLength(int lineNumber) => (int)Control.GetGeneralProperty(NativeMethods.SCI_LINELENGTH, lineNumber);

        private const int ErrorIndex = 20;
        private const int WarningIndex = 21;
        public void SetupIndicatorStyles()
        {
            SetupIndicator(ErrorIndex, NativeMethods.INDIC_SQUIGGLE, Eto.Drawing.Colors.Red);
            SetupIndicator(WarningIndex, NativeMethods.INDIC_SQUIGGLE, Eto.Drawing.Colors.Gold);
        }

        void SetupIndicator(uint index, int style, Eto.Drawing.Color forecolor)
        {
            Control.Message(NativeMethods.SCI_INDICSETSTYLE, new IntPtr(index), new IntPtr(style));
            int abgr = forecolor.Rb | forecolor.Gb << 8 | forecolor.Bb << 16;
            Control.Message(NativeMethods.SCI_INDICSETFORE, new IntPtr(index), new IntPtr(abgr));
            Control.Message(NativeMethods.SCI_INDICSETALPHA, new IntPtr(index), new IntPtr(0));
            Control.Message(NativeMethods.SCI_INDICSETUNDER, new IntPtr(index), new IntPtr(1));
        }

        public void ClearAllErrorIndicators()
        {
            Control.Message(NativeMethods.SCI_SETINDICATORCURRENT, new IntPtr(ErrorIndex), IntPtr.Zero);
            int length = Text.Length;
            Control.Message(NativeMethods.SCI_INDICATORCLEARRANGE, IntPtr.Zero, new IntPtr(length));
        }

        public void ClearAllWarningIndicators()
        {
            Control.Message(NativeMethods.SCI_SETINDICATORCURRENT, new IntPtr(WarningIndex), IntPtr.Zero);
            int length = Text.Length;
            Control.Message(NativeMethods.SCI_INDICATORCLEARRANGE, IntPtr.Zero, new IntPtr(length));
        }

        public void AddErrorIndicator(int position, int length)
        {
            Control.Message(NativeMethods.SCI_SETINDICATORCURRENT, new IntPtr(ErrorIndex), IntPtr.Zero);
            Control.Message(NativeMethods.SCI_INDICATORFILLRANGE, new IntPtr(position), new IntPtr(length));
        }

        public void AddWarningIndicator(int position, int length)
        {
            Control.Message(NativeMethods.SCI_SETINDICATORCURRENT, new IntPtr(WarningIndex), IntPtr.Zero);
            Control.Message(NativeMethods.SCI_INDICATORFILLRANGE, new IntPtr(position), new IntPtr(length));
        }

        public void ClearAllTypeNameIndicators() { }
        public void AddTypeNameIndicator(int position, int length) { }

        public bool AutoCompleteActive
        {
            get
            {
                return Control.Message(NativeMethods.SCI_AUTOCACTIVE, IntPtr.Zero, IntPtr.Zero) != IntPtr.Zero;
            }
        }

        public unsafe void InsertText(int position, string text)
        {
            if (position < -1)
                throw new ArgumentOutOfRangeException(nameof(position), "Position must be greater or equal to -1");
            if (position != -1)
            {
                int textLength = Control.Message(NativeMethods.SCI_GETLENGTH, IntPtr.Zero, IntPtr.Zero).ToInt32();
                if (position > textLength)
                    throw new ArgumentOutOfRangeException(nameof(position), "Position cannot exceed document length");
            }

            fixed (byte* bp = Eto.CodeEditor.Mac.Helpers.GetBytes(text ?? string.Empty, Encoding, zeroTerminated: true))
                Control.Message(NativeMethods.SCI_INSERTTEXT, new IntPtr(position), new IntPtr(bp));
        }

        public void DeleteRange(int position, int length)
        {
            var textLength = (int)Control.GetGeneralProperty(NativeMethods.SCI_GETLENGTH);
            position = Mac.Helpers.Clamp(position, 0, textLength);
            length = Mac.Helpers.Clamp(length, 0, textLength - position);

            // Convert to byte position/length
            //var byteStartPos = Lines.CharToBytePosition(position);
            //var byteEndPos = Lines.CharToBytePosition(position + length);

            Control.Message(NativeMethods.SCI_DELETERANGE, new IntPtr(position), new IntPtr(length));
        }

        public void SetTargetRange(int start, int end)
        {
            var textLength = Control.Message(NativeMethods.SCI_GETLENGTH, IntPtr.Zero, IntPtr.Zero).ToInt32();
            start = Mac.Helpers.Clamp(start, 0, textLength);
            end = Mac.Helpers.Clamp(end, 0, textLength);

            //start = Lines.CharToBytePosition(start);
            //end = Lines.CharToBytePosition(end);

            Control.Message(NativeMethods.SCI_SETTARGETRANGE, new IntPtr(start), new IntPtr(end));
        }

        public unsafe int ReplaceTarget(string text, int start, int end)
        {
            SetTargetRange(start, end);
            if (text == null)
                text = string.Empty;

            var bytes = Mac.Helpers.GetBytes(text, Encoding, false);
            fixed (byte* bp = bytes)
                Control.Message(NativeMethods.SCI_REPLACETARGET, new IntPtr(bytes.Length), new IntPtr(bp));

            return text.Length;
        }

        public unsafe void ReplaceFirstOccuranceInLine(string oldText, string newText, int lineNember)
        {
            var lineStartPos = Control.GetGeneralProperty(NativeMethods.SCI_POSITIONFROMLINE, CurrentLineNumber);
            var lineEndPos = Control.GetGeneralProperty(NativeMethods.SCI_GETLINEENDPOSITION, CurrentLineNumber);
            Control.Message(NativeMethods.SCI_SETTARGETRANGE, new IntPtr(lineStartPos), new IntPtr(lineEndPos));

            int bytePos = 0;
            var bytes = Mac.Helpers.GetBytes(oldText ?? string.Empty, Encoding, zeroTerminated: false);
            fixed (byte* bp = bytes)
                bytePos = Control.Message(NativeMethods.SCI_SEARCHINTARGET, new IntPtr(bytes.Length), new IntPtr(bp)).ToInt32();

            if (bytePos == -1)
                return;

            Control.Message(NativeMethods.SCI_SETTARGETRANGE, new IntPtr(bytePos), new IntPtr(bytePos + bytes.Length));

            bytes = Mac.Helpers.GetBytes(newText ?? string.Empty, Encoding, zeroTerminated:false);
            fixed (byte* bp = bytes)
                Control.Message(NativeMethods.SCI_REPLACETARGET, new IntPtr(bytes.Length), new IntPtr(bp));
        }

        public int WordStartPosition(int position, bool onlyWordCharacters)
        {
            var onlyWordChars = (onlyWordCharacters ? new IntPtr(1) : IntPtr.Zero);
            int textLength = Control.Message(NativeMethods.SCI_GETLENGTH, IntPtr.Zero, IntPtr.Zero).ToInt32();
            position = Eto.CodeEditor.Mac.Helpers.Clamp(position, 0, textLength);
            position = Control.Message(NativeMethods.SCI_WORDSTARTPOSITION, new IntPtr(position), onlyWordChars).ToInt32();
            return position;
        }

        public string GetTextRange(int position, int length)
        {
            string txt = Text;
            return txt.Substring(position, length);
        }

        public unsafe void AutoCompleteShow(int lenEntered, string list)
        {
            if (string.IsNullOrEmpty(list))
                return;
            lenEntered = Eto.CodeEditor.Mac.Helpers.ClampMin(lenEntered, 0);
            if( lenEntered > 0 )
            {
                int endPos = Control.Message(NativeMethods.SCI_GETCURRENTPOS, IntPtr.Zero, IntPtr.Zero).ToInt32();
                int startPos = endPos;
                for (int i = 0; i < lenEntered; i++)
                    startPos = Control.Message(NativeMethods.SCI_POSITIONRELATIVE, new IntPtr(startPos), new IntPtr(-1)).ToInt32();
                lenEntered = (endPos - startPos);
            }

            var bytes = Eto.CodeEditor.Mac.Helpers.GetBytes(list, Encoding, zeroTerminated: true);
            fixed (byte* bp = bytes)
                Control.Message(NativeMethods.SCI_AUTOCSHOW, new IntPtr(lenEntered), new IntPtr(bp));
            // if the following property is not set, items after 'import' that start with an uppercase
            // closes the completion window. Ex: 'import R' closes the window even though 'Rhino' is
            // in the list.
            Control.Message(NativeMethods.SCI_AUTOCSETIGNORECASE, new IntPtr(1), IntPtr.Zero);
        }


        unsafe void NotificationProtocol_Notify(object sender, SCNotifyEventArgs e)
        {
            var n = e.Notification;
            switch (n.nmhdr.code)
            {

                case NativeMethods.SCN_CHARADDED:
                    CharAdded?.Invoke(this, new CharAddedEventArgs((char)n.ch));
                    break;
                case NativeMethods.SCN_MODIFIED:
                    if ((n.modificationType & NativeMethods.SC_MOD_INSERTCHECK) > 0)
                    {
                        var text = Mac.Helpers.GetString(n.text, (int)n.length, Encoding);
                        InsertCheck?.Invoke(this, new InsertCheckEventArgs(text));
                    }
                    TextChanged?.Invoke(this, new TextChangedEventArgs());
                    break;
                default:
                    break;
            }
        }

        public event EventHandler<CharAddedEventArgs> CharAdded;
        public event EventHandler<TextChangedEventArgs> TextChanged;
        public event EventHandler<InsertCheckEventArgs> InsertCheck;

        public unsafe void ChangeInsertion(string text)
        {
            var bytes = Mac.Helpers.GetBytes(text ?? string.Empty, Encoding, zeroTerminated: false);
            fixed (byte* bp = bytes)
            Control.Message(NativeMethods.SCI_CHANGEINSERTION, new IntPtr(bytes.Length), new IntPtr(bp));
        }


        Encoding Encoding
        {
            get
            {
                int codePage = (int)Control.Message(NativeMethods.SCI_GETCODEPAGE, IntPtr.Zero, IntPtr.Zero);
                return (codePage == 0) ? Encoding.Default : Encoding.GetEncoding(codePage);
            }
        }

    }
}

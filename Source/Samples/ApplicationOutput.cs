using System;
using System.Diagnostics;
using Gtk;

namespace Samples
{
    static class ApplicationOutput
    {
        public static Widget Widget { get; set; }
        private static ScrolledWindow _scrolledWindow;
        private static TextView _textView;

        static ApplicationOutput()
        {
            var vbox = new VBox();

            var labelTitle = new Label();
            labelTitle.Text = "Application Output:";
            labelTitle.Margin = 4;
            labelTitle.Xalign = 0f;
            vbox.PackStart(labelTitle, false, true, 0);

            _scrolledWindow = new ScrolledWindow();
            _textView = new TextView();
            _scrolledWindow.Child = _textView;
            vbox.PackStart(_scrolledWindow, true, true, 0);

            Widget = vbox;
        }

        public static void WriteLine(object o, string e)
        {
            WriteLine("[" + Environment.TickCount + "] " + o.GetType().ToString() + ": " + e);
        }

        public static void WriteLine(string line)
        {
            var enditer = _textView.Buffer.EndIter;
            _textView.Buffer.Insert(ref enditer, line + Environment.NewLine);
            _textView.ScrollToIter(enditer, 0, false, 0, 0);
        }
    }
}
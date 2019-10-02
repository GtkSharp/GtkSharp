// This is free and unencumbered software released into the public domain.
// Happy coding!!! - GtkSharp Team

using System;
using Gtk;

namespace Samples
{
    static class ApplicationOutput
    {
        private static readonly ScrolledWindow _scrolledWindow;
        private static readonly TextView _textView;

        static ApplicationOutput()
        {
            var vbox = new VBox();

            var labelTitle = new Label
            {
                Text = "Application Output:",
                Margin = 4,
                Xalign = 0f
            };
            vbox.PackStart(labelTitle, false, true, 0);

            _scrolledWindow = new ScrolledWindow();
            _textView = new TextView();
            _scrolledWindow.Child = _textView;
            vbox.PackStart(_scrolledWindow, true, true, 0);

            Widget = vbox;

            _textView.SizeAllocated += TextView_SizeAllocated;
        }

        public static Widget Widget { get; set; }

        private static void TextView_SizeAllocated(object o, SizeAllocatedArgs args)
        {
            _textView.ScrollToIter(_textView.Buffer.EndIter, 0, false, 0, 0);
        }

        public static void WriteLine(object o, string e)
        {
            WriteLine("[" + Environment.TickCount + "] " + o.GetType() + ": " + e);
        }

        public static void WriteLine(string line)
        {
            var enditer = _textView.Buffer.EndIter;
            if (_textView.Buffer.Text.Length > 0)
                line = Environment.NewLine + line;
            _textView.Buffer.Insert(ref enditer, line);

        }
    }
}
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
            var vbox = new Box(Orientation.Vertical, 0);

            var labelTitle = new Label
            {
                Text = "Application Output:",
                MarginTop = 4,
                MarginBottom = 4,
                MarginStart = 4,
                MarginEnd = 4,
                Xalign = 0f
            };
            vbox.Append(labelTitle);

            _scrolledWindow = new ScrolledWindow();
            _textView = new TextView();
            _scrolledWindow.Child = _textView;
            _scrolledWindow.Vexpand = true;
            vbox.Append(_scrolledWindow);

            Widget = vbox;
        }

        public static Widget Widget { get; set; }

        // Gtk 3 kept the view pinned to the bottom by scrolling on every
        // size-allocate, which fired as appended text grew the view. Gtk 4 has
        // no such signal, so scrolling happens where the text is actually
        // appended. A mark is used rather than an iterator because the scroll
        // has to survive until layout has run; an iterator would be invalidated
        // by the next edit.
        private static void ScrollToEnd()
        {
            var endMark = _textView.Buffer.CreateMark(null, _textView.Buffer.EndIter, false);
            _textView.ScrollMarkOnscreen(endMark);
            _textView.Buffer.DeleteMark(endMark);
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
            ScrollToEnd();
        }
    }
}
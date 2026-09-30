using System;
using Gtk;

namespace GettingStarted.Pages
{
    /// <summary>
    /// One section of getting-started.md. A vertical Box, because Gtk 4 has no
    /// GtkContainer to derive from -- a widget that holds children just holds
    /// them, through its own API.
    /// </summary>
    public abstract class TourPage : Box
    {
        protected TourPage(string title, string summary) : base(Orientation.Vertical, 12)
        {
            Title = title;

            var heading = new Label(title);
            heading.AddCssClass("title-1");
            heading.Xalign = 0;
            Append(heading);

            var blurb = new Label(summary) { Xalign = 0, Wrap = true };
            blurb.AddCssClass("dim-label");
            Append(blurb);

            Append(new Separator(Orientation.Horizontal));
        }

        public string Title { get; }

        /// <summary>Set by the window; writes to the status line.</summary>
        public Action<string> Report { get; set; } = _ => { };

        // ------------------------------------------------------------ helpers

        /// <summary>A labelled group of widgets, so each page reads as a list
        /// of demonstrations rather than a wall of controls.</summary>
        protected Frame Group(string caption, Widget content)
        {
            content.MarginStart = content.MarginEnd = 8;
            content.MarginTop = content.MarginBottom = 8;

            return new Frame { Label = caption, Child = content };
        }

        protected static Box Row(int spacing = 8)
            => new Box(Orientation.Horizontal, spacing);

        protected static Box Column(int spacing = 8)
            => new Box(Orientation.Vertical, spacing);

        /// <summary>A left-aligned, wrapping, selectable label -- the shape
        /// every explanatory line on these pages wants.</summary>
        protected static Label Note(string text)
            => new Label(text) { Xalign = 0, Wrap = true, Selectable = true };

        /// <summary>Monospaced output, for results a demonstration computes.</summary>
        protected static Label Output(string text = "")
        {
            var label = new Label(text) { Xalign = 0, Wrap = true, Selectable = true };
            label.AddCssClass("monospace");
            return label;
        }
    }
}

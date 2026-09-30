// This is free and unencumbered software released into the public domain.
// Happy coding!!! - GtkSharp Team

using System;
using Gtk;

namespace Samples
{
    public class ListSection : Box
    {
        private Grid _grid;
        private int _position;

        public ListSection() : base(Orientation.Vertical, 0)
        {
            _position = 0;
            _grid = new Grid
            {
                RowSpacing = 6,
                ColumnSpacing = 6
            };

            // Gtk 4 has no PackStart: a box appends children, and what used to
            // be the "expand" packing flag is now the child's own Vexpand or
            // Hexpand property. The empty expanding box keeps the grid pinned
            // to the top, exactly as it did under Gtk 3.
            Append(_grid);
            Append(new Box(Orientation.Vertical, 0) { Vexpand = true });
        }

        public void AddItem((string, Widget) turp)
        {
            AddItem(turp.Item1, turp.Item2);
        }

        public void AddItem(string label, Widget widget)
        {
            _grid.Attach(new Label
            {
                Text = label,
                Hexpand = true,
                Halign = Align.Start
            }, 0, _position, 1, 1);

            var hbox = new Box(Orientation.Horizontal, 0);
            hbox.Append(new Box(Orientation.Horizontal, 0) { Hexpand = true });
            hbox.Append(widget);

            _grid.Attach(hbox, 1, _position, 1, 1);
            _position++;
        }
    }
}

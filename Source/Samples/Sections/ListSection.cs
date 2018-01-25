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

            PackStart(_grid, false, true, 0);
            PackStart(new VBox(), true, true, 0);
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

            var hbox = new HBox();
            hbox.PackStart(new VBox(), true, true, 0);
            hbox.PackStart(widget, false, true, 0);

            _grid.Attach(hbox, 1, _position, 1, 1);
            _position++;
        }
    }
}

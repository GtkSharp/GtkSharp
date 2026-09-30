using System.Text;
using Gtk;

namespace GettingStarted.Pages
{
    /// <summary>
    /// "Laying out widgets". There is no Container: a Window holds exactly one
    /// child, a Box appends, a Grid attaches, and what used to be packing flags
    /// are now properties of the child itself.
    /// </summary>
    public class LayoutPage : TourPage
    {
        public LayoutPage() : base(
            "Layout",
            "GtkContainer is gone. Every parent exposes its own child API, children are walked "
          + "as a linked list, and expand/align/margins live on the child widget.")
        {
            Append(Group("Box: Append, Prepend, and expansion as a widget property", BoxDemo()));
            Append(Group("Grid: Attach(child, column, row, width, height)", GridDemo()));
            Append(Group("Walking children: FirstChild / NextSibling", ChildWalkDemo()));
            Append(Group("Measure takes an orientation AND a for-size", MeasureDemo()));
        }

        private Box _walkTarget;

        private Widget BoxDemo()
        {
            var demo = Row();
            demo.Append(new Button { Label = "no expand" });

            // "Expand" was a flag passed to PackStart in Gtk 3. It is now a
            // property of the child, set before or after adding it.
            var stretchy = new Button { Label = "Hexpand = true", Hexpand = true };
            demo.Append(stretchy);

            // PackEnd is gone; Halign.End on a child of an expanding box is the
            // replacement for "pack this one against the far edge".
            demo.Append(new Button { Label = "Halign.End", Halign = Align.End });

            var frame = new Frame { Child = demo };
            _walkTarget = demo;
            return frame;
        }

        private Widget GridDemo()
        {
            var grid = new Grid { ColumnSpacing = 6, RowSpacing = 6 };
            grid.Attach(new Label("0,0"), 0, 0, 1, 1);
            grid.Attach(new Label("1,0"), 1, 0, 1, 1);
            grid.Attach(new Button { Label = "spans two columns" }, 0, 1, 2, 1);
            return grid;
        }

        private Widget ChildWalkDemo()
        {
            var column = Column();
            var output = Output();
            var button = new Button { Label = "Walk the box above", Halign = Align.Start };

            button.Clicked += (o, e) =>
            {
                var text = new StringBuilder();
                var count = 0;

                // The replacement for Container.Children: every widget carries
                // the linked list itself.
                for (var child = _walkTarget.FirstChild; child != null; child = child.NextSibling)
                {
                    text.Append(count++ > 0 ? " -> " : "");
                    text.Append(child.GetType().Name);
                }

                output.Text = $"{count} children: {text}";
                Report($"Walked {count} children with FirstChild/NextSibling.");
            };

            column.Append(button);
            column.Append(output);
            return column;
        }

        private Widget MeasureDemo()
        {
            var column = Column();
            var label = new Label(
                "A paragraph long enough to wrap, so that its height genuinely "
              + "depends on the width it is given.") { Wrap = true, Xalign = 0 };

            var output = Output();
            var button = new Button { Label = "Measure at 200px and at 400px", Halign = Align.Start };

            button.Clicked += (o, e) =>
            {
                // Height-for-width: the same widget reports a different natural
                // height depending on the width it is offered, which is why
                // Measure needs the for-size that SizeRequest never had.
                label.Measure(Orientation.Vertical, 200, out var min200, out var nat200, out _, out _);
                label.Measure(Orientation.Vertical, 400, out var min400, out var nat400, out _, out _);

                output.Text = $"for-size 200 -> minimum {min200}, natural {nat200}\n"
                            + $"for-size 400 -> minimum {min400}, natural {nat400}";
                Report("Measure: height depends on the width the widget is given.");
            };

            column.Append(label);
            column.Append(button);
            column.Append(output);
            return column;
        }
    }
}

using System;
using Gtk;

namespace Samples
{
    // Gtk 3 had a single mechanism for "properties a container keeps about its
    // child": GtkContainer child properties, reached through an indexer that
    // returned a Box.BoxChild, Grid.GridChild or Stack.StackChild record.
    //
    // Gtk 4 removed GtkContainer, and with it child properties. What replaced
    // them is not one mechanism but three, and this sample now demonstrates
    // each in turn:
    //
    //   Box    nothing at all -- expansion is a property of the child widget
    //          itself, and ordering is a method on the box
    //   Grid   attach data queried and re-set through the grid
    //   Stack  a real GObject per child, GtkStackPage, carrying the properties
    [Section(ContentType = typeof(ContainerChildProperties), Category = Category.Miscellaneous)]
    class ContainerChildPropertiesSection : Box
    {
        public ContainerChildPropertiesSection() : base(Orientation.Vertical, 3)
        {
            ContainerChildProperties.CreateBoxProperties(this);
            ContainerChildProperties.CreateGridProperties(this);
            ContainerChildProperties.CreateStackProperties(this);
        }
    }

    static class ContainerChildProperties
    {
        public static void CreateBoxProperties(Box parent)
        {
            var title = new Label { Text = "Box child layout" };
            parent.Append(title);

            // "Expand" was a packing flag in Gtk 3. In Gtk 4 it is Hexpand or
            // Vexpand on the child, which means any widget can carry it without
            // knowing what contains it.
            var box1 = new Box(Orientation.Horizontal, 3);
            var btn1 = new Button() { Label = "Expand" };
            btn1.Clicked += delegate
            {
                btn1.Hexpand = !btn1.Hexpand;
                ApplicationOutput.WriteLine(btn1, "Hexpand changed to " + btn1.Hexpand);
            };
            box1.Append(btn1);
            parent.Append(box1);

            // "PackType" is gone: a child is placed by whether it was appended
            // or prepended, and can be moved afterwards.
            var box2 = new Box(Orientation.Horizontal, 3);
            var btn2 = new Button() { Label = "Prepend/Append" };
            var marker = new Label { Text = "Marker" };
            box2.Append(btn2);
            box2.Append(marker);
            btn2.Clicked += delegate
            {
                // ReorderChildAfter with a null sibling moves the child first.
                bool first = box2.FirstChild == btn2;
                if (first)
                    box2.ReorderChildAfter(btn2, marker);
                else
                    box2.ReorderChildAfter(btn2, null);

                ApplicationOutput.WriteLine(box2, "Button moved " + (first ? "after" : "before") + " the marker");
            };
            parent.Append(box2);
        }

        public static void CreateGridProperties(Box parent)
        {
            var title = new Label { Text = "Grid child layout" };
            parent.Append(title);

            var grid = new Grid { ColumnSpacing = 3, RowSpacing = 3 };
            var btn1 = new Button { Label = "Column" };
            var lbl = new Label { Text = "Neighbor" };
            btn1.Clicked += delegate
            {
                // Gtk 4 keeps the attach data on the grid rather than on a
                // child record: query it, then attach again to change it.
                int c1, r1, w1, h1, c2, r2, w2, h2;
                grid.QueryChild(btn1, out c1, out r1, out w1, out h1);
                grid.QueryChild(lbl, out c2, out r2, out w2, out h2);

                grid.Remove(btn1);
                grid.Remove(lbl);
                grid.Attach(btn1, c1 == 0 ? 1 : 0, r1, w1, h1);
                grid.Attach(lbl, c2 == 0 ? 1 : 0, r2, w2, h2);

                ApplicationOutput.WriteLine(grid, "Columns swapped");
            };

            var btn2 = new Button { Label = "Width (column span)", Hexpand = true };
            btn2.Clicked += delegate
            {
                int col, row, width, height;
                grid.QueryChild(btn2, out col, out row, out width, out height);

                grid.Remove(btn2);
                grid.Attach(btn2, col, row, width == 1 ? 2 : 1, height);

                ApplicationOutput.WriteLine(grid, "Width changed to " + (width == 1 ? 2 : 1));
            };

            grid.Attach(btn1, 0, 0, 1, 1);
            grid.Attach(lbl, 1, 0, 1, 1);
            grid.Attach(btn2, 0, 1, 1, 1);
            parent.Append(grid);
        }

        public static void CreateStackProperties(Box parent)
        {
            var title = new Label { Text = "Stack page properties" };
            parent.Append(title);

            var stack = new Stack();

            var box = new Box(Orientation.Horizontal, 3);
            var lbl = new Label { Text = "Page 2 label", Halign = Align.Start };

            // Adding a child returns the GtkStackPage that carries what used to
            // be the stack's child properties. It is a real GObject, so its
            // properties can be bound and notified like any other.
            StackPage page1 = stack.AddTitled(box, "1", "Page 1");
            stack.AddTitled(lbl, "2", "Page 2");

            var btn1 = new Button { Label = "Title" };
            btn1.Clicked += delegate
            {
                page1.Title = page1.Title == "Page 1" ? "Page 1 abc" : "Page 1";
                ApplicationOutput.WriteLine(page1, "Title changed to " + page1.Title);
            };
            box.Append(btn1);

            var btn2 = new Button { Label = "Name" };
            btn2.Clicked += delegate
            {
                // The page can also be looked up again from the child widget.
                StackPage page = stack.GetPage(box);
                page.Name = page.Name == "1" ? "first" : "1";
                ApplicationOutput.WriteLine(page, "Name changed to " + page.Name);
            };
            box.Append(btn2);

            var switcher = new StackSwitcher();
            switcher.Stack = stack;

            parent.Append(switcher);
            parent.Append(stack);
        }
    }
}

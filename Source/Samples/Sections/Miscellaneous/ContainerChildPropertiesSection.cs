using System;
using Gtk;

namespace Samples
{
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
            var title = new Label { Text = "Box child properties" };
            parent.PackStart(title, false, true, 0);

            var box1 = new Box(Orientation.Horizontal, 3);
            var btn1 = new Button() { Label = "Expand" };
            btn1.Clicked += delegate
            {
                var child = (Box.BoxChild)box1[btn1];
                child.Expand = !child.Expand;
                ApplicationOutput.WriteLine(child, "Expand changed to " + child.Expand);
            };
            box1.PackStart(btn1, true, true, 0);
            parent.PackStart(box1, false, true, 0);

            var box2 = new Box(Orientation.Horizontal, 3);
            var btn2 = new Button() { Label = "PackType" };
            btn2.Clicked += delegate
            {
                var child = (Box.BoxChild)box2[btn2];
                child.PackType = child.PackType == PackType.Start ? PackType.End : PackType.Start;
                ApplicationOutput.WriteLine(child, "PackType changed to " + child.PackType);
            };
            box2.PackStart(btn2, false, false, 0);
            parent.PackStart(box2, false, true, 0);

            var box3 = new Box(Orientation.Horizontal, 3);
            var btn3 = new Button() { Label = "Position" };
            btn3.Clicked += delegate
            {
                var child = (Box.BoxChild)box3[btn3];
                child.Position = child.Position == 0 ? 1 : 0;
                ApplicationOutput.WriteLine(child, "Position changed to " + child.Position);
            };
            box3.PackStart(btn3, false, false, 0);
            box3.PackStart(new Label { Text = "Neighbor" }, false, false, 0);
            parent.PackStart(box3, false, true, 0);
        }

        public static void CreateGridProperties(Box parent)
        {
            var title = new Label { Text = "Grid child properties" };
            parent.PackStart(title, false, true, 0);

            var grid = new Grid { ColumnSpacing = 3, RowSpacing = 3 };
            var btn1 = new Button { Label = "LeftAttach" };
            var lbl = new Label { Text = "Neighbor" };
            btn1.Clicked += delegate
            {
                var child1 = (Grid.GridChild)grid[btn1];
                var child2 = (Grid.GridChild)grid[lbl];
                if (child1.LeftAttach == 0)
                {
                    child1.LeftAttach = 1;
                    child2.LeftAttach = 0;
                }
                else
                {
                    child1.LeftAttach = 0;
                    child2.LeftAttach = 1;
                }
                ApplicationOutput.WriteLine(child1, "Child 1 LeftAttach changed to " + child1.LeftAttach);
                ApplicationOutput.WriteLine(child1, "Child 2 LeftAttach changed to " + child2.LeftAttach);
            };

            var btn2 = new Button { Label = "Width (column span)", Hexpand = true };
            btn2.Clicked += delegate
            {
                var child = (Grid.GridChild)grid[btn2];
                child.Width = child.Width == 1 ? 2 : 1;
                ApplicationOutput.WriteLine(child, "Width changed to " + child.Width);
            };

            grid.Attach(btn1, 0, 0, 1, 1);
            grid.Attach(lbl, 1, 0, 1, 1);
            grid.Attach(btn2, 0, 1, 1, 1);
            parent.PackStart(grid, false, true, 0);
        }

        public static void CreateStackProperties(Box parent)
        {
            var title = new Label { Text = "Stack child properties" };
            parent.PackStart(title, false, true, 0);

            var stack = new Stack();

            var box = new Box(Orientation.Horizontal, 3);
            var btn1 = new Button { Label = "Title" };
            btn1.Clicked += delegate
            {
                var child = (Stack.StackChild)stack[box];
                child.Title = child.Title == "Page 1" ? "Page 1 abc" : "Page 1";
                ApplicationOutput.WriteLine(child, "Title changed to " + child.Title);
            };
            box.PackStart(btn1, false, false, 0);

            var btn2 = new Button { Label = "Position" };
            var lbl = new Label { Text = "Page 2 label", Halign = Align.Start };
            btn2.Clicked += delegate
            {
                var child1 = (Stack.StackChild)stack[box];
                var child2 = (Stack.StackChild)stack[lbl];
                if (child1.Position == 0)
                {
                    child1.Position = 1;
                    child2.Position = 0;
                }
                else
                {
                    child1.Position = 0;
                    child2.Position = 1;
                }
                ApplicationOutput.WriteLine(child1, "Child 1 Position changed to " + child1.Position);
                ApplicationOutput.WriteLine(child1, "Child 2 Position changed to " + child2.Position);
            };
            box.PackStart(btn2, false, false, 0);

            stack.AddTitled(box, "1", "Page 1");
            stack.AddTitled(lbl, "2", "Page 2");

            var switcher = new StackSwitcher();
            switcher.Stack = stack;

            parent.PackStart(switcher, false, true, 0);
            parent.PackStart(stack, false, true, 0);
        }
    }
}
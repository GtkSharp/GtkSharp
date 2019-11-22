// This is free and unencumbered software released into the public domain.
// Happy coding!!! - GtkSharp Team

using Gtk;

namespace Samples
{
    [Section(ContentType = typeof(Range), Category = Category.Widgets)]
    class RangeSection : ListSection
    {
        public RangeSection()
        {
            AddItem(CreateHorizontalRange());
            AddItem(CreateVerticalRange());
        }

        public (string, Widget) CreateHorizontalRange()
        {
            var adj = new Adjustment(0.0, 0.0, 101.0, 0.1, 1.0, 1.0);
            var hScale = new HScale(adj);
            hScale.SetSizeRequest(200, -1);
            hScale.ValueChanged += (sender, e) => ApplicationOutput.WriteLine(sender, $"Value Change: {((HScale)sender).Value}");
            return ("Horizontal", hScale);
        }

        public (string, Widget) CreateVerticalRange()
        {
            var adj = new Adjustment(0.0, 0.0, 101.0, 0.1, 1.0, 1.0);
            var vScale = new VScale(adj);
            vScale.SetSizeRequest(-1, 200);
            vScale.ValueChanged += (sender, e) => ApplicationOutput.WriteLine(sender, $"Value Change: {((VScale)sender).Value}");
            return ("Vertical", vScale);
        }
    }
}
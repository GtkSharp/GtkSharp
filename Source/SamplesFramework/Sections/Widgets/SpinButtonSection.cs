// This is free and unencumbered software released into the public domain.
// Happy coding!!! - GtkSharp Team

using Gtk;

namespace Samples
{
    [Section(ContentType = typeof(SpinButton), Category = Category.Widgets)]
    class SpinButtonSection : ListSection
    {
        public SpinButtonSection()
        {
            AddItem(CreateSpinButton());
        }

        public (string, Widget) CreateSpinButton()
        {
            // Spinbutton constructor takes MinValue, MaxValue and StepValue 
            var btn = new SpinButton(0, 1000, 1);

            // Button constructor also takes the adjustment object
            // and it can be redefined any time like CurrentVal, MinVal, MaxVal, Step, PageStep, PageSize
            btn.Adjustment.Configure(888, 0, 1000, 1, 100, 0);

            // Default values are double, use ValueAsInt method to get Int
            btn.ValueChanged += (sender, e) =>
                ApplicationOutput.WriteLine(sender, $"Spin button changed: {btn.ValueAsInt}");

            return ("Spin button:", btn);
        }
    }
}

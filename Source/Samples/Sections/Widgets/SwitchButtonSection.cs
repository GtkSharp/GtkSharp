// This is free and unencumbered software released into the public domain.
// Happy coding!!! - GtkSharp Team

using Gtk;

namespace Samples
{
    [Section(Name = "Switch Button", Category = Category.Widgets)]
    class SwitchButtonSection : ListSection
    {
        public SwitchButtonSection()
        {
            AddItem(CreateSwitchButton());
        }

        public (string, Widget) CreateSwitchButton()
        {
            var btn = new Switch();

            btn.ButtonReleaseEvent += (o, args) =>
                ApplicationOutput.WriteLine(o, $"Switch is now: {!btn.Active}");

            return ("Switch:", btn);
        }
    }
}

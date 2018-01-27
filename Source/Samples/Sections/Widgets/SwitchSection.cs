// This is free and unencumbered software released into the public domain.
// Happy coding!!! - GtkSharp Team

using Gtk;

namespace Samples
{
    [Section(ContentType = typeof(Switch), Category = Category.Widgets)]
    class SwitchSection : ListSection
    {
        public SwitchSection()
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

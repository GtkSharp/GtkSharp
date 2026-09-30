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

            // Gtk 4 has no button-release-event on a widget: input arrives
            // through gestures and event controllers. A switch reports being
            // flipped with StateSet, whose argument is the state being asked
            // for -- returning false lets the default handler apply it.
            btn.StateSet += (o, args) => {
                ApplicationOutput.WriteLine(o, $"Switch is now: {args.State}");
                args.RetVal = false;
            };

            return ("Switch:", btn);
        }
    }
}

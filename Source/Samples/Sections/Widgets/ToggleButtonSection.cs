// This is free and unencumbered software released into the public domain.
// Happy coding!!! - GtkSharp Team

using Gtk;

namespace Samples
{
    [Section(ContentType = typeof(ToggleButton), Category = Category.Widgets)]
    class ToggleButtonSection : ListSection
    {
        public ToggleButtonSection()
        {
            AddItem(CreateToggleButton());
        }

        public (string, Widget) CreateToggleButton()
        {
            var btn = new ToggleButton("Toggle Me");
            btn.Toggled += (sender, e) =>
            {
                if (btn.Active)
                    btn.Label = "Untoggle Me";
                else
                    btn.Label = "Toglle Me";
                
                ApplicationOutput.WriteLine(sender, "Buton Toggled");
            };

            return ("Toggle button:", btn);
        }
    }
}

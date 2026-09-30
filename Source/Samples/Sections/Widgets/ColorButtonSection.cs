// This is free and unencumbered software released into the public domain.
// Happy coding!!! - GtkSharp Team

using Gtk;

namespace Samples
{
    [Section(ContentType = typeof(ColorButton), Category = Category.Widgets)]
    class ColorButtonSection : ListSection
    {
        public ColorButtonSection()
        {
            AddItem(CreateColorButton());
        }

        public (string, Widget) CreateColorButton()
        {
            var btn = new ColorButton();

            // Set RGBA color. Rgba is read-only in Gtk 4 -- the colour is
            // applied with SetRgba -- and its components are floats in the 0..1
            // range, so full blue is 1f. (The Gtk 3 version wrote 255 here,
            // which was already out of range and clamped.)
            btn.SetRgba(new Gdk.RGBA()
            {
                Red = 0f,
                Green = 0f,
                Blue = 1f,
                Alpha = 0.2f // 20% translucent
            });

            // Or parse hex. Parse fills in the RGBA it is called on, so it has
            // to be applied afterwards -- calling it on the button's own colour
            // would only fill in a copy and change nothing.
            var parsed = new Gdk.RGBA();
            if (parsed.Parse("#729FCF"))
                btn.SetRgba(parsed);

            // UseAlpha default is false
            btn.UseAlpha = true;

            return ("Color button:", btn);
        }
    }
}

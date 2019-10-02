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

            // Set RGBA color
            btn.Rgba = new Gdk.RGBA()
            {
                Red = 0,
                Green = 0,
                Blue = 255,
                Alpha = 0.2 // 20% translucent
            };

            // Or Parse hex
            btn.Rgba.Parse("#729FCF");

            // UseAlpha default is false
            btn.UseAlpha = true;

            return ("Color button:", btn);
        }
    }
}

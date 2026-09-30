using System;
using Gtk;

namespace Samples
{
    [Section(ContentType = typeof(StyleContext), Category = Category.Miscellaneous)]
    class StyleContextSection : ListSection
    {
        public StyleContextSection()
        {
            var btn = new Button() { Label = "Press me" };
            btn.Clicked += OnBtnClicked;
            AddItem("Press button to output style context properties:", btn);
        }

        private void OnBtnClicked(object sender, System.EventArgs e)
        {
            var styleCtx = ((Button)sender).StyleContext;

            // Gtk 4 removed gtk_style_context_get_property, and with it the
            // ability to ask for an arbitrary CSS property by name. What remains
            // is a handful of typed getters for the values a widget actually
            // needs in order to lay itself out.
            ApplicationOutput.WriteLine($"State: {styleCtx.State}");
            ApplicationOutput.WriteLine($"Color: {styleCtx.Color}");
            ApplicationOutput.WriteLine($"Padding: {Describe(styleCtx.Padding)}");
            ApplicationOutput.WriteLine($"Margin: {Describe(styleCtx.Margin)}");
            ApplicationOutput.WriteLine($"Border: {Describe(styleCtx.Border)}");
        }

        private static string Describe(Border border)
        {
            return $"left {border.Left}, right {border.Right}, top {border.Top}, bottom {border.Bottom}";
        }
    }
}

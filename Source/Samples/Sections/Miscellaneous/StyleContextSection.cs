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

        private void OnBtnClicked(object sender, EventArgs e)
        {
            var styleCtx = ((Button)sender).StyleContext;

            var props = new[] { "padding-left", "padding-right", "padding-top", "padding-bottom", "min-width", "min-height", "color", "background-color", "font-size", "font-style" };

            foreach (var prop in props)
            {
                GLib.Value val = styleCtx.GetProperty(prop, styleCtx.State);
                string msg = string.Format("Property {0}, type {1}, value {2}", prop, val.Val.GetType().Name, val.Val.ToString());
                ApplicationOutput.WriteLine(msg);
            }
        }
    }
}

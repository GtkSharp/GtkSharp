using System;
using Gtk;

namespace Samples
{
    [SectionAttribute(Name = "Button", Category = Category.Widgets)]
    class ButtonCategory : Box
    {
        public ButtonCategory() : base(Orientation.Vertical, 0)
        {
            var btn = new Button("Click Me");
            PackStart(btn, true, true, 0);
        }
    }
}
using System;
using System.Collections.Generic;
using Gtk;

namespace Samples
{
    [Section(ContentType = typeof(CssNameDemo), Category = Category.Miscellaneous)]
    class CssNameSection : Box
    {
        public CssNameSection() : base(Orientation.Vertical, 3)
        {
            CssNameDemo.Create(this);
        }
    }

    class CssNameDemo
    {
        // inherited label has same css name as parent
        internal class MyLabel : Label
        {
        }

        // css name can be set by [CssName]
        [CssName("my-label-attrib")]
        internal class MyLabelWithAttrib : Label
        {
        }

        // css name can be set in class initializer method
        [GLib.TypeInitializer(typeof(MyLabelWithInit), nameof(MyLabelWithInit.ClassInit))]
        internal class MyLabelWithInit : Label
        {
            static void ClassInit(GLib.GType gtype, Type type)
            {
                SetCssName(gtype, "my-label-init");
            }
        }

        public static void Create(Box box)
        {
            var css = new CssProvider();
            css.LoadFromData(@"
label.x {
    background: LightCoral;
}
my-label-attrib {
    background: LightGreen;
}
my-label-init {
    background: LightSkyBlue;
}
my-label-init.x {
    background: LightBlue;
}
");
            StyleContext.AddProviderForScreen(Gdk.Screen.Default, css, StyleProviderPriority.Application);

            string name;

            name = Widget.GetCssName(Label.GType);
            var label1 = new Label { Text = "Label, css name: " + name };
            label1.StyleContext.AddClass("x");

            name = Widget.GetCssName((GLib.GType)typeof(MyLabel));
            var label2 = new MyLabel { Text = "Inherited Label, css name: " + name };
            label2.StyleContext.AddClass("x");

            name = Widget.GetCssName((GLib.GType)typeof(MyLabelWithAttrib));
            var label3 = new MyLabelWithAttrib { Text = "Inherited Label with [CssName], css name: " + name };

            name = Widget.GetCssName((GLib.GType)typeof(MyLabelWithInit));
            var label4 = new MyLabelWithInit { Text = "Inherited Label with class initializer, css name: " + name };

            name = Widget.GetCssName((GLib.GType)typeof(MyLabelWithInit));
            var label5 = new MyLabelWithInit { Text = "Inherited Label with class initializer and css class, css name: " + name };
            label5.StyleContext.AddClass("x");

            box.PackStart(label1, false, false, 0);
            box.PackStart(label2, false, false, 0);
            box.PackStart(label3, false, false, 0);
            box.PackStart(label4, false, false, 0);
            box.PackStart(label5, false, false, 0);
        }
    }
}

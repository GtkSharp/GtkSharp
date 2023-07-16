using System;
using Gtk;

namespace Samples
{
    [Section(ContentType = typeof(CompositeWidget), Category = Category.Widgets)]
    class CompositeWidgetSection : ListSection
    {
        public CompositeWidgetSection()
        {
            AddItem("CompositeWidget:", new CompositeWidget());
            AddItem("Other instance:", new CompositeWidget());
        }
    }

    [Template("CompositeWidget.glade", true)]
    [GLib.TypeName(nameof(CompositeWidget))]
    class CompositeWidget : Bin
    {
#pragma warning disable CS0649, CS0169
        [Child] readonly Button btn1;
        [Child] readonly Button btn2;
        [Child("label")] readonly Entry entry;
#pragma warning restore CS0649, CS0169

        public CompositeWidget()
        {
            // Base constructor sets [Child] fields
            // if [Template(throwOnUnknownChild = true) and GTK can't bind any [Child] field then base constructor throws
            // GTK writes invalid field/widget name in console (project <OutputType> must be Exe to see console on Windows OS)
            System.Diagnostics.Debug.Assert(btn1 != null);
            System.Diagnostics.Debug.Assert(btn2 != null);
            System.Diagnostics.Debug.Assert(entry != null);
        }

        private void on_btn1_clicked(object sender, EventArgs e)
        {
            entry.Text = DateTime.Now.ToString();
            ApplicationOutput.WriteLine(this, "Instance handler clicked");
        }

        private static void on_btn2_clicked(object sender, EventArgs e)
        {
            ApplicationOutput.WriteLine("Static handler clicked");
        }
    }
}

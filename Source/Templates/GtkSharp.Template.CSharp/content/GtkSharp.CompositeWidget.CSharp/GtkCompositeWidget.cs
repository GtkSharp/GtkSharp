using System;
using Gtk;

namespace GtkNamespace
{
    [Template("GtkCompositeWidget.glade")]
    [TypeName(nameof(GtkCompositeWidget))]
    public class GtkCompositeWidget : Bin
    {
        [Child]
        private Button Button;

        public GtkCompositeWidget()
        {
            
        }

        private void button_clicked(object obj, EventArgs args)
        {
            Button.Label = "Clicked!";
        } 
    }
}

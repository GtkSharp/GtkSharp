using System;
using Gtk;

namespace GtkNamespace
{
    [Template("Gtk_Composite_Widget.glade")]
    [TypeName(nameof(Gtk_Composite_Widget))]
    public class Gtk_Composite_Widget : Bin
    {
        [Child]
        private Button Button;

        public Gtk_Composite_Widget()
        {
            
        }

        private void button_clicked(object obj, EventArgs args)
        {
            Button.Label = "Clicked!";
        } 
    }
}

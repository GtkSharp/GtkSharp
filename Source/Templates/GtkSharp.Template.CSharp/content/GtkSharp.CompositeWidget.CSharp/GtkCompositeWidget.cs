using System;
using Gtk;

namespace GtkNamespace
{
    [Template("GtkCompositeWidget.glade")]
    [TypeName(nameof(GtkCompositeWidget))]
    public class GtkCompositeWidget : Bin
    {
        [Child] private Button button;

        public GtkCompositeWidget()
        {
            button.Clicked += OnButtonClicked;
        }

        private void OnButtonClicked(object obj, EventArgs args)
        {
            button.Label = "Clicked!";
        } 
    }
}

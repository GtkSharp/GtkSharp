using System;
using Gtk;
using UI = Gtk.Builder.ObjectAttribute;

namespace GtkNamespace
{
    class Gtk_Dialog : Dialog
    {
        public Gtk_Dialog() : this(new Builder("Gtk_Dialog.glade")) { }

        private Gtk_Dialog(Builder builder) : base(builder.GetObject("Gtk_Dialog").Handle)
        {
            DefaultResponse = ResponseType.Cancel;

            Response += OnResponse;
        }

        private void OnResponse(object o, ResponseArgs args)
        {
            Hide();
        }
    }
}

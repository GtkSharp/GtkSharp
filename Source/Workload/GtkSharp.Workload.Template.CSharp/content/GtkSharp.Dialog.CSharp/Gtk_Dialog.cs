using System;
using Gtk;
using UI = Gtk.Builder.ObjectAttribute;

namespace GtkNamespace
{
    class Gtk_Dialog : Dialog
    {
        public Gtk_Dialog() : this(new Builder("Gtk_Dialog.glade")) { }

        private Gtk_Dialog(Builder builder) : base(builder.GetRawOwnedObject("Gtk_Dialog"))
        {
            builder.Autoconnect(this);
            DefaultResponse = ResponseType.Cancel;

            Response += Dialog_Response;
        }

        private void Dialog_Response(object o, ResponseArgs args)
        {
            Hide();
        }
    }
}

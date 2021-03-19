using System;
using Gtk;
using UI = Gtk.Builder.ObjectAttribute;

namespace GtkNamespace
{
    class Gtk_Window : Window
    {
        public Gtk_Window() : this(new Builder("Gtk_Window.glade")) { }

        private Gtk_Window(Builder builder) : base(builder.GetRawOwnedObject("Gtk_Window"))
        {
            builder.Autoconnect(this);
        }
    }
}

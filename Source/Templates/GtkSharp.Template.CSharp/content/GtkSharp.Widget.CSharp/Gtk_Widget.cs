using System;
using Gtk;
using UI = Gtk.Builder.ObjectAttribute;

namespace GtkNamespace
{
    public class Gtk_Widget : Box
    {
        public Gtk_Widget() : this(new Builder("Gtk_Widget.glade")) { }

        private Gtk_Widget(Builder builder) : base(builder.GetObject("Gtk_Widget").Handle)
        {
            builder.Autoconnect(this);
        }
    }
}

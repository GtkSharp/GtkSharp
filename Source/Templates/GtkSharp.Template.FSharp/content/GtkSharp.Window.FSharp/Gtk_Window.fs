namespace GtkNamespace

open Gtk

type Gtk_Window (builder : Builder) =
    inherit Window(builder.GetRawOwnedObject("Gtk_Window"))

    new() = new Gtk_Window(new Builder("Gtk_Window.glade"))

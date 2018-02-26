namespace GtkNamespace

open Gtk

type Gtk_Dialog (builder : Builder) as this =
    inherit Dialog(builder.GetObject("Gtk_Dialog").Handle)
    do
        this.DefaultResponse <- ResponseType.Cancel;
        this.Response.Add(fun _ ->
            this.Hide();
        )

    new() = new Gtk_Dialog(new Builder("Gtk_Dialog.glade"))

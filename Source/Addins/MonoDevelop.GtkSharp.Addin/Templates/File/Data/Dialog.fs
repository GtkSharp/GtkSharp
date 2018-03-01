namespace ${Namespace}

open Gtk

type ${EscapedIdentifier} (builder : Builder) as this =
    inherit Dialog(builder.GetObject("${EscapedIdentifier}").Handle)
    do
        this.DefaultResponse <- ResponseType.Cancel;
        this.Response.Add(fun _ ->
            this.Hide();
        )

    new() = new ${EscapedIdentifier}(new Builder("${Namespace}.${EscapedIdentifier}.glade"))

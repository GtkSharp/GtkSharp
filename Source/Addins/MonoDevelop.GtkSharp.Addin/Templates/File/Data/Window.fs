namespace ${Namespace}

open Gtk

type ${EscapedIdentifier} (builder : Builder) =
    inherit Window(builder.GetObject("${EscapedIdentifier}").Handle)

    new() = new ${EscapedIdentifier}(new Builder("${Namespace}.${EscapedIdentifier}.glade"))

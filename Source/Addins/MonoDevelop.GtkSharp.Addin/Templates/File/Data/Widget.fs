namespace ${Namespace}

open Gtk

type ${EscapedIdentifier} (builder : Builder) =
    inherit Box(builder.GetObject("${EscapedIdentifier}").Handle)

    new() = new ${EscapedIdentifier}(new Builder("${Namespace}.${EscapedIdentifier}.glade"))

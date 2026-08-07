# PangoSharp

PangoSharp is a C# wrapper for the Pango library.

Part of [GtkSharp](https://github.com/GtkSharp/GtkSharp), a binding for Gtk 4 and its
companion libraries that needs no glue library: every native call is resolved by
symbol lookup at runtime, so the package carries no compiled shim of its own.

Targets `net10.0` and `netstandard2.0`. A Gtk 4 runtime has to be present to
use it, because a missing native export surfaces only when the call is reached.

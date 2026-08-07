# Net4x.GtkSourceSharp

GtkSourceSharp is a C# wrapper for GtkSourceView: a source-code editing widget with syntax highlighting, completion and search.

Part of [GtkSharp](https://github.com/pieroviano/GtkSharp), a C# binding for Gtk 4.22 and its companion
libraries.

```sh
dotnet add package Net4x.GtkSourceSharp
```

## What it needs

Targets `net10.0` and `netstandard2.0`.

At run time it needs the native library it wraps, **`libgtksourceview-5.so.0`**
(Debian and Ubuntu: `libgtksourceview-5-0`). The binding does not carry a copy of it.

On Windows that runtime is installed for you: this package pulls in
`GtkSharp.targets`, which downloads a gvsbuild Gtk 4 build into
`%LOCALAPPDATA%\Gtk\4.22.4` before the first build. Set
`SkipGtkInstall=true` to manage it yourself.

## How it binds

There is no glue library. Every native entry point is resolved by symbol
lookup at run time rather than through a fixed `DllImport` library name,
which is what lets one package work across Windows, Linux and macOS. It
also means a symbol missing from the installed native library surfaces
when it is first called, not when the assembly loads.

## Licence

GNU Library General Public License v2. Sources, samples and the full
licence text are in the [repository](https://github.com/pieroviano/GtkSharp).

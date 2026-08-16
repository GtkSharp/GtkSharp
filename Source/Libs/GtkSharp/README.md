# Net4x.GtkSharp

GtkSharp is a C# wrapper for Gtk 4, the widget toolkit.

Part of [GtkSharp](https://github.com/pieroviano/GtkSharp), a C# binding for Gtk 4.22 and its companion
libraries.

```sh
dotnet add package Net4x.GtkSharp
```

## What it needs

Targets `netstandard2.0`, so one build serves every consumer: .NET 10,
.NET Framework 4.x and Mono alike.

At run time it needs the native library it wraps, **`libgtk-4.so.1`**
(Debian and Ubuntu: `libgtk-4-1`). The binding does not carry a copy of it.

On Windows that runtime is installed for you: this package pulls in
`GtkSharp.targets`, which downloads a gvsbuild Gtk 4 build into
`%LOCALAPPDATA%\Gtk\4.22.4` before the first build. Set
`SkipGtkInstall=true` to manage it yourself.

This is the package to start from. It pulls in the rest of the stack it needs, and it is the one that installs a Gtk runtime on Windows.

## How it binds

There is no glue library. Every native entry point is resolved by symbol
lookup at run time rather than through a fixed `DllImport` library name,
which is what lets one package work across Windows, Linux and macOS. It
also means a symbol missing from the installed native library surfaces
when it is first called, not when the assembly loads.

## Licence

GNU Library General Public License v2. Sources, samples and the full
licence text are in the [repository](https://github.com/pieroviano/GtkSharp).

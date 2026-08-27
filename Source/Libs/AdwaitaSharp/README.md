# AdwaitaSharp4

AdwaitaSharp is a C# wrapper for libadwaita, the GNOME design-language widgets and adaptive layouts built on Gtk 4.

Part of [GtkSharp](https://github.com/pieroviano/GtkSharp), a C# binding for Gtk 4.22 and its companion
libraries.

```sh
dotnet add package AdwaitaSharp4
```

## What it needs

Targets `netstandard2.0`, so one build serves every consumer: .NET 10,
.NET Framework 4.x and Mono alike.

At run time it needs the native library it wraps, **`libadwaita-1.so.0`**
(Debian and Ubuntu: `libadwaita-1-0`). The binding does not carry a copy of it.

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

# Net4x.GdkSharp

GdkSharp is a C# wrapper for Gdk: displays, surfaces, monitors, input devices, the clipboard and drag and drop.

Part of [GtkSharp](https://github.com/pieroviano/GtkSharp), a C# binding for Gtk 4.22 and its companion
libraries.

```sh
dotnet add package Net4x.GdkSharp
```

## What it needs

Targets `netstandard2.0`, so one build serves every consumer: .NET 10,
.NET Framework 4.x and Mono alike.

At run time it needs the native library it wraps, **`libgtk-4.so.1`**
(Debian and Ubuntu: `libgtk-4-1`). The binding does not carry a copy of it.

This package does not install a Windows runtime of its own. Reference
`Net4x.GtkSharp` as well if you want the gvsbuild download it brings,
or put the library on the loader's search path yourself.

Gdk is not a separate shared library in Gtk 4. It lives inside `libgtk-4`, which is why the file named above is Gtk's.

## How it binds

There is no glue library. Every native entry point is resolved by symbol
lookup at run time rather than through a fixed `DllImport` library name,
which is what lets one package work across Windows, Linux and macOS. It
also means a symbol missing from the installed native library surfaces
when it is first called, not when the assembly loads.

## Licence

GNU Library General Public License v2. Sources, samples and the full
licence text are in the [repository](https://github.com/pieroviano/GtkSharp).

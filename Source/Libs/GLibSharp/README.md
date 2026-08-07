# Net4x.GLibSharp

GLibSharp is a C# wrapper for GLib and GObject: the type system, main loop, and the primitives every other binding here is built on.

Part of [GtkSharp](https://github.com/pieroviano/GtkSharp), a C# binding for Gtk 4.22 and its companion
libraries.

```sh
dotnet add package Net4x.GLibSharp
```

## What it needs

Targets `net10.0` and `netstandard2.0`.

At run time it needs the native library it wraps, **`libglib-2.0.so.0`**
(Debian and Ubuntu: `libglib2.0-0`). The binding does not carry a copy of it.

This package does not install a Windows runtime of its own. Reference
`Net4x.GtkSharp` as well if you want the gvsbuild download it brings,
or put the library on the loader's search path yourself.

## How it binds

There is no glue library. Every native entry point is resolved by symbol
lookup at run time rather than through a fixed `DllImport` library name,
which is what lets one package work across Windows, Linux and macOS. It
also means a symbol missing from the installed native library surfaces
when it is first called, not when the assembly loads.

## Licence

GNU Library General Public License v2. Sources, samples and the full
licence text are in the [repository](https://github.com/pieroviano/GtkSharp).

# Net4x.CairoSharp

CairoSharp is a C# wrapper for Cairo, the 2D vector drawing library Gtk renders through.

Part of [GtkSharp](https://github.com/pieroviano/GtkSharp), a C# binding for Gtk 4.22 and its companion
libraries.

```sh
dotnet add package Net4x.CairoSharp
```

## What it needs

Targets `netstandard2.0`, so one build serves every consumer: .NET 10,
.NET Framework 4.x and Mono alike.

At run time it needs the native library it wraps, **`libcairo.so.2`**
(Debian and Ubuntu: `libcairo2`). The binding does not carry a copy of it.

This package does not install a Windows runtime of its own. Reference
`Net4x.GtkSharp` as well if you want the gvsbuild download it brings,
or put the library on the loader's search path yourself.

CairoSharp stands on its own: it does not depend on Gtk and can be used for drawing without it.

## How it binds

There is no glue library. Every native entry point is resolved by symbol
lookup at run time rather than through a fixed `DllImport` library name,
which is what lets one package work across Windows, Linux and macOS. It
also means a symbol missing from the installed native library surfaces
when it is first called, not when the assembly loads.

## Licence

GNU Library General Public License v2. Sources, samples and the full
licence text are in the [repository](https://github.com/pieroviano/GtkSharp).

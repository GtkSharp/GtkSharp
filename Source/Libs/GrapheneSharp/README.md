# GrapheneSharp4

GrapheneSharp4 is a C# wrapper for Graphene, the SIMD-backed types Gsk uses for points, rectangles, matrices and vectors.

Part of [GtkSharp4](https://github.com/pieroviano/GtkSharp), a C# binding for Gtk 4.22 and its companion
libraries.

```sh
dotnet add package GrapheneSharp4
```

## What it needs

Targets `netstandard2.0`, so one build serves every consumer: .NET 10,
.NET Framework 4.x and Mono alike.

At run time it needs the native library it wraps, **`libgraphene-1.0.so.0`**
(Debian and Ubuntu: `libgraphene-1.0-0`). The binding does not carry a copy of it.

This package does not install a Windows runtime of its own. Reference
`GtkSharp4` as well if you want the gvsbuild download it brings,
or put the library on the loader's search path yourself.

GrapheneSharp stands on its own and is useful as a small maths library independently of Gtk.

## How it binds

There is no glue library. Every native entry point is resolved by symbol
lookup at run time rather than through a fixed `DllImport` library name,
which is what lets one package work across Windows, Linux and macOS. It
also means a symbol missing from the installed native library surfaces
when it is first called, not when the assembly loads.

## Licence

GNU Library General Public License v2. Sources, samples and the full
licence text are in the [repository](https://github.com/pieroviano/GtkSharp).

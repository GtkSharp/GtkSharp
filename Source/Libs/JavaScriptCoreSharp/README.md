# JavaScriptCoreSharp

C# bindings for **JavaScriptCore** (`jsc`) — evaluating JavaScript and marshalling values to and from it.

Mostly used alongside `WebkitGtkSharp`, whose script results *are* `JSCValue`s.

## What it binds

Native library: `libjavascriptcoregtk-6.0`

Depends on: GLibSharp, GioSharp

> **Not available on Windows.** The gvsbuild runtime ships no JavaScriptCore,
> so guard use with `JavaScriptCore.Global.IsSupported`.

## Requirements

- **.NET 10** — or any runtime that resolves `netstandard2.0`: the package is
  built for that and nothing else, so .NET Framework 4.x and Mono get the same
  assembly.
- **A Gtk 4 runtime must be installed.** This package binds the real libraries;
  it does not contain them. On Windows the `GtkSharp` package downloads a
  gvsbuild runtime into `%LOCALAPPDATA%\Gtk\4.22.4` on first build; on Linux and
  macOS install them from your package manager.

Native functions are resolved by **runtime symbol lookup**, not `DllImport`, so a
missing library or a missing export is not a link error — it surfaces the first
time the call is reached.

## Links

- [Source and issues](https://github.com/GtkSharp/GtkSharp)
- [Getting started](https://github.com/GtkSharp/GtkSharp/blob/develop/Docs/getting-started.md)
- [Migrating from gtk-sharp 2 or 3](https://github.com/GtkSharp/GtkSharp/blob/develop/Docs/migrating-to-gtk4.md)

Licensed under the LGPL v2.1.

# WebkitGtkSharp

C# bindings for **WebKitGTK 6** — an embeddable web view, user scripts, and two-way messaging with page content.

## What it binds

Native library: `libwebkitgtk-6.0`

Depends on: GtkSharp and its dependencies, JavaScriptCoreSharp

> **Not available on Windows.** The gvsbuild runtime ships no WebKit, so guard
> use with `WebKit.Global.IsSupported`.

## Requirements

- **.NET 10** (the package also targets `netstandard2.0`).
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

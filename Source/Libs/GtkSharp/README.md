# GtkSharp

C# bindings for **Gtk 4.22** — the widgets, windows, layout and event controllers a desktop application is built from.

This is the package to reference if you are writing a Gtk application: it pulls in the rest of the stack.

## What it binds

Native library: `libgtk-4` (Gtk, Gdk and Gsk all live in this one library)

Depends on: GskSharp, GdkSharp, PangoSharp, GrapheneSharp, CairoSharp, GioSharp, GLibSharp

```csharp
Application.Init();

var app = new Application("org.example.Hello", GLib.ApplicationFlags.None);
app.Register(GLib.Cancellable.Current);

var window = new Window { Title = "Hello", DefaultWidth = 480, DefaultHeight = 240 };
window.Child = new Label("Hello, world");

app.AddWindow(window);
window.Present();
Application.Run();
```

This package also carries `GtkSharp.targets`, which on Windows downloads and
unpacks a Gtk 4 runtime on first build. Set `SkipGtkInstall=True` to supply your
own.

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

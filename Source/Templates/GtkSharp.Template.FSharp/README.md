# GtkSharp.Template.FSharp

`dotnet new` templates for writing Gtk 4 applications in F#.

## Install

```sh
dotnet new install GtkSharp.Template.FSharp
```

## Use

```sh
dotnet new gtkapp -o HelloGtk
cd HelloGtk
dotnet run
```

You get a window with a label and a button that counts clicks, built from an
embedded `.ui` file and wired up in code.

Item templates are included too: `gtkwindow`, `gtkwidget` and `gtkdialog`.

> The `gtkdialog` item template still contains Gtk 3 markup (`GtkButtonBox`,
> `internal-child="vbox"`, `use-stock`), all of which Gtk 4 removed. Treat it
> as unverified; the application template is the one that is known good.

These templates reference the [`GtkSharp`](https://www.nuget.org/packages/GtkSharp)
package directly and need no workload.

## Links

- [Source and issues](https://github.com/GtkSharp/GtkSharp)
- [Getting started](https://github.com/GtkSharp/GtkSharp/blob/develop/Docs/getting-started.md)

Licensed under the LGPL v2.1.

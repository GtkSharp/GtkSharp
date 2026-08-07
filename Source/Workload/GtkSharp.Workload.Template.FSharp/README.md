# GtkSharp.Workload.Template.FSharp

`dotnet new` templates for Gtk 4 applications in F#, for use with the `gtk`
.NET workload.

These are the workload-flavoured templates: the project they generate targets
`net10.0-gtk` and relies on the SDK to resolve the bindings, rather than
referencing the `GtkSharp` package.

```sh
dotnet workload install gtk
dotnet new gtkapp -o HelloGtk
```

If you are not using the workload, install
[`GtkSharp.Template.FSharp`](https://www.nuget.org/packages/GtkSharp.Template.FSharp)
instead.

## Links

- [Source and issues](https://github.com/GtkSharp/GtkSharp)
- [Getting started](https://github.com/GtkSharp/GtkSharp/blob/develop/Docs/getting-started.md)

Licensed under the LGPL v2.1.

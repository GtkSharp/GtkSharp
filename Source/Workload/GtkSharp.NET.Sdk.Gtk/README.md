# GtkSharp.NET.Sdk.Gtk

The workload manifest for `gtk`. It tells the .NET SDK which packs make up the
workload and what versions to resolve.

A manifest is published once per SDK **feature band**, which is why this package
appears several times with a band in its version — `10.0.100`, `10.0.200` and so
on. An SDK only ever reads the manifest matching its own band.

> **You do not install this package directly.** It is part of the `gtk`
> .NET workload and is resolved by the SDK. Install the workload instead:
> 
> ```sh
> dotnet workload install gtk
> ```
> 
> If you would rather not use a workload at all, reference the
> [`GtkSharp`](https://www.nuget.org/packages/GtkSharp) package directly —
> it needs no SDK integration.

## Links

- [Source and issues](https://github.com/GtkSharp/GtkSharp)
- [Getting started](https://github.com/GtkSharp/GtkSharp/blob/develop/Docs/getting-started.md)

Licensed under the LGPL v2.1.

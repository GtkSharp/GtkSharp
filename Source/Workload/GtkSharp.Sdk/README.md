# GtkSharp.Sdk

MSBuild SDK for the `gtk` .NET workload. It registers the `net10.0-gtk` target
framework and wires the reference and runtime packs into the build.

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

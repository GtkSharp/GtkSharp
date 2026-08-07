# Net4x.GtkSharp.NET.Sdk.Gtk

The workload manifest for `gtk`. It is what tells the .NET SDK which packs make up the workload and at which version. It is published once per SDK feature band, as `Net4x.GtkSharp.NET.Sdk.Gtk.Manifest-<band>`.

## Not referenced directly

This package is a **manifest** of the `gtk` .NET SDK workload. You do not
add it with `dotnet add package`; the SDK resolves it by name from the
workload manifest when you install the workload:

```sh
dotnet workload install gtk
```

A project then opts in through the target framework:

```xml
<TargetFramework>net10.0-gtk4.22</TargetFramework>
```

To use the bindings without the workload, reference
[`Net4x.GtkSharp`](https://www.nuget.org/packages/Net4x.GtkSharp) instead.

## Licence

GNU Library General Public License v2. Sources and the full licence text
are in the [repository](https://github.com/pieroviano/GtkSharp).

# GtkSharp4.Runtime

The runtime pack for the `gtk` workload: the implementation assemblies that ship with an application targeting `net10.0-gtk4.22`.

## Not referenced directly

This package is a **runtime pack** of the `gtk` .NET SDK workload. You do not
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
[`GtkSharp4`](https://www.nuget.org/packages/GtkSharp4) instead.

## Licence

GNU Library General Public License v2. Sources and the full licence text
are in the [repository](https://github.com/pieroviano/GtkSharp).

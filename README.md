# GtkSharp

[![Build Status](https://dev.azure.com/cra0zy/GtkSharp/_apis/build/status/GtkSharp.GtkSharp?branchName=develop)](https://dev.azure.com/cra0zy/GtkSharp/_build/latest?definitionId=1&branchName=develop)

GtkSharp is a C# wrapper for Gtk and its related components. The component list includes the following libraries: glib, gio, cairo, pango, atk, gdk. This is a fork of https://github.com/mono/gtk-sharp and is maintained completely separately from that project.

Differences can be seen with the following table:

|               | Target framework   | Target Gtk Version                                 | Extra notes                   |
|:--------------|:-------------------|:---------------------------------------------------|:------------------------------|
| GtkSharp      | .NET Standard 2.0  | Gtk 3.22                                           | Does not need glue libraries. |
| mono/gtksharp | .NET Framework 4.5 | Gtk 2 (also Gtk 3.0 but never officially released) |                               |

* [Building from source](#building-from-source)
* [Using the library](#using-the-library)
* [License](#license)

## Building from source

Pre requirements for building from source are that you have .Net 6 installed on the system.

To build the repository, simply do:

```sh
git clone https://github.com/GtkSharp/GtkSharp.git
cd GtkSharp
dotnet tool restore
dotnet cake build.cake
```

A breakdown on how the source is structured:

* Tools that are needed to generate wrapper code are found in [Tools](Source/Tools) folder
* The actual wrappers code is found in [Libs](Source/Libs) folder
* Templates are located in [Templates](Source/Templates) folder
* Build script is separated between [build.cake](build.cake) and [CakeScripts](CakeScripts) folder

## Using the library

On macOS, you'll need to manually install Gtk, see [Installing Gtk on Mac](https://github.com/GtkSharp/GtkSharp/wiki/Installing-Gtk-on-Mac) wiki page for more details on how to do it.

Available NuGet packages:

* [GtkSharp](https://www.nuget.org/packages/GtkSharp/)
* [GdkSharp](https://www.nuget.org/packages/GdkSharp/)
* [GioSharp](https://www.nuget.org/packages/GioSharp/)
* [GLibSharp](https://www.nuget.org/packages/GLibSharp/)
* [AtkSharp](https://www.nuget.org/packages/AtkSharp/)
* [PangoSharp](https://www.nuget.org/packages/PangoSharp/)
* [CairoSharp](https://www.nuget.org/packages/CairoSharp/)
* [GtkSourceSharp](https://www.nuget.org/packages/GtkSourceSharp/)

To create a new gtk app project, simply use `dotnet new` templating engine:

* install: `dotnet new --install GtkSharp.Template.CSharp`
* uninstall: `dotnet new --uninstall GtkSharp.Template.CSharp`
* generate project: `dotnet new gtkapp`

## License

GtkSharp and its related components are licensed under [LGPL v2.0 license](LICENSE), while [Samples](Source/Samples) are licenced under [The Unlicense](Source/Samples/LICENSE).

# Net4x.GtkSharp.Workload.Template.VBNet

Visual Basic templates for Gtk 4 applications, built on
[GtkSharp](https://github.com/pieroviano/GtkSharp).

## Installed with the workload

These templates ship as part of the `gtk` .NET SDK workload rather than
being installed on their own:

```sh
dotnet workload install gtk
dotnet new gtkapp -o HelloGtk
```

## Templates

| Short name | Creates |
| --- | --- |
| `gtk` | A Gtk application, ready to run |
| `gtkwindow` | A window class |
| `gtkdialog` | A dialog class |
| `gtkwidget` | A widget class |

```sh
dotnet new gtk -o MyApp
```

The generated project targets `net10.0-gtk4.22` and gets the bindings
from the workload's packs, with no PackageReference of its own. Note the
application template is `gtk` here, where the standalone
`Net4x.GtkSharp.Template.*` packages call it `gtkapp`.

## Licence

GNU Library General Public License v2. Sources and the full licence text
are in the [repository](https://github.com/pieroviano/GtkSharp).

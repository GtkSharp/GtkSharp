# Net4x.GtkSharp.Template.CSharp

C# templates for Gtk 4 applications, built on
[GtkSharp](https://github.com/pieroviano/GtkSharp).

## Installing

```sh
dotnet new install Net4x.GtkSharp.Template.CSharp
```

## Templates

| Short name | Creates |
| --- | --- |
| `gtkapp` | A Gtk application, ready to run |
| `gtkwindow` | A window class |
| `gtkdialog` | A dialog class |
| `gtkwidget` | A widget class |

```sh
dotnet new gtkapp -o MyApp
```

The generated project references `Net4x.GtkSharp` directly, so it needs
no workload. It still needs a Gtk 4 runtime present; on Windows the
package installs one on first build.

## Licence

GNU Library General Public License v2. Sources and the full licence text
are in the [repository](https://github.com/pieroviano/GtkSharp).

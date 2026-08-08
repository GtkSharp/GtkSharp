# Building GTK 4 Desktop Apps in C#

Cross-platform desktop UI in .NET usually means picking between Avalonia, MAUI, or
staying on Windows with WPF. There is a fourth option that gets less attention:
**GTK 4**, the toolkit behind GNOME, bound to C# by
[GtkSharp](https://github.com/pieroviano/GtkSharp/tree/net4x.gtk4). One codebase
runs natively on Linux, Windows and macOS, and — unlike most desktop stacks — you
are calling the same C library that shipped with the operating system.

The code in this article targets the GTK 4.22 binding on the
[`net4x.gtk4`](https://github.com/pieroviano/GtkSharp/tree/net4x.gtk4) branch,
which is what the `Net4x.*` packages below are built from.

This article walks through building a GTK 4 application in C#: the first window,
layout, signals, UI files, input, actions, lists, custom drawing and CSS. It also
flags the handful of places where the code compiles fine and behaves in a way you
did not expect, because those cost far more time than the parts that fail loudly.

Everything here is the GTK **4** way of doing things. If you have used gtk-sharp 2
or 3, be warned: there is no `GtkContainer`, no `ShowAll()`, no `delete-event`, no
stock items, and button presses now arrive through gesture controllers rather than
widget signals.

## 1. What you need

Two things:

- **The .NET 10 SDK.** The package also targets `netstandard2.0`, so an older
  runtime still resolves an assembly — it simply gets that one.
- **A GTK 4 runtime.** The binding calls into the real native libraries; it does
  not bundle them.

| Platform | How to get it |
|:--|:--|
| Windows | Nothing to do — the build downloads a gvsbuild runtime into `%LOCALAPPDATA%\Gtk\4.22.4` on first build. Set `SkipGtkInstall=True` to supply your own. |
| Debian / Ubuntu | `apt install libgtk-4-1` |
| Fedora | `dnf install gtk4` |
| macOS | `brew install gtk4` |

Then add the binding to your project:

```sh
dotnet add package Net4x.GtkSharp
```

**Note the `Net4x.` prefix — it is on the NuGet id only.** The assembly inside the
package is still `GtkSharp.dll` and the namespaces are unchanged, so your code
says `using Gtk;` exactly as you would expect. The same rule applies across the
whole family: `Net4x.GLibSharp`, `Net4x.GioSharp`, `Net4x.GdkSharp`,
`Net4x.CairoSharp`, `Net4x.PangoSharp`, `Net4x.AdwaitaSharp`,
`Net4x.GtkSourceSharp` and so on. In most cases you only reference
`Net4x.GtkSharp` and the rest arrive as dependencies.

One caveat worth knowing up front: **WebKit is not available on Windows.** The
gvsbuild bundle ships no WebKit, so `Net4x.WebkitGtkSharp` and
`Net4x.JavaScriptCoreSharp` are Linux/macOS in practice. Guard any use of them.

The quickest way to see something on screen:

```sh
dotnet new install Net4x.GtkSharp.Template.CSharp
dotnet new gtkapp -o HelloGtk
cd HelloGtk
dotnet run
```

That gives you a window with a label and a button that counts clicks, built from
an embedded `.ui` file. There are `gtkwindow`, `gtkwidget` and `gtkdialog` item
templates too, plus F# and VB equivalents.

## 2. A window from scratch

Without the template, this is the whole of a minimal application:

```csharp
using System;
using Gtk;

class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        Application.Init();

        var app = new Application("org.example.HelloGtk", GLib.ApplicationFlags.None);
        app.Register(GLib.Cancellable.Current);

        var window = new Window { Title = "Hello", DefaultWidth = 480, DefaultHeight = 240 };
        window.Child = new Label("Hello, world");

        app.AddWindow(window);
        window.Present();

        Application.Run();
    }
}
```

Three details differ from GTK 3, and each one catches people:

- **`Present()`, not `ShowAll()`.** Widgets are visible by default in GTK 4.
  `ShowAll` no longer exists.
- **`window.Child = …`, not `window.Add(…)`.** `GtkContainer` is gone. A `Window`
  holds exactly one child; use a `Box` or `Grid` to hold more.
- **Closing is `CloseRequest`, and the return value is inverted.** `delete-event`
  is gone:

```csharp
window.CloseRequest += (o, args) =>
{
    Application.Quit();
    args.RetVal = false;   // true would VETO the close
};
```

If you are porting, note that inversion carefully. `DeleteEvent`'s
`RetVal = true` meant "handled, don't close"; `CloseRequest`'s `RetVal = true`
vetoes the close, so *allowing* it means `false`.

## 3. Layout without containers

Every widget has a child list, and the container-ish widgets expose their own
methods for it:

```csharp
var box = new Box(Orientation.Vertical, 6);
box.Append(new Label("first"));
box.Append(new Label("second"));
box.Prepend(new Label("zeroth"));

var grid = new Grid();
grid.Attach(new Label("0,0"), 0, 0, 1, 1);
grid.Attach(new Label("spans two columns"), 0, 1, 2, 1);
```

Walk children through the linked list on `Widget`:

```csharp
for (var child = box.FirstChild; child != null; child = child.NextSibling)
    Console.WriteLine(child.GetType().Name);
```

"Expand" used to be a packing flag passed to `PackStart`; it is now a property on
the widget itself. Use `Hexpand`/`Vexpand` and `Halign`/`Valign` for how a widget
takes space, and `MarginStart`/`MarginEnd`/`MarginTop`/`MarginBottom` for spacing —
there is no single `Margin` property.

Size negotiation is `Measure`, which takes an orientation **and a for-size**,
because height depends on the width a widget is given:

```csharp
label.Measure(Orientation.Vertical, forSize: 200,
              out var minimum, out var natural, out _, out _);
```

## 4. Signals, and the one trap to learn first

Signals are ordinary C# events:

```csharp
var button = new Button { Label = "Click me" };
button.Clicked += (o, e) => Console.WriteLine("clicked");
```

**Read this before you write a handler that inspects state.** `+=` connects your
handler **after** the widget's own default handler. GtkSharp connects *before*
only when the handler is a **named method** carrying `[GLib.ConnectBefore]` — a
lambda cannot carry it, because the attribute is read off the delegate's
`MethodInfo`.

For most signals that is exactly what you want. For a signal whose default handler
*performs the operation*, it means you observe the world after the fact:

```csharp
// WRONG if you meant to inspect the text before it is inserted.
buffer.InsertText += (o, args) => { /* the text is already in the buffer */ };

// Right:
[GLib.ConnectBefore]
void OnInsertText(object o, InsertTextArgs args) { /* not yet inserted */ }
buffer.InsertText += OnInsertText;
```

A `DeleteRange` handler attached with a lambda reads the **empty string** out of
the range it was handed, because the iterators have already collapsed onto the
deletion point. Nothing throws; you just get the wrong half of the transaction.

## 5. Building UI from `.ui` files

Design in [Cambalache](https://gitlab.gnome.org/jpu/cambalache) or write the XML by
hand, embed it, and load it with `Builder`. Fields marked `[UI]` are bound by name:

```csharp
using UI = Gtk.Builder.ObjectAttribute;

class MainWindow : Window
{
    [UI] private Label _label1 = null;
    [UI] private Button _button1 = null;

    public MainWindow() : this(new Builder("MainWindow.ui")) { }

    private MainWindow(Builder builder) : base(builder.GetRawOwnedObject("MainWindow"))
    {
        builder.Autoconnect(this);           // binds the [UI] fields
        _button1.Clicked += OnButtonClicked; // wire signals in C#
    }
}
```

Embed the file in your `.csproj`:

```xml
<ItemGroup>
  <None Remove="**\*.ui" />
  <EmbeddedResource Include="**\*.ui">
    <LogicalName>%(Filename)%(Extension)</LogicalName>
  </EmbeddedResource>
</ItemGroup>
```

**Do not put `<signal>` elements in your `.ui` file.** GTK 4 replaced
`gtk_builder_connect_signals_full` with `GtkBuilderScope`, which GtkSharp does not
implement yet. `Autoconnect` binds fields happily, but a document declaring signals
throws `NotSupportedException` naming the cause — deliberately, rather than
silently ignoring every click. Connect handlers in C#.

## 6. Input: gestures and controllers

GTK 4 routes input through **event controllers** you attach to a widget, not
through widget signals. There is no `EventBox` and no event-mask bookkeeping — a
controller declares its own interest:

```csharp
var click = new GestureClick();
click.Pressed += (o, args) => Console.WriteLine($"press at {args.X},{args.Y}");
widget.AddController(click);

var keys = new EventControllerKey();
keys.KeyPressed += (o, args) =>
{
    if (args.Keyval == (uint) Gdk.Key.Escape)
        window.Close();
    args.RetVal = false;   // false lets the event continue
};
window.AddController(keys);
```

`EventControllerMotion`, `EventControllerFocus`, `EventControllerScroll`,
`DropTarget` and `DragSource` all follow the same shape. Keyboard shortcuts go
through a `ShortcutController` rather than the old `AccelGroup`.

## 7. Actions and menus

`GAction` replaced `GtkAction`, and `GMenu` replaced `GtkUIManager`. An action is a
named, optionally stateful, optionally parameterised callback:

```csharp
var save = new GLib.SimpleAction("save", null);
save.Activated += (o, args) => SaveDocument();

var group = new GLib.SimpleActionGroup();
group.AddAction(save);
window.InsertActionGroup("win", group);   // now addressable as "win.save"

var button = new Button { Label = "Save", ActionName = "win.save" };
```

Menus are a *model*, not a widget tree — you hand the model to a `MenuButton` or a
`PopoverMenu`:

```csharp
var fileMenu = new GLib.Menu();
fileMenu.Append("Save", "win.save");
fileMenu.Append("Quit", "app.quit");

var menubar = new GLib.Menu();
menubar.AppendSubmenu("File", fileMenu);
```

**Stateful actions have a trap.** `StateChanged` is the `change-state` signal, not
a notification after the fact. GLib's default handler is what applies the new
state, and connecting *replaces* it — so a handler that only reads the value leaves
the action stuck on its old state:

```csharp
toggle.StateChanged += (o, args) =>
{
    ApplyTheSetting((bool) args.Value);
    toggle.State = args.Value;    // you must do this yourself
};
```

## 8. Lists

`GtkTreeView` and its models still exist but are deprecated. New code uses a
`GListModel` pipeline: a source model, optional filter and sort stages, a selection
model, and a view with a factory that recycles row widgets. Each stage is itself a
model, so they compose:

```csharp
var source = new StringList(new[] { "delta", "alpha", "charlie", "bravo" });

var sorter = new StringSorter(new PropertyExpression(StringObject.GType, null, "string"));
var sorted = new SortListModel(source, sorter);

var filter = new CustomFilter(item =>
    ((StringObject) GLib.Object.GetObject(item)).String.Length == 5);
var filtered = new FilterListModel(sorted, filter);

var view = new ListView(new SingleSelection(filtered), MakeFactory());
```

For your own row type, subclass `GLib.Object` and use a `GLib.ListStore`:

```csharp
class Row : GLib.Object
{
    public Row() : base(IntPtr.Zero) { }
    public string Name { get; set; }
}

var store = new GLib.ListStore((GLib.GType) typeof(Row));
store.Append(new Row { Name = "first" }.Handle);

var view = new ColumnView(new SingleSelection(store));
view.AppendColumn(new ColumnViewColumn("Name", new SignalListItemFactory()));
```

A `SignalListItemFactory` has a `Setup` signal (create the row widget) and a `Bind`
signal (fill it from the item). The big structural win over `TreeView` is that GTK
4 puts **real widgets** in rows — an editable cell is just an `Entry`, with no
`CellRendererText.Edited` dance.

## 9. Custom drawing with Cairo

Assign a `DrawFunc` to a `DrawingArea` and you get a Cairo context plus the current
size:

```csharp
var area = new DrawingArea();
area.SetSizeRequest(200, 200);
area.DrawFunc = (da, cr, width, height) =>
{
    cr.SetSourceRGB(0.9, 0.9, 0.9);
    cr.Rectangle(0, 0, width, height);
    cr.Fill();

    cr.SetSourceRGB(0.8, 0, 0);
    cr.LineWidth = 5;
    cr.Translate(width / 2d, height / 2d);
    cr.Arc(0, 0, Math.Min(width, height) / 2 - 10, 0, 2 * Math.PI);
    cr.Stroke();
};
```

Call `area.QueueDraw()` when your state changes.

**Dispose every Cairo object you create.** `Cairo.Path`, `ImageSurface`, `Context`
and `Pattern` are all `IDisposable`, and leaking one does not merely warn — the
finalizer takes the process down, at whatever moment the GC next runs, so the crash
lands nowhere near the cause. The context handed to a `DrawFunc` is not yours;
anything you create inside it is.

For text, use Pango rather than Cairo's toy text API:

```csharp
using var layout = Pango.CairoHelper.CreateLayout(cr);
layout.FontDescription = Pango.FontDescription.FromString("Sans 12");
layout.SetText("measured and drawn");
Pango.CairoHelper.ShowLayout(cr, layout);
```

## 10. Styling with CSS

The GTK 3 theming API is gone; everything is CSS, and providers are registered per
`GdkDisplay` (there is no `GdkScreen` any more):

```csharp
var css = new CssProvider();
css.ParsingError += (o, args) => Console.Error.WriteLine("bad CSS");
css.LoadFromString(@"
    .danger { color: white; background: #c01c28; }
    button:hover { opacity: 0.8; }
");

StyleContext.AddProviderForDisplay(Gdk.Display.Default, css,
                                   StyleProviderPriority.Application);

button.AddCssClass("danger");
```

`AddCssClass` / `RemoveCssClass` / `HasCssClass` replace the old style-context
juggling. `Widget.GetCssName(Button.GType)` tells you the element name a widget
type uses as a selector (`button`, `label`, `window`).

## 11. Shipping

```sh
dotnet publish -c Release -r linux-x64 --self-contained
```

The GTK runtime is **not** included — your users need it installed, or you ship it
alongside. On Windows, the gvsbuild tree downloaded at build time is what you would
redistribute.

There is also a .NET workload — `Net4x.GtkSharp.Ref`, `Net4x.GtkSharp.Runtime` and
`Net4x.GtkSharp.Sdk`, published through the `Net4x.GtkSharp.NET.Sdk.Gtk` manifest —
if you prefer

```xml
<Project Sdk="Net4x.GtkSharp.NET.Sdk.Gtk">
```

to a `PackageReference`. Installing it mutates your SDK directory, so prefer the
package unless you specifically want the SDK integration.

## 12. The traps, in one table

These all compile cleanly, which is what makes them expensive. Every one comes from
a defect GtkSharp's own test suite caught:

| Trap | What actually happens |
|:--|:--|
| `+=` with a lambda | connects **after** the default handler — use a named method with `[GLib.ConnectBefore]` |
| `<signal>` in a `.ui` file | `NotSupportedException` — connect in C# |
| `Widget.Activate()` on a button | does **not** raise `Clicked`; GTK 4 routes presses through a gesture |
| `CloseRequest` with `RetVal = true` | **vetoes** the close — the opposite of `DeleteEvent` |
| `SimpleAction.StateChanged` | it is `change-state`; you must apply the state yourself |
| Leaking a `Cairo.Path` or surface | the finalizer kills the process, far from the cause |
| `new ValueArray(2)`, `new Date(2)` | `int` converts to `IntPtr` implicitly, so these hit the **raw-pointer** constructor — use `2u` / `2L` |
| `GLib.Bytes.Data` on an empty `Bytes` | returns `null`, not an empty array — `foreach` throws |
| `ForwardWordEnd` at end of buffer | returns `false`, so `while (iter.ForwardWordEnd())` drops the last word |
| A left-gravity `TextMark` | is the one that **stays put** when text is inserted |

There is a structural reason this list exists. GtkSharp resolves native functions by
**runtime symbol lookup** rather than `DllImport` of a fixed library name — that is
what lets it work with no glue libraries across three platforms. The cost is that a
native function which no longer exists is not a link error: the binding compiles and
throws `NullReferenceException` at the first call, naming nothing. So when something
misbehaves, **run the code path** rather than trusting that it built.

## Summary

GTK 4 gives .NET developers a genuinely cross-platform, natively-rendered desktop
toolkit with a mature widget set — list views, source editors, WebKit, libadwaita —
and GtkSharp exposes it as ordinary C#: properties, events and `IDisposable`.

The mental adjustment is mostly about what GTK 4 *removed*. No containers, so
parents expose their own child API. No event signals, so input arrives through
controllers. No nested main loops, so every dialog is asynchronous. No theming API,
so styling is CSS. Once those four are internalised, the rest reads like any other
.NET UI framework.

Start with `dotnet new gtkapp`, keep the traps table above open while you write the
first few hundred lines, and run the sample browser in the repository when you want
to see a widget working before you commit to it — it has a section per widget and
is the widest worked example available.

The source, the sample browser and the test suite are at
[github.com/pieroviano/GtkSharp](https://github.com/pieroviano/GtkSharp), on the
`net4x.gtk4` branch. The `develop` branch is the same work prepared as a pull
request against the upstream
[GtkSharp/GtkSharp](https://github.com/GtkSharp/GtkSharp) project, so if the GTK 4
binding lands upstream, that is where it will arrive.

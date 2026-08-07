# Getting started with GtkSharp

Writing desktop applications in C# against Gtk 4. This is for people *using* the
binding; for how the binding itself is built, see
[architecture.md](architecture.md).

Everything below is the Gtk **4** way of doing things. If you are porting from
gtk-sharp 2/3, most of what you remember has moved: there is no `GtkContainer`,
no `ShowAll`, no `delete-event`, no stock items, and button presses arrive
through gesture controllers rather than widget signals.

- [What you need](#what-you-need)
- [The fastest start](#the-fastest-start)
- [A window from scratch](#a-window-from-scratch)
- [Laying out widgets](#laying-out-widgets)
- [Signals, and one trap worth knowing first](#signals-and-one-trap-worth-knowing-first)
- [Building UI from `.ui` files](#building-ui-from-ui-files)
- [Input: gestures and controllers](#input-gestures-and-controllers)
- [Actions and menus](#actions-and-menus)
- [Lists](#lists)
- [Custom drawing with Cairo](#custom-drawing-with-cairo)
- [Styling with CSS](#styling-with-css)
- [Text](#text)
- [Shipping your application](#shipping-your-application)
- [Traps](#traps)

---

## What you need

- **.NET 10 SDK**. The package also targets `netstandard2.0`, so a project on
  an older runtime still resolves an assembly — it simply gets that one.
- **A Gtk 4 runtime.** The binding calls into the real libraries; it does not
  bundle them.

| Platform | How to get it |
|:--|:--|
| Windows | Nothing to do. `GtkSharp.targets` downloads a gvsbuild runtime into `%LOCALAPPDATA%\Gtk\4.22.4` on first build. Set `SkipGtkInstall=True` to opt out and supply your own. |
| Debian/Ubuntu | `apt install libgtk-4-1` — plus `libadwaita-1-0`, `libgtksourceview-5-0`, `libwebkitgtk-6.0-4` if you use those |
| Fedora | `dnf install gtk4` |
| macOS | `brew install gtk4` |

**WebKit is not available on Windows.** gvsbuild ships no WebKit, so
`WebkitGtkSharp` and `JavaScriptCoreSharp` are Linux/macOS in practice. Guard any
use of them with `WebKit.Global.IsSupported`.

---

## The fastest start

```sh
dotnet new install GtkSharp.Template.CSharp
dotnet new gtkapp -o HelloGtk
cd HelloGtk
dotnet run
```

You get a window with a label and a button that counts clicks, built from an
embedded `.ui` file. There are also `gtkwindow`, `gtkwidget` and `gtkdialog` item
templates, and F#/VB equivalents.

> The `gtkdialog` template still contains Gtk 3 markup (`GtkButtonBox`,
> `internal-child="vbox"`, `use-stock`), all of which Gtk 4 removed. Treat it as
> unverified until someone fixes it; the application template is the one that is
> known good.

---

## A window from scratch

Without a template, the whole of a minimal application:

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

Three things differ from Gtk 3 and each bit someone:

- **`Present()`, not `ShowAll()`.** Widgets are visible by default in Gtk 4;
  `ShowAll` is gone.
- **`window.Child = …`, not `window.Add(…)`.** `GtkContainer` no longer exists.
  A `Window` holds exactly one child; use a `Box` or `Grid` to hold more.
- **Closing.** `delete-event` is gone. Handle `CloseRequest` and set
  `args.RetVal = false` to let the close proceed:

```csharp
window.CloseRequest += (o, args) =>
{
    Application.Quit();
    args.RetVal = false;   // true would veto the close
};
```

`Application.Run()` runs a `GLib.MainLoop`. (`gtk_main` was removed in Gtk 4;
this binding's `Application.Run` is built on `GLib.MainLoop` instead.)

---

## Laying out widgets

There is no `Container`. Every widget has a child list, and the container-ish
widgets expose their own methods:

```csharp
var box = new Box(Orientation.Vertical, 6);
box.Append(new Label("first"));
box.Append(new Label("second"));
box.Prepend(new Label("zeroth"));

var grid = new Grid();
grid.Attach(new Label("0,0"), 0, 0, 1, 1);
grid.Attach(new Label("spans two columns"), 0, 1, 2, 1);
```

Walk children with the linked list on `Widget`:

```csharp
for (var child = box.FirstChild; child != null; child = child.NextSibling)
    Console.WriteLine(child.GetType().Name);
```

Sizing is `Measure`, which takes an orientation **and a for-size** — height
depends on the width a widget is given:

```csharp
label.Measure(Orientation.Vertical, forSize: 200,
              out var minimum, out var natural, out _, out _);
```

Use `Hexpand`/`Vexpand` and `Halign`/`Valign` to control how a widget takes up
space, and `MarginStart`/`MarginEnd`/`MarginTop`/`MarginBottom` for spacing.

---

## Signals, and one trap worth knowing first

Signals are C# events:

```csharp
var button = new Button { Label = "Click me" };
button.Clicked += (o, e) => Console.WriteLine("clicked");
```

**Read this before you write a handler that inspects state.** `+=` connects your
handler **after** the widget's own default handler. This binding only connects
*before* when the handler is a **named method** carrying `[GLib.ConnectBefore]` —
a lambda cannot, because the attribute is read off the delegate's `MethodInfo`.

For most signals that is exactly what you want. For a signal whose default
handler performs the operation, it means you see the world *after* the fact:

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
deletion point. Nothing errors; you just get the wrong half of the transaction.

---

## Building UI from `.ui` files

Design in [Cambalache](https://gitlab.gnome.org/jpu/cambalache) or write the XML
by hand, embed it, and load it with `Builder`. Fields marked `[UI]` are bound by
name:

```csharp
using UI = Gtk.Builder.ObjectAttribute;

class MainWindow : Window
{
    [UI] private Label _label1 = null;
    [UI] private Button _button1 = null;

    public MainWindow() : this(new Builder("MainWindow.ui")) { }

    private MainWindow(Builder builder) : base(builder.GetRawOwnedObject("MainWindow"))
    {
        builder.Autoconnect(this);          // binds [UI] fields
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

**Do not put `<signal>` elements in your `.ui`.** Gtk 4 replaced
`gtk_builder_connect_signals_full` with `GtkBuilderScope`, which this binding does
not implement yet. `Autoconnect` binds fields happily, but a document that
declares signals throws `NotSupportedException` naming the cause — deliberately,
rather than silently ignoring every click. Connect handlers in C#.

You can also build from a string, which is handy in tests:

```csharp
var builder = new Builder();
builder.AddFromString("<interface><object class='GtkLabel' id='hi'/></interface>");
var label = (Label) builder.GetObject("hi");
```

---

## Input: gestures and controllers

Gtk 4 routes input through **event controllers** you attach to a widget, not
through widget signals:

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
`DropTarget` and `DragSource` follow the same shape. `RemoveController` detaches
one, and `controller.Widget` tells you what it is attached to.

Keyboard shortcuts go through a `ShortcutController`:

```csharp
var shortcuts = new ShortcutController();
shortcuts.AddShortcut(new Shortcut(new ShortcutTrigger("<Control>s"),
                                   new ShortcutAction("action(win.save)")));
window.AddController(shortcuts);
```

---

## Actions and menus

`GAction` replaced `GtkAction`, and `GMenu` replaced `GtkUIManager`. An action is
a named, optionally stateful, optionally parameterised callback:

```csharp
var save = new GLib.SimpleAction("save", null);
save.Activated += (o, args) => SaveDocument();

var group = new GLib.SimpleActionGroup();
group.AddAction(save);
window.InsertActionGroup("win", group);   // now addressable as "win.save"
```

Anything that can be activated refers to it by name:

```csharp
var button = new Button { Label = "Save", ActionName = "win.save" };
```

Menus are built as a model, not as widgets:

```csharp
var fileMenu = new GLib.Menu();
fileMenu.Append("Save", "win.save");
fileMenu.Append("Quit", "app.quit");

var menubar = new GLib.Menu();
menubar.AppendSubmenu("File", fileMenu);
```

Actions carrying a parameter take a `VariantType` and receive a `Variant`:

```csharp
var open = new GLib.SimpleAction("open", GLib.VariantType.String);
open.Activated += (o, args) => Open((string) args.Parameter);
open.Activate(new GLib.Variant("/tmp/file.txt"));
```

**Stateful actions have a trap.** `StateChanged` is the `change-state` signal, not
a notification after the fact — GLib's default handler is what applies the new
state, and connecting *replaces* it. A handler that only reads the value leaves
the action on its old state:

```csharp
toggle.StateChanged += (o, args) =>
{
    ApplyTheSetting((bool) args.Value);
    toggle.State = args.Value;    // you must do this yourself
};
```

---

## Lists

`GtkTreeView` and its models still exist but are deprecated. New code uses a
`GListModel` pipeline: a model, optional filter and sort stages, a selection
model, and a view with a factory that recycles row widgets.

```csharp
var source = new StringList(new[] { "delta", "alpha", "charlie", "bravo" });

var sorter = new StringSorter(new PropertyExpression(StringObject.GType, null, "string"));
var sorted = new SortListModel(source, sorter);

var filter = new CustomFilter(item =>
    ((StringObject) GLib.Object.GetObject(item)).String.Length == 5);
var filtered = new FilterListModel(sorted, filter);

var selection = new SingleSelection(filtered);
var view = new ListView(selection, MakeFactory());
```

Each stage is itself a model, so they stack. For your own row type, subclass
`GLib.Object` and use a `GLib.ListStore`:

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

A `SignalListItemFactory` has `Setup` (create the row widget) and `Bind` (fill it
from the item) signals. `selection.SelectedItem` comes back as an `IntPtr` —
`GLib.Object.GetObject(...)` turns it into your type.

---

## Custom drawing with Cairo

Assign a `DrawFunc` to a `DrawingArea`; it is called with a Cairo context and the
current size:

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

**Dispose every Cairo object you create.** `Cairo.Path`, `ImageSurface`,
`Context`, `Pattern` and friends are `IDisposable`, and leaking one does not
merely warn — the finalizer takes the process down, at whatever moment the GC next
runs, so the crash lands nowhere near the cause. The context handed to a
`DrawFunc` is not yours; anything you create inside it is.

For text, use Pango rather than Cairo's toy text API:

```csharp
using var layout = Pango.CairoHelper.CreateLayout(cr);
layout.FontDescription = Pango.FontDescription.FromString("Sans 12");
layout.SetText("measured and drawn");
Pango.CairoHelper.ShowLayout(cr, layout);
```

---

## Styling with CSS

```csharp
var css = new CssProvider();
css.ParsingError += (o, args) => Console.Error.WriteLine("bad CSS");
css.LoadFromString(@"
    .danger { color: white; background: #c01c28; }
    button:hover { opacity: 0.8; }
");

StyleContext.AddProviderForDisplay(Gdk.Display.Default, css,
                                   Gtk.StyleProviderPriority.Application);

button.AddCssClass("danger");
```

`AddCssClass`/`RemoveCssClass`/`HasCssClass` replace the Gtk 3 style-context
juggling. `Widget.GetCssName(Button.GType)` tells you the element name a widget
type uses as a CSS selector (`button`, `label`, `window`).

---

## Text

```csharp
var view = new TextView();
var buffer = view.Buffer;

buffer.Text = "Hello, world";

buffer.GetBounds(out var start, out var end);
Console.WriteLine(buffer.GetText(start, end, includeHiddenChars: false));

var bold = buffer.TagTable.Lookup("bold")
           ?? buffer.CreateTag("bold", "weight", (int) Pango.Weight.Bold);
buffer.GetIterAtOffset(out var from, 0);
buffer.GetIterAtOffset(out var to, 5);
buffer.ApplyTag(bold, from, to);
```

Two things surprise people:

- **A left-gravity `TextMark` is the one that does *not* move** when text is
  inserted at its position; the right-gravity mark is pushed along. The name
  describes which side of the insertion the mark ends up on, not where it goes.
- **`ForwardWordEnd` returns `false` at the end of the buffer**, even though the
  last word ends there — so `while (iter.ForwardWordEnd())` silently drops the
  final word.

---

## Shipping your application

```sh
dotnet publish -c Release -r linux-x64 --self-contained
```

The Gtk runtime is **not** included: your users need it installed, or you ship it
alongside. On Windows the gvsbuild tree that `GtkSharp.targets` downloads is what
you would redistribute.

There is also a .NET workload (`GtkSharp.Ref`, `GtkSharp.Runtime`, `GtkSharp.Sdk`)
if you prefer `<Project Sdk="GtkSharp.NET.Sdk.Gtk">` to a `PackageReference`.
Installing it mutates your SDK directory, so prefer the package unless you need
the SDK integration.

---

## Traps

Collected from defects the test suite has actually caught. Each of these
*compiles cleanly*.

| Trap | What happens |
|:--|:--|
| `+=` with a lambda connects **after** the default handler | your handler sees the operation already done |
| `<signal>` in a `.ui` file | `NotSupportedException` — connect in C# |
| `Widget.Activate()` on a button | does **not** raise `Clicked`; Gtk 4 routes presses through a gesture |
| `SimpleAction.StateChanged` | it is `change-state`; you must apply the state yourself |
| Leaking a `Cairo.Path` or surface | the finalizer kills the process, far from the cause |
| `new ValueArray(2)`, `new Date(2)` | `int` converts to `IntPtr` implicitly, so these hit the **raw-pointer** constructor. Use `2u`/`2L` |
| `GLib.Bytes.Data` on an empty `Bytes` | returns `null`, not an empty array — `foreach` throws |
| `ForwardWordEnd` at end of buffer | returns `false`; the last word is skipped |
| A left-gravity `TextMark` | is the one that stays put |

---

## Where to look next

- **`Source/Samples`** — a browsable application with a section per widget. Run it
  with `dotnet cake build.cake --BuildTarget=RunSamples`. It is the widest worked
  example in the repository.
- **`Source/Tests/GtkSharp.Tests`** — several hundred behavioural tests that
  double as short, verified examples of nearly every API mentioned above.
- [testing.md](testing.md) — the defects found so far and how, worth skimming
  before you conclude something is your bug.
- [architecture.md](architecture.md) — how the binding is generated, if you want
  to fix something in it rather than around it.
- Upstream [Gtk 4 documentation](https://docs.gtk.org/gtk4/). Method names map
  predictably: `gtk_widget_set_visible` → `widget.Visible`,
  `gtk_box_append` → `box.Append`.

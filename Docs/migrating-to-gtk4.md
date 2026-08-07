# Migrating an application to GtkSharp 4

For application authors moving from **gtk-sharp 2.12** (Gtk 2) or
**GtkSharp 3.24** (Gtk 3) to **GtkSharp 4.22**. If you are new to the binding
rather than porting, read [getting-started.md](getting-started.md) instead.

Everything here comes from actually doing this port: the 37 sample sections in
`Source/Samples` were carried across, and the notes left in that code are the
source for most of the tables below.

- [Set expectations first](#set-expectations-first)
- [Start by making it fail to compile](#start-by-making-it-fail-to-compile)
- [Application lifecycle](#application-lifecycle)
- [Containers and packing](#containers-and-packing)
- [Visibility and sizing](#visibility-and-sizing)
- [Drawing](#drawing)
- [Input](#input)
- [Menus, actions and toolbars](#menus-actions-and-toolbars)
- [Dialogs](#dialogs)
- [Icons and stock items](#icons-and-stock-items)
- [Lists and trees](#lists-and-trees)
- [Styling](#styling)
- [Gdk types](#gdk-types)
- [Builder and Glade files](#builder-and-glade-files)
- [Extra work if you are coming from gtk-sharp 2](#extra-work-if-you-are-coming-from-gtk-sharp-2)
- [The dangerous part: what changes silently](#the-dangerous-part-what-changes-silently)
- [Checklist](#checklist)

---

## Set expectations first

**This is a rewrite of your UI layer, not a version bump.** Gtk 4 deleted
`GtkContainer`, the whole Gtk 3 theming API, stock items, `GtkMenu`, nested main
loops, and the entire event-signal model. There is no compatibility shim in this
binding and **none should be added** — a fake `Container` that silently
misbehaves is worse than a compile error.

What survives untouched is more than you might fear:

- **Your model and business logic.** Nothing below `Gtk` changed.
- **GObject-level code** — properties, `notify::`, custom signals, subclassing
  `GLib.Object`, `GLib.Value`, `Variant`.
- **Gio** — `GFile`, streams, `GSettings`, `GAction`.
- **Cairo drawing code**, once you change how you obtain the context.
- **Pango** layout and attributes.

The rough shape of the work is: containers and packing (mechanical, large),
events → controllers (mechanical, medium), menus/dialogs/stock (a redesign,
small but fiddly), and lists (a redesign, large *if* you choose to do it now —
see below).

**Start the port with `PackageReference` updated and expect hundreds of compile
errors.** That is the good case. The bad case is the handful of things that
compile and behave differently, collected in
[the dangerous part](#the-dangerous-part-what-changes-silently).

---

## Start by making it fail to compile

Before changing anything, find the blast radius:

```sh
grep -rlE "\b(Container|Bin|VBox|HBox|Table|Alignment|Misc|Stock|IconFactory|IconSet|UIManager|StatusIcon|EventBox|ShowAll|Gdk\.Window|Gdk\.Screen|Gdk\.Color|StyleContext)\b" **/*.cs
```

That is the same grep that drove this repository's own port. Re-run it as you go;
it should converge to empty. Anything it lists is guaranteed work.

---

## Application lifecycle

`gtk_main` and its whole family were **removed**. So was `gtk_init(&argc, &argv)`
— init takes no arguments now and does not parse the command line.

| Gtk 2/3 | Gtk 4 |
|:--|:--|
| `Application.Init(ref args)` | `Application.Init()` |
| `Application.Run()` (gtk_main) | `Application.Run()` — now a `GLib.MainLoop` |
| `Application.Quit()` | unchanged in name |
| `Window.DeleteEvent += …` | `Window.CloseRequest += …`, set `args.RetVal = false` to allow |
| `Widget.Destroy()` | `Window.Destroy()` for windows; unparent otherwise |
| `Application.CurrentEvent` | gone — events reach `EventController`s |

```csharp
// before
win.DeleteEvent += (o, args) => { Application.Quit(); args.RetVal = true; };

// after
win.CloseRequest += (o, args) => { Application.Quit(); args.RetVal = false; };
```

Note the inverted sense: `DeleteEvent`'s `RetVal = true` meant "handled, do not
close"; `CloseRequest`'s `RetVal = true` **vetoes** the close, so allowing it
means `false`.

---

## Containers and packing

The single most user-visible break. `GtkContainer` is gone, and with it
`Add()`, `Remove()`, `Children`, and child properties.

| Gtk 2/3 | Gtk 4 |
|:--|:--|
| `container.Add(child)` | the parent's own API: `window.Child = w`, `box.Append(w)`, `grid.Attach(w, c, r, 1, 1)` |
| `box.PackStart(w, expand, fill, pad)` | `box.Append(w)` + `w.Hexpand`/`w.Vexpand` |
| `box.PackEnd(w, …)` | `box.Append(w)` with `w.Halign = Align.End`, or a second box |
| `container.Children` | walk `w.FirstChild` / `w.NextSibling` |
| `container.Remove(w)` | the parent's `Remove`, or `w.Unparent()` |
| `new HBox(…)` / `new VBox(…)` | `new Box(Orientation.Horizontal, spacing)` |
| child properties (`Container.ChildSetProperty`) | gone; use widget properties or a layout manager |
| `Gtk.Alignment` | `Halign`/`Valign` + the four margin properties |
| `Gtk.Misc` (`Xalign`, …) | `Halign`/`Valign` |
| `Gtk.EventBox` | gone — every widget can take a controller |
| `Gtk.Bin` | gone — a single-child widget just has a `Child` property |

Two details that catch people:

- **"Expand" was a packing flag; now it is a widget property.** `PackStart(w,
  expand: true, …)` becomes `w.Hexpand = true` before `Append`.
- **There is no single `Margin` property.** Set `MarginStart`, `MarginEnd`,
  `MarginTop`, `MarginBottom`.

```csharp
// before
var box = new VBox(false, 6);
box.PackStart(label, true, true, 0);
box.PackStart(button, false, false, 0);

// after
var box = new Box(Orientation.Vertical, 6);
label.Vexpand = true;
box.Append(label);
box.Append(button);
```

---

## Visibility and sizing

**`ShowAll` is gone; widgets are visible by default.** A port that mechanically
deletes `ShowAll()` calls is usually correct. Use `Visible = false` for the ones
you actually wanted hidden.

Size negotiation changed shape: `SizeRequest`/`SizeAllocate` became `Measure`,
which takes an orientation **and a for-size**, because height depends on width.

```csharp
widget.Measure(Orientation.Vertical, forSize: 200,
               out var minimum, out var natural, out _, out _);
```

`gtk_widget_is_toplevel` is gone — a toplevel is a `Gtk.Window`, so test with
`is Window`.

---

## Drawing

Gtk 3 drew by overriding `OnDrawn` and receiving a `cairo_t`. Gtk 4 renders
through a retained scene graph (`GtkSnapshot` / `GskRenderNode`), and the
Cairo path is now confined to `GtkDrawingArea`.

| Gtk 2 | Gtk 3 | Gtk 4 |
|:--|:--|:--|
| `ExposeEvent` | `Drawn` / `OnDrawn` | `DrawingArea.DrawFunc`, or override `OnSnapshot` |

```csharp
// after
var area = new DrawingArea();
area.DrawFunc = (da, cr, width, height) =>
{
    cr.SetSourceRGB(0.8, 0, 0);
    cr.Rectangle(0, 0, width, height);
    cr.Fill();
};
area.QueueDraw();   // when your state changes
```

Your existing Cairo body ports unchanged — only how you get `cr`, and that the
size arrives as arguments rather than from an allocation, are different.

`Gdk.Threads.Enter`/`Leave` are gone: Gtk 4 removed the global GDK lock. Marshal
onto the main thread with `GLib.Idle.Add` instead.

---

## Input

Widget event signals are gone. Input arrives through **event controllers** you
attach to a widget.

| Gtk 2/3 signal | Gtk 4 |
|:--|:--|
| `ButtonPressEvent` / `ButtonReleaseEvent` | `GestureClick` → `Pressed` / `Released` |
| `MotionNotifyEvent` | `EventControllerMotion` → `Motion` |
| `KeyPressEvent` / `KeyReleaseEvent` | `EventControllerKey` → `KeyPressed` / `KeyReleased` |
| `ScrollEvent` | `EventControllerScroll` |
| `EnterNotifyEvent` / `LeaveNotifyEvent` | `EventControllerMotion` → `Enter` / `Leave` |
| `FocusInEvent` / `FocusOutEvent` | `EventControllerFocus` |
| `AddEvents(…)` masks | gone — a controller declares its own interest |
| `Gtk.Drag.*` | `DragSource` / `DropTarget` with `GdkContentProvider` |
| `AccelGroup` / `AddAccelerator` | `ShortcutController` + `Shortcut` |
| `GtkBindingSet` | gone entirely — install shortcuts on the widget class |

```csharp
// before
widget.AddEvents((int) Gdk.EventMask.ButtonPressMask);
widget.ButtonPressEvent += (o, args) => { … };

// after
var click = new GestureClick();
click.Pressed += (o, args) => { /* args.X, args.Y, args.NPress */ };
widget.AddController(click);
```

`gdk_device_get_position` is gone — a client cannot ask where the pointer is;
it learns from a motion controller.

---

## Menus, actions and toolbars

`GtkAction`, `GtkActionGroup`, `GtkUIManager`, `GtkMenu`, `GtkMenuItem`,
`GtkImageMenuItem` and `GtkToolbar` are all gone. Menus are now a **model**
(`GMenu`) rendered by a `GtkPopoverMenu` or `GtkMenuButton`, and actions are
`GAction`.

```csharp
// before: GtkAction + UIManager XML

// after
var save = new GLib.SimpleAction("save", null);
save.Activated += (o, args) => Save();

var group = new GLib.SimpleActionGroup();
group.AddAction(save);
window.InsertActionGroup("win", group);      // addressable as "win.save"

var menu = new GLib.Menu();
menu.Append("Save", "win.save");
var button = new MenuButton { MenuModel = menu };
```

`StatusIcon` is gone with no replacement in Gtk itself — use a desktop
notification or a `libayatana-appindicator` binding.

---

## Dialogs

**`gtk_dialog_run()` was removed**, because Gtk 4 has no nested main loops. Every
dialog is asynchronous. This is the change most likely to force a restructure of
your code, since `Run()` let you write straight-line logic.

| Gtk 2/3 | Gtk 4 |
|:--|:--|
| `dialog.Run()` | connect `Response`, or use the async `…Dialog` APIs |
| `FileChooserDialog` + `Run()` | `Gtk.FileDialog` (4.10+), async |
| `MessageDialog` + `Run()` | `Gtk.AlertDialog`, async |
| `ColorSelectionDialog` | `Gtk.ColorDialog` |
| `FontSelectionDialog` | `Gtk.FontDialog` |

```csharp
// before
if (dialog.Run() == (int) ResponseType.Accept) { Use(dialog.Filename); }
dialog.Destroy();

// after
dialog.Response += (o, args) =>
{
    if (args.ResponseId == (int) ResponseType.Accept) Use(dialog.Filename);
    dialog.Destroy();
};
dialog.Present();
```

---

## Icons and stock items

Stock items were removed in Gtk 3.10 and are entirely gone now.

| Gtk 2 | Gtk 4 |
|:--|:--|
| `Stock.Open`, `IconFactory`, `IconSet` | icon names from the icon theme |
| `new Button(Stock.Ok)` | `new Button { IconName = "emblem-ok-symbolic" }` |
| `new Image(Stock.X, IconSize.Button)` | `Image.NewFromIconName("…")` |
| `IconSize.Button` etc. | mostly gone; sizes come from CSS |
| `Image.FromAnimation` | gone — animate through a `GdkPaintable` |
| `IconTheme.Default` | per-`GdkDisplay`; returns a `GtkIconPaintable` |

A Gtk 4 button takes an icon by name directly; Gtk 3 needed a child `Image`.

---

## Lists and trees

`GtkTreeView`, `GtkListStore`, `GtkTreeStore`, `GtkCellRenderer` and friends
**still exist in 4.22 and this binding still ships them**, but they were
deprecated in 4.10 in favour of `GListModel` + `GtkListView`/`GtkColumnView`.

**You have a genuine choice**, and it is fine to defer:

- **Keep TreeView for now.** It compiles and works. `CellRenderer` subclasses need
  attention (`gtk_cell_renderer_get_size` was removed). You will get deprecation
  warnings, and you are building on something that will eventually go.
- **Move to the list stack.** Larger, but it is where Gtk is going, and the
  pipeline model is genuinely nicer: each stage is itself a model, so filters and
  sorters compose.

```csharp
// the Gtk 4 stack
var store = new GLib.ListStore((GLib.GType) typeof(Row));
var filtered = new FilterListModel(store, new CustomFilter(item => …));
var selection = new SingleSelection(filtered);
var view = new ColumnView(selection);
view.AppendColumn(new ColumnViewColumn("Name", new SignalListItemFactory()));
```

One structural difference worth knowing before you choose: **Gtk 4 puts real
widgets in the rows**, so an editable cell is just an `Entry` — no
`CellRendererText.Edited` dance. And a `TreeIter` identifies a row, not a
position, which trips reordering code in either version.

---

## Styling

The Gtk 3 theming API is gone; everything is CSS.

| Gtk 2/3 | Gtk 4 |
|:--|:--|
| `Widget.ModifyBg`, `ModifyFont`, `ModifyFg` | CSS |
| `Gtk.Style`, `Gtk.Rc` | CSS |
| `StyleContext.GetProperty` | gone — style properties were removed with the Gtk 3 theming API |
| `StyleContext.AddClass` | `widget.AddCssClass(…)` |
| `CssProvider` + `Gdk.Screen` | `StyleContext.AddProviderForDisplay(Gdk.Display.Default, …)` |

```csharp
var css = new CssProvider();
css.LoadFromString(".danger { color: white; background: #c01c28; }");
StyleContext.AddProviderForDisplay(Gdk.Display.Default, css,
                                   StyleProviderPriority.Application);
widget.AddCssClass("danger");
```

---

## Gdk types

| Gtk 2/3 | Gtk 4 |
|:--|:--|
| `Gdk.Window` | `Gdk.Surface` — `widget.Native` gives you the surface |
| `Gdk.Screen` | gone — `Gdk.Display`, and monitors are a `GListModel` |
| `Gdk.Color` | `Gdk.RGBA` (already the case in Gtk 3) |
| `Gdk.Pixbuf` for display | still fine for images; `Gdk.Texture`/`GdkPaintable` for rendering |
| `Gdk.Display.GetMonitor(i)` | `display.Monitors` — a `GListModel`, not an indexed array |
| `GtkClipboard` | `GdkClipboard`, async, `GdkContentProvider`-based |

---

## Builder and Glade files

`gtk_builder_add_from_string` survives; **your XML does not.** Glade's Gtk 3
format is not Gtk 4 `.ui`. Convert with
[gtk4-builder-tool](https://docs.gtk.org/gtk4/gtk4-builder-tool.html):

```sh
gtk4-builder-tool simplify --3to4 window.ui > window4.ui
```

Then check by hand for: `<packing>` blocks (child properties are gone),
`internal-child="vbox"`, `GtkButtonBox`, `use-stock`, `type-hint` — all removed.

**In this binding, `<signal>` elements in a `.ui` file are not supported yet.**
Gtk 4 replaced `gtk_builder_connect_signals_full` with `GtkBuilderScope`, which
GtkSharp does not implement. `Builder.Autoconnect` still binds `[UI]` fields; a
document that declares signals throws `NotSupportedException` naming the cause,
rather than silently ignoring every click. Move those connections into C#.

---

## Extra work if you are coming from gtk-sharp 2

You are crossing two major versions, so you hit the Gtk 2 → 3 removals first.
Deal with these before anything above:

| Gtk 2 | Replacement |
|:--|:--|
| `Gtk.Table` | `Gtk.Grid` |
| `Gtk.HBox` / `Gtk.VBox` | `Gtk.Box` with an `Orientation` |
| `Gtk.HScale` / `Gtk.VScale` | `Gtk.Scale` with an `Orientation` |
| `Gtk.HPaned` / `Gtk.VPaned` | `Gtk.Paned` with an `Orientation` |
| `Gtk.ComboBoxEntry` | `ComboBox.WithEntry` |
| `Gtk.Tooltips` | `widget.TooltipText` |
| `Gdk.Color` / `Gdk.Colormap` | `Gdk.RGBA` |
| `ExposeEvent` | `Drawn` in 3, then a `DrawFunc` in 4 |
| `Gtk.Style` / `.gtkrc` | CSS |

The practical advice: **do not port 2 → 3 → 4 in two passes.** Go straight to 4.
The intermediate step makes you write code against APIs (`GtkContainer`, `Drawn`,
`StyleContext`) that Gtk 4 then deletes again.

---

## The dangerous part: what changes silently

Most of the migration is compile errors, which is the easy kind. These are the
ones that compile and behave differently — collected from defects this
repository's own test suite caught:

| What you write | What actually happens |
|:--|:--|
| `button.Clicked += (o,e) => …` | connects **after** the default handler. This binding connects "before" only for a **named method** with `[GLib.ConnectBefore]` — a lambda cannot carry it. For signals whose default handler does the work (`TextBuffer.InsertText`, `DeleteRange`), your handler sees the operation already done. |
| `widget.Activate()` to simulate a click | does **not** raise `Clicked` on a button; Gtk 4 routes presses through a gesture |
| `CloseRequest` with `RetVal = true` | **vetoes** the close — the opposite of `DeleteEvent`'s convention |
| `SimpleAction.StateChanged` handler | it is `change-state`, not a notification. GLib's default handler applies the state and you have just replaced it — set `action.State` yourself |
| A leaked `Cairo.Path`/surface | the finalizer takes the process down, at whatever moment the GC runs, far from the cause |
| `new ValueArray(2)`, `new Date(2)` | `int` converts to `IntPtr` implicitly on modern C#, so these reach the **raw-pointer** constructor. Use `2u` / `2L` |
| `GLib.Bytes.Data` on an empty `Bytes` | returns `null`, not an empty array |

Test the ported paths by **running** them, not by compiling them. This binding
resolves native functions at runtime, so a call that no longer exists is a
`NullReferenceException` at the call site naming nothing — not a link error. See
[testing.md](testing.md) for why that shapes everything.

---

## Checklist

1. Update `PackageReference` to `GtkSharp` 4.22.x; install a Gtk 4 runtime.
2. Run the grep above; note the file list.
3. `Application.Init()` niladic; `DeleteEvent` → `CloseRequest` (invert the
   `RetVal`); drop `ShowAll`.
4. Containers: `Add`/`PackStart` → the parent's own API; packing flags → widget
   properties.
5. Events → controllers.
6. `OnDrawn` → `DrawingArea.DrawFunc`.
7. Menus/actions → `GAction` + `GMenu`; drop stock items for icon names.
8. Dialogs → async; remove every `Run()`.
9. Theming → CSS.
10. `.ui` files through `gtk4-builder-tool --3to4`; move `<signal>` into C#.
11. Decide about TreeView: keep with warnings, or move to the list stack.
12. **Run every code path**, especially error paths and rarely-opened dialogs.

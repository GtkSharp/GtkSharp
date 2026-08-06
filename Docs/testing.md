# Testing GtkSharp

`Source/Tests/GtkSharp.Tests` is an xunit project and the only end-to-end
verification this repository has. Before the Gtk 4 migration there was none at
all: `CLAUDE.md` described verification as "run the samples and look at them".

```sh
dotnet cake build.cake --BuildTarget=Test      # or: dotnet test Source/Tests/GtkSharp.Tests
```

It requires a **Gtk 4 runtime to be installed**, because it calls into Gtk
rather than merely compiling against it. CI runs it on `ubuntu-24.04` under
`xvfb-run`.

---

## Why calling matters far more than compiling here

This is the single most important thing to understand about testing this
repository.

GtkSharp binds native functions by **runtime symbol lookup**, not `DllImport` of
a fixed name. `FuncLoader.LoadFunction` returns `default(T)` when an export is
missing:

```csharp
public static T LoadFunction<T>(IntPtr procaddress)
{
    if (procaddress == IntPtr.Zero)
        return default(T);          // <-- a null delegate, not an error

    return Marshal.GetDelegateForFunctionPointer<T>(procaddress);
}
```

**A function that Gtk 4 removed is therefore not a link error.** It compiles
perfectly, and throws `NullReferenceException` the first time it is called —
with a message that names nothing at all.

That is not a hypothetical. Porting to Gtk 4 left whole families of removed
functions in place, every one of which built cleanly:

| Symbol | What was broken |
|:-------|:----------------|
| `gtk_main`, `gtk_main_quit`, `gtk_events_pending`, `gtk_main_iteration*` | `Application.Run()` — **no Gtk 4 application could start** |
| `gtk_widget_destroy` | disposing any window |
| `gtk_button_new_from_stock` | `new Button("text")`, for every caller |
| `gtk_style_context_get_property` | `StyleContext.GetProperty` |
| `gtk_cell_renderer_get_size` | any `CellRenderer` subclass, at class-init |
| `gtk_binding_set_by_class` + `gtk_binding_entry_add_signall` | the whole `[Binding]` attribute |
| 27 × `jsc_*` | loaded from the WebKit handle, which cannot resolve them |

**So: add a test that calls the thing you changed.** A green build says almost
nothing.

### The sweep

`Docs/`-adjacent tooling aside, the systematic form of this check is to compare
every symbol the hand-written layer loads against the vendored `.gir` files:

```python
SYMBOL = re.compile(r'GetProcAddress\([^,]+,\s*"([a-z0-9_]+)"\)')
declared = set(re.findall(r'c:identifier="([a-z0-9_]+)"', gir_text))
```

Run it after touching anything in `Source/Libs/*/*.cs`. It found 13 dead symbols
in one pass that would otherwise have been found one crash at a time. Symbols
starting `g_`, `cairo_` and `jsc_` are excluded, because the Gtk-stack girs do
not describe them.

---

## What the tests assert

**Coverage is a by-product of behavioural tests, never the goal.** Every test
asserts a real oracle — a round-trip, a state change, a computed value, a
rendered pixel — so that it would fail if the behaviour regressed.

Explicitly not present, and not to be added:

- `Try(() => api())` sweeps that pass as long as nothing throws
- reflection loops that only read properties
- "instantiate every constructor" tests with no assertions

A test that cannot fail cannot find anything; it converts a real signal into a
coverage number. If an assertion fails, that is a suspected defect to
investigate — not something to relax.

### Test groups

| File | What it does |
|:-----|:-------------|
| `SampleSectionTests` | Constructs every `[Section]` type — one case each — asserting a live widget comes back. The samples are the widest exercise of the bindings in the repository. |
| `ChildWindowTests` | Presses every button a section contains and requires any window that opens to be a live toplevel that can be closed. Plus targeted About/file-chooser dialog tests. |
| `MainWindowTests` | Builds the sample's main window and asserts the Gtk 4 layout the port produced. |
| `SectionBrowsingTests` | Selects every row in the section tree, the way someone clicking through the application would — the closest thing here to a manual pass over the whole app. |
| `BindingTests` | Behavioural round-trips pinning individual fixes. |
| `SignalTests` | The machinery everything else rests on: connect/disconnect, sender identity, `notify::`, signal argument values, enum and flags marshalling, interface dispatch through a generated adapter, and a managed subclass's vfunc override actually being reached by Gtk. |
| `WidgetTests` | State round-trips across the widgets applications use — label, check button, progress bar, scale, spin button, notebook, stack, paned, revealer, CSS classes. Cheap individually; the value is breadth, since a codegen change touches every property at once. |
| `ListModelTests` | The Gtk 4 list stack that replaced TreeView: a managed `GLib.Object` subclass as a row, `GLib.ListStore`, `SingleSelection`, `ColumnView`. |
| `GioAndTextTests` | Gio actions and streams, `GtkBuilder` from XML, `TextBuffer` iterators/marks/tags, and GtkSourceView languages. |
| `GraphicsStackTests` | Cairo, Pango, Graphene, Gdk and Gsk, several asserting rendered pixels. |
| `GLibTests` | `GLibSharp` is entirely hand-written, so nothing else covers it: `Value`, `GType`, `Bytes`, idle and timeout dispatch, `Variant`, marshalling, and `Opaque` ownership. |
| `OptionalLibraryTests` | WebKit and JavaScriptCore, skipped where the library is absent. |
| `AdwaitaTests` | libadwaita, which had **zero** coverage: it built and packed and no line of it had ever run. |
| `DeeperStackTests` | Gio variants, streams, cancellables and files; Gdk colours, rectangles, pixbufs and textures; Pango layout and wrapping; Gsk nodes and transforms. |

### Guards against vacuous passes

Two tests exist only to make sure the others are doing something:

- `There_are_sections_to_test` — an empty section list would make every theory
  pass while testing nothing.
- `Pressing_the_file_chooser_button_opens_a_toplevel` — the button-press theory
  once passed on all 31 sections *while pressing nothing*, because
  `Widget.Activate` does not reach the handler in Gtk 4 (a press is routed
  through a gesture, and `gtk_button_clicked` is gone). This names the one
  sample that must open a window.

---

## Threading

Gtk may only be used from the thread that called `gtk_init`, and xunit makes no
promise about which thread a test body runs on. Disabling parallelism is
**necessary but not sufficient** — sequential tests still land on different pool
threads.

`GtkFixture` therefore starts one thread, initialises Gtk on it, and marshals
every test body onto it via `Run(...)`. After each body it drains pending
main-loop work, so a failure in layout, a draw function or an idle callback is
attributed to the test that caused it rather than to whichever test runs next.

```csharp
protected void Run(Action body) => Fixture.Invoke(body);
```

Anything touching Gtk must be inside `Run`.

---

## Things a test must not do

Two sample buttons are skipped by name in `ChildWindowTests`, and **neither is a
defect** — both are the sample behaving as designed:

- **`PixbufDemo`** is a manual leak-stress toggle. The first press enters an
  unbounded allocation loop that exits only when a second press clears its
  running flag. With no user, it never returns. It is skipped by name rather
  than by a timeout, which would make the suite's duration a matter of luck.
- **`LinkButton`** asks the desktop to open a URI, which launches a browser. A
  test suite must not open browser windows on the machine running it.

Similarly, `MainWindowTests` asserts `Visible` after `Present` rather than
asserting `Application.Windows` membership: window tracking requires the
`GApplication` to have registered, and registration needs a session bus that is
not present everywhere (notably Windows). Asserting it would fail for reasons
that are not defects.

---

## Native library availability

The tests exercise whatever the installed runtime provides. On Windows that is
the gvsbuild bundle in `%LOCALAPPDATA%\Gtk\4.22.4`, which contains:

| Library | Windows file | In the bundle |
|:--------|:-------------|:--------------|
| GLib, GObject, Gio | `glib-2.0-0.dll`, `gobject-2.0-0.dll`, `gio-2.0-0.dll` | yes |
| Cairo | `cairo-2.dll` | yes |
| Pango, PangoCairo | `pango-1.0-0.dll`, `pangocairo-1.0-0.dll` | yes |
| Graphene | `graphene-1.0-0.dll` | yes |
| GdkPixbuf | `gdk_pixbuf-2.0-0.dll` | yes |
| Gdk, Gsk, Gtk | `gtk-4-1.dll` (one library — see V3) | yes |
| GtkSourceView | `gtksourceview-5-0.dll` | yes |
| libadwaita | `adwaita-1-0.dll` | yes |
| WebKitGTK, JavaScriptCore | `webkitgtk-6.0-4.dll`, `javascriptcoregtk-6.0-1.dll` | **no** |

WebKit is not shipped by gvsbuild, so WebKit-dependent tests must be guarded
with `GLibrary.IsSupported(Library.Webkit)` — `WebviewSection` already checks
`WebKit.Global.IsSupported` for the same reason.

`GLibrary`'s filename table is ordered `{windows, linux, macos}` and `TryGet`
reads index 0 on Windows, 1 on Linux, 2 on macOS. **There is no index 3.**
WebKit's entry had the Linux `.so` name in the Windows slot and a `.dll` in an
unread fourth slot, so it could never have loaded on Windows.

---

## Fixed: caller-allocates out-parameters

Kept here because the shape recurs. C functions like these write into storage
**the caller** provides:

```c
void graphene_rect_union (const graphene_rect_t *a, const graphene_rect_t *b,
                          graphene_rect_t *res);      /* caller-allocates */
```

Codegen used to pass `out IntPtr` — an 8-byte slot for a 16-byte struct, so the
callee wrote past it and the pointer read back was garbage. Stack corruption, not
a wrong answer, across 232 parameters.

The fix could not be inferred from the api.xml, because `pass_as="out"` is
*correct* for value types: `Gdk.Rectangle` out-parameters work, and both
`GdkRectangle` and `graphene_rect_t` are emitted as `<boxed opaque="true">` —
`GdkRectangle` is a struct only because `GdkSharp-symbols.xml` overrides it. So
the fact is now carried explicitly, `caller_allocates="1"`, and codegen allocates
`abi_info.Size` bytes and passes the pointer by value.

## Ownership and refcounting

`GLib.Opaque`'s `Raw` setter takes a reference through the `Ref` hook. That is
right when wrapping a borrowed pointer and one too many for a transfer-full
constructor result, so the ordering matters:

- The parameterless `Opaque()` sets `owned = true` **before** `Raw` is assigned,
  and the hook is guarded on `!Owned`. Generated opaque constructors chain to it,
  so they do not double-reference.
- `Opaque(IntPtr)` sets `owned = false`, so the hook *does* take a reference —
  correct for a borrowed pointer. `GetOpaque` undoes it when the caller said the
  pointer was already owned.
- Fundamental types chain `base (IntPtr.Zero)`, which is the `Opaque(IntPtr)`
  path, so their generated constructors emit `Owned = true` before assigning
  `Raw`.

`Disposing_a_second_wrapper_does_not_free_the_first` is the test that would catch
a regression here: it disposes a borrowed wrapper and then keeps using the owner,
which touches freed memory if the reference counting is wrong.

## Fixed: a stale wrapper unreffing a reused address

Kept here because the shape is easy to reintroduce.

`GLib.Object` keeps a map from **native address** to `ToggleRef`, and native
addresses are reused. `Widget.Destroy` tears the object down behind the
wrapper's back — the wrapper survives, still holding that address, and is not
disposed. When it is finalized later, `Dispose(false)` looked the address up and
queued an unref on whatever it found. By then the entry could belong to a
different object entirely, so the unref corrupted *that* object's bookkeeping
and crashed later inside `ToggleRef.Free`, on a main-loop timeout, with nothing
left to say which object caused it:

```
Fatal error. System.AccessViolationException: Attempted to read or write protected memory.
   at GLib.ToggleRef.Free()
   at GLib.ToggleRef.PerformQueuedUnrefs()
```

The fix is one ownership check in `GLib.Object.Dispose`: act on the registration
only when it is still ours.

```csharp
if (ReferenceEquals (tref.Target, this))
    Objects.Remove (Handle);
else
    tref = null;          // address was reused; not ours to touch
```

**Three earlier attempts failed** — taking the compensating reference in
`Destroy`, releasing the wrapper from `Destroy`, and both together — because all
of them treated the window as the victim. It was not: the wrapper at fault is
whichever one happened to hold a reused address, typically a child freed along
with its window. Each was reverted rather than left in.

The fix was confirmed by removing it again: with the guard, 161 tests pass under
instrumentation; without it, the run aborts. `A_window_destroyed_and_finalized_does_not_disturb_later_objects`
pins it, and asserts the *later* object still works rather than merely that
nothing crashed.

Why it only showed under coverage: instrumentation shifts GC timing, so
finalizers run at different moments relative to allocation. The bug was always
there.

## Fixed: two managed types claiming one GType

Only reproducible on Linux, because Windows has no WebKit to load.

`WebkitGtkSharp` carried a hand-written `JavaScript.Value` registered against
`JSCValue`'s GType. `JavaScriptCoreSharp` now binds `JSCValue` properly and
registers `JavaScriptCore.Value` for the same GType, so the registry held two
managed types for one native type and the later registration decided what a
signal argument came back as. The `script-message-received` handler then threw
`InvalidCastException` *inside the signal marshaller*, where there is no caller
to catch it, and the run aborted. The duplicate is deleted.

A GType may therefore be registered by exactly one managed type. When a
hand-written wrapper and a generated one describe the same native type, the
hand-written one has to go.

## Fixed: a signal argument that no name rule can resolve

With the duplicate gone the same signal produced a bare `GLib.Object`, because
nothing had registered `JSCValue` at all. `ObjectManager.Initialize()` runs from
the static constructor of that assembly's own types, and a WebKit program never
touches a JavaScriptCore type first — the first one it meets arrives *as* the
signal argument, by which time the lookup has already missed.

`GType.LookupType`'s name-based fallback cannot cover for it either. It splits a
C name at the second capital, so `JSCValue` becomes `J.SCValue`. Any library
whose prefix is an acronym is beyond it.

So `WebkitGtkSharp.ObjectManager.InitializeExtras` chains
`JavaScriptCoreSharp.ObjectManager.Initialize()` explicitly. **An assembly whose
signals hand out another assembly's types must initialise that assembly's
registry**, because a signal argument is resolved by GType at runtime, not by the
static type in the handler's signature.

## Fixed: one missing type killing a whole assembly

The api.xml files are generated from Debian forky's girs, and the installed
library is routinely older. `gtksourceview` 5.18 added `GtkSourceAnnotation`;
5.16, which Debian trixie ships, has no `gtk_source_annotation_get_type` at all.

Reading `GtkSource.Annotation.GType` therefore called a null delegate and threw
`NullReferenceException` — from inside `ObjectManager.Initialize()`, which
registers every type in the assembly in one unguarded run. The first missing
symbol aborted it, so *no* GtkSource type was registered, and every later use of
the assembly failed with `TypeInitializationException` naming an unrelated class.
One class absent from the installed library made the whole binding unusable: 38
tests failed, none of them about annotations.

`GapiCodegen`'s mapper now routes each registration through a helper that catches
`NullReferenceException` and skips that type. A type whose `get_type` is missing
cannot be constructed anyway, and the rest of the assembly keeps working. Only
`NullReferenceException` is caught, so a real fault still surfaces.

## Open: boxed types with no allocator cannot be constructed

`Gsk.RoundedRect` has no `_alloc` function in C, so the binding generates no
allocator either. `new Gsk.RoundedRect()` therefore resolves to the inherited
`GLib.Opaque()` constructor, which leaves the handle at `IntPtr.Zero` — and the
`Init*` methods, whose whole job is to write through that pointer, then write to
null:

```csharp
var rounded = new Gsk.RoundedRect();   // handle is IntPtr.Zero
rounded.InitFromRect(bounds, 5);       // writes through it -> AccessViolation
```

The type is unusable from managed code, and nothing says so until the process
dies. The shape is the same as the caller-allocates bug: a boxed type whose
storage the caller is expected to provide, with no way provided to provide it.

A fix would generate an allocating constructor for boxed types that carry
`abi_info`, sizing the buffer from `abi_info.Size` exactly as the caller-allocates
fix does. `A_rounded_rect_keeps_its_bounds` is `Skip`ped, pointing here.

## Measuring coverage

Coverage is measured, not chased:

```sh
dotnet test Source/Tests/GtkSharp.Tests --collect:"XPlat Code Coverage"
```

Read the number as a map of what is untested, then write **behavioural** tests
for the parts that matter. Do not write tests to move the number.

At 160 tests the figures are:

| | line rate |
|:--|--:|
| overall | 8.1% |
| `Samples` | 73.7% |
| `GLibSharp` | 38.0% |
| `CairoSharp` | 25.0% |
| `GtkSourceSharp` | 13.7% |
| `GrapheneSharp` | 9.2% |
| `GtkSharp` | 8.0% |
| `GioSharp` | 2.5% |
| `AdwaitaSharp` | 4.3% |

`SectionBrowsingTests` is what a manual tester does: it drives `MainWindow`'s
selection handler for every row rather than constructing sections directly, so it
covers resolving a label back to a type, lazy instantiation on first selection,
clearing the previous section out of the content pane, mounting the new one, and
loading that section's source into the code view. Two bugs surfaced the first time
it ran, both of which a manual tester would have hit and the isolated tests could
not: a section that threw because the application had not been created, and two
sections declaring the same `ContentType`, which collided in the app's
label-to-type map and left one of them unreachable from the tree.

**These numbers mean less than they appear to.** `GtkSharp` alone generates tens
of thousands of lines of property getters and P/Invoke declarations, uniform by
construction: testing one property round-trip exercises the same emission path as
the thousand like it, so line coverage measures how many bindings exist far more
than how well they work. `GLibSharp` scores highest of the libraries precisely
because it is the one that is *hand-written*.

The useful reading is the ordering, not the percentage. `AdwaitaSharp` at zero is
real information — nothing exercises libadwaita at all. `GioSharp` at 2% likewise.
Those are worth tests. Raising `GtkSharp` from 7.8% by walking generated
properties would not be.

Note that most of `Source/Libs/*` is generated, and generated code is uniform by
construction: testing one property round-trip exercises the same emission path
as the thousand others like it. High line coverage over generated code is
therefore close to meaningless. The parts worth testing are the hand-written
layer, the codegen's decisions, and the places where Gtk 4 changed semantics.

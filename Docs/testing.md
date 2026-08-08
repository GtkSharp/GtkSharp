# Testing GtkSharp

`Source/Tests/GtkSharp.Tests` is an xunit project and the only end-to-end
verification this repository has. Before the Gtk 4 migration there was none at
all: `CLAUDE.md` described verification as "run the samples and look at them".

```sh
dotnet cake build.cake --BuildTarget=Test      # or: dotnet test Source/Tests/GtkSharp.Tests
```

It requires a **Gtk 4 runtime to be installed**, because it calls into Gtk
rather than merely compiling against it. CI runs it in a `debian:forky`
container under `xvfb-run`.

**Run it on both platforms before trusting a change.** Windows and Linux each
see defects the other structurally cannot: gvsbuild ships no WebKit, so two
tests skip there, while the `g_spawn_*_utf8` symbols only exist on Windows and
so only broke there. At 1236 tests both report 1233 passing with 3 skips — the
container is run with `GTKSHARP_TESTS_SKIP_WEBKIT=1`, which is why its two
WebKit skips coincide with gvsbuild's.

### Running the suite on the Gtk the bindings describe

WSL's Debian is *trixie* — Gtk 4.18.6 — and reports false failures for anything
the gir marks `version="4.22"`. At 1247 tests it fails 16 of them, and every one
is a symbol trixie's Gtk does not export: `gtk_expression_new_try`,
`gsk_copy_node_new`, `gsk_render_node_get_children`, `gsk_paste_node_new`,
`gsk_composite_node_new`, and the accessibility and `AdwEnumListModel`
properties added since 4.18. Confirm before spending time on one:

```sh
nm -D --defined-only /usr/lib/x86_64-linux-gnu/libgtk-4.so.1 | grep ' T gsk_copy_node_new$'
```

(`nm` is in `binutils`, which trixie's WSL image does not install — and without
it that command prints nothing, which reads exactly like a missing symbol. It is
worth installing rather than trusting the empty output.) A **null delegate**, not
a link error, is what a missing export becomes, so these arrive as
`NullReferenceException` from inside a wrapper rather than as anything that names
the symbol.

The container is the reference environment:

```sh
docker run --rm -v /path/to/GtkSharp:/src -w /src debian:forky bash -lc '
  apt-get update -qq &&
  apt-get install -y -qq --no-install-recommends ca-certificates curl git dbus \
    libicu-dev libgtk-4-1 libadwaita-1-0 libgtksourceview-5-0 \
    libwebkitgtk-6.0-4 libjavascriptcoregtk-6.0-1 xvfb xauth &&
  curl -sSL https://dot.net/v1/dotnet-install.sh | bash -s -- --channel 10.0 --install-dir /usr/local/dotnet &&
  export PATH=/usr/local/dotnet:$PATH &&
  WEBKIT_DISABLE_SANDBOX_THIS_IS_DANGEROUS=1 \
    dbus-run-session -- xvfb-run -a dotnet test Source/Tests/GtkSharp.Tests -c Release'
```

Four things in that line are load-bearing, and each one fails in a way that does
not say what is wrong:

- **`libicu-dev`** — without it the .NET host aborts before `Main` with
  "Couldn't find a valid ICU package installed on the system".
- **`dbus-run-session`** — Gtk 4.22 on forky decodes images through **glycin**,
  which runs each loader in a `bwrap` sandbox it talks to over a bus. With no
  session bus the loader cannot start and takes the test host with it.
- **`WEBKIT_DISABLE_SANDBOX_THIS_IS_DANGEROUS=1`** — WebKit spawns its network
  and web processes into the same kind of sandbox, which needs user namespaces
  a container does not necessarily have. It does not fail the call; it aborts
  the process (`bwrap: Creating new namespace failed`, then `Failed to fully
  launch dbus-proxy`), part-way through the suite.
- **`dotnet-install.sh`** — forky has no `dotnet-sdk-8.0` package at all.

Both aborts land under a **"Passed!" line with a truncated total** — the failure
mode this document keeps returning to. 135 of 766 was the shape of the WebKit
one.

### The WebKit sandbox, and why the switch is a switch

The alternative to that flag is `GTKSHARP_TESTS_SKIP_WEBKIT=1`, which is what CI
sets. `TestEnvironment` reads it and skips the WebKit-backed cases —
`OptionalLibraryTests`, and the WebView section in `SampleSectionTests`,
`ChildWindowTests` and `SectionBrowsingTests`. **Set one or the other when
running the suite in a container**; leave both unset on a desktop, where the
sandbox starts and WebKit is covered.

It has to be decided in advance either way. The abort is a `g_error` inside
WebKit, not an exception: no `try` reaches it, and by the time it prints, the
host is gone.

**Detecting it instead was tried and does not work.** The obvious probe — run
the operation bwrap begins with, `bwrap --unshare-user --ro-bind / / /bin/true`,
against the binary WebKit will spawn — reports success in a container where
WebKit still aborts. Shimming `/usr/bin/bwrap` to log its argv shows why: WebKit
makes four calls, and the first is a capability check shaped like the probe,
which passes. The one that fails is the fourth,

```text
bwrap --args 217 -- /usr/bin/xdg-dbus-proxy --args=213
```

whose failure is `Creating new namespace failed: Operation not permitted` —
bubblewrap's message for a namespace other than the user one, not the
`No permissions to create a new namespace…` a blocked `CLONE_NEWUSER` produces.
So the probe answers a question WebKit is not asking, and a probe that can say
"usable" and then abort is worse than no probe: a wrong "skip" costs coverage, a
wrong "run" costs the whole suite.

Loosening the container does not earn the coverage back cheaply either. Measured
on one image, running only the section theory:

| container | result |
|---|---|
| *(default)* | aborts, 18 of 32 |
| `--security-opt seccomp=unconfined` | aborts, 18 of 32 |
| `--privileged` | 32 of 32 |

`seccomp=unconfined` is enough for `unshare -U true` and for bwrap on its own —
Docker's default profile is what blocks `clone(CLONE_NEWUSER)` — and still not
enough for WebKit. Only `--privileged` is, and that is a far wider grant than a
job holding a `packages:write` token should have.

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
| `KeyFileTests` | `GLib.KeyFile` round-trips: every value goes out through `ToData` and back into a *fresh* `KeyFile`, so nothing can be satisfied from an in-memory cache. Covers the cases where naive marshalling gives itself away — a value holding both the list separator and a backslash, non-ASCII text, a single-element list, an integer too wide for an `int`. |
| `TimeTests` | `GLib.Date` and `GLib.DateTime`. The oracles are facts about the calendar rather than about the binding: 2024 has a 29 February, 2100 is not a leap year, 1 January 2021 falls in ISO week 53 of 2020. |
| `CairoTests` | Renders to an `ImageSurface` and reads the pixels back, so what is asserted is what landed in the buffer: fill rules, premultiplied alpha, clipping, dash gaps, gradients, tiling. Plus `Matrix`, `Region` and `FontOptions` value semantics. |
| `StreamAndBuilderTests` | `GioStream` as a `System.IO.Stream` — chunked reads, offsets, seeking from all three origins, the file modes — and `Gtk.Builder` producing widgets from XML with the structure the markup described. |
| `MarshallingTests` | The layer under everything else: `GLib.Value` at each numeric width, strings and arrays through unmanaged memory, `GLib.List` and `PtrArray` as collections. |
| `TreeModelTests` | `ListStore`, `TreeStore`, `TreeModelFilter`, `TreeModelSort`, `TreePath` — deprecated in Gtk 4, still shipped, still what a ported application arrives holding. |
| `PixbufAndLogTests` | Image encode/decode round-trips (PNG is lossless, so the comparison is exact), sub-pixbuf aliasing, and the callback plumbing in `GLib.Log` and the idle/timeout sources. |
| `NodeStoreTests` | `NodeStore` is a managed `GtkTreeModel` implementation rather than a binding of one, so every column read and change notification is code Gtk calls back into. Asserted through a real `NodeView`, including that a column value follows the node's property rather than being copied in at `AddNode` time. |
| `IOChannelAndSpawnTests` | `GLib.IOChannel` and `GLib.Spawn`, both with oracles outside the library: a file the test wrote, and a child process whose output the test chose. |
| `PangoTests` | The hand-rolled `Pango.Attribute` hierarchy, attribute iteration, layout measurement and wrapping, tab arrays, font descriptions and metrics. |
| `CairoTextAndPathTests` | The rest of `Cairo.Context`: text measurement and drawing, path copying and flattening, groups, masks, clip extents, PNG round-trips. |
| `CairoDeepTests` | What the two files above did not reach, with the oracle outside the library wherever one exists: the `PathData` union walked back into operations and coordinates, joins and caps decided by trigonometry, the compositing operators decided by arithmetic, `PushGroup(Content)`, `ScaledFont` and `FontFace`, the pattern subclasses, and the region operations and hashing. |
| `WidgetBehaviourTests` | What a Gtk 4 application does: `Measure` with an orientation and a for-size, walking the child list that replaced `GtkContainer`, grid coordinates and spans, event controllers, CSS classes and providers, layout managers. |
| `ActionsAndModelsTests` | The two stacks the port introduced wholesale — `GAction`/`GMenu`, and the `GListModel` filter/sort chain — so neither has any history of working. |
| `TreeWrapperTests` | The hand-written halves of `TreeModelSort`, `TreeModelFilter` and `TreeStore`, and `NodeSelection`, which had none: iterator and path conversion both ways, a filter rooted at a subtree, `SetModifyFunc` synthesising a column, node selection by node and by path. |
| `MainLoopTests` | `Source`, `Idle`, `Timeout`, `MainContext`, `MainLoop`. The oracles are ordering rather than completion: a high-priority idle before a low-priority one, a shorter timeout before a longer one, a removed source never. |
| `ObjectAndValueTests` | The wrapper identity map, notifications, per-object data, and the `Value` cases the numeric round-trips do not reach — boxed opaques, value arrays, a managed object carried through unmanaged code. |
| `GdkDeepTests` | The Gdk that needs no display: `PixbufLoader` fed a PNG seven bytes at a time and required to agree with the whole file, its signal ordering and the rows its area-updated events name, pixbuf save options, `PixbufFormat`, texture download compared against the bytes the test chose, `RGBA` parsing and hashing, rectangle arithmetic against Gdk's own hit test, and the Gtk 4 clipboard data model — `ContentFormats`, `ContentFormatsBuilder`, `ContentProvider` — which nothing had ever called. |
| `TextStackTests` | `TextBuffer`, `TextIter`, `TextMark`, `TextTag`, `TextTagTable`, `TextChildAnchor` and `EntryBuffer` — the stack a text-editing application is built out of. The oracles are mostly outside Gtk: how many Unicode scalars a string has against how many UTF-16 units C# stores, how many bytes UTF-8 needs for them, which characters terminate a sentence, that a combining mark is not a cursor position. Plus mark gravity, tag toggle boundaries, search across a child anchor, the undo history, and the commit-notify callback added in Gtk 4.16. |
| `SatelliteAssemblyTests` | The three assemblies whose generated surface nothing had reached: libadwaita, GtkSourceView and Gsk. Render-node trees serialised and read back, node bounds composed by arithmetic the test does itself, `GskTransform`'s builder chain and the NULL that means the identity, `GskPathBuilder`; GtkSourceView's language guessing, line sorting, search occurrence counts, syntax context classes, regions and snippets; libadwaita's navigation stack, view-stack pages, toasts, style manager, spring params and breakpoint conditions. |
| `AuthoringTests` | The other direction: a C# program **writing** Gtk types rather than calling them, which is the code that runs C into managed. GType registration and `[GLib.TypeName]`, `[GLib.Property]` read and written through GObject rather than through C#, `notify::`, `Gtk.Builder` constructing a managed type by name, declaring an activation signal by overriding `OnActivate`, virtual overrides proved by asking a *parent* for the answer, chaining to a base vfunc, a `Gtk.LayoutManager` subclass placing children by transforms the test does the arithmetic for, and `[Gtk.Template]`/`[Child]`. |
| `GLibDeepTests` | The rest of hand-written GLib: `HookList`'s ABI description checked against the struct `g_hook_list_init` actually writes, the container half of `Variant`/`VariantType` (tuples, arrays, maybes, dict entries, subtyping), `Bytes` slicing and ownership, the `Marshaller` helpers below the ones every binding uses, and the two branches of `GLib.Signal` that only a returning signal or an emission hook reaches. |
| `GioDeepTests` | The half of Gio that needs a filesystem and a main loop. GSettings over a schema the test writes and compiles with `glib-compile-schemas`, stored through a **keyfile backend** so the oracle is the ini file on disk rather than anything GSettings remembers: defaults, writes, resets, user value versus default, ranges, the `changed` signal, delay/apply/revert, enum and flags nicknames, a child schema, and a two-way binding to a widget property. Then `GFileMonitor` over a directory the test builds, the async pattern end to end (callback thread, `GAsyncResult`, what `Finish` returns), `GCancellable` stopping a walk that has already started, `GFileInfo` attributes, `GFileEnumerator`, and `GFile` copy/move/rename/delete with their error codes. |
| `GrapheneMathTests` | Graphene, and the arithmetic half of Gsk — the corner of the tree with the best oracles there are, because every answer can be worked out in the test: a matrix times its inverse, a 3-4-5 triangle's area, a ray entering a box spanning [-1,1] at t=4, the six planes of a 60-degree frustum meeting the axis at 30 degrees. Matrix multiply/invert/decompose/transpose/interpolate/project, the vectors, rectangle intersection and the in-place trap, quad, triangle and barycentric coordinates, box, sphere, plane, ray, frustum, euler and quaternion; then `GskTransform`'s conversions and render-node bounds. Four codegen defects and a heap corruption; four pieces of graphene behaviour pinned because the obvious expectation is wrong. |
| `ExpressionTests` | `GtkExpression`: how the Gtk 4 list stack reads a value out of an item, and how a property is kept in step with one on another object. Property, constant, object, closure, cclosure and try expressions; evaluation against a this-object and the GValue it fills; watches and their invalidation; `Bind` and what a failed evaluation does to the target; expression-driven `StringSorter`, `NumericSorter`, `StringFilter` and `BoolFilter` over a list model. Four defects; the oracles are the length of a word, an alphabet and a set of ages chosen here. |
| `TreeViewStackTests` | The legacy tree *view*, where the largest block of untested hand-written `GtkSharp` was: `TreeViewColumn`'s attribute mapping and cell data funcs proved through `CellSetCellData`, the column list and its reordering, a header click driving the model's sortable interface, `TreeSelection` including a select function that vetoes, `TreeRowReference` against a `TreePath` that does not move, expansion and `MapExpandedRows`, a managed `CellRenderer` subclass measured and snapshotted *by Gtk*, `CellArea`/`ICellLayout`, the toggle and accel renderers, and a row moved between positions through `TreeDragSource`/`TreeDragDest` end to end. One use-after-free; three pieces of behaviour pinned. |
| `PangoShapingTests` | The half of Pango that turns text into glyphs, where `PangoTests` stops at attributes and measurement: itemization, shaping, the Unicode break algorithm, bidi, the layout iterator, `ScriptIter`, `AttrList` splice/filter/update, tab arrays, coverage, font families and faces, `Matrix`, cursor movement and layout serialisation. The oracles are outside the library — Unicode says where the word boundaries are, the bidi algorithm says which run gets an odd embedding level, a cluster's widths have to add up to the run's width, and index-to-position has to invert position-to-index. Eleven array parameters bound as scalars, a double free on every borrowed attribute, a mutable static identity matrix, and a field holding a struct by value that was read as a pointer to one. |
| `ControlsAndTransferTests` | The controls an application is built out of, and the two subsystems Gtk 4 replaced wholesale. Entry and `GtkEditable` over non-ASCII text (a position is characters, a length is bytes); adjustment clamping and the two signals that separate a change of range from a change of value; spin button stepping, wrapping and snapping; scale marks; level-bar offsets; progress-bar pulse; calendar; notebook reordering; `Gtk.Stack.Pages` as a list model; expander, popover, drop-down, scrolled window, search entry and search bar. Then `Gdk.Clipboard` — set, read back asynchronously, and the mime types Gdk negotiates around a `GValue` — and `GtkDragSource`/`GtkDropTarget`, whose signals are emitted directly against a subclass's vfuncs, because no drag can be started without a pointer device. Five defects. |
| `DesktopIntegrationTests` | Everything that talks to the desktop rather than to the screen, none of which had a test. The Gtk 4 async dialogs — `FileDialog`, `AlertDialog`, `ColorDialog`, `FontDialog` — driven to their Finish methods the only way a test without a user can, by cancelling the `GCancellable` they were started with; `FileFilter` matching a `GFileInfo` by suffix, pattern and content type, serialised through a `GVariant` and built from a `GtkFileFilter` buildable description; the launchers, held but never launched; the legacy `GtkFileChooser`; and the printing stack, which is nearly all pure data — `PaperSize`, `PageSetup` and `PrintSettings` through key files, every typed accessor and every unit, and a `PrintOperation` exported to a PDF so the whole signal chain runs with no printer. The oracles are ISO 216, ANSI, the definition of a point, and the file on disk. Three defects. |
| `AccessibilityTests` | `GtkAccessible`, which is where Gtk 4 put ATK and which nothing had ever called. The role every widget class declares, checked twice over — the property, and Gtk's own `gtk_test_accessible_has_role` — against the ARIA names, which are the fixed point when a member is inserted into the middle of `GtkAccessibleRole`; a role reassigned, and one named in a `.ui` file. Then the accessible tree, which is not the widget tree: a composite widget's parts, a parent assigned without reparenting, the sibling that only `SetAccessibleParent` can set. Then states, properties and relations set through the rebound update API and read back through Gtk's test API, the value type each attribute wants, the `<accessibility>` block in a `.ui` file, and `AccessibleList`. Three defects; `GtkAccessibleText` and `GtkAccessibleRange` pinned as unreachable. |
| `ApplicationTests` | The application object and the global state around it — the code every program runs before it does anything else, and which the rest of the suite only ever touched by accident. `GLib.Application` registration, the once-only `::startup` against the every-time `::activate`, the id rules, the busy counter and the property that drives it; `g_application_open` end to end; a `GApplicationCommandLine` built by the test, because `::command-line` needs a session bus and Windows re-reads the real process command line anyway. Then `Gtk.Application`'s window list — newest first, which is also what `ActiveWindow` means — accelerators through `SetAccelsForAction`/`GetAccelsForAction`, an `ApplicationWindow` as a `GActionGroup` under `win.` and the `app.` actions its widgets reach; `Gtk.Settings` overridden and reset; `Gtk.IconTheme` search and resource paths and an icon the test wrote; window modality, transient-for, groups, default size and the `::close-request` veto; `HeaderBar`/`WindowControls`; `Gtk.Accelerator`; `Gtk.Global`; and the `GLib.MainLoop` that `Application.Run` became when `gtk_main` was deleted. Five defects. |

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

**Never assert that a floating-point result is inexact.** Whether an operation
comes out exact is a property of the vector unit and the compiler that built the
native library, never of the binding. Two graphene tests asserted a round trip
was *not* exact — reasonable-looking, since it is inexact under gvsbuild — and
CI failed on both, because on the runner's hardware the same operations are
exact. Assert the property that holds everywhere (the translation moves
linearly; the dot product of two quaternions is 1) to a stated tolerance, and
say in a comment why the tolerance is what it is.

The same applies to `Marshal.SizeOf`, to the set of gdk-pixbuf loaders
installed, and to anything else that describes the machine rather than the code.
A test that pins the host will pass on the host it was written on and fail
somewhere else, which costs more than the coverage it bought.


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

### Two gdk-pixbuf implementations, and what may be asserted of both

Debian forky routes gdk-pixbuf through **glycin**; gvsbuild ships the classic
loaders. They are not interchangeable, and three tests had to be written to the
intersection rather than to whichever one was in front of them:

| | classic (gvsbuild) | glycin (forky) |
|:--|:--|:--|
| half a PNG, `Close()` | succeeds, full-size pixbuf, missing rows blank | raises `org.gnome.glycin.Error.LoadingError` |
| saving an **RGBA** pixbuf as JPEG | drops the alpha channel | refuses: "the encoder or decoder for Jpeg does not support the color type `Rgba8`" |
| `Gdk.Pixbuf.Formats` | every format names an extension | at least one names none |

So: `Close()`'s return value is not a completeness check on **either** — on one
it reports partial data as success, on the other it reports it as failure, and
neither says how much arrived. A JPEG test must start from a pixbuf with no
alpha, which is what an application saving a photograph has anyway. And a loop
of `NotEmpty` over `Formats` asserts a property of the installed module set, not
of this binding — the png assertions beside it are the ones with an oracle.

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

## Fixed: GLib.PtrArray looked its symbols up in the wrong library

All seven `g_ptr_array_*` symbols were loaded from `Library.GObject`. They live
in GLib. `GetProcAddress` found none of them, `FuncLoader` returned null
delegates, and **every constructor threw `NullReferenceException` the first time
it was called** — with nothing in the message naming a missing symbol. The class
had never worked.

The same failure mode as the removed Gtk 3 functions, from a different cause: not
a symbol that went away, but a symbol looked for in the wrong place. Both are
invisible until something calls, and 186 lines nothing called is exactly where
it survives.

When adding a `FuncLoader` lookup, check which library actually exports the
symbol rather than which binding the type belongs to. `GLib.KeyFile` and
`GLib.PtrArray` are both in `GLibSharp`, and only one of them is a GObject.

## Fixed: GioStream could not open a file, and Read overran its buffer

`GioStream` adapts a GLib stream to `System.IO.Stream` in 254 lines that nothing
had called.

Both file constructors — by path and by URI — threw `NotImplementedException`, so
the class could only wrap a stream the caller had already opened, which is the
one case where it saves nobody any work. They now go through Gio and map
`FileMode`: `Open`/`OpenOrCreate` read, `Create`/`Truncate` replace, `CreateNew`
creates and fails if the file exists, `Append` appends.

`Read` guarded with `offset + count - 1 > buffer.Length`, which admits a request
exactly one byte too long. With `offset == 0` that count went straight to the
native read as a length larger than the managed buffer — an overrun, not an
exception. `Write` ten lines below has the same guard written correctly, which is
what gave it away. Compare adjacent guards when one of a pair is tested and the
other is not.

`Read`'s offset path also used `buf.CopyTo(buffer, offset)`, copying the whole
scratch buffer rather than the bytes actually read, so a short read wrote the
scratch buffer's trailing zeroes over data the caller already had.

## Behaviour worth knowing, found by an assertion that was wrong

Three tests failed because *my* expectation was wrong, not the library's. Each is
now pinned as what it is, because in every case the plausible assumption is the
one that produces silently wrong results:

- **`GLib.Date.DaysBetween` reads backwards from its name.** The receiver is
  `date1`, the argument `date2`, and the C function returns `date2 - date1`, so
  `a.DaysBetween(b)` is `b - a`.
- **`KeyFile` discards translations on load** unless `KeepTranslations` is given,
  and `GetLocaleString` then falls back to the untranslated value rather than
  failing — so forgetting the flag yields plausible wrong answers, not an error.
- **A `TreeIter` identifies a row, not a position.** After swapping the first and
  last rows, the iter that named the first still names it, now at the end.
  Reordering code that treats an iter as an index moves the wrong row silently.

Also: `GLib.Value(object, name)` initialises a `Value` of the property's *type*
without reading the property — it exists to be an out-parameter for
`g_object_get`. Reading is `GetProperty`'s job. And `ListBase.Empty` frees the
native list only when the wrapper owns it, so on the single-argument constructor
most callers reach for, it does nothing at all.

## Fixed: three more symbols loaded from the wrong library

`GLib.PtrArray`'s seven `g_ptr_array_*` lookups were the first of these. An audit
of every `g_`-prefixed symbol in the tree against the library that exports it
found three more:

- `PtrArray` also loads `g_object_unref` from GLib, so disposing an owning array
  of GObjects threw.
- All five `g_spawn_*_utf8` entry points — the ones `GLib.Spawn` uses **on
  Windows only** — were loaded from GObject. GLib exports them, so every spawn
  on Windows called a null delegate, while Linux, which takes the plain names
  beside them, worked. A defect on one platform only is the signature of this
  mistake, because the two code paths were written at different times.

The audit is worth re-running after touching any `FuncLoader` lookup:

```python
GOBJECT = ('g_type_','g_object_','g_signal_','g_param_','g_value_','g_closure_',
           'g_enum_','g_flags_','g_boxed_','g_binding_','g_cclosure_')
pat = re.compile(r'GLibrary\.Load\(Library\.(\w+)\),\s*"([a-z0-9_]+)"')
# a g_ symbol not in GOBJECT belongs to GLib, unless it is a Gio one
```

**Check which library exports the symbol, not which binding the type belongs
to.** `GLib.KeyFile` and `GLib.PtrArray` are both in `GLibSharp`, and only one of
them is a GObject.

## Fixed: a stack-corrupting ABI in Cairo's matrix getters

Reading `Context.FontMatrix` crashed the process outright. `Cairo.Matrix` is a
**class**, so it already marshals as `cairo_matrix_t*`; declaring the parameter
`out` made it `cairo_matrix_t**`, and Cairo wrote 48 bytes of doubles through the
address of an 8-byte reference slot. Three getters had it —
`cairo_get_font_matrix`, `cairo_scaled_font_get_ctm`,
`cairo_scaled_font_get_font_matrix` — while the setters directly beside them,
taking the same type, were already right.

Same shape as the caller-allocates defect above: a function that writes into
storage **the caller** provides, bound as though it returned something.

**Why it survived: a crash is quieter than a failure.** An
`AccessViolationException` takes down the test host rather than failing a test,
so the run reports whatever finished first and stops. Before the fix, the class
reported *nine passing tests of twenty-one* and the whole suite reported 68 of
484 — both with a "Passed!" line. When a run's total is lower than it should be,
that is the thing to chase, not the pass count.

## Fixed: two hash functions that could not be wrong in a way equality notices

`Cairo.Matrix.GetHashCode` was six terms of the shape `(int)Xx ^ (int)Xx>>32`,
written as though the fields were 64 bits wide. They are not: the cast makes each
one an `int`, and C# masks an `int` shift count to five bits, so `>>32` is `>>0`
and **every term cancelled against itself. The hash was always zero**, for every
matrix. The cast also discarded the fractional part, so `1.5` and `1.9` were
indistinguishable to it even in principle.

`Cairo.Region.GetHashCode` returned the handle's, while `Equals` calls
`cairo_region_equal`. Two regions covering the same ground were therefore equal
and hashed apart, which breaks the one rule a hash has to keep: a `Region` could
not be used as a dictionary key at all, because the lookup missed even when an
equal key was in the table. It now hashes the region's rectangles, which cairo
keeps in canonical form, so equal regions produce the same list.

**A broken hash survives the obvious test.** `Assert.Equal(a.GetHashCode(),
b.GetHashCode())` on a value and its copy is exactly what a constant satisfies,
and `CairoTests.Matrices_compare_by_value` had been asserting it against zero
since it was written. What catches it is hashing *distinct* values and requiring
them to differ, or putting the type in a `Dictionary` and looking a key back out.

While there: `Matrix.operator ==` dereferenced both sides unconditionally, so
`matrix == null` — the first thing any caller writes about a class — threw
`NullReferenceException` from inside the operator.

### And a third: every generated struct hashed its fields commutatively

`GapiCodegen` emitted `GetHashCode` as `typename ^ f1 ^ f2 ^ …`. XOR is
commutative, so **any permutation of a struct's fields produced the same hash**:
`Gdk.RGBA` red `(1,0,0,1)` and green `(0,1,0,1)` collided exactly, as did
`Gdk.Rectangle(1,2,3,4)` and `Gdk.Rectangle(2,1,4,3)`. Legal — equal values still
hash equal — but a colour or a rectangle is exactly the kind of thing that ends up
as a dictionary key, and the collisions are not rare accidents but a structural
property. `StructBase.GenHashCode` now folds the fields in order
(`hash = hash * 397 ^ field.GetHashCode ()`), and `NativeStructGen` shares it.

## Fixed: Pango's attribute iterator and font-description equality

`AttrIterator` built its `GLib.SList` without an element type, in both `Attrs`
and `GetFont`. With none, `ListBase.DataMarshal` falls through to treating each
item as a GObject — which a `PangoAttribute` is not — and returns `null`, so
unboxing to `IntPtr` threw. Reading the attributes back off an iterator, the only
way to get them out of an `AttrList`, failed every time.

`Pango.FontDescription` inherited `GLib.Opaque`'s equality, which compares
handles. Two descriptions built from the same string are equal by every measure
Pango offers — `Equal` says so, `Hash` agrees — but were unequal here and hashed
differently, so one could not be used to find the other in a dictionary. It now
overrides `Equals`/`GetHashCode` onto Pango's own two functions rather than
reimplementing the comparison: which fields count is Pango's business.

## Fixed: a number that binds to the raw-pointer constructor

Since .NET 7, `IntPtr` is `nint` and `int` converts to it **implicitly**. Where a
type has both a raw-pointer constructor and a numeric one, the obvious call binds
to the wrong one:

```csharp
new GLib.ValueArray(2)   // reads as "preallocate 2"  -> ValueArray(IntPtr)
new GLib.Date(2)         // reads as a Julian day     -> Date(IntPtr)
new GLib.DateTime(2)     // reads as a Unix time      -> DateTime(IntPtr)
```

Each then dereferences address 2. `GLib.Value` and `GLib.Variant` have the same
pair but are safe, because their `int` overload is an exact match and wins.

**The wrapper libraries are `LangVersion 9`, where the conversion does not
exist**, so nothing inside this repository could hit it — only consumers of the
package, on any modern language version. That is the worst place for a hazard to
live, and it is why it survived.

`Opaque.CheckRaw` now rejects any address in the first page, which is never
mappable on any platform this runs on — the null page is reserved precisely so a
small integer faults. It returns the pointer so it can be used **in a base-call
argument**, which is the only position that runs before the handle is stored: a
guard in the constructor body throws *after* `base(raw)`, and the finalizer then
frees address 2 anyway.

## The failure modes that do not fail a test

Three crashes in this suite have been mistaken for something else, and they share
a shape worth naming: **a run whose *total* is lower than it should be, under a
"Passed!" line.** The pass count looks fine because the tests that finished did
pass.

| Cause | What it looked like |
|:------|:--------------------|
| `out`-on-a-class ABI in `cairo_get_font_matrix` | 9 of 21 tests in a class; 68 of 484 in the suite |
| Infinite recursion in `TreeModelSort.AppendValues` | stack overflow, uncatchable |
| A leaked `Cairo.Path` | 315 of 484, **at whatever moment the GC ran** — so it landed on an unrelated test and looked intermittent |

The third was recorded in this document as an unexplained flake for exactly that
reason. It is not a flake: `Cairo.Path` must be disposed, and a copied path
outlives the context it came from, which is what makes it easy to forget.

So: **check the total, not the pass count.** And when a crash appears to move
around between runs, suspect a finalizer before suspecting nondeterminism.

## Fixed: a lost level of indirection on `const char* const*`

`GirToGapi`'s C-type normaliser had a rule to tidy const qualifiers:

```csharp
(const\s+)?(\w+)\*\s+const\*   ->   const $2*
```

`const char* const*` is a pointer to const pointers to const char — `char**`
with both levels qualified — and that rule **dropped one of the two stars**. So
every parameter and return value spelled that way arrived as a single string, and
the call handed GTK the bytes of that string to read as an array of pointers.

Sixty-seven of them, across seven assemblies. `gtk_string_list_new` is the one to
remember: `new StringList(text)` compiled, read correctly, and was wrong.

**Why it was partial, and so harder to see.** The strv detection already accepted
`gchar**`, `char**`, `const gchar**` and `const char**`, which between them cover
most of the girs. The `const T* const*` spelling accounts for 85 more, and only
those were broken — so string arrays worked in enough places to look fine.

A one-character fix in the replacement, then `RegenerateApi`. The affected sites
now carry `type="const-char**" null_term_array="true"` and bind as `string[]` —
which is also how this became testable at all: `StringList(string[])` did not
exist before the fix.

## Behaviour worth knowing, found by an assertion that was wrong

Beyond the three above:

- **`SimpleAction.StateChanged` is the `change-state` signal**, not a
  notification after the fact. `GSimpleAction`'s default handler is what applies
  the new state, and connecting *replaces* it — so a handler that only reads the
  value leaves the action on its old state. Named like an observer, behaves like
  a veto.
- **`Widget.Activate` does not reach a `Clicked` handler**, because Gtk 4 routes
  a press through a gesture on the button. This is the behaviour that once made
  the sample button-press theory pass while pressing nothing.
- **`GMenuModel`'s items-changed carries signed counts** while `GListModel`'s
  carries unsigned ones — separate signals with separate args classes, which is
  what the port had to split them into.
- **Writing to a closed `PixbufLoader` is not a `GError`.** It is a
  `g_return_val_if_fail`: gdk-pixbuf logs a CRITICAL, returns `FALSE`, and leaves
  the error pointer NULL. `Write`'s bool return is therefore the only thing that
  says the bytes went nowhere — and `PixbufLoader.LoadFromStream`, in this
  repository, ignores it.
- **A truncated PNG usually closes without error.** Cut one in half and
  `Close ()` succeeds and hands back a full-size pixbuf with the missing rows
  blank; only a stream too short to have produced any pixbuf at all raises. So
  "did `Close` throw" is not a completeness check.
- **`ContentFormatsBuilder.ToFormats` resets the builder.** A second call returns
  empty formats rather than the same set again, which is silent: nothing
  distinguishes "offered nothing" from "read twice".
- **`gdk_texture_download` always produces `GDK_MEMORY_DEFAULT`**, i.e. cairo
  ARGB32 — which is `B,G,R,A` in memory on a little-endian machine, the reverse
  of a `Gdk.Pixbuf`'s channel order. Copying one buffer into the other without
  swapping is a red/blue swap that survives every size assertion.
- **`+=` on a signal connects the handler `after` the default one**, unless the
  handler *method* carries `[GLib.ConnectBefore]` — `Signal.AddDelegate` reads
  the attribute off the delegate's `MethodInfo`, which a lambda cannot carry.
  For `RUN_LAST` signals whose default handler does the work, that inverts what
  the Gtk documentation describes: a `TextBuffer.InsertText` handler sees the
  text already inserted and `::changed` already emitted, and a `DeleteRange`
  handler reads the *empty string* out of the range it was handed, because both
  iterators have collapsed onto the deletion point. Neither errors. So a
  handler that needs the pre-edit state must be a named method with the
  attribute.
- **`gtk_text_iter_forward_word_end` and `forward_sentence_end` return `FALSE`
  at the end of the buffer**, even though the last word and the last sentence
  end there. A `while (iter.ForwardWordEnd())` loop therefore drops the final
  one; the buffer's end has to be added back by hand.
- **`GtkTextBufferCommitNotify` reports zero for `AFTER_DELETE`'s length.** The
  range is already gone, so there is nothing left to describe — a handler that
  read it as "how much was removed" would see every deletion as empty.
- **A `TextMark` with left gravity is the one that does *not* move.** "Left
  gravity" means it stays to the left of text inserted at it; the right-gravity
  mark is pushed along. The name reads like a description of where it goes.
- **GSK's identity transform is a NULL `GskTransform *`.** Every function in the
  family accepts NULL for it, so a composition that cancels out returns nothing
  at all and the binding hands back `null`:
  `translate.With (translate.Invert ())` is null. `gsk_transform_invert`
  overloads the same NULL for "not invertible", so a successful inverse and a
  failed one are indistinguishable from managed code. Meanwhile
  `gsk_transform_new` *does* allocate an object, which prints `none` and
  compares `Equal (null)` — so equality, not a null check, is how a caller asks
  "is this the identity", and `Gsk.Transform.Parse ("none", out t)` returns
  **true with `t == null`**.
- **`gsk_render_node_deserialize` hands back a node even when the parse failed.**
  Arbitrary text yields an *empty container node* plus errors through the
  callback — and an empty document yields the same node with no errors. Testing
  the result for null concludes that rubbish parsed fine; the callback is the
  only thing that separates the two.
- **`gtk_source_buffer_sort_lines` appends a newline.** It rebuilds the range out
  of the lines it collected, joined and then terminated, so a buffer whose last
  line had none grows one — a sort in an editor silently adds a blank line at the
  end of the document. It is idempotent, because the new empty last line is not
  one of the lines the next sort collects.
- **`gtk_source_snippet_context_expand` is not a template engine.** The input has
  to be exactly one reference: `$name` expands, `hello $name` and `$name $name`
  come back verbatim, with no error. An unknown name resolves to itself rather
  than to the empty string. What it does have is a filter language —
  `$name|capitalize`, `|upper`, `|camelize`, `|functify` — and a set of calendar
  constants (`CURRENT_YEAR`, `CURRENT_MONTH`, zero-padded strings rather than
  numbers). The `${...}` form belongs to the snippet *parser*, not to a context.
- **`gtk_source_snippet_copy` drops the name.** It carries the trigger, the
  language and the chunks across; `name` — the one field a snippet chooser puts
  on screen — comes back null.
- **A language's style ids are namespaced with its own id.** There is no
  `def:comment` in `GtkSource.Language.StyleIds`; there is `c-sharp:comment`,
  whose `GetStyleFallback` is `def:comment`. That indirection is what lets one
  theme colour every language.
- **A page may appear on an `Adw.NavigationView` stack only once.** Pushing a tag
  that is already on it does not raise it to the top: libadwaita logs a critical
  and does nothing, so a "go to section" button wired straight to `PushByTag`
  works once and is then inert.
- **`GLib.GException (IntPtr)` frees the `GError` it is handed** (`g_clear_error`
  in the constructor). That is right for the `out GError **` a Gio call fills in,
  and wrong for the borrowed error a callback like `Gsk.ParseErrorFunc` is
  passed — constructing one there is a double free. `ParseErrorFunc`'s error is
  bound as a bare `IntPtr`, so nothing stops it.

## Fixed: emitting a signal that returns a value took the process down

`GLib.Signal.Emit` has two branches, and only one of them had ever run. When the
signal returns something it emits into a `GLib.Value` — and it passed
`GLib.Value.Empty`, a zeroed `GValue` holding `G_TYPE_INVALID`. `g_signal_emitv`
requires that value to be **initialised to the signal's return type**, so what
happened was:

```
GLib-GObject-CRITICAL **: g_value_set_boolean: assertion 'G_VALUE_HOLDS_BOOLEAN (value)' failed
```

— no emission at all, followed by reading `.Val` off the uninitialised value,
which aborted the test host. Every signal in Gtk that returns a `gboolean`
(`close-request`, `keynav-failed`, `query-tooltip`, …) was unreachable through
`Emit`.

`GSignalQuery.return_type` also carries `G_SIGNAL_TYPE_STATIC_SCOPE` in its low
bit, which is never part of a `GType`, so it has to be masked off before the type
is compared or used.

The branch existed since the mono era and had no test, which is the whole point:
`Emit` is *the* signal entry point, exercised constantly — but only ever on
`void` signals, so the other half of an `if` sat there compiling.

## Fixed: three ways a GVariantType lies about itself

A `GVariantType`'s string is **not nul-terminated**. It is a pointer and a
length, and `g_variant_type_peek_string` returns only the pointer.

- **`ToString` read it as a C string**, so it ran off the end.
  `g_variant_type_new_maybe` allocates exactly one byte per character and writes
  no terminator, so `VariantType.NewMaybe (Int32).ToString ()` came back
  `"mivoke"` — the type, plus whatever the allocator had next door. Every type
  built by `NewArray`/`NewMaybe`/`NewTuple`/`NewDictionaryEntry` printed garbage;
  only the ones built from a string worked, because `g_variant_type_new` goes
  through `g_strndup`. It now copies `g_variant_type_get_string_length` bytes.

- **`First ()` and `Next ()` copied.** They are *positions inside the enclosing
  tuple's string* — "next" means "advance past the type at this address" — and
  every accessor in the class wrapped its result in `new VariantType (ptr)`,
  which is `g_variant_type_copy`. A copy's next address is its own end, so
  `First ().Next ()` returned the empty type, and so did everything after it:
  walking a tuple gave `["s", "", ""]`. The walk `g_variant_type_first` exists
  for could not be done at all. Both now hold a borrowed pointer plus a reference
  to the type that owns the string, `Dispose` frees only what it owns, and
  `Next ()` returns **null** past the last item — which is the terminator a loop
  needs and there previously was none of.

`Element ()`, `Key ()` and `Value ()` keep copying, which is right: they are not
chained, and a copy nul-terminates.

## Fixed: gulong is not 8 bytes on Windows

`GLib.HookList` is 118 lines that describe the layout of `GHookList` and bind
nothing at all — the highest uncovered-line count in `GLibSharp` and, until now,
with nothing whatsoever reaching it.

Its first field is `seq_id`, a `gulong`. That is C's `unsigned long`: 8 bytes
under LP64, **4 under Windows' LLP64** and on any 32-bit target. The generated
code said `sizeof (ulong)`, which is 8 everywhere, and took the alignment from a
helper struct holding a `UIntPtr`, which is wrong in exactly the same place. So
on Windows every field after it was described four bytes too far along — `hooks`
at 16 where glib writes it at 8 — and the struct was reported as 56 bytes instead
of 48.

The oracle is glib itself: `g_hook_list_init` writes `seq_id = 1`, `hook_size =`
its argument, `is_setup = TRUE`, NULLs `hooks` and `dummy3`, and installs its own
`default_finalize_hook` — that last one being the better probe, because a wrong
offset lands on one of the NULLs on either side of it. A second test links a real
hook with `g_hook_insert_before` and requires the pointer `g_hook_alloc` returned
to be readable at the `hooks` offset, which needs no knowledge of `GHook`'s own
layout.

**A defect on one platform only is the signature of this mistake**, the same way
the `g_spawn_*_utf8` lookups were. `HookList` is the only `sizeof (ulong)` in the
tree, so the fix is local; a `gulong` field in a struct that *is* regenerated
would need the same treatment in `GapiCodegen`.

`HookList` itself is still inert: it declares no methods, and being a boxed type
with no allocator, `new HookList ()` leaves the handle null. A test pins that, so
that adding an operation forces a test to be added with it. (The comment there
names `Gsk.RoundedRect` as the same shape; it is not — see "a generated struct
that is eight bytes shorter than the C one" below.)

## Fixed: three helpers that handed glib memory it was not allowed to have

- **`Bytes.NewTake`** passed a `byte[]` to `g_bytes_new_take`, which assumes
  ownership of the pointer and `g_free`s it on the last unref. A blittable array
  is *pinned* by the marshaller, not copied, so glib was handed an interior
  pointer into the GC heap and would eventually free it. It now copies into
  `g_malloc`'d memory, and takes the reference correctly through `GetOpaque` —
  the old code double-referenced, so the `GBytes` was never freed and the defect
  could not fire.

- **`Bytes.NewStatic`** is worse in principle: `g_bytes_new_static` keeps the
  pointer forever and never frees it, and the pin lasts only for the duration of
  the call. "Static" cannot be honoured for a managed array at all, so it copies.

- **`Marshaller.StructArrayToNullTerminatedStructArrayIntPtr`** returned `mem`
  *after* the loop had advanced it past every element, so the caller got a
  pointer to the null terminator — an empty array — and the allocated block was
  unreachable. Its reader half walked the base pointer forward by `sizeof(T)` and
  stopped when the *pointer* went null, which it never does, and passed a boxed
  `default(T)` to the `object` overload of `PtrToStructure`, which rejects value
  types. Nothing in the tree calls either, which is how both survived; they are
  now inverses, and the test asserts a round trip rather than either half alone.

## Fixed: the api.xml cannot say a method eats its receiver

`gdk_content_formats_union` and its four `union_*` siblings take the formats they
are called on as **`(transfer full)`** — the callee consumes a reference — and
`gdk_content_formats_builder_free_to_formats` frees the builder outright. An
api.xml `<method>` describes the ownership of its *parameters* and has no way to
describe the instance's, so codegen passed `Handle` and the wrapper went on
owning it. The formats were freed underneath a live wrapper, which unreffed them
again when it was disposed or finalized.

```
Gdk-CRITICAL **: gdk_content_formats_unref: assertion 'formats->ref_count > 0' failed
```

Then, later, an access violation somewhere unrelated. **The full suite crashed
roughly one run in three, at a different point each time**, because the second
unref is queued onto the main loop by the generated finalizer — the exact shape
this document warns about under "a crash that appears to move around is a
finalizer". A single test never reproduced it; sixteen wrappers plus a forced
`GC.Collect` and a drained main loop made it certain.

The fix takes the reference the callee eats, so the managed object keeps
behaving like every other one here — a method does not destroy the object it was
called on. Scanning every vendored gir for `<instance-parameter …
transfer-ownership="full">` finds **51 such functions across the stack**; most
are `*_unref`/`*_free`, which the binding already handles, but
`gtk_snapshot_free_to_node`, `gsk_path_builder_free_to_path`,
`gtk_expression_bind`, `g_string_free_to_bytes` and
`g_bytes_unref_to_array` are the same shape. `gsk_path_builder_free_to_path` and
the twelve `GskTransform` builders were fixed next; `gtk_expression_bind` after
them. `g_string_free_to_bytes` and `g_bytes_unref_to_array` are still open.

```python
# the audit, run over Source/Gir/*.gir
re.finditer(r'<(?:method|constructor|function)\b[^>]*?c:identifier="([a-z0-9_]+)"[^>]*>(.*?)</...>', t, re.S)
# flag any whose <instance-parameter ... transfer-ownership="full">
```

## Fixed: three caller-allocates buffers in Gdk bound as scalars

Same family as the `graphene_rect_union` and `cairo_get_font_matrix` defects
above: a function that writes into storage **the caller** provides, bound as
though it returned something.

- **`gdk_texture_download`** writes `Height * stride` bytes through a `guchar *`.
  Codegen bound it `out byte` and returned that byte, so
  `texture.Download (stride)` handed Gdk the address of one stack slot and let it
  write a whole image through it. `gdk_texture_downloader_download_into` is
  identical. Both are now hidden and rebound onto a `byte[]` the caller sizes,
  with the guard the C API has no way to enforce.

- **`gdk_content_formats_new`** takes `const char **mime_types` plus a count. The
  api.xml says `const-char**` with an explicit length rather than
  `null_term_array`, a shape codegen has no rule for, so it emitted
  `ContentFormats (string, uint)` and passed **one** strdup'd string — which Gdk
  then read as an array of pointers, using the characters of the string as
  addresses. `gdk_content_formats_get_gtypes` is the mirror image: it returns a
  `GType` array and its length, and the binding wrapped the **array pointer**
  in a `GLib.GType`, producing a type whose value was an address.

- **`gdk_content_provider_get_value`** fills a `GValue` the caller has already
  `g_value_init`ed to the type it is asking for — the type is an *input*, and
  the provider answers `G_VALUE_HOLDS` and refuses anything else. Codegen made
  it a plain out-parameter over `Marshal.AllocHGlobal`, so Gdk read a `GType`
  out of uninitialised heap. It is now `GetValue (GType, out Value)`, which is
  the only signature that can express the call.

## Fixed: two text-stack calls that Gtk 4 reshaped underneath

`gtk_text_child_anchor_get_widgets` took one argument in Gtk 3 and returned a
`GList*`. In Gtk 4 it takes a second, an out-parameter for the count, and
returns a `GtkWidget**` array. The hand-written `TextChildAnchor.Widgets` still
had the Gtk 3 shape, so it left the callee writing the count through whatever
the caller happened to have left in the second argument register, and then read
an array of pointers as though it were a linked list. Reading the property took
the process down:

```
Fatal error. System.AccessViolationException: Attempted to read or write protected memory.
   at Gtk.TextChildAnchor.get_Widgets()
```

`GetWidgets` is `hidden` in the metadata, so the hand-written property was the
only way to reach it at all, and nothing had ever called it. This is the same
family as the removed Gtk 3 symbols: a signature that changed is quieter than a
symbol that vanished, because it still links and still compiles.

`gtk_text_iter_order` writes through **both** its pointers — it swaps the pair
so the receiver holds the earlier position. The gir does not mark the second
parameter `inout` (only its C type gives it away: `GtkTextIter*` where
`gtk_text_iter_assign`'s is `const GtkTextIter*`), so codegen passed it by
value and dropped the write-back. Ordering a reversed pair therefore returned
`(2, 2)` where Gtk had produced `(2, 7)`: the range silently collapsed to a
point, and every range operation performed on the "ordered" pair became a
no-op. Fixed with `pass_as="ref"` in `GtkSharp.metadata`.

## Fixed: every public field in the tree was write-only

`FieldBase.Readable` has two rules, chosen by the api.xml's `parser_version`:

```csharp
if (Parser.GetVersion (elem.OwnerDocument.DocumentElement) <= 2)
    return elem.GetAttribute ("readable") != "false";     // default: readable
return elem.HasAttribute ("readable") && ...;             // default: NOT readable
```

Every file here says `parser_version="3"`, so a `<field>` gets a getter **only if
the attribute is actually present**. GIR is the other way round: `readable` is the
default and is simply omitted, so `GirToGapi` never wrote one. The result is that
codegen emitted a **setter and no getter** for all 889 public fields across the
eleven assemblies — `Graphene.Point.X` could be assigned and never read back, and
graphene has no `graphene_point_get_x` to compensate.

It survived because most of the types people reach for get their accessors from
*methods* instead: `Graphene.Rect.Width` is `graphene_rect_get_width`, not the
`width` field, so the rectangles the existing tests measure looked fine. The
types with nothing but fields are the ones nothing had called —
`Gsk.ParseLocation`, `Graphene.Point`, `Graphene.Size` — which is exactly where a
missing getter hides.

Two lines in `GirToGapi` (`ObjectEmitter.EmitField` and
`TypeEmitters.EmitRecordField`) now emit `readable="true"` unless GIR says
`readable="0"`, mirroring the `writable` rule immediately below them. The
`RegenerateApi` diff is 889 lines and contains nothing else.

## Fixed: three more array parameters whose length is a separate argument

Same family as `gdk_content_formats_new`. Codegen has a rule for a
NULL-terminated array and none for `T **items, n_items`, so each of these came
out taking a **single value**, and the callee read the first machine word of it
as element zero:

- `gsk_container_node_new (GskRenderNode **, guint)` — passed one node, whose own
  first word is its `GskRenderNodeClass` pointer, which GSK then reffed as a
  child.
- `gsk_render_node_get_children` — the mirror image: the *array* address wrapped
  in one `RenderNode`.
- `adw_navigation_view_replace (AdwNavigationPage **, int)` and
  `adw_navigation_view_replace_with_tags (const char **, int)` — a page's
  `GTypeInstance` class pointer, and eight bytes of a tag's own characters used
  as an address.

All four are `hidden` in the metadata and rebound over real arrays in
`Source/Libs/GskSharp/{ContainerNode,RenderNode}.cs` and
`Source/Libs/AdwaitaSharp/NavigationView.cs`.

## Fixed: thirteen more methods that eat their receiver

The `gdk_content_formats_union` family again, in Gsk. **Every one of the twelve
`GskTransform` builders takes its receiver as `(transfer full)`** — the transform
that comes back links the old one into its own chain and keeps that reference —
and `gsk_path_builder_free_to_path` frees the builder outright. An api.xml
`<method>` describes its *parameters'* ownership and has no way to describe the
instance's, so codegen passed `Handle` and the wrapper went on owning a reference
the callee had already consumed. Two finalizers, one deferred onto the main loop:
the crash lands wherever the GC happens to run.

The gir says so plainly, which is what makes the audit worth repeating:

```
gsk_transform_invert | <instance-parameter name="self" transfer-ownership="full" ...>
```

All thirteen are `hidden` in `GskSharp.metadata` and rebound in
`Source/Libs/GskSharp/{Transform,PathBuilder}.cs`, taking the reference the callee
eats so that a method does not destroy the object it was called on. The test that
would catch a regression builds fifty chains, drops them, forces a collection and
then drains the main loop with a log handler installed — no single call
reproduces it.

## Open: a generated struct that is eight bytes shorter than the C one

`DeeperStackTests.A_rounded_rect_keeps_its_bounds` is `Skip`ped, and **the
reason recorded here was wrong**. `Gsk.RoundedRect` is not a boxed type with a
missing allocator; it is a generated `[StructLayout (Sequential)] struct`. What
kills the process is that its layout is not `GskRoundedRect`'s:

```c
struct _GskRoundedRect {
  graphene_rect_t bounds;      /* 16 bytes, embedded by value */
  graphene_size_t corner[4];   /* 4 x 8 = 32 bytes            */
};                             /* 48 bytes                    */
```

Both member types are bound as **classes** — `graphene_rect_t` and
`graphene_size_t` are `<boxed opaque="true">` — so codegen emitted `bounds` as a
single `IntPtr` and `corner` as a `ByValArray` of four object references.
Measured:

| | managed | C |
|:--|--:|--:|
| `sizeof` | 40 | 48 |
| offset of `corner` | 8 | 16 |

Every generated method then does
`AllocHGlobal (Marshal.SizeOf<Gsk.RoundedRect> ())` and hands that pointer to a
GSK function that reads and writes 48 bytes through it: an eight-byte heap
overrun on **every** call, with `bounds` read back as an address assembled out of
two floats. A default instance also has `Corner == null`, so there is nothing to
marshal out in the first place.

The fix is in `StructBase`: a field whose type is a boxed opaque with a known ABI
size has to be embedded by value rather than referenced. Hiding the two fields
instead is not enough, because `GenEqualsAndHash` skips hidden fields and would
emit `Equals` as `return true`. Nothing else in the repository touches the type,
so the damage is confined to whoever reaches for it first.

`SatelliteAssemblyTests.The_rounded_rect_struct_holds_pointers_where_gsk_embeds_the_values`
pins the mismatch against arithmetic over the C declaration, and is written to
fail once the layout is corrected — so the person who fixes it is sent back to
unskip `A_rounded_rect_keeps_its_bounds`.

It asserts the **field types** rather than `Marshal.SizeOf`, because the runtime
cannot be asked to measure this struct portably: .NET on Linux refuses outright
("Type 'Gsk.RoundedRect' cannot be marshaled as an unmanaged structure" — a
`ByValArray` of a class is not a layout the marshaller has), while on Windows it
answers 40. Those are two reports of one defect, and the defect is upstream of
both: a pointer field and an array-of-references field cannot describe sixteen
embedded bytes followed by thirty-two more, whatever number a given runtime is
willing to put on them.

## Fixed: no managed widget could override OnActivate

`Gtk.Widget.ConnectActivate` is what runs when a managed subclass overrides
`OnActivate`: it registers a signal with `g_signal_newv` and tells Gtk that this
is the type's *activation* signal. Gtk 3 said that by writing the signal id into
a public `GtkWidgetClass` field, and this still did:

```csharp
uint* raw_ptr = (uint*)(((long) gtype.GetClassPtr())
                        + (long) class_abi.GetFieldOffset ("activate_signal"));
```

Gtk 4 made that field private. `GtkWidgetClass` has no `activate_signal` member,
so the generated ABI description has no such field, and `AbiStruct.GetFieldOffset`
indexed an `OrderedDictionary` with a name that is not in it and dereferenced the
null it got back. **The exception is thrown from class-init**, i.e. the first time
the type is used at all, so the subclass could not be constructed — with a
`NullReferenceException` naming nothing, out of a method the author never called.

The Gtk 4 way to say the same thing is `gtk_widget_class_set_activate_signal`,
which is now what it calls. That also makes `Widget.Activate ()` emit the signal,
so a managed widget can be activated the way any other one is. The signal keeps
its Gtk 3 name, `activate_signal`, rather than becoming `activate`: `Button`,
`Entry` and others already have a signal called `activate`, and registering a
second of that name on a subclass's own GType is an error.

The same shape as every other Gtk 3 leftover in this document — it compiled, and
nothing called it.

## Fixed: a nullable transfer-full argument released before the null check

Codegen emits, for an owned opaque parameter, one line that hands ownership over
and then a call that already copes with `null`:

```csharp
public void Allocate (int width, int height, int baseline, Gsk.Transform transform) {
    transform.Owned = false;                                  // <-- dereferences it
    gtk_widget_allocate (Handle, width, height, baseline,
                         transform == null ? IntPtr.Zero : transform.Handle);
}
```

`gtk_widget_allocate`'s transform is `nullable="1" transfer-ownership="full"`, and
**NULL is the identity transform** — exactly what a layout manager passes for a
child that sits at the origin. So the obvious call threw `NullReferenceException`
from inside the binding, before any native code ran and with nothing in the
message naming the argument. `Parameter.Prepare` now guards the assignment;
fourteen call sites across four assemblies were affected, among them
`ListView.ScrollTo`, `Viewport.ScrollTo` and `DropTargetAsync.Formats`, all of
which document their argument as optional.

## Authoring: what Gtk 4 changed about overriding

Three things surfaced writing `AuthoringTests`, none of them binding defects, all
of them able to make an override look broken:

- **A widget's layout manager answers instead of its `measure` and
  `size_allocate` vfuncs.** Gtk 4 consults `priv->layout_manager` first and only
  falls back to the class vtable when there is none. `GtkBox` always has a
  `GtkBoxLayout`, so `OnMeasure` on a `Box` subclass is installed in the class
  struct and never called — setting `LayoutManager = null` makes the same
  override take effect immediately, which is how the test proves the patch was
  there all along. A `Box` subclass that wants to measure differently has to
  replace the layout manager. `Gtk.Label` has none, so it is what the chain-up
  tests subclass.
- **An unmapped widget is never snapshotted.** `SnapshotChild` on a widget whose
  window has not been presented returns *no node at all* and does not call
  `OnSnapshot`. A snapshot test that skips `Present ()` therefore asserts
  nothing, quietly.
- **A `[GLib.Property]` setter is an ordinary C# setter.** Assigning it in C#
  emits no `notify`; only a write that goes through GObject does. A property that
  wants to be observable has to call `Notify` itself.

And one thing that cannot be tested at all: a managed `GLib.Object` subclass with
no `(IntPtr)` constructor raises `MissingIntPtrCtorException` **from inside
GObject's constructor callback**. Windows unwinds that back to the caller and it
looks like an ordinary exception; Linux cannot unwind a managed exception through
a native frame, and the test host dies mid-run — under a "Passed!" line, with 54
of 797. The requirement is therefore asserted by reflection in
`Gtk_Builder_constructs_a_managed_type_by_name_and_sets_its_declared_properties`
rather than demonstrated by breaking it.

## Fixed: an evaluation that threw its own answer away

`gtk_expression_evaluate` and `gtk_expression_watch_evaluate` fill a `GValue`
**the caller** provides — the same shape as `graphene_rect_union`,
`cairo_get_font_matrix` and `gdk_content_provider_get_value`. Codegen bound the
`GValue*` as a by-value parameter, which produced:

```csharp
public bool Evaluate (IntPtr this_, GLib.Value value) {
    IntPtr native_value = GLib.Marshaller.StructureToPtrAlloc (value);
    bool ret = gtk_expression_evaluate (Handle, this_, native_value);
    Marshal.FreeHGlobal (native_value);       // <-- the answer was in there
    return ret;
}
```

So every evaluation reported `true` and produced nothing. `GtkExpression` is how
the whole Gtk 4 list stack gets a value out of an item, and `Evaluate` is the
only way to ask an expression for one directly, so nothing in the hierarchy
could be used from managed code at all. Both are now
`Evaluate (… out GLib.Value value)`, initialising the block to `G_VALUE_INIT`
(zeroed, which is what `gtk_expression_evaluate` requires because it
`g_value_init`s the value itself) and copying the filled struct back out.

**This is the fourth caller-allocates defect in this document.** They are all
found the same way: a C function whose out-parameter is a pointer to storage
rather than a return value, bound as though it returned something.

## Fixed: gtk_expression_bind, the last of the receiver-eating calls in Gtk

Predicted by the audit under "the api.xml cannot say a method eats its
receiver", and the same defect: `gtk_expression_bind`'s instance parameter is
`(transfer full)` — the watch it creates owns the expression — so the wrapper
went on holding a reference the callee had already consumed and unreffed it
again from the generated finalizer, on a 50 ms main-loop timeout. `Bind` is
hidden in `GtkSharp.metadata` and rebound in `Source/Libs/GtkSharp/Expression.cs`
over a `Consumed` property that takes the reference first, exactly as
`Gsk.Transform` does. It also now takes `GLib.Object` for its target and
this-object rather than the two bare `gpointer`s the gir declares.

## Fixed: two more array-plus-count constructors, and one that was never emitted

`gtk_closure_expression_new (GType, GClosure *, guint n_params, GtkExpression **params)`
and `gtk_try_expression_new (guint, GtkExpression **)` are the
`gsk_container_node_new` shape again — codegen has a rule for a
NULL-terminated array and none for "pointer plus count" — so each came out
taking a **single** `Gtk.Expression` whose own first machine word GTK then read
as element zero. Both are hidden and rebound over `Gtk.Expression[]`.

`gtk_cclosure_expression_new` is worse: it takes a `GClosureMarshal`, a
`GCallback` and a `GClosureNotify`, none of which `SymbolTable` maps, so
**codegen emitted no constructor for `GtkCClosureExpression` at all** and the
class was left holding nothing but its `GType`. That is the type a Gtk 4 list
view reaches for to derive a display value from an item, so the job it exists
for could not be done through the binding. It is now hand-written over GObject's
libffi marshaller (`g_cclosure_marshal_generic`), taking a managed delegate:

```csharp
[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
delegate int NameLengthCallback (IntPtr this_, IntPtr name, IntPtr userData);

new CClosureExpression (GLib.GType.Int, callback, propertyExpression)
```

The marshaller calls it with the C signature the `GValue`s describe —
`(this_object, param1 … paramN, user_data)` — and the delegate is rooted by a
`GCHandle` passed as that `user_data` and released by the closure's destroy
notify, so it lives exactly as long as the closure does. `ClosureExpression`
gets the same convenience beside its raw `GClosure*` constructor, because
managed code has no other way to obtain a `GClosure`.

## Expressions: behaviour worth knowing

- **A `GtkSorter` is a comparison function, not an observer.** A `StringSorter`
  reads its property through an expression but installs no watch on any item, so
  renaming a row leaves a `SortListModel` in an order that is now wrong and says
  nothing. And the obvious remedy is not one:
  `gtk_sorter_changed (GTK_SORTER_CHANGE_DIFFERENT)` re-runs the sort over the
  sort **keys** the model cached when the item arrived, so it reorders nothing —
  while the same sorter, asked directly with `gtk_sorter_compare`, already gives
  the new answer. What works is telling the *model* the item changed
  (`g_list_model_items_changed`), which is what makes it recompute that key.
  Bind and Watch, by contrast, follow a property immediately.
- **`gtk_expression_bind` is one-way and has no bidirectional mode**, unlike
  `g_object_bind_property`, which is the API it resembles. Writing the target
  neither writes the source nor survives the next evaluation.
- **A failed evaluation leaves the target alone.** A chain through a null link
  is a normal outcome rather than an error, and `Bind` responds by not writing —
  so the failure mode on screen is a stale value, not a blank one. Wrapping the
  chain in a `TryExpression` with a constant is the documented way to get a
  fallback, and is the only difference between the two tests that pin this.
- **A closure expression is not consulted at all when one of its parameters
  fails to evaluate**, so a closure cannot be used to supply that default
  either.
- **`[GLib.Property]` setters are why a binding looks broken.** An ordinary C#
  setter emits no `notify`, so a binding fires once, at `Bind` time, and never
  again — while the property itself reads back correctly the whole time. Writing
  the same property through GObject does notify, which is how the test tells the
  two apart.

## Fixed: a main-loop test that a deferred finalizer could steal

`MainLoopTests.Iterating_the_context_dispatches_one_pending_source` counts a
single dispatch. Every generated `Opaque` finalizer in this binding queues its
unref onto a **50 ms timeout**, and a timeout outranks an idle, so a source
belonging to no test at all could become ready mid-test and take that dispatch.
It failed about one run in twenty-five, always in a test that counts dispatches.
`Drain ()` cannot prevent it, because it only clears what is ready *now*; the
two affected tests now `Quiesce ()` first — run until nothing has been
dispatched for longer than that 50 ms, capped so that a permanently-ready idle
fails the run instead of hanging it.

## Fixed: the error was checked after the return value had been converted

Every generated method that both returns something and takes a `GError **`
emitted this order:

```csharp
IntPtr raw_ret = gdk_clipboard_read_value_finish (Handle, …, out error);
GLib.Value ret = (GLib.Value) Marshal.PtrToStructure (raw_ret, typeof (GLib.Value));
if (error != IntPtr.Zero) throw new GLib.GException (error);   // never reached
```

On failure a C function's return value is undefined and is, in practice, NULL —
and `Marshal.PtrToStructure` raises `NullReferenceException` on NULL. So asking
the clipboard for a type it does not hold, which is an ordinary answer rather
than a fault, produced an exception naming nothing, from a line that had the
real reason sitting in a variable one line below, and leaked the `GError`.

`Method.GenerateBody` now emits the throw **after** the parameter clean-up (so
nothing marshalled for the call leaks) and **before** the return value is
converted. Seven call sites convert through `Marshal.PtrToStructure` and would
have crashed; sixteen more wrap NULL in a `GLib.Bytes` or `GLib.List` and merely
made the wrong object first.

`ByRefGen.FromNative` gained the matching NULL guard, because NULL is not always
a failure: `gtk_drop_target_get_value` returns it whenever no drag is in
progress, which is nearly always, so simply **reading `DropTarget.Value` — the
first thing anyone does writing a drop handler — threw**. It answers
`default (GLib.Value)` now, which is `G_VALUE_INIT`. The guard names the source
twice, so it is only applied when that source is a plain identifier, the same
rule `ManualGen`'s `NullIsNull` guard follows.

## Fixed: two more array-plus-count parameters

The `gsk_container_node_new` family again, this time in the drag-and-drop stack.

- **`gtk_drop_target_set_gtypes (const GType *, gsize)`** came out as
  `SetGtypes (GLib.GType types, ulong n_types)` and passed `types.Val` as the
  address of the array — `G_TYPE_STRING` is 64, so GTK dereferenced address 64 —
  while `gtk_drop_target_get_gtypes` wrapped the array's address in a
  `GLib.GType` and returned a type whose value is a pointer. The constructor
  takes a single type, so this pair is the **only** way to make one drop target
  accept two, and it could not be used at all.

- **`gdk_content_provider_new_union (GdkContentProvider **, gsize)`** came out as
  `ContentProvider (Gdk.ContentProvider providers, ulong n_providers)`, so GDK
  read the provider's own `GTypeInstance` class pointer as element zero. This is
  how a drag source offers one thing several ways — a file as a URI and as an
  image — which is the entire reason the union provider exists. Both the array
  and a reference to each element are `(transfer full)`: the array has to come
  from `g_malloc` because GDK keeps it and `g_free`s it, and the references have
  to be **taken** here rather than surrendered, or the managed wrappers are left
  holding pointers the union has already released.

Both are hidden in the metadata and rebound in
`Source/Libs/GtkSharp/DropTarget.cs` and `Source/Libs/GdkSharp/ContentProvider.cs`.

## Fixed: a selection model that could not be enumerated

`GtkSelectionModel`'s gir says `<prerequisite name="Gio.ListModel"/>`: every
selection model **is** a list model, and the selection interface has no way to
ask what is in it. `GirToGapi` drops prerequisites and the api.xml has nowhere to
put them, so `Gtk.ISelectionModel` derived from `GLib.IWrapper` alone.

That was invisible for as long as the object behind the interface was a bound
concrete type — `Gtk.SingleSelection` and `Adw.ViewStackPages` are generated as
`: GLib.Object, GLib.IListModel, Gtk.ISelectionModel`, so `(GLib.IListModel)` on
them succeeds. But `SelectionModelAdapter.GetObject` falls back to wrapping the
handle whenever the concrete GType is one this binding does not know, and
**`GtkStackPages` is private to GTK and appears in no gir**. So `Gtk.Stack.Pages`
— the only way in Gtk 4 to enumerate a stack's pages, and the object a
`GtkStackSwitcher` is driven from — came back as an adapter that threw
`InvalidCastException` and had no `NItems`, no `GetObject` and no
`items-changed`.

`Source/Libs/GtkSharp/SelectionModelAdapter.cs` adds `GLib.IListModel` to the
partial interface (C# unions the base lists of a partial declaration) and
implements it **explicitly** on the adapter over a `GLib.ListModelAdapter`,
which keeps it clear of the adapter's own static `GetObject` overloads. The
general fix — teaching `GirToGapi` and `GapiCodegen` about interface
prerequisites — is still open; only `SelectionModelAdapter` was in this state.

## Fixed: two signal arguments Signal.Emit could not build

`GLib.Signal.Emit` builds each parameter's `GValue` with `new GLib.Value (arg)`,
which reads the GType off the argument's own managed type. Two ordinary
arguments have no such type:

- **A null object.** `GtkDropTarget::accept` is emitted with a nullable
  `GdkDrop`, and `obj.GetType ()` on null is a `NullReferenceException` thrown
  from inside the constructor. The type now comes from the signal itself —
  `g_signal_query` already returns `param_types`, whose entries carry
  `G_SIGNAL_TYPE_STATIC_SCOPE` in the low bit exactly as `return_type` does and
  have to be masked the same way.

- **A `GLib.Value`.** `GtkDropTarget::drop` declares its payload as
  `G_TYPE_VALUE`, a boxed GValue inside the signal's own GValue. `typeof
  (GLib.Value)` is not a GType at all, so the emission produced a value of no
  usable type, went through, and the handler threw "Unknown type" out of the
  marshaller where nothing can catch it. `GLib.Value.NewBoxedValue` boxes it
  properly, which is what makes **the one signal a drop target exists for**
  reachable from managed code. It is a named static rather than a constructor
  overload because `new Value (someValue)` already binds to `Value (object)` and
  means something else.

`Emit` also checks the argument count against `query.n_params` now and names the
mismatch, rather than handing `g_signal_emitv` a short array.

## Controls and transfer: behaviour worth knowing

Found by assertions that were wrong, and pinned because in each case the
plausible reading produces a wrong answer rather than an error:

- **`gtk_spin_button_spin` reads its `increment` argument for a step and ignores
  it for a page.** `STEP_FORWARD` moves by the *argument* — the adjustment's
  step increment plays no part at all, which makes it identical to
  `USER_DEFINED` — while `PAGE_FORWARD` moves by the adjustment's *page
  increment* and ignores the argument. So a caller who sets a step increment of
  3 and asks for one step gets 1.
- **`gtk_adjustment_set_upper` does not re-clamp the value.** An adjustment
  whose model shrank reports a value its own range no longer contains, and only
  the *next* write — even a write of the same number — brings it back.
  `Configure` clamps; the individual setters do not.
- **An adjustment's reachable maximum is `upper - page_size`.** Code that treats
  `Upper` as the maximum scrolls to a position the adjustment will not take and
  is told nothing.
- **`SnapToTicks` acts when the text is parsed, not when `Value` is assigned**,
  so nothing snaps until `Update ()`. Setting the value and reading it straight
  back suggests the property does nothing.
- **`GtkCalendar:month` counts from zero** (it is `struct tm`'s) while the
  `GDateTime` from `gtk_calendar_get_date` counts from one (it is GLib's).
  Round-tripping a date through a calendar without the conversion moves it a
  month and yields a perfectly plausible answer.
- **`GtkLevelBar::offset-changed` is emitted by *defining* an offset**, not by
  the value crossing one, and removing an offset says nothing at all.
- **Pulsing a progress bar is invisible from managed code.** `Fraction` stays 0
  and emits no `notify`, so there is nothing to bind to and no way to ask
  whether the bar is in pulse mode.
- **A `GtkDropDown` always has something selected.** It wraps its model in a
  `GtkSingleSelection` with autoselect on, so `GTK_INVALID_LIST_POSITION` — the
  value that means "nothing" everywhere else in the list stack — is refused
  without a word, and swapping the model resets the selection to 0.
- **`GtkSearchEntry` delays `::search-changed` by `SearchDelay`, except when the
  entry becomes empty**, which is reported at once. A test that only types never
  sees the asymmetry.
- **A widget's measured size is not stable across this suite.** `AdwaitaTests`
  calls `adw_init`, which replaces the process's stylesheet, so metrics measured
  before and after it differ: a `GtkScale` with an unlabelled mark measures 40
  against a bare 34 on its own and **28** against 34 inside the suite. Only
  comparisons that hold under any stylesheet may be asserted — a label is a line
  of text, so it needs a line of room either way.
- **`GtkStack::transition-running` is a fact about being drawn.** The transition
  runs off the frame clock, which an unmapped widget does not have, so switching
  the visible child of a stack that is not on screen never starts one.
- **A popover's parent is set with `gtk_widget_set_parent`**, not by adding it to
  a container, and its child's parent is not the popover — it is wrapped in the
  popover's own contents box.

## Measuring coverage

Coverage is measured, not chased:

```sh
dotnet test Source/Tests/GtkSharp.Tests --collect:"XPlat Code Coverage"
```

Read the number as a map of what is untested, then write **behavioural** tests
for the parts that matter. Do not write tests to move the number.

**Measure the hand-written layer, not the whole assembly.** Almost everything
under `Generated/` is a property getter or a P/Invoke declaration emitted from a
template, uniform by construction — one round-trip exercises the same emission
path as the thousand like it. The overall figure therefore tracks how many
bindings exist far more than how well they are tested, and it moves when the
api.xml changes. The number worth reading excludes `Generated/` and `Samples`:

```sh
# after collecting, per file, excluding Generated:
python - <<'EOF'
import xml.etree.ElementTree as ET, glob
r = ET.parse(sorted(glob.glob('BuildOutput/Coverage/*/coverage.cobertura.xml'))[-1]).getroot()
for p in r.iter('package'):
    for c in p.iter('class'):
        fn = c.get('filename') or ''
        if 'Generated' in fn or p.get('name') == 'Samples': continue
        lines = list(c.iter('line'))
        if not lines: continue
        hit = sum(1 for l in lines if int(l.get('hits')) > 0)
        print(f'{len(lines)-hit:5d} uncovered  {hit:4d}/{len(lines):4d}  {fn}')
EOF
```

Ranked by *uncovered lines*, that list is a work queue. Every defect found in
§"Fixed" below came off it.

At 1183 tests, measured on Windows (so the two WebKit tests are skipped and
those two assemblies are understated):

| | line rate |
|:--|--:|
| **hand-written (Generated and Samples excluded)** | **62.1%** (13480/21706) |
| overall, including generated | 17.7% (45702/257906) |

Per assembly, hand-written only, ordered by how much hand-written code there is
to cover — which is the ordering that says where the work is:

| assembly | covered / total | line rate |
|:--|--:|--:|
| `GLibSharp` | 6642 / 9656 | 68.8% |
| `GtkSharp` | 2264 / 4590 | 49.3% |
| `CairoSharp` | 2050 / 3204 | 64.0% |
| `PangoSharp` | 764 / 1226 | 62.3% |
| `GdkSharp` | 624 / 1110 | 56.2% |
| `GioSharp` | 372 / 630 | 59.0% |
| `GskSharp` | 190 / 280 | 67.9% |
| `GrapheneSharp` | 186 / 272 | 68.4% |
| `AdwaitaSharp` | 128 / 214 | 59.8% |
| `GtkSourceSharp` | 90 / 180 | 50.0% |
| `WebkitGtkSharp` | 86 / 174 | 49.4% |
| `JavaScriptCoreSharp` | 84 / 170 | 49.4% |

The hand-written totals grow as well as the covered counts, because each sweep
rebinds what it found broken — `GtkSharp` and `PangoSharp` are 588 and 246
hand-written lines larger than when this table was last measured. A rate can
therefore move less than the work behind it suggests, which is another reason to
read the covered/total column rather than the percentage.

`GtkSharp` is the lowest of the large ones and has by far the most hand-written
lines left uncovered — 2326 — which is where the next pass belongs.

`Gtk/SignalConnector.cs` will not move: `ConnectSignals` throws
`NotSupportedException` because Gtk 4 replaced
`gtk_builder_connect_signals_full` with `GtkBuilderScope`, which this binding
does not implement. Its `ConnectFunc` — the reflection that did the wiring — is
now unreachable from anywhere, and stays only as the shape of what a
`GtkBuilderScope` implementation would need.

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

The useful reading is the ordering, not the percentage. When this table was first
written `AdwaitaSharp` stood at zero and `GioSharp` at 2%, and that was real
information: nothing exercised libadwaita at all. Those were worth tests, and
`AdwaitaTests`, `ActionsAndModelsTests` and `SatelliteAssemblyTests` are what
came of it. Raising `GtkSharp` by walking generated properties would not have
been.

Note that most of `Source/Libs/*` is generated, and generated code is uniform by
construction: testing one property round-trip exercises the same emission path
as the thousand others like it. High line coverage over generated code is
therefore close to meaningless. The parts worth testing are the hand-written
layer, the codegen's decisions, and the places where Gtk 4 changed semantics.

## Fixed: two signals called "changed", one args class

Exactly the shape `GMenuModel::items-changed` had, one namespace over.
`GSettings::changed` carries a key name; `GFileMonitor::changed` carries
`(GFile *file, GFile *other_file, GFileMonitorEvent event_type)`. GapiCodegen
names a signal's args class after the signal, so both asked for
`GLib.ChangedArgs`, and only one of them could have it.

GSettings won. A `FileMonitor.Changed` handler was therefore handed an args
object whose single member reads `Args[0]` as a `string` — and `Args[0]` is a
`GFile`. The cast threw `InvalidCastException` from inside the signal
marshaller, and nothing on the args named the file, the event type or the rename
destination. **The one signal a file monitor exists to raise could not be used
for anything.**

The monitor's signal is renamed in `GioSharp.metadata`, so it is
`FileMonitor.FileChanged` with a `FileChangedArgs` carrying `File`, `OtherFile`
and `EventType`. The cname stays `changed`; nothing about what is connected
natively changes.

**Where else to look:** two signals of the same name in one assembly is a
collision by construction, not an accident. The check is a grep for
`typeof (GLib.XArgs)` across an assembly's `Generated/` and a count of the
distinct types that emit each one.

## Fixed: g_settings_schema_source_list_schemas read an array as a string

The function fills two `gchar ***` out-parameters — two NULL-terminated string
arrays the caller owns. `SymbolTable` has no rule for a triple pointer, so
codegen fell back to `out IntPtr` and then ran `PtrToStringGFree` over it: it
read the **array of pointers** as though the first bytes of a heap address were
UTF-8 text, freed the array, and leaked every string in it. It is hidden in the
metadata and rebound over `string[]` in
`Source/Libs/GioSharp/SettingsSchemaSource.cs`.

## Fixed: a property that generated nothing also suppressed its own accessor

`ClassBase.IgnoreMethod` drops a `GetX` method whenever a property called `X`
exists, on the assumption that the property will carry the getter. It does not
always: `Property.Generate` bails out on `!Readable && !Writable`, and GObject
reports `GFileEnumerator:container` as construct-only and write-only. So the
property emitted nothing, the method was suppressed, and
`g_file_enumerator_get_container` — the only way to turn a `GFileInfo` back into
a path without remembering where the walk started — **could not be reached from
managed code at all**.

The early-out now also asks whether the property has a real accessor method
behind it, which is what the `Getter`/`Setter` machinery in `PropertyBase`
exists for. Five properties across the whole tree are in this state, and all
five were inaccessible: `GFileEnumerator:container`,
`GApplicationCommandLine:arguments` and `:platform-data`,
`GSubprocessLauncher:flags`, and `JSCWeakValue:value`.

## Fixed: a NULL GVariant came back as a wrapper around IntPtr.Zero

`g_settings_get_user_value` returns NULL to say "the user has never written this
key" — that is the whole point of the call, and the only way to tell a value
apart from a default. `ManualGen.FromNative` emitted `new GLib.Variant (raw_ret)`
with no guard, so the caller got a live-looking object with `Handle == 0`, which
compares non-null, blows up on use, and made `g_variant_ref_sink` log a CRITICAL
on the way in. 67 GVariant returns and 17 GVariantType returns had the shape.

The guard is **opt-in** rather than blanket, because it is not true of every
manual type: a NULL `GList *` **is** the empty list, and turning that into null
would break every caller that iterates a result. Only `GVariant` and
`GVariantType` declare `NullIsNull`. It also only applies when the source
expression is a plain identifier, since the guard names it twice — `FieldBase`
and `DefaultSignalHandler` pass a call expression and keep the unguarded form.

## Fixed: Dispose released the object before disconnecting its handlers

`GLib.Object.Dispose (true)` did this:

```csharp
tref.Dispose ();                 // g_object_remove_toggle_ref -> may finalize
foreach (var sig in signals.Keys)
        signals[sig].Free ();    // g_signal_handler_is_connected (raw_ptr, id)
```

`SignalClosure` keeps its own copy of the raw GObject pointer. When the toggle
ref held the last reference — the normal case for an object the program made and
then disposed — the GObject was finalized inside `tref.Dispose ()`, and the
disconnect that followed read freed memory. The finalizer branch ten lines below
already had the order right (`QueueSignalFree ()` then `tref.QueueUnref ()`); the
disposing branch is now the same way round.

This was found while chasing the monitor crash below, and it is **not** what
caused it. It has no test of its own, because making it fail on demand needs the
GObject's freed memory to be reused between the two calls; it is kept because
the two branches of one method disagreeing about ordering is a defect whichever
way the race happens to fall.

## Fixed: GLib.FileFactory leaked every GFile it made

`g_file_new_for_path`, `_for_uri` and `_for_commandline_arg` all return a new
reference. `FileFactory` passed `owned: false` to `FileAdapter.GetObject`, which
then took a *second* one, so nothing created through this class was ever freed —
and this class is how the samples, the tests and the documentation all make a
`GFile`. The generated `GLib.File.NewForPath` sitting beside it passes `true`,
which is the authority for what the ownership is.

## Open: unreffing a cancelled GFileMonitor corrupts the heap on Windows

Reproducible outside the test host, in about four runs in five:

```csharp
var monitor = GLib.FileFactory.NewForPath (dir)
              .MonitorDirectory (GLib.FileMonitorFlags.None, null);
monitor.Cancel ();
/* iterate the main loop for ~1 s */
monitor.Dispose ();          // exit code 0xC0000374, STATUS_HEAP_CORRUPTION
```

All three parts are needed: without the `Cancel ()`, or without the main-loop
iterations in between, twenty rounds run clean. No signal handler need be
attached, and it is a heap corruption rather than an access violation, which
points at glib's win32 backend freeing a buffer an outstanding
`ReadDirectoryChangesW` still owns rather than at anything in this binding. It
does not reproduce on Linux's inotify backend.

`GioDeepTests`' two monitor tests therefore do not dispose their monitor; the
wrapper is collected instead, which defers the unref onto a main-loop timeout,
and is what every other test here does anyway. The comment in the test says so,
so that nobody "tidies up" by adding a `using`.

It is also a reminder that the exit code is worth reading. `dotnet test` prints
"Test host process crashed" for every one of these, and the diagnostic log has
nothing in it; running the same code standalone and asking Windows for the
process exit code separated a heap corruption (`0xC0000374`) from the access
violations the rest of this document is about in one step.

## Gio: behaviour worth knowing

- **`g_file_copy`'s progress callback is called a platform-dependent number of
  times.** On Linux a local-to-local copy is one `copy_file_range` and reports
  **once**; the fallback path Windows takes reports per buffer. What holds on
  both is that every report carries the same total and the last one has
  `current == total`. An assertion that a megabyte takes more than one buffer
  passes on Windows and fails on Linux.
- **Cancelling from that progress callback therefore does not reliably cancel
  the copy.** With no loop there is no place to check the cancellable, so on
  Linux the copy finishes and reports success. An in-flight cancellation test
  needs an operation that is genuinely iterative — a `GFileEnumerator` walk is
  the portable one.
- **A cancelled async operation still calls back.** Skipping the `Finish` call
  because "it was cancelled anyway" leaks the `GTask` every time; the callback
  runs and `Finish` raises `G_IO_ERROR_CANCELLED`.
- **A Gio async callback runs on the thread that started the operation**, via
  its thread-default main context — which is what makes the pattern usable from
  a Gtk program at all, and is worth an explicit assertion rather than an
  assumption.
- **A `GFileInfo` only carries the attributes that were asked for**, and reading
  one that was not is not an error: `standard::size` off an info queried for
  `standard::name` is **0**, which looks exactly like an empty file.
- **`g_settings_reset` deletes the key rather than writing the default back**,
  which is what lets a later change of default reach the user. In a keyfile
  backend the line disappears from the file.
- **A `GSettingsSchemaKey`'s range is `(sv)`** — the word `"range"` and a
  *boxed* variant holding `(min, max)`. Reading the second child as the tuple
  gives one child, not two.
- **An interface adapter is a fresh wrapper every time.**
  `FileAdapter.GetObject` does not cache the way `GLib.Object.GetObject` does,
  so two lookups of one `GFile` are not reference-equal; `g_file_equal` is how
  to compare them.
- **`glib-compile-schemas` is not on `PATH` on Debian.** `libglib2.0-0` puts it
  in `/usr/lib/<triplet>/glib-2.0/`, and only `libglib2.0-dev-bin` — which
  nothing in the Gtk dependency chain pulls in — installs the copy in
  `/usr/bin`. A test that needs it has to look in the multiarch directory or it
  will skip on the reference container while passing on Windows, where gvsbuild
  ships it in `bin/`.

## Fixed: the two things graphene's gir says that nothing else's does

Graphene had 52% coverage and no defects on record, which turned out to mean
that almost none of it existed.

**Its predicates returned a type the symbol table did not know.** Graphene's
headers include `<stdbool.h>`, so its gir says
`<type name="gboolean" c:type="bool"/>` where every other library in the tree
says `gboolean`. `GirToGapi` writes the C type through, `SymbolTable` had no
entry for `bool`, and codegen drops a method whose return type does not
resolve — **silently, and all 49 of them**:

| gone | what could not be done |
|:--|:--|
| `graphene_matrix_inverse`, `graphene_matrix_decompose` | undo or take apart a transform |
| `graphene_matrix_is_2d`, `_is_identity`, `_is_singular` | ask a matrix anything |
| `graphene_rect_contains_point`, `_contains_rect`, `_intersection` | hit-test or clip |
| `graphene_box_intersection`, `_contains_point`, `_contains_box` | the same in 3D |
| `graphene_ray_intersects_*`, `graphene_frustum_intersects_*` | picking, culling |
| every `_equal` and `_near`, on every type | compare two values |

`CBoolGen` maps it. C99's `bool` is **one byte** and only the low byte of the
return register is architecturally defined, so it marshals as a `byte` and is
compared against zero rather than being handed to the runtime as a `bool`,
whose default marshalling reads the four bytes a `gboolean` occupies. gcc and
clang zero-extend; MSVC does not promise to, and gvsbuild is MSVC.

**And a fixed-size array parameter came out as one element.** `CTypeMapper`
drops `float v[16]` to its element type, which is right for a *field* — the
`array_len` attribute carries the count — and wrong for a parameter, where it
left the binding passing a single `float` in a vector register to a callee that
writes sixty-four bytes through a pointer register nobody set. Twenty-one
parameters, across four assemblies:

- `graphene_matrix_to_float` / `_init_from_float` — reading a matrix's sixteen
  elements out, and building one from them, which is the most basic thing there
  is to do with a matrix.
- `graphene_vec2/3/4_to_float` / `_init_from_float`,
  `graphene_triangle_init_from_float`.
- `gsk_border_node_new` and `gtk_snapshot_append_border` — `const float [4]`
  plus `const GdkRGBA [4]`, so GSK read four colours out of one.
- `gdk_texture_downloader_download_bytes_with_planes` — two `gsize [4]`
  out-buffers filled through scalars.

`ArrayParameter` already had a `FixedArrayLength` field and nothing working
behind it: it emitted an allocation for a by-value parameter and passed the
array as `out T[]`, which is a `T**`. It now sizes and pins the buffer for a
callee-filled one, **checks the length** of a caller-supplied one — C cannot,
because there is no count argument — and never passes an array as an `out`.

Four of the twenty-one are N boxed structs end to end, which no attribute can
express: every graphene type is bound as a class, so a `Vec3[]` marshals as an
array of addresses rather than as 8 x 16 bytes. `Parameters.Validate` drops
those with a warning instead of emitting the overrun, and
`GrapheneSharp/FixedVertexArrays.cs` binds `Rect.GetVertices`,
`Box.GetVertices`, `Frustum.GetPlanes` and `Quad.InitFromPoints` by hand over
contiguous buffers.

## Fixed: graphene's SIMD vector is aligned and the ABI description could not see it

`graphene_simd4f_t` is `__m128` on any SIMD build, so it aligns to 16. Its
managed replica is four plain floats and aligns to 4, and `AbiStruct` measures a
field's alignment by asking the runtime where the replica lands after a leading
`sbyte` — so **everything embedding it was measured short**:

| | abi_info said | C |
|:--|--:|--:|
| `graphene_plane_t` `{ vec3; float }` | 20 | 32 |
| `graphene_euler_t` `{ vec3; enum }` | 20 | 32 |
| `graphene_sphere_t` `{ vec3; float }` | 20 | 32 |
| `graphene_frustum_t` `{ plane[6] }` | 120 | 192 |

That number is what every caller-allocates out parameter of those types
allocates before handing the pointer to graphene, so `Plane.Negate`,
`Plane.Normalize`, `Plane.Transform`, `Triangle.Plane`, `Box.BoundingSphere`,
`Sphere.Translate` and `Euler.Reorder` each let it write twelve bytes past the
end. Reading a frustum's planes gave four plausible ones and two assembled out
of denormal floats, because the stride was twelve short — the four that looked
right are how it survived.

The measurements come from graphene: writing a plane with a marked constant into
a zeroed block puts the constant at offset **16**, not 12, and the six planes of
a 60-degree perspective frustum land **32 bytes** apart.
`GenBase.GenerateAlign` now honours an `align` attribute on a `<struct>`, and
`GrapheneSharp.metadata` states it once, for `graphene_simd4f_t`. Every other
graphene type's size was already right and stays right.

## Fixed: freeing a caller-allocated block with the wrong allocator

The worst of the four, and the one with no symptom at the call site.

A caller-allocates out parameter is storage this side provides and the wrapper
then *owns*, so it ends up at the type's own free function. **Those do not all
free what `g_malloc` allocates.** Graphene allocates everything holding a SIMD
vector with `graphene_aligned_alloc` — `_aligned_malloc` where the compiler has
it — and frees it with `_aligned_free`, which cannot be given a `g_malloc`
pointer. `Matrix.Multiply`, `Matrix.Inverse`, `Matrix.Transpose`, `Vec3.Cross`,
`Vec3.Normalize`, `Plane.Negate` and two dozen others handed it one every time.

Nothing happens at the call. The generated `Opaque` finalizer queues its free
onto a **50 ms main-loop timeout**, so the damage lands whenever the GC ran and
the loop turned — in a test run that is some unrelated test, as a bare "test
host process crashed" with an empty diagnostic log and a total that moves
between runs. Asking Windows for the exit code is what named it:
`0xC0000374`, `STATUS_HEAP_CORRUPTION`. Reproducing it needs all three of
allocate, collect and pump; twenty rounds of that fail every time and a single
call never does.

`graphene_rect_t` is the exception that let 871 tests pass over this: it needs
no alignment, so its allocator is `calloc` and its free is `free`. Nothing in
the suite had ever allocated a graphene *matrix* or *vector* through this path.

`GLib.Opaque.AllocateNative` now takes the block from the type's own allocator —
its parameterless constructor (`new Graphene.Vec3 ()` *is*
`graphene_vec3_alloc`) or its static `Alloc` — and falls back to the zeroed
`g_malloc` that all of these used to get for the two types that have neither,
`Gtk.BitsetIter` and `Gsk.PathPoint`. The factory is resolved once per type and
cached, because this is what every matrix multiply allocates.
`Pango.GlyphString` was in the same state and is fixed by the same change.

**The rule: whatever will free the block has to have allocated it.** The size
being right is not enough.

Caller-allocated buffers are also `g_malloc0`'d rather than `g_malloc`'d now,
because a callee does not always write the whole struct — see
`graphene_sphere_translate` below. Zeroing does not make the answer right; it
makes it the same every time, which is the difference between a defect a test
can pin and one that looks like a flake.

## Graphene: behaviour worth knowing

Four of these were found by an assertion that turned out to be wrong, and every
one of them is a plausible assumption that produces silently wrong results.

- **`graphene_matrix_determinant` returns the *negative* determinant.** The
  identity's is `-1`. The magnitude is right, so it survives every test that
  squares it or compares it against zero — and the sign is the one thing a
  determinant is actually used for, since a negative one means the handedness
  was flipped. A renderer reversing its winding order on `determinant < 0` does
  it on exactly the wrong matrices. `IsSingular` is unaffected: zero has no
  sign.
- **`graphene_euler_reorder` does not preserve the rotation.** It reads like a
  change of spelling and is documented as one. Reordering `Sxyz(10,20,30)` into
  `Rzyx` gives the angles the textbook identity predicts — `(30,20,10)` — and
  then graphene's own `to_matrix` reads them back as a different rotation, the
  3x3 anti-transpose. Of the thirty-one orders only the ones that are `Sxyz`
  under another name survive the round trip. Normalising a set of euler angles
  into a preferred order silently rotates the object.
- **`graphene_matrix_interpolate` is not exact at its endpoints.** It does not
  blend the sixteen elements: it decomposes both matrices into translation,
  scale, shear, perspective and a quaternion, blends those and multiplies a
  fresh matrix out. Factor 0 therefore does *not* return the source — it returns
  it carrying about 2.4e-4 of rotation that was never there. Quaternion slerp,
  which has no decomposition in the way, *is* exact at 0 and within a couple of
  ulp at 1.
- **`graphene_sphere_translate` never copies the radius.** It assigns the centre
  and leaves the rest of the struct as it found it, so the moved sphere comes
  back with whatever radius the allocator had lying there — 1.49e15 on one run
  before caller-allocated buffers were zeroed, 0 afterwards, and `IsEmpty` then
  says the sphere has vanished.
- **`a.Multiply (b)` applies `a` first.** The documentation says "multiplies a
  by b", which reads like the mathematical product *AB* and is the other way
  round from what happens to a point. It only shows up where the operations do
  not commute, which a scale and a translate do not.
- **`Inset`, `Offset` and `Normalize` change the rectangle they are called on;
  `InsetR`, `OffsetR` and `NormalizeR` do not.** graphene spells the in-place
  operation without a suffix, which is the reverse of what a C# caller expects
  from a method that returns a value: `var smaller = rect.Inset (1, 1);`
  compiles, reads like a pure function, and shrinks the rectangle the caller
  still holds.
- **`graphene_rect_contains_point` is inclusive on all four edges**, so two
  rectangles laid side by side both contain the point where they meet.
  Hit-testing adjacent regions by asking each in turn has no unique answer on a
  boundary; the order of the walk decides it. `graphene_box_contains_point` is
  the same.
- **A failed `Intersection` still fills its out parameter.** It is a zeroed
  rectangle, not null, so the `bool` is the only thing separating "no overlap"
  from "an empty rectangle at the origin" — and a caller who tests the result
  for null concludes that everything intersects.
- **`graphene_triangle_get_barycoords` hands back the *third* and *second*
  weights.** `res.x` is the weight of **c** and `res.y` the weight of **b**,
  with a's left implicit as `1 - x - y`. So vertex `a` comes back as `(0,0)`,
  `b` as `(0,1)` and `c` as `(1,0)`, and code that reconstructs a point from
  them swaps two corners with no error.
- **`Euler.Alpha`, `Beta` and `Gamma` follow the rotation order, not x, y, z.**
  They are the first, second and third rotations applied, in whatever order the
  angle carries — for `Sxyz` they coincide with X, Y and Z, which is exactly why
  reading alpha as "the x angle" survives testing, and for `Ryxz` alpha is the
  *y* angle. They are also in radians while `X`, `Y` and `Z` are in degrees.
- **`Matrix.IsIdentity` and `Quaternion.Equal` are exact comparisons**, so
  neither is a question to ask about a value that has been through arithmetic. A
  scale times its own inverse is the identity to the last bit under gvsbuild and
  a few ulp away from it on Debian — same graphene, different compiler and
  vector unit — so neither answer is a fact about the binding. `Near`, and for
  quaternions the dot product, are the comparisons that mean something. The same
  goes for how far `graphene_matrix_interpolate`'s recomposition lands from the
  endpoint: 4.9e-3 under gvsbuild, 6.9e-3 under Debian's build.
- **`GskTransform`'s category records how it was built, not what it does.**
  Handing it a matrix that is a plain scale gives `Unknown`, so a renderer's
  fast path for an affine transform is missed while the matrix itself reads
  back element for element.
- **A `GskTransform` chain applies its *last* operation to a point first**, the
  way a CSS transform list does and the opposite of the order the calls are
  written in.

## Fixed: a boxed signal argument that died with the emission

`GLib.Value`'s conversion to `GLib.Opaque` wrapped whatever
`g_value_get_boxed` returned, without copying:

```csharp
public static explicit operator GLib.Opaque (Value val)
{
	return GLib.Opaque.GetOpaque (g_value_get_boxed (ref val), (Type) new GType (val.type), false);
}
```

That pointer belongs to the **GValue**, and `g_value_unset` frees it. So the
wrapper is alive exactly as long as the value is — until the end of a signal
emission, or until the generated property getter three lines below it calls
`Dispose` — and nothing about the object the caller is holding says so.

`GtkTreeView::row-activated` declares its `GtkTreePath` **without**
`G_SIGNAL_TYPE_STATIC_SCOPE`, so `g_signal_emit` copies the path into the
emission's `GValue`: the pointer the handler is handed is measurably *not* the
one the caller passed to `gtk_tree_view_row_activated`, and it is freed the
moment the emission ends. Keeping `args.Path` — which is what an application
does when it remembers the activated row — therefore read freed memory. The
symptom was the one this document keeps returning to: an
`AccessViolationException` out of `Gtk.TreePath.ToString`, landing on whatever
test happened to be running when the allocator handed the block out again, and a
run that reports "Passed!" with a truncated total. It reproduced about one run
in three; the rest of the time the freed block still held plausible bytes.

The fix has to be conditional, because **53 opaque types in the tree override
the `Ref` hook**. For those, `Opaque (IntPtr)` already takes a reference of its
own through the `Raw` setter, so the wrapper outlives the value and copying as
well would leak. For the rest — `Gtk.TreePath`, `Pango.FontDescription`,
`Gtk.PaperSize` — the wrapper is a bare alias and the only way for it to survive
is `g_boxed_copy`. `GLib.Opaque.WrappingTakesAReference` answers which, by
asking whether the type declares `Ref (IntPtr)` itself, cached because this is
on the path every boxed signal argument and every boxed property getter takes.

`GLib.Opaque.GetOpaque (o, type, owned: false)` is *supposed* to make a copy —
that is what its `else` branch calls `Copy` for — but `OpaqueGen`'s `Copy`
override has been inside `#if false` since the mono era, so the default
`Copy` returns `this` and the promise has never been kept for any opaque type.
Fixing that generally is still open; it would change every transfer-none opaque
return in eleven assemblies at once.

`An_activated_rows_path_outlives_the_emission_that_carried_it` pins it, and
allocates 256 tree paths between the emission and the read so that a regression
**fails** rather than passing on luck: without the fix the captured path reads
back as `7:7:7:7`, the churn's value, rather than crashing some tests later.

## The tree view: behaviour worth knowing

- **`gtk_tree_view_expand_to_path` opens the row it names, not just its
  ancestors.** It walks depths 1 to *depth* inclusive, so a row that has
  children of its own ends up showing them — one level more than "expand to"
  suggests.
- **A `CellArea` reports its renderers in the order they were added, not the
  order they are laid out.** A renderer packed at the end still comes back where
  it was added, so `Cells` and `Foreach` are no guide to what is drawn where.
- **`gtk_cell_area_foreach` stops when the callback returns TRUE**, which is the
  reverse of the "keep going" convention most callbacks in this stack follow.
- **A `GtkCellRendererToggle` does not toggle itself.** Activating it emits
  `toggled` with the row's path and leaves `active` exactly as it was; a handler
  that assumes the renderer already flipped writes the old value back into the
  model.
- **`GtkCellRendererPixbuf:pixbuf` is write-only in Gtk 4** (`readable="0"` in
  the gir), so the binding emits a setter and no getter and the image has to be
  read back through `:texture`. The image properties are one slot: setting any
  of them clears the others.
- **A cell data func runs after the attribute mapping and overrides it**, and
  clearing the func hands the column back to its attributes. `ClearAttributes`
  leaves the renderer packed, so the cell keeps whatever it was last given
  rather than being reset — a column that has gone stale on screen is what that
  produces.

## Fixed: eleven Pango arrays whose length is a separate argument

The `gsk_container_node_new` family again, and the largest single instance of it
in the tree. Codegen has a rule for a NULL-terminated array and none for
"pointer plus count" — or for "one entry per character of the text", which is
what Pango's break and shaping functions take — so each of these came out taking
or returning a **single value**, and Pango wrote the rest past the end of it:

| function | what it came out as | what happened |
|:--|:--|:--|
| `pango_get_log_attrs` | `PangoLogAttr attrs, int attrs_len` | one four-byte struct marshalled, `attrs_len` of them written through it |
| `pango_default_break`, `pango_break`, `pango_tailor_break` | the same | the same |
| `pango_glyph_string_get_logical_widths` | `out int` | one width per character written through a four-byte stack slot |
| `pango_glyph_item_get_logical_widths` | `out int` | as above, `item->num_chars` of them |
| `pango_glyph_string_index_to_x_full` | `PangoLogAttr attrs` | read past the end of one struct for any index beyond the first cluster |
| `pango_glyph_item_letter_space` | `PangoLogAttr log_attrs` | as above |
| `pango_language_get_scripts` | `PangoScript` | the low 32 bits of the array's address returned as a script |
| `pango_font_face_list_sizes` | `out int` | an eight-byte address written through a four-byte slot |
| `pango_log2vis_get_embedding_levels` | `byte` | the low eight bits of the array's address returned as a level, array leaked |
| `pango_coverage_from_bytes` | `out byte` | the *input* array bound as an out-parameter, so the one thing the caller had to supply could not be supplied |

`pango_default_break` is the one that explains why none of this had ever
surfaced: its `attrs_len` argument is `G_GNUC_UNUSED`, so the overrun is silent
and the first entries are even correct.

All eleven are `hidden` in `PangoSharp.metadata` and rebound over real arrays in
`Source/Libs/PangoSharp/{Global,GlyphString,GlyphItem,Language,FontFace,Coverage}.cs`,
with the length computed **by the binding** from the text — `g_utf8_strlen`'s
count, not `string.Length`, because a character outside the BMP is two UTF-16
units and one code point.

`PangoGlyphString`'s two array *fields*, `glyphs` and `log_clusters`, were
already hidden with nothing in their place, so the glyphs a shaping run produced
and the character each one came from could not be reached from managed code at
all. They are properties over `abi_info`'s offsets now, sized by `num_glyphs`.

## Fixed: a field holding a struct by value, read as a pointer to one

`FieldBase`'s ABI-offset accessor reads the machine word at the field's offset
and hands it to `FromNative`. For a `T *` field that is right. For a struct
embedded **by value** the offset *is* the address, and reading the first word of
the struct as though it were the address of the struct is not:

```csharp
IntPtr* raw_ptr = (IntPtr*)(((byte*)Handle) + abi_info.GetFieldOffset ("analysis"));
return Pango.Analysis.New ((*raw_ptr));       // *raw_ptr is analysis.shape_engine
```

`PangoAnalysis` begins with the two deprecated engine pointers, which are always
NULL, and `StructBase.FromNative` maps NULL to `Zero` — so **every `PangoItem`
reported a zeroed analysis**: script `Common`, no language, no font, bidi level
0, and no error anywhere. Shaping still worked, because `pango_shape` is handed
the analysis straight back and never looks at the managed copy; only a caller
*reading* it saw nothing, and reading it is how a caller finds out what script
or direction the itemizer decided on.

`FieldBase.IsEmbeddedStruct` now takes the offset as the address in both the
getter and the setter. **Regenerating all eleven assemblies changes exactly one
field** — `Pango.Item.analysis` is the only struct-by-value field with a public
accessor in the tree — which is the check to re-run before touching this: the
generic path is the right one for every other field, and a wrong
`IsEmbeddedStruct` would silently turn a pointer field into a garbage read.

`An_items_analysis_reports_the_script_language_and_bidi_level` is the test; the
whole shaping half of `PangoShapingTests` fails without the fix, because
`Pango.Global.Shape (text, item.Analysis)` would be handed a zeroed analysis.

## Fixed: Pango.Attribute destroyed whatever it was handed

`Pango.Attribute` is hand-written, wraps a bare `PangoAttribute *`, and its
finalizer called `pango_attribute_destroy` unconditionally. A `PangoAttribute *`
arriving from C says nothing about who owns it, and four of the five places one
arrives are **borrowed**:

- the attribute a `PangoAttrFilterFunc` is handed — which a `PangoAttrList` is
  about to move into the list `Filter` returns, or to keep;
- the attribute a `PangoShapeRendererFunc` and `PangoRenderer::draw_shape` are
  handed;
- `pango_attr_iterator_get`'s return, which belongs to the list;
- the attributes hanging off a `PangoAnalysis`.

So each of those was freed under the list that still owned it, and the *second*
free landed wherever the allocator handed the block out again. `AttrList.Filter`
reproduces it every time.

Borrowed is therefore the default and the transfer-full callers ask —
`pango_attr_iterator_get_attrs` and `get_font`'s extra attributes, both of which
are documented as needing `pango_attribute_destroy` per item, and
`pango_attribute_copy`. `pango_attr_font_features_new` is the one generated
function that allocates, so it is hidden and rebound in `AttrFontFeatures.cs`;
the other four call sites of the manual symbol's `from_fmt` are all borrowed.

`GetAttribute (IntPtr.Zero)` also returned a live-looking wrapper whose `Type`
read `Invalid` and whose `StartIndex` read address zero. NULL is how
`pango_attr_iterator_get` says "no attribute of that kind here", so the null
check every caller writes never fired. It returns `null` now.

## Fixed: a static field that every caller could rotate

`Pango.Matrix.Identity` was a **static field**, and every `PangoMatrix`
operation mutates in place. `Pango.Matrix.Identity.Rotate (90)` compiles, reads
like arithmetic on a constant, and leaves the identity permanently rotated for
every other caller in the process. It is a get-only property handing back a
fresh value now, so the mutation lands on the temporary.
`The_identity_matrix_survives_being_rotated_where_it_stands` pins it.

## Pango: behaviour worth knowing

- **A font's coverage is read-only on the fontconfig backend.**
  `pango_coverage_set` is a vfunc and `PangoFcCoverage` overrides it with an
  empty body, so a `Set`/`Get` round trip on the coverage
  `pango_font_get_coverage` returns succeeds under gvsbuild's win32 backend and
  silently does nothing on Debian. Which of the two happens is a fact about the
  host; the round trip belongs on a coverage the caller made with
  `pango_coverage_new`, which is Pango's own class either way.
- **Coverage serialisation is inert, not broken.** Pango 1.44 reimplemented
  coverage over `hb_set`: `pango_coverage_to_bytes` writes NULL and 0, and
  `pango_coverage_from_bytes` returns NULL for any input. The binding has to
  guard, because `Marshal.Copy` rejects a null source whatever the length —
  otherwise "nothing to serialise" arrives as an `ArgumentNullException`. 1.44
  also folded every level other than `NONE` into `EXACT`, so asking for
  `APPROXIMATE` and reading back `EXACT` is the answer rather than a fault.
- **Face names are not unique within a family.** `pango_font_family_get_face` is
  a linear search that stops at the first match, so on a machine carrying a
  family with two faces called "Thin" — this one does — the third and fourth
  faces cannot be looked up at all. A test that asserted `GetFace (f.FaceName)`
  is `f` for every face was one font install away from failing, and the order
  `list_families` returns is the order the platform enumerated its fonts, so
  indexing into it asserts something about the machine too.
- **`pango_layout_move_cursor_visually` reports running off the layout with two
  different sentinels**: `-1` at the beginning and `G_MAXINT` at the end.
  Neither is a byte offset, and a loop written as `while (index >= 0)` therefore
  does not terminate going forwards — it feeds `G_MAXINT` back in for ever. That
  is what the test that pins it was written as first, and it hung.
- **A layout's line box is not `ascent + descent`.** Measured at Sans 12 it is
  21504 against 19776, because the line box is rounded up to whole pixels while
  the context's metrics are not — and how much hinting rounds is a property of
  the backend. What holds everywhere is proportion: the same family at twice the
  size gives twice the ascent, twice the descent and twice the line.
- **`pango_attribute_equal` ignores the range.** It compares the value, because
  it is what an attr list uses to decide two runs can be merged — so "bold here"
  and "bold there" are equal, and code that de-duplicates attributes with it
  loses every range but the first.
- **`pango_glyph_item_letter_space` puts the space between clusters, not around
  them.** *n* clusters grow the run by *n-1* spacings and a one-letter run does
  not grow at all, so text set with letter spacing measures narrower than
  "characters times spacing" predicts. Its two array arguments are also indexed
  differently and neither says so: `text` is the whole paragraph, while
  `log_attrs` starts at *this item's* first character. The same split runs
  through the two logical-width calls — a glyph string is given only the text it
  shaped, a glyph item the whole paragraph and its own `Item.Offset`.
- **The layout iterator hands back a null run once it has passed the last one**,
  and the wrapper turns that into a zeroed `GlyphItem` rather than into null, so
  a `do … while (NextRun ())` loop reads `Item` and gets nothing on its last
  turn. `run.Item == null` is the test.
- **`PangoLanguage` values are interned**, so `Language.FromString ("en-gb")`
  and `("EN-GB")` are the same pointer — which is what lets the itemizer compare
  them by pointer. A language Pango has no table entry for answers **every**
  script to `IncludesScript`, because the empty script list means "unknown"
  rather than "none": a caller filtering fonts by script gets everything through
  and nothing looks wrong.
- **A tab whose decimal point was never set reports U+0000**, not `'.'`, so
  reading it as a character and printing it produces a NUL.

## Fixed: a file chooser that still spoke Gtk 3's filenames

Two metadata rules in `GtkSharp.metadata` retyped `GtkFileChooser`'s folders as
filenames:

```xml
<attr path="…/method[@name='GetCurrentFolder']/return-type" name="type">gfilename*</attr>
<attr path="…/method/parameters/*[@name='folder']" name="type">const-gfilename*</attr>
```

That was true of **Gtk 3**, where a chooser spoke in paths. Gtk 4 takes and
returns `GFile *`, and nothing failed when the API changed underneath, because a
pointer is a pointer:

- `IFileChooser.CurrentFolder` came out as a `string`, so the getter took the
  `GFile *` that `gtk_file_chooser_get_current_folder` hands back, read the
  object's memory as a NUL-terminated string, and then **`g_free`d the GObject**
  — the return value is transfer-full, so the binding "owned" it. An application
  that set a folder and read it back corrupted the heap.
- `AddShortcutFolder (string)` and `RemoveShortcutFolder (string)` marshalled a
  `char *` into a parameter Gtk dereferences as a `GFile *`.

The rules are deleted; the interface now says `GLib.IFile` in all three places.
The half of the pair that was already right — `SetCurrentFolder (GLib.IFile)`,
whose parameter is named `file` rather than `folder` — is what made the mismatch
survive: setting worked, so only a program that read back was hurt.

Worth knowing while testing it: a `GtkFileChooserWidget` loads its folder
through the main loop, so `CurrentFolder` is **null** until the loop has turned.
A test that reads it straight after setting it concludes the getter is broken.

## Fixed: page ranges nobody could read past the first

`gtk_print_settings_get_page_ranges` returns a `GtkPageRange *` array plus a
count, transfer full; `gtk_print_settings_set_page_ranges` takes the same pair.
The api.xml has no way to say "array whose length is that other argument", so
codegen bound **both** over a single `GtkPageRange`:

- the getter marshalled the first element and leaked the rest of the `g_malloc`
  block on every call;
- the setter marshalled one struct and told Gtk to read `num_ranges` of them.

"Pages 1-3, 6 and 10-12" is the ordinary thing to type into a print dialog, and
only the first range ever arrived. Both are hidden in the metadata and rebound
over real arrays in `Source/Libs/GtkSharp/PrintSettings.cs`, the same shape as
the eleven Pango array parameters above.

## Fixed: the dialog that replaced GtkMessageDialog had no constructor

`GtkAlertDialog`'s only C constructor is
`gtk_alert_dialog_new (const char *format, ...)`, and codegen emits nothing for
an ellipsis. With `ctors.Count == 0`, `ObjectGen` falls back to the **protected**
void constructor it gives an abstract base class like `GtkFilter` — so
`new Gtk.AlertDialog ()` did not compile for anyone outside the assembly, and
the type Gtk 4 offers in place of `GtkMessageDialog` could not be used at all.

`disable_void_ctor="1"` turns the fallback off and `AlertDialog.cs` writes the
constructors by hand. They go through `g_object_new` rather than the varargs
entry point on purpose: `gtk_alert_dialog_new` runs its first argument through
`g_strdup_vprintf`, so binding it directly would make
`new AlertDialog ("Copied 50% of the files")` undefined behaviour — the hazard
`Gtk.MessageDialog` still carries.

## The desktop dialogs and the print stack: behaviour worth knowing

- **Rotating a sheet changes which margins bound the page.** A margin belongs to
  the sheet and never moves — `GetLeftMargin` is 15mm in every orientation — but
  `gtk_page_setup_get_page_width` subtracts *left and right* in portrait and
  *top and bottom* in landscape. So the formula the accessor names invite,
  `paper width − left − right`, is silently wrong in landscape by the difference
  between the two pairs. A test whose four margins are chosen so that
  left+right equals top+bottom cannot tell the two apart; the one here uses four
  different numbers.
- **`GtkFileDialog.InitialFile` does not round-trip.** `set_initial_file` is
  documented as a shortcut for `set_initial_folder` + `set_initial_name`, and
  that is all it is: it stores nothing of its own, so reading the property back
  returns **null** while the other two hold the answer.
- **The 4.10 dialog family does not agree on what a cancel is.**
  `GtkAlertDialog` answers with `G_IO_ERROR_CANCELLED` (19);
  `GtkFileDialog` and `GtkColorDialog` answer with `GTK_DIALOG_ERROR_CANCELLED`,
  a different domain whose code is **1**. Comparing the code without the domain
  mistakes the second for `G_IO_ERROR_NOT_FOUND`.
- **A content type is not a mime type.** `gtk_file_filter_add_mime_type` stores
  `g_content_type_from_mime_type` of what it was given, and matching compares
  content types — which are the mime strings themselves on Linux and registry
  entries like `".png"` on Windows. Putting a mime type straight into a
  `GFileInfo`'s `standard::content-type` therefore matches on one platform and
  not the other; converting on both sides, as Gtk does internally, is portable.
  The same asymmetry makes a **mime rule lossy through a `GVariant`**:
  `to_gvariant` writes the stored *content* type and `new_from_gvariant` feeds it
  back to `add_mime_type`, which converts again — on Windows the rule comes back
  as `"*"` and matches everything.
- **`gtk_paper_size_is_equal` is a `strcmp` on the names.** A custom sheet cut to
  exactly 210×297mm is not equal to `iso_a4`, and two independently constructed
  A4s are.
- **A standard paper size is written to a key file under its *PPD* name**, and
  its own name is left out entirely — there is no `iso_a5` anywhere in the file,
  only `PPDName=A5`, and the name is recovered from Gtk's table on the way back
  in.
- **A print settings paper *format* and paper *width* are independent keys.**
  Naming a standard sheet records the name and nothing else, so
  `GetPaperWidth` answers **0** right after `PaperSize` was assigned. Only
  `PaperSize` knows how to look a name up.
- **`set_resolution_xy (300, 1200)` leaves the plain `resolution` key on the
  horizontal one**, so reading `Resolution` back gives 300 — not 1200, and not
  an average.
- **An exported print operation never reaches `Finished`.** That status comes
  from a print backend watching a spooled job and there is no backend behind
  `GTK_PRINT_OPERATION_ACTION_EXPORT`, so the operation sits at
  `GeneratingData` with `IsFinished` false even though `::done` has run and the
  PDF is complete. Waiting on `IsFinished` after an export waits for ever.
- **An export with no `export-filename` fails through the return value only.**
  Gtk fails a `g_return_val_if_fail` and hands back
  `GTK_PRINT_OPERATION_RESULT_ERROR` **without** filling in the `GError`, so a
  caller that only catches `GException` sees an export that silently did
  nothing.


## Fixed: the whole GtkAccessible update API, and a registry that bootstrapped itself

`GtkAccessible` is how Gtk 4 replaced ATK, and setting a state, a property or a
relation on a widget could not be done at all.

Gtk offers each of the three twice. The varargs spelling —
`gtk_accessible_update_state (self, GTK_ACCESSIBLE_STATE_BUSY, TRUE, -1)` — is
what the documentation shows and what no binding can call; codegen drops it, as
it should. The other spelling is

```c
void gtk_accessible_update_state_value (GtkAccessible      *self,
                                        int                 n_states,
                                        GtkAccessibleState  states[],
                                        const GValue        values[]);
```

— **two parallel arrays behind one count**, which the api.xml has no way to say
and codegen has no rule for. So `states` was read as a pointer-to-enum and
emitted as the method's *return value*, and `values` as one `GValue` by value:

```csharp
public Gtk.AccessibleProperty UpdatePropertyValue (int n_properties, GLib.Value values) {
        int native_properties;                               // uninitialised
        gtk_accessible_update_property_value (Handle, n_properties, out native_properties, …);
```

Gtk then read `native_properties[0]` — a stack slot nothing had written — as
*which* property to set. Being interface methods, the three appeared on
`IAccessible`, on the adapter, and on all 190-odd widget classes at once.

They are hidden in the metadata and rebound in `Source/Libs/GtkSharp/Accessible.cs`
as extension methods on `IAccessible` taking real arrays, plus the single-attribute
form every caller actually wants.

Beside them, **`gtk_accessible_{state,property,relation}_init_value` are now
bound by hand.** Gtk's documentation says of them "this function is mostly meant
for language bindings", and this language binding could not reach them: the gir
attaches them to the *enum* (`moved-to="AccessibleState.init_value"`) and gapi
enums carry no methods, so they never appeared in the api.xml. Without them a
caller has to know that `checked` is a tristate, `invalid` is its own enum,
`expanded` is an int, and a reference relation is a bare `gpointer`.

`GLib.Value` gained `ValueType` for the same reason: `Val` answers with an
instance, and an object-typed value holding NULL is indistinguishable from a
value of some other type that way. The relation API needs the distinction to
tell `active-descendant`, which points at one accessible, from every other
reference relation, which points at a list.

### `gtk_accessible_list_new_from_array` cannot be used at all

Its own constructor had the same shape — `GtkAccessible **` plus a count, bound
as one `GtkAccessible` — but rebinding it over an array does not help, because
Gtk 4.22 guards it with

```c
g_return_val_if_fail (accessibles == NULL || n_accessibles == 0, NULL);
```

an inverted assertion that rejects every non-empty array and returns NULL. (The
string is in the shipped library; that is how it was confirmed rather than
inferred.) `AccessibleList (IAccessible[])` therefore goes through
`gtk_accessible_list_new_from_list`, which has no such guard.

### A registry that only a program already using the type could install

`GtkSharp.GtkSharp.ObjectManager.Initialize ()` maps GType to managed type for
the types whose managed name `GType.LookupType`'s mangler cannot guess, and
`ObjectGen` emits the call into the static constructor of *each such type*:

```csharp
if (cs_parent != String.Empty && GetExpected (CName) != QualifiedName) { … }
```

In `GtkSharp` there is exactly one such type — `GtkText`, bound as
`Gtk.TextWidget` because `Gtk.Text` cannot also carry `GtkEditable`'s `Text`
member — so the registry was populated only by a program that had **already
named `Gtk.TextWidget`**. Until then every `GtkText*` Gtk handed back came out
as a bare `Gtk.Widget`: the mangler turns `GtkText` into `Gtk.Text`, finds
nothing, and walks up to the parent GType. A `GtkSpinButton`'s inner text
widget, reached through `GetFirstAccessibleChild`, is how this surfaced.

`GtkSourceSharp`, `WebkitGtkSharp` and `JavaScriptCoreSharp` are not affected —
nearly every type in them is renamed, so any one of them bootstraps the
registry. It is the assembly with *one* renamed type that cannot. `Gtk.Widget`'s
hand-written partial now carries the call, since Widget is the root of
everything Gtk hands out.

## GtkAccessibleText and GtkAccessibleRange cannot be reached from managed code

Not fixed, and pinned by a test so that fixing it is noticed.

`InterfaceVM.Validate` drops a vfunc that has no C function to invoke:

```csharp
if (target == null && !(container_type as InterfaceGen).IsConsumeOnly) {
        log.Warn ("No matching target method to invoke. Add target_method attribute with fixup.");
        return false;
}
```

That is right for an interface whose vfuncs mirror public functions, and wrong
for one that is *only* a vfunc table. `GtkAccessibleText` has ten vfuncs —
`get_contents`, `get_caret_position`, `get_selection`, `get_attributes` — and
Gtk exports no function that calls any of them; `GtkAccessibleRange` has one,
`set_current_value`; `GtkAccessibleHypertext` has three. All are dropped, and
the generated `IAccessibleTextImplementor`, `IAccessibleRangeImplementor` and
`IAccessibleHypertextImplementor` are **empty interfaces**.

So a managed widget cannot tell an assistive technology what its text is, and no
managed caller can ask another widget. The consumer half is bound and the
widgets do implement the interfaces — `GtkLabel`, `GtkTextView`, `GtkText` and
`GtkInscription` are `IAccessibleText` — but the only members on it are the
three `update_*` notifications, which are real C functions.

Fixing it means falling back to the vm's own name when there is no target, which
is a change to how every interface in eleven assemblies is emitted, and it
cannot be verified from a test: there is no public function that invokes these
vfuncs, so a managed implementation would have no observable effect.

## Accessibility: behaviour worth knowing

- **A widget and its AT context do not report the same role.** A role that comes
  from the widget class is applied when the context is *realized*, which for a
  widget that was never shown never happens, so `GetAtContext ().AccessibleRole`
  is `Widget` while `GetAccessibleRole ()` is `Label`. Only a role that was
  explicitly assigned appears in both.
- **Assigning `AccessibleRole.Widget` is not a change.** It is the abstract "some
  widget" role; the widget goes on reporting what its class declared, with no
  warning. Clearing a role by assigning the base one silently keeps the old one.
- **`gtk_test_accessible_has_state` means "is this attribute present", not "is it
  true".** A `GtkCheckButton` publishes `checked=false` from the moment it is
  built, so it *has* the checked state while `Active` is false. Meanwhile a
  sensitive button does not have the disabled state at all.
- **Gtk maintains part of the accessible description itself** — insensitive
  becomes `disabled`, `GtkToggleButton:active` becomes `pressed`,
  `GtkExpander:expanded` becomes `expanded`, a `GtkRange` publishes
  `value-now`/`value-min`/`value-max`, a placeholder becomes `placeholder`, and
  `gtk_label_set_mnemonic_widget` sets `labelled-by` **on the target**. What it
  does not do is publish a button's own label as the accessible label: that is
  computed when an AT asks, so `has_property (LABEL)` is false on a
  `Button ("press me")`.
- **`active-descendant` is the only reference relation that takes one
  accessible.** Every other one is a `GList` of them behind a `gpointer` —
  including `error-message`, which reads like a single thing. Passing the wrong
  shape sets nothing and reports nothing.
- **Gtk 4.22 has no `init_value` case for `GTK_ACCESSIBLE_STATE_VISITED`**, added
  in 4.12. The GValue comes back uninitialised, and assigning `Val` to one of
  those throws rather than doing nothing. A plain boolean works.
- **A `GValue` of the wrong type is refused silently.** No exception, no return
  value: `UpdateProperty (Label, new GLib.Value (42))` logs a critical and leaves
  the property unset, so the only way to know is to ask afterwards.
- **`SetAccessibleParent` works in one direction only.** The child reports the
  new parent, and the parent goes on reporting the children it really has. The
  next sibling has to be passed to `SetAccessibleParent` too —
  `UpdateNextAccessibleSibling` on a widget whose accessible parent was never set
  does nothing at all.
- **A widget's accessible id is its `GtkBuilder` id**, and a widget nothing named
  has **null**, not the empty string.
- **`gtk_accessible_get_bounds` is not `gtk_widget_get_width`.** It reports the
  widget's border box, which for a window's child came out as the surface width
  where `get_width` gave the content width — 220 against 186 under gvsbuild's
  client-side decorations. Neither number is portable, so the test asserts
  geometry it arranged itself: two buttons stacked in a spacing-free box are the
  same width, and the second starts exactly where the first ends.


## Fixed: five more arrays, and a property emitted as the wrong type

All in the layer an application crosses before it draws anything, and none of it
had ever been called.

- **`g_application_open` could not be called at all.** It takes
  `GFile **files, gint n_files`, and the api.xml has no way to tie the two
  together, so codegen bound `files` as a single `GFile` and passed the
  GObject's own address where Gio dereferences an array of pointers — the first
  "file" it read was that object's class pointer. This is the one entry point
  `G_APPLICATION_HANDLES_OPEN` exists to serve. Rebound in
  `Source/Libs/GioSharp/Application.cs`, with the count taken from the array,
  which is the only place it can be right.

- **`g_application_command_line_get_arguments` returned the program name and
  leaked the rest.** It is `gchar **` with its length in an out-parameter and,
  says the gir, *without* a terminating NULL — the one array shape codegen has
  no rule for, so the return value came out as a single string. Reading its own
  command line is the whole point of an application registered with
  `HANDLES_COMMAND_LINE`.

- **`Gtk.IconTheme.SearchPath` was a `string`, and worked in neither
  direction.** `GetSearchPath`/`SetSearchPath` were hidden by a mono-era
  metadata rule, written when they took Gtk 3's `(char ***, int *)` and
  `IconTheme.cs` bound them by hand. Gtk 4 gives them the plain strv shape, but
  with the methods hidden `PropertyBase.Getter` found nothing and the
  `search-path` *GObject property* was emitted instead — as a `string`, because
  that is what `SymbolTable` makes of `const-gchar**` with no array rule. The
  value holds a `G_TYPE_STRV`, so the getter read **null** and the setter asked
  GObject to transform a string into a strv and was silently refused.
  `ResourcePath`, whose methods were never hidden, sat right beside it working
  perfectly. Both metadata rules are gone, and the two dead Gtk 3 delegates that
  were the reason for them.

- **`gtk_accelerator_parse_with_keycode` wrote a pointer through a four-byte
  slot.** `accelerator_codes` is a `guint **` out-parameter for a
  zero-terminated array the caller must free; bound as `out uint` it gave Gtk
  four bytes to write an eight-byte pointer into, reported the low half of an
  address as a keycode, and leaked the array. Rebound in
  `Source/Libs/GtkSharp/Accelerator.cs`.

- **`gtk_distribute_natural_allocation` threw its own answer away.** It reads
  `n_requested_sizes` structs and writes each one's allocation *back* into
  `MinimumSize`. Codegen marshalled one struct by value into memory it freed on
  return, so with more than one size Gtk wrote past a 24-byte block and with
  exactly one the result was unreachable. Rebound over a real array in
  `Source/Libs/GtkSharp/Global.cs` — and note that a **blittable managed array
  is not enough**: the first attempt passed `Gtk.RequestedSize[]` straight to the
  delegate on the assumption that the runtime would pin it, and the distribution
  came back unchanged. It copies in and out explicitly.

While there, `gtk_widget_get_settings` was un-hidden. Nothing replaced it and
nothing explained the rule, so the widget-level way to reach the settings an
application reads did not exist. It is `Widget.Settings` now.

## The application object: behaviour worth knowing

- **`gtk_application_get_windows` runs newest first**, because it prepends — and
  `gtk_application_get_active_window` is *defined* as the head of that list, not
  as whatever has the pointer focus. So `Windows[0]` is the last window added,
  and `ActiveWindow` is deterministic with no window manager present. Reading
  `Windows[0]` as "the window I added first" is the natural mistake.
- **An accelerator's action name is stored normalised.** What goes in as
  `app.open('x')` comes back out of `GetActionsForAccel` as `app.open::x`, and
  the untargeted `app.open` is a different action with no accelerator at all.
- **`gtk_accelerator_valid` is about the key, not about the shortcut.** A bare
  letter with no modifier passes; only a key that cannot be an accelerator at
  all — a modifier key — is refused.
- **A `GtkHeaderBar`'s packed children are not its children.** Everything goes
  behind a `GtkWindowHandle`, so that a drag on the bar moves the window — and
  so walking `FirstChild`/`NextSibling` for a packed button finds the handle.
- **Registration is idempotent and activation is not.** `Register` twice emits
  `::startup` once; `Activate` twice emits `::activate` twice, which is the
  point — a second launch of a running application arrives as a second
  activation. And `gtk_application_add_window` does nothing whatsoever before
  registration: it logs a critical and returns.
- **Busy is a counter and a hold is not part of it.** Two `MarkBusy` calls need
  two `UnmarkBusy` calls, and `Hold` — which is what keeps `g_application_run`
  from returning — leaves `IsBusy` false.
- **`gtk_check_version`'s message is written from the caller's point of view.**
  Asking for a *lower* major version than the one running reports the library as
  "too new".
- **`gtk_window_close` destroys the window** when the `::close-request` handler
  does not veto it, so the managed wrapper is left pointing at freed memory:
  asking it `Visible` afterwards is a use-after-free that shows up only as a
  `GTK_IS_WIDGET` critical. `gtk_window_list_toplevels` is the thing left to ask,
  and it is also how `DestroyWithParent` can be tested at all.
- **Replacing an icon theme's search path can make a missing icon uncatchable.**
  A lookup never returns null — it falls back to `image-missing`, which is itself
  an icon that has to be found somewhere. A display-less `GtkIconTheme` whose
  `SearchPath` has been *assigned* one directory has nowhere to find it, and Gtk
  4.22 blows the stack rather than giving up. Appending with `AddSearchPath`,
  which is what an application shipping its own icons does, is safe — so this is
  arranged so it cannot happen rather than pinned by a test, because a stack
  overflow takes the host with it.
- **Starting a `GtkApplication` registers `<resource-base-path>/icons/` with the
  display's icon theme.** That is the only part of `gtk_application`'s
  `::startup` observable from managed code, and it is why an application's own
  icons are found by name with no code at all.
- **A `GApplicationCommandLine` can be constructed.** Its `arguments` property
  is construct-only and write-only and holds an `aay` — an array of
  NUL-terminated byte strings, which is what an argv is and what a C# string is
  not. That is the only way to reach a command-line reader without a second
  process, because `::command-line` is emitted by the primary instance over
  D-Bus and Windows ignores the argv passed to `g_application_run` entirely in
  favour of the real process command line.
- **`g_application_get_default` is a bare static pointer.** GLib stores it
  without taking a reference and never clears it, so a `GApplication` collected
  while it is the process default leaves the next caller holding freed memory.
  Anything creating applications in a long-lived process has to keep them alive.

## Fixed: sixteen signals whose args class belonged to another signal

The GtkSharp package could not be consumed at all: `GtkSharp.targets`, which
ships inside it and is imported by the **consumer's** build, had a `--` inside an
XML comment. MSBuild rejected it with `MSB4024`, naming a path inside the user's
package cache. Nothing in this repository imports that file — `Source/Samples`
uses `ProjectReference` — so no amount of building `GtkSharp.sln` could see it.
`SampleApps/GettingStarted` exists partly to keep that path exercised, and found
this on its first build.

The second defect it found is the one this section is named for.
`Gtk.PressedArgs` was reporting `GtkGestureLongPress`'s arguments to
`GtkGestureClick`'s handlers, so a click's `args.X` was really its `n_press` and
nothing on the args held the coordinates at all.

That is the collision [GioSharp fixed for `GFileMonitor::changed`](#fixed-two-signals-called-changed-one-args-class),
and finding a second one by accident is the reason to look for the rest
systematically. **GapiCodegen names a signal's args class after the signal**, so
two signals of one name in one assembly ask for the same class and only one shape
can have it. Every other user is handed members that read the wrong `Args[]`
slot: the wrong type, or past the end of the array. It compiles, the handler
still runs, and it reads the wrong thing.

The audit groups every signal in an assembly by name and reports any name with
two or more **distinct parameter shapes**. The filter that matters is that a
signal with no parameters emits `System.EventHandler` and claims no args class at
all, so it cannot collide: 31 shared names fall to 16 real ones.

| Assembly | Args class | Who lost, and what they read instead |
|:--|:--|:--|
| Gtk | `PressedArgs` | `GestureClick` — `X` was `n_press`, no coordinates at all |
| Gtk | `ChangeValueArgs` | `Range` — the proposed value was simply absent |
| Gtk | `DragBeginArgs`, `DragEndArgs` | `DragSource` — could not reach the `GdkDrag` it exists to hand you |
| Gtk | `PrepareArgs` | `Assistant` — read a `GtkWidget*` as a `double` |
| Gtk | `TagRemovedArgs` | `TextBuffer` — no `Start`/`End`, so nothing said which range lost the tag |
| Gtk | `ChangedArgs` | `Filter` and `CellRendererCombo` — three shapes, one name |
| Gtk | `MoveCursorArgs` | four shapes; the three-parameter ones read a fourth argument that is not there |
| Gtk | `ResponseArgs` | both — `GtkDialog`'s `GtkResponseType` arrived as a bare `gint` |
| Gtk | `MoveFocusOutArgs`, `ChangeCurrentPageArgs` | a parameter *name* only |
| Gdk | `ComputeSizeArgs` | `DragSurface` — read a `GdkDragSurfaceSize` as a `GdkToplevelSize` |
| Gio | `InterfaceAddedArgs`, `InterfaceRemovedArgs` | `DBusObject` — read its interface as an object, then looked past the end |
| Gio | `SocketEventArgs` | `SocketClient` — wrong enum, and a `GSocketConnectable` read as a `GSocket` |
| Adw | `SetupMenuArgs` | `Sidebar` — read an `AdwSidebarItem` as an `AdwTabPage` |

All sixteen are fixed in the `.metadata` files, following the precedent: **the
less-used side moves and the cname never changes**, so nothing about what is
connected natively is affected. Where the two sides disagreed only about a
parameter's name or its width, the parameter is normalised instead and no signal
is renamed — which is how `GtkNativeDialog::response` came to be typed
`GtkResponseType` at both ends.

The renames are `GestureLongPress.LongPressed`, `GestureDrag.DragStarted` /
`DragEnded`, `SpinButton.ChangeValueByScroll`, `Assistant.PreparePage`,
`TextBuffer.TagUnapplied` (pairing with `TagApplied`, and leaving `TagRemoved` to
`TextTagTable`, which owns the `TagAdded`/`TagChanged`/`TagRemoved` triple),
`Filter.FilterChanged`, `Sorter.SorterChanged`, `CellRendererCombo.ComboChanged`,
`Label`/`TextView.MoveTextCursor`, `Text.MoveEntryCursor`,
`TreeView.MoveTreeCursor`, `DragSurface.ComputeDragSurfaceSize`,
`DBusObjectManager.ObjectInterfaceAdded` / `ObjectInterfaceRemoved`,
`SocketListener.ListenerEvent` and `Sidebar.SetupItemMenu`.

`SignalArgsCollisionTests` pins them by emitting each signal directly and reading
the args back, because most of them need a pointer device, a display or a session
bus that a test does not have — and the argument marshalling, which is the part
that was wrong, is exercised either way.

**Where else to look:** the audit is cheap and should be re-run after any change
to `GirToGapi`'s name mangling or to a `.metadata` signal rename, because a
rename is itself capable of *creating* a collision. `TextBuffer.TagRemoved` was
exactly that: a deliberate rename, made to avoid clashing with the `RemoveTag`
method, that landed on a name `GtkTextTagTable` already had.

## Behaviour worth knowing: a lambda on some signals never runs at all

Found while pinning the above. `+=` connects **after** the class closure, and a
signal whose accumulator ends the emission as soon as the class handler has
answered therefore never reaches an "after" handler — it is not late, it is not
called. `GtkDragSource::prepare` and `GtkDropTarget::accept`, the two signals a
drag-and-drop implementation is built on, are exactly those.

```csharp
source.Prepare += (o, args) => args.RetVal = MakeProvider();   // never invoked
```

Since a lambda cannot carry `[GLib.ConnectBefore]` — the attribute is read off
the delegate's `MethodInfo` — those two signals cannot be handled with one at
all. `GtkRange::change-value` is the contrast that makes the rule legible: its
accumulator stops only for a handler returning `true`, so an "after" lambda does
run there. The distinction is the accumulator, not the return type.

## Fixed: six Gsk render-node APIs that could not be called correctly

`GskRenderNodeTests` went in over the scene graph Gtk 4 actually draws through,
and every constructor it needed on the way turned out to be one of two shapes
codegen has no rule for.

**Arrays whose length is a sibling parameter.** The same family as
`gsk_container_node_new`, already rebound, and `gdk_content_formats_new` before
it. Five gradient constructors took `(const GskColorStop *stops, gsize n_stops)`
and came out as *one* `ColorStop` by value plus a count the caller supplied
separately:

```csharp
// Before. A gradient needs two stops; there is one struct's worth of memory here.
new Gsk.LinearGradientNode(bounds, start, end, oneStop, 2);
```

There was no way to write a correct call — passing the true count read past the
end of a 20-byte allocation, and passing 1 built a gradient GSK rejects. The
matching `GetColorStops` wrapped only the first element, so a gradient could not
be read back either. `gsk_shadow_node_new` had it too, and a drop shadow with two
shadows is ordinary rather than exotic. All rebound over `ColorStop[]` /
`Shadow[]` in `GradientNodes.cs` and `ShadowNode.cs`, where the count is the
array's own length and cannot disagree with it.

**`gsk_stroke_set_dash` was worse than uncallable.** The array parameter became
an *out* parameter:

```csharp
public float SetDash(ulong n_dash) {
    float dash;                                    // four bytes of stack
    gsk_stroke_set_dash(Handle, out dash, new UIntPtr(n_dash));   // read n_dash floats from it
    return dash;
}
```

So there was no way to set a dash pattern at all, and the obvious call corrupted
the caller's frame. Rebound as a `float[] Dash` property in `Stroke.cs`.

**Fixed-size arrays returned as scalars.** `gsk_border_node_get_widths` returns
`const float *` — four floats, one per edge — and the api.xml records the element
type with no length, so codegen declared the P/Invoke as *returning a `float`*.
On x86-64 the wrapper read XMM0 while GSK had put the pointer in RAX, so
`BorderNode.Widths` answered with whatever the last floating-point operation had
left behind. `get_colors` had the same shape and returned the array's first
element as the whole answer. Both now return arrays.

**Where else to look:** grep the generated tree for a P/Invoke whose return type
is a value type where the api.xml says `const-<T>*`, and for a `<parameter>` that
is an array sitting beside a `gsize`/`guint` count. Neither shape is expressible
in api.xml, so neither will ever be caught by the build — only by someone trying
to call it.

## Behaviour worth knowing: only rasterising tests a render node

`GskRenderNodeTests` ends almost every test at `RenderNode.Draw` onto a Cairo
image surface and reads the pixels back, rather than asserting on the node's
properties. This is not thoroughness for its own sake: a render node *is* a
description of drawing, so "was this node built correctly" and "does it describe
the drawing I asked for" are the same question, and only rasterising answers it.

A property-only test passes just as happily against a node built from the wrong
arguments — which is precisely the state four of the node types above were in.
The pattern is cheap:

```csharp
using var surface = new Cairo.ImageSurface(Cairo.Format.Argb32, 8, 8);
using (var cr = new Cairo.Context(surface)) node.Draw(cr);
surface.Flush();
// ARGB32 is premultiplied BGRA on little-endian: byte 0 B, 1 G, 2 R, 3 A.
```

Assert on *both* directions — a pixel that should be painted and one that should
not. Half the assertions in that file would pass against a surface nothing ever
touched, and the other half are what stop that from being a green run.

## Fixed: GskRoundedRect was 40 bytes where GSK reads 48

The single defect that had blocked the most surface. `GskRoundedRect` is two
structs embedded by value:

```c
struct GskRoundedRect {
    graphene_rect_t bounds;      /* 16 bytes: x, y, width, height       */
    graphene_size_t corner[4];   /* 32 bytes: 4 x (width, height)       */
};                               /* 48 bytes, all of it floats          */
```

`graphene_rect_t` and `graphene_size_t` are bound as opaque boxed **classes**, so
`SymbolTable` had no by-value form for either. The generated struct came out as
an `IntPtr` for the bounds followed by a managed `Graphene.Size[]` marshalled
`ByValArray` — 40 bytes on Windows, and on Linux not marshallable at all
("Type 'Gsk.RoundedRect' cannot be marshaled as an unmanaged structure"). Every
method then did `AllocHGlobal(Marshal.SizeOf<Gsk.RoundedRect>())` and handed that
to a function reading and writing 48 bytes through it.

Nothing built on a rounded rectangle worked: `BorderNode`, `RoundedClipNode`,
`InsetShadowNode`, `OutsetShadowNode`, `PathBuilder.AddRoundedRect` and
`Snapshot.AppendBorder`. That is four of the thirty-eight render-node types.

**Hiding the struct is not the fix.** Tried first, and codegen then drops every
dependent that mentions `const-GskRoundedRect*` with an "Unknown type" warning
rather than keeping it — `BorderNode.New`, `RoundedClipNode.New`,
`Snapshot.AppendBorder` and the rest simply vanish from the binding. What works
is removing the two *fields* and declaring them by hand:

```xml
<remove-node path="/api/namespace/struct[@cname='GskRoundedRect']/field[@cname='bounds']" />
<remove-node path="/api/namespace/struct[@cname='GskRoundedRect']/field[@cname='corner']" />
<attr path="/api/namespace/struct[@cname='GskRoundedRect']" name="noequals">1</attr>
<attr path="/api/namespace/struct[@cname='GskRoundedRect']" name="nohash">1</attr>
```

`noequals`/`nohash` are load-bearing, and the reason is worth remembering:
`StructBase.GenEqualsAndHash` builds `Equals` by folding the field list, so a
struct with no api.xml fields gets `return true;` — every rounded rectangle equal
to every other. The two attributes already existed for other reasons; without
them this fix would have traded a crash for a silent wrong answer.

`RoundedRect.cs` then declares the twelve floats. Because it is the *only* file
that declares any, sequential layout is that file's declaration order and nothing
else — which is what makes hand-completing a generated struct safe at all.

**And a use-after-free that fell out on the way.** The six methods returning
`GskRoundedRect*` return their own receiver, and the wrapper freed its copy
before reading the return value out of it:

```csharp
ReadNative (this_as_native, ref this);          // correct: the receiver is updated
Marshal.FreeHGlobal (this_as_native);
ret = Gsk.RoundedRect.New (raw_ret);            // raw_ret == this_as_native
```

So `rect.InitFromRect(bounds, 5)` left `rect` right and returned garbage. Now
bound over `ref this`, which needs no copy at all once the struct is blittable.

**Where else to look:** any api.xml `<field>` whose type is a boxed opaque is
embedded by value in C and cannot be described by the class codegen emits for it.
`grep` the generated tree for `IntPtr _` fields inside a `[StructLayout]` struct,
and for `ByValArray` over anything that is not a primitive.

## Fixed: a `.ui` file with a `<signal>` failed in a way that named the wrong thing

`BuilderBindingTests` went in over `Builder.Autoconnect` — binding `[UI]` fields
from a `.ui` document, which is what the templates generate and what
`getting-started.md` teaches. `Builder.cs`, `BuilderXml.cs` and
`BindingAttribute.cs` are entirely hand-written, so none of it was checked by
compiling, and none of it was covered.

The field binding turned out to be sound: by field name, by explicit name,
private fields, fields inherited from a base class, static fields via
`Autoconnect(Type)`, and `throwOnUnknownObject` in both positions. All now
pinned.

**The signal path was not what anyone thought.** The code and the guide both
described a document that loads with its handlers unconnected, and an
`Autoconnect` that throws `NotSupportedException` "deliberately, rather than
silently ignoring every click". What actually happens is that Gtk 4 resolves a
`<signal>` handler through `GtkBuilderScope` at **parse** time. The default scope
is `GtkBuilderCScope`, which looks the name up as an exported C symbol. It never
finds a managed method, so the document does not load at all:

```
GLib.GException: No function named `OnClicked`.
```

`Autoconnect` is never reached, so its `NotSupportedException` never fires — and
the error the user does get reads as a missing *native* symbol, sending them to
look for a C function they never wrote.

Two things were wrong at once, which is why neither had been noticed: the guide
documented an exception the library could not raise, and the check that would
have raised it only ever ran on the `Stream` constructor. `AddFromString`,
`AddFromFile` and `AddFromResource` — the three ordinary ways in — never
inspected the document at all.

All three are now hidden in the metadata and rebound in `Builder.cs`. They
inspect the XML *before* the native call, and if the call then fails on a
document that declared a handler, that is what the exception says, with
GtkBuilder's own error kept as `InnerException`. `Builder.DeclaresSignals` is
public and is set even when the load fails, so a caller can tell "my XML is
wrong" from "this is not supported yet".

**The real remedy is still open**: implementing `GtkBuilderScope` so a managed
method can be resolved. `Gtk.IBuilderScope`, `Gtk.BuilderCScope` and
`Builder.Scope` are all bound already; what is missing is a scope whose
`create_closure` returns a `GClosure` over a managed delegate. Until then the
failure is at least legible.

**Where else to look:** a comment or a doc that describes an exception is a claim
nobody checks. Grep `Docs/` for exception type names and confirm each one is
reachable — this one had been wrong since the Gtk 4 port, in the file that
teaches the binding.

## Behaviour worth knowing: parse the XML, do not grep it

`BuilderXml.DeclaresSignals` parses the document rather than searching for
`"<signal"`, and the test that matters is a document whose only mention of the
word is a comment saying it deliberately has none:

```xml
<!-- No <signal> elements here: handlers are connected in code. -->
```

Grepping reads that as a declaration and refuses a perfectly good file. Now that
the string drives an *exception*, getting it wrong turns a working document into
a rejected one rather than merely producing a spurious warning.

## Fixed: the CSS example in the guide named a type that did not exist

`ThreadAndStyleTests` covers `Gtk.ThreadNotify` and the `Gtk.StyleContext` render
helpers — two hand-written files with no coverage at all. Writing it turned up a
second documentation-versus-library mismatch, in the same file as the `<signal>`
one:

```csharp
StyleContext.AddProviderForDisplay(Gdk.Display.Default, css,
                                   Gtk.StyleProviderPriority.Application);
```

There was no `Gtk.StyleProviderPriority`. `AddProvider` takes a bare `uint`, and
the only way to call it was to write `600`.

`GTK_STYLE_PROVIDER_PRIORITY_APPLICATION` is a `<constant>` in `Gtk-4.0.gir`, and
**GirToGapi emits no constants at all** — `grep -c '<constant' Source/Libs/*/*-api.xml`
is zero everywhere. Gtk has 98, GLib 142, Pango 14, and Gdk 2459. Gdk's are the
`GDK_KEY_*` keyvals and they are covered only because someone hand-wrote
`Source/Libs/GdkSharp/Key.cs`; the rest are simply absent.

The five priorities are now declared by hand in `StyleProviderPriority.cs`, as
`const uint` rather than an enum — `AddProvider` takes a number and any value
between two named ones is legal, which an enum would deny. Teaching GirToGapi to
emit `<constant>` remains open, and would rewrite every api.xml, so it belongs to
its own reviewable pass rather than to a test sweep.

**Where else to look:** the same grep is the audit. A constant that a C
programmer would reach for by name is one a C# caller currently has to
hard-code, and hard-coded numbers do not fail loudly when a version changes them.

## Behaviour worth knowing: what makes a drawing test an oracle

The render helpers are the null-delegate trap's natural habitat — several
`gtk_render_*` functions were removed outright in Gtk 4, and a removed one is a
`NullReferenceException` at the call site, not a link error. Testing them by
calling and seeing whether anything was thrown would be the assertion-free sweep
this document keeps banning.

What makes them testable is that CSS is an oracle the test writes itself:

```csharp
provider.LoadFromData("label { background-color: rgb(255,0,0); }");
// ... RenderBackground into an ImageSurface, then assert the pixel is 255,0,0
```

A themed default cannot be mistaken for success, because the test chose the
colour. Each of these is paired with its opposite — `background-color:
transparent`, `border: 0px` — so "something was painted" is reporting the CSS
rather than the fact that a call happened at all. `RenderLayout` is paired with
an empty `Pango.Layout` for the same reason.

`ThreadNotify` gets the same treatment. The oracle is not that the delegate ran
but *which thread it ran on*: `Thread.CurrentThread.ManagedThreadId` inside the
delegate, compared against the fixture's Gtk thread and against the worker that
called `WakeupMain`. A delegate that ran on the wrong thread — which is the only
failure that matters for a class whose entire purpose is thread affinity — would
satisfy any test that merely counted invocations.

## Behaviour worth knowing: what `await` does in a Gtk application

`AsyncContextTests` covers `GLib.GLibSynchronizationContext`, which had no
coverage at all despite being what makes `await` usable in a Gtk application.
`Application.Init` installs it on the thread that called it, so an `await` in an
event handler captures it and resumes on the Gtk thread — which is the only
reason the code after an `await` may touch a widget.

Nothing here asserts that a continuation *ran*. That is not the question: a
continuation that resumed on a thread-pool thread satisfies any test that waits
for a flag, and then corrupts Gtk from a thread that never called `gtk_init`. The
question is **where**, so every test compares `ManagedThreadId`, and every
positive is paired with the arrangement that must not come back:

| | resumes on |
|:--|:--|
| `await task` | the Gtk thread |
| `await task.ConfigureAwait(false)` | wherever the task completed |
| `await task` with the context removed | wherever the task completed |
| `await Task.CompletedTask` | inline, without the loop turning |

The last is worth knowing on its own: an already-completed task takes the
awaiter's synchronous path, so code after that `await` runs without a single turn
of the main loop.

`ConfigureAwait(false)` is the trap that bites hardest, because it is what a
library author is told to write. Anything after it must not touch a widget, and
the failure is timing-dependent rather than deterministic.

**Send deadlocks if you call it from the Gtk thread**, and that is deliberately
not tested: `Send` posts an idle and blocks until it runs, so calling it from the
thread that would have to dispatch that idle waits forever. A test for it would
hang the fixture rather than fail, and hanging is the one outcome this suite
cannot report — see the truncated-total failure mode this document keeps
returning to. `Send` is for worker threads; the Gtk thread should call the code
directly, or `Post`.

**Where else to look:** anything that captures `SynchronizationContext.Current`
and replays it later. The context is installed per *thread* by `Init`, not
process-wide, so a helper that marshals work by capturing the current context on
whatever thread happens to construct it will silently do nothing useful.

## Fixed: `StringList.Splice` deleted rows the caller never asked it to

Found while writing `ListViewTests` over the half of the list pipeline nothing
covered — `ListView`, `MultiSelection`, `NoSelection`, `ListItem` and `Bitset` had
no mention in the suite at all, and `ListView` is the widget
`getting-started.md` tells people to use instead of `TreeView`.

The C function is

```c
void gtk_string_list_splice (GtkStringList *self, guint position,
                             guint n_removals, const char * const *additions);
```

and the api.xml records all three parameters correctly. What came out was

```csharp
public void Splice(uint position, string[] additions)     // n_removals is gone
```

with `additions.Length` passed as `n_removals`. So `list.Splice(1, new[] {"a","b"})`
— which reads as an insertion — removed two rows and added two, and there was no
way to express a pure insertion at all. Silent, and destructive.

**The cause is a name heuristic with no cross-check.** `Parameter.IsCount` is true
for *any* integer parameter whose name starts with `n_`, and `Parameters` then
pairs it with the next parameter if that one `IsArray`. `n_removals` starts with
`n_`; `additions` is an array; the two got married. But `additions` is
NULL-terminated — it carries its own length and has no count parameter to pair
with. `IsArray` was true for both kinds, so the distinction did not exist.

`Parameter.NeedsCount` now makes it: `array` **and not** `null_term_array`. Fixed
in the generator rather than the metadata, because the misjudgement will recur on
the next API of this shape.

**How the blast radius was measured**, which matters more than the fix: a
generator change rewrites every assembly, so the check is to diff the generated
public surface, not to run the tests and see green.

```sh
find Source/Libs -path "*/Generated/*" -name "*.cs" -print0 \
  | xargs -0 grep -hE "^[[:space:]]+public .*\(.*\)" | sed 's/^[[:space:]]*//' | sort > after.txt
# stash the change, dotnet cake --BuildTarget=Prepare, repeat into before.txt
diff before.txt after.txt
```

7844 signatures, one line changed. Do this for any `GapiCodegen` edit — the suite
passing says nothing about the 7843 signatures no test mentions.

A second bug fell out of the same block: `if (next != null || next.Name == "parameter")`
dereferences `next` in exactly the case the null check was guarding. `||` for
`&&`, and it also meant a comment inside `<parameters>` would crash codegen.

## Fixed: a second `<constant>` casualty, and what the first one should have taught

`GTK_INVALID_LIST_POSITION` is what `SingleSelection.Selected` holds when nothing
is selected and what `StringList.Find` answers when the string is not there. Like
`GTK_STYLE_PROVIDER_PRIORITY_*` before it, it is a `<constant>` in the gir, so it
did not exist in the binding and the only way to ask "is anything selected" was
to compare against `uint.MaxValue` and hope that is what it means. Now
`Gtk.Global.InvalidListPosition`.

That is two of the 98 found by accident, each while writing a test for something
else. The rest are still missing, and the way to find them is not to wait for the
next accident — see the earlier section.

## Behaviour worth knowing: a single selection takes two flags to empty

I expected `CanUnselect = true` to be enough to clear a `SingleSelection`, and it
is not. There are two independent guards, both defaulting to the value that keeps
a row selected:

| | |
|:--|:--|
| `CanUnselect` (false) | refuses the unselect outright |
| `Autoselect` (true) | allows it, then immediately picks a row again |

So clearing a selection needs `CanUnselect = true` **and** `Autoselect = false`.
This is why a `ListView` always has a row highlighted, and why turning off only
the flag whose name mentions unselecting appears to do nothing at all. The test
asserts the state after each of the three steps, so the one that works is
distinguishable from the two that quietly do not.

## Behaviour worth knowing: how to read a widget's own painting back

`DrawingAreaTests` covers the custom-drawing path — `Docs/getting-started.md`
devotes a section to it and the suite had no mention of `DrawingArea` or
`SetDrawFunc` at all. It is the most common thing an application does beyond
arranging widgets, and it crosses every boundary in the binding at once: a
managed delegate marshalled into Gtk, invoked from native code, handed a
`Cairo.Context` it did not create.

Counting invocations is not enough. A draw function that *is* called but whose
context is wrong paints nothing, and an invocation counter calls that a pass. The
oracle has to be the pixels — but the context belongs to Gtk's surface, so it
cannot be read directly. The route that works goes through the scene graph Gtk
itself draws through:

```csharp
var paintable = new Gtk.WidgetPaintable(widget);
var snapshot = new Gtk.Snapshot();
paintable.Snapshot(snapshot, width, height);
var node = snapshot.ToNode();          // null if the widget painted nothing

using var surface = new Cairo.ImageSurface(Cairo.Format.Argb32, width, height);
using (var cr = new Cairo.Context(surface)) node.Draw(cr);
```

This is worth knowing beyond drawing areas: it rasterises **any** widget, so it
is the general way to assert on what Gtk rendered rather than on what it was
asked to render. It leans on `RenderNode.Draw`, which `GskRenderNodeTests`
pins independently.

Each drawing test is paired with a control that must paint nothing — an empty
draw function, an empty `Pango.Layout` — so "there are pixels" reports the
drawing rather than the theme, the window background, or anything else that ends
up in a snapshot.

**And a trap worth stating plainly.** The `width` and `height` a draw function
receives are the widget's **allocation**, not its `ContentWidth`/`ContentHeight`.
Those two are a natural-size request; a window stretches its child past them. A
test that asked for 16 and asserted 16 got 188 — the area filling the smallest
window the display would make. Asserting the requested size would have been
asserting a fact about the window manager, which is the class of mistake this
document keeps coming back to. Compare against `AllocatedWidth`/`AllocatedHeight`
instead, and assert the *requested* size through `Measure` where it really is the
contract.

## Fixed: `AttrList.Attributes` handed back a list of nulls

`PangoAttributeTests` covers `Pango.AttrList` and `Pango.AttrIterator` — nearly
all hand-written, and `AttrIterator` had no mention in the suite at all.

`AttrList.Attributes` was generated as

```csharp
public GLib.SList Attributes {
    get { return new GLib.SList (pango_attr_list_get_attributes (Handle)); }
}
```

with no element type. `GLib.SList` then marshals each item as a GObject, which a
`PangoAttribute` is not, so every element came back **null** and touching one
threw `NullReferenceException`. There was no way to enumerate a list's
attributes.

What makes this one worth reading is that the fix already existed twelve lines
away. `AttrIterator.Attrs` is hand-written specifically to avoid this, and says
so in a comment. The list's own getter had the identical bug and kept it, because
**nothing called it** — the hand-written file was written in response to a crash
somebody hit, and the neighbouring method nobody happened to use was never
looked at. Rebound over `Pango.Attribute[]` in `AttrList.cs`.

**Where else to look:** `grep` the generated tree for `new GLib.SList (` and
`new GLib.List (` with a single argument. Every one of those is a list whose
elements will marshal as GObject regardless of what they are.

## Behaviour worth knowing: an attribute iterator has a run after the last attribute

`AttrIterator` does not stop when the attributes do. After the last attributed
run it yields one more, from the end of the last attribute to `G_MAXINT`,
carrying no attributes — the unformatted remainder of whatever text the list is
eventually applied to. The list has no idea how long that text is, which is why
the end is a sentinel rather than a length.

I expected `Next()` to return false there, and a caller who assumes the same will
attribute the trailing run's (empty) formatting to the last real run.

Attribute indices are **byte** offsets, not character offsets, which is pinned
with a two-character, three-byte string. Getting it wrong formats half a letter
and Pango does not complain.

## And a repeat of a mistake this document already records

The metadata edit for the fix above contained `--` inside an XML comment, which
is not legal XML. `GapiFixup` failed, codegen produced nothing for the whole of
PangoSharp, and **MSBuild still printed "0 Error(s)"** — the Cake task failed
underneath a build that reported success. Grepping the log for `error CS` and
`Error(s)` found nothing wrong.

This is the third variant of the same failure recorded here: a truncated test
total, a schema that would not load, and now a metadata file that would not
parse. The lesson has to be mechanical rather than remembered:

```sh
dotnet cake build.cake --BuildTarget=Build > build.log 2>&1; echo "EXIT: $?"
```

**Check the exit code.** Grepping for the word "error" is not a substitute, and
`0 Error(s)` from MSBuild says nothing about whether the tool that runs before it
did its job. A cheap second check, since the metadata is XML and XML is
checkable:

```sh
python -c "import glob,xml.dom.minidom as m; [m.parse(f) for f in glob.glob('Source/Libs/*/*.metadata')]"
```

## Fixed: every `Cairo.Glyph` in a run hashed to the same value

`CairoTextTests` covers Cairo's text and glyph API. `CairoSharp` has no
`.metadata` and nothing generated — every line is hand-written, so nothing about
it is checked by compiling — and `FontFace`, `Glyph` and `ShowGlyphs` had no
mention in the suite at all.

`Glyph.GetHashCode` was

```csharp
return (int) Index ^ (int) X ^ (int) Y;
```

wrong twice over. XOR is commutative, so every permutation of the same three
numbers shared one hash: `(1,2,3)`, `(3,2,1)` and `(2,1,3)` all came out as
**zero**. A glyph run is mostly permutations of small numbers, so that is the
ordinary case rather than a rare one. And the casts threw away the fractional
part of `X` and `Y` — which is exactly what sub-pixel glyph positioning puts
there, so every glyph between two whole numbers hashed alike too.

This is the same defect, with the same reasoning, that `StructBase.GenHashCode`
in `GapiCodegen` was already fixed for. The generated structs got the fix; the
hand-written one beside them did not, because nobody was looking at it. That is
the second time in two sweeps — `AttrList.Attributes` kept the bug its own
neighbour was hand-written to avoid.

**Where else to look:** `grep` the hand-written tree for `GetHashCode` bodies
containing `^` without a multiply. A commutative fold is only visible as a bug
when something hashes a struct whose fields are permutations of each other, which
is rare enough to survive for years and common enough to matter when it bites.

## Behaviour worth knowing: testing glyph drawing without assuming a font

Glyph indices are a property of the font file, so no particular number can be
assumed on a machine whose fonts the test did not choose. Scanning for one with
non-empty extents makes the test independent of what `"sans"` resolves to:

```csharp
for (long index = 1; index < 300; index++)
    if (cr.GlyphExtents(new[] { new Cairo.Glyph(index, 0, 0) }).Width > 0)
        return index;
throw new InvalidOperationException("no drawable glyph found");
```

It fails loudly rather than quietly drawing nothing, which is the difference
between this and hard-coding an index that happens to work here.

With one such index in hand, the oracles are positional rather than absolute: the
same glyph at `x=2` and `x=40` must leave ink at different places, and three
glyphs must reach further right than one. Between them those pin the hand-written
`Glyph[]` copy into unmanaged memory — index, x and y all surviving it, and the
whole array arriving rather than only its first element, which is the failure
this repository has hit repeatedly elsewhere.

## Fixed: five defects in the list marshalling every binding call goes through

`GLibListTests` covers `GLib.List`, `GLib.SList` and the `ListBase` beneath
them — 290 hand-written lines that nothing in the suite referenced by name, and
the machinery both of this sweep's earlier defects actually lived in. Writing
sixteen tests against it found five more.

**`Clone` dropped the element type, and cloning a list of strings crashed the
process.** It was

```csharp
public override object Clone () => new List (g_list_copy (Handle));
```

with no element type, so every element of the clone went through `DataMarshal`'s
last resort — "is this pointer a GObject?" — which dereferences it as a
`GTypeInstance`. For a list of strings that reads a `char*` as an object header:
an access violation that took the test host down, not an exception. Now carries
`element_type` across, `owned: true` (the spine is a copy) and
`elements_owned: false` (`g_list_copy` is shallow).

**`Count` was cached and never invalidated by a mutation.** `length` was dropped
only when the list was emptied, so

```csharp
int before = list.Count;   // walks the chain, caches the answer
list.Append (item);
int after = list.Count;    // still the old number
```

and because LINQ preallocates from `ICollection.Count`, a single `Cast<T>()` was
enough to leave a list lying about its length for the rest of its life.
`Append`/`Prepend` now drop the cache.

**The enumerator restarted once it had finished.** `current == IntPtr.Zero` meant
both "not started" and "ran off the end", so `MoveNext` sent it back to the head
and answered `true` forever — a loop that kept asking never terminated. Split
with a `finished` flag that `Reset` clears.

**`SyncRoot` returned null**, so the documented `lock (collection.SyncRoot)` was
a `NullReferenceException`.

**`Prepend` took only an `IntPtr`** while `Append` had taken a `string` and an
`object` since the mono era, so building a list front-to-back meant marshalling
every element by hand. Two overloads added, the `Append` ones with the direction
changed.

**Where else to look:** `Source/Libs/GLibSharp/PtrArray.cs` has the same
`DataMarshal` fallthrough at line 193 and was not part of this pass.

## Behaviour worth knowing: `Append(object)` is not `Append(IntPtr)`

`AllocNativeElement` copies a value type into fresh native memory and stores
*that* address. For a struct that is right; for an `IntPtr` it means the list
holds a pointer to a copy of your pointer. Two of these tests were written
against the wrong one and read back addresses nobody recognised.

Use `Append(IntPtr)` when the element *is* the pointer.

## And the crash that made the point again

The first draft of the element-type test built a list holding `new IntPtr(0x1234)`
and read it back with no element type — which asks GLib whether address `0x1234`
is a GObject, and GLib reads through it. Access violation, test host gone,
`Total` down by the rest of the class.

A fabricated pointer is only safe in a list whose element type stops anything
dereferencing it. Where the point of the test *is* the dereferencing path, use
`IntPtr.Zero`: it exercises the same branch and is the one address that is
defined to be safe.

## Fixed: `PtrArray.Clone` called an arbitrary address as a function

The `ListBase` pass ended by recording `PtrArray` as unexamined — same
`DataMarshal`, same `ICollection` surface, same enumerator shape, written
separately. `GLibContainerTests` is that examination. It shares two of the five
defects found there, and has a worse one of its own.

```csharp
delegate IntPtr d_g_ptr_array_copy(IntPtr raw);              // one parameter
```

The C function has taken three since GLib 2.62:

```c
GPtrArray *g_ptr_array_copy (GPtrArray *array, GCopyFunc func, gpointer user_data);
```

So `Clone` left `func` and `user_data` as whatever happened to be in the argument
registers — and a non-NULL `func` is **called**, once per element. This did not
return a wrong answer or throw; it jumped to an arbitrary address. The test host
died with `FailFast` and no managed stack.

Now declared with all three, passing NULL for a shallow copy, and the result is
marked owned — `g_ptr_array_copy` is transfer full, so the old
`owned: false` leaked every clone as well.

**The other two are the ones `ListBase` had**, in independently written code:
`SyncRoot` returned null, and the enumerator restarted after finishing because
`current = -1` means both "not started" and "ran off the end".

**Where else to look:** every `d_g_*` delegate in the hand-written tree is a
signature nobody checks. `grep` for delegates whose parameter count differs from
the gir's, starting with anything added after GLib 2.50 — the older calls have
had decades of use, these have not. A wrong *type* usually misbehaves; a missing
**callback** parameter executes data.

## Behaviour worth knowing: the total is the crash detector

Two crashes in two sweeps, and both announced themselves the same way — not as a
failure, but as a **smaller `Total`**:

```
Failed:     2, Passed:     7, Total:     9      <- fourteen tests were written
```

Nine ran. Five never got the chance, because the host was gone. Had the two
failures not been there, the line would have read `Passed! ... Total: 9` and
looked like a clean run of a smaller class.

The habit that catches it is counting the tests you wrote and comparing. When the
total is short, bisect by filter — the crash here was one test, and running the
six `PtrArray` tests one at a time named it in under a minute:

```sh
for t in <names>; do dotnet test --filter "FullyQualifiedName~$t"; done
```

Then read the *class* boundary too: `Argv` passing 5/5 in isolation while the
combined run died proved the fault was not in the half that looked suspicious.

## The delegate-arity audit, and the one real defect it found

The `PtrArray` pass ended by saying every `d_g_*` delegate in the hand-written
tree is a signature nobody checks. That audit is mechanical, so it was worth
writing rather than describing: extract every `delegate ... d_<c_name>(...)` from
the non-generated sources, look `<c_name>` up in the girs, and compare the
parameter count (instance parameter included, `throws` adding one).

**706 delegates checked, five mismatches, one real.**

The four false positives are all variadic C functions where the extra managed
parameter is the argument behind the format — `gdk_pixbuf_save`,
`gdk_pixbuf_save_to_stream`, `gtk_message_dialog_new` and its markup twin. An
arity-only audit cannot know that, so the script reports and a human reads.

The real one was `g_logv`:

```c
void g_logv (const gchar *domain, GLogLevelFlags level,
             const gchar *format, va_list args);      /* four */
```

```csharp
delegate void d_g_logv(IntPtr log_domain, LogLevelFlags flags, IntPtr message);  // three
```

Two faults at once. The `va_list` was never passed, so GLib read the argument
list out of whatever was in the register; and the already-composed message was
handed over as the **format**, so any per cent sign in it became a conversion
consuming from that garbage list. `Log.WriteLog(domain, level, "100% complete")`
was undefined behaviour and `"%s"` was a wild pointer dereference. The test host
died with a `FailFast` and no managed stack.

Now `g_log` with a literal `"%s"` and the message as the argument behind it.

**The instructive part is the sibling that was fine.** `MessageDialog` passes a
composed message into the same kind of parameter, and was *safe*, because it
composed through `Marshaller.StringFormat`, which doubles every per cent sign so
printf renders one. Two wrappers, the same hazard, opposite outcomes — and the
protection was three files away from the code that needed it.

`MessageDialog` now passes `"%s"` too, which meant **removing** the escaping:
doubling and then not un-doubling gave "100%% complete". Passing text as data is
the more robust arrangement, but the two mechanisms must not be half-applied.

**Why no test caught either:** every existing test of these APIs used a message
with no per cent sign. `"100% complete"` is an ordinary thing to log.

**Where else to look:** the audit script only compares *counts*. A delegate with
the right number of parameters and the wrong types is still wrong, and
`IntPtr`-for-everything hides most of it. The counts are the cheap half; the
types need reading.

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
so only broke there. At 730 tests Windows reports 727 passing with 3 skips.

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
| `GLibDeepTests` | The rest of hand-written GLib: `HookList`'s ABI description checked against the struct `g_hook_list_init` actually writes, the container half of `Variant`/`VariantType` (tuples, arrays, maybes, dict entries, subtyping), `Bytes` slicing and ownership, the `Marshaller` helpers below the ones every binding uses, and the two branches of `GLib.Signal` that only a returning signal or an emission hook reaches. |

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
with no allocator, `new HookList ()` leaves the handle null — the same shape as
the `Gsk.RoundedRect` item below. A test pins that, so that adding an operation
forces a test to be added with it.

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
`g_bytes_unref_to_array` are the same shape and are not yet covered.

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

At 603 tests:

| | line rate |
|:--|--:|
| **hand-written (Generated and Samples excluded)** | **53.0%** |
| overall, including generated | 11.1% |
| `GLibSharp` hand-written | 62.3% |
| `CairoSharp` hand-written | 52.1% |
| `GioSharp` hand-written | 52.1% |
| `PangoSharp` hand-written | 42.2% |
| `GdkSharp` hand-written | 40.4% |
| `GtkSharp` hand-written | 38.0% |

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

The useful reading is the ordering, not the percentage. `AdwaitaSharp` at zero is
real information — nothing exercises libadwaita at all. `GioSharp` at 2% likewise.
Those are worth tests. Raising `GtkSharp` from 7.8% by walking generated
properties would not be.

Note that most of `Source/Libs/*` is generated, and generated code is uniform by
construction: testing one property round-trip exercises the same emission path
as the thousand others like it. High line coverage over generated code is
therefore close to meaningless. The parts worth testing are the hand-written
layer, the codegen's decisions, and the places where Gtk 4 changed semantics.

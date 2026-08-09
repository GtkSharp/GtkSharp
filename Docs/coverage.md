# Coverage of the hand-written code

Measured at **1513 tests** (1511 passing, 2 skipped) on Windows, `Release`.

```sh
dotnet test Source/Tests/GtkSharp.Tests -c Release --collect:"XPlat Code Coverage" \
  --results-directory BuildOutput/Coverage
```

For how the suite is built and what a test in it is expected to be, see
[testing.md](testing.md). Coverage here is a **map of where to look next**, never
a target — every defect this suite has found came from asking what a piece of
code promises, not from raising a percentage.

---

## Why only the hand-written code

| | covered / total | rate |
|:--|--:|--:|
| **hand-written** | **14 966 / 22 512** | **66.5%** |
| generated | 33 038 / 233 316 | 14.2% |

The generated number is close to meaningless and including it in a headline is
actively misleading. `Generated/` is a mechanical rendering of the `api.xml`: it
contains a wrapper for every function in every bound library, most of which no
application will ever call, and covering it would mean calling all of Gtk. Its
14% is not a gap to close.

The hand-written code is different in kind. Somebody typed it, usually *because*
codegen could not express something — an array whose length is a sibling
parameter, a struct embedded by value, a lifetime the api.xml cannot describe.
**Every defect found in this suite's history has lived there**, and nothing about
it is checked by compiling.

---

## By assembly, hand-written only

Ordered by uncovered lines, because that is the ordering that says where the work
is. A rate on its own hides how much code is behind it.

| assembly | covered / total | rate | uncovered |
|:--|--:|--:|--:|
| `GLibSharp` | 6 974 / 9 662 | 72.2% | **2 688** |
| `GtkSharp` | 2 584 / 4 532 | 57.0% | **1 948** |
| `Shared` | 1 056 / 2 016 | 52.4% | **960** |
| `CairoSharp` | 2 270 / 3 168 | 71.7% | **898** |
| `GdkSharp` | 536 / 942 | 56.9% | 406 |
| `PangoSharp` | 716 / 1 084 | 66.1% | 368 |
| `GioSharp` | 284 / 462 | 61.5% | 178 |
| `GskSharp` | 396 / 476 | 83.2% | 80 |
| `GtkSourceSharp` | 2 / 12 | 16.7% | 10 |
| `AdwaitaSharp` | 40 / 46 | 87.0% | 6 |
| `WebkitGtkSharp` | 2 / 6 | 33.3% | 4 |
| `GrapheneSharp` | 104 / 104 | 100.0% | 0 |
| `JavaScriptCoreSharp` | 2 / 2 | 100.0% | 0 |

**The bottom five rates mean nothing.** `GtkSourceSharp` has twelve hand-written
lines and `JavaScriptCoreSharp` has two; those assemblies are almost entirely
generated, so their hand-written rate is a rounding artefact. They are tested —
see `SatelliteAssemblyTests` — just not *here*.

`GrapheneSharp` at 100% is real but small: 104 lines, all of it
`FixedVertexArrays.cs`, covered by `GrapheneVertexArrayTests`.

---

## What 66.5% does not mean

About **a sixth of the uncovered lines cannot be covered by this suite at all**,
and reading the number without that is how a coverage target turns into busywork.

### Platform-split code — roughly 960 lines

`Shared/FuncLoader.cs` (31%, 546 uncovered) and `Shared/GLibrary.cs` (66%, 414
uncovered) are the entire cross-platform loading layer. They branch on the
operating system, so on any one run the other platforms' branches are dead by
construction. A Windows run cannot execute `dlopen`; a Linux run cannot execute
`LoadLibrary`.

The honest measurement would union a Windows run with a Linux one. Until that
exists, treat `Shared` as **not a gap** — and note that its code *is* exercised,
constantly, by every other test in the suite.

### Dead code — 242 lines

`GtkSharp/SignalConnector.cs` (178 lines, 0%) and
`GtkSharp/HandlerNotFoundException.cs` (64 lines, 0%) are unreachable.

Gtk 4 removed `gtk_builder_connect_signals_full`, so both public entry points of
`SignalConnector` throw `NotSupportedException` immediately. Its ~150-line
`ConnectFunc` — the reflection machinery that used to match handler names to
events — **has no caller anywhere in the tree**, and `HandlerNotFoundException`
is thrown only from inside it.

This is not untested code. It is code that cannot run, and it will stay at 0%
until either `GtkBuilderScope` is implemented (see `testing.md`) or it is
deleted. Counting it as a gap invites somebody to write tests for a dead path.

### Backends that need a display server — 70 lines

`CairoSharp/XlibSurface.cs` (52) and `XcbSurface.cs` (18) are X11-only surface
types. Nothing on Windows can reach them, and the suite draws to image surfaces
by design.

### ABI declarations — roughly 134 lines

`Cond.cs`, `Mutex.cs`, `RecMutex.cs`, `PollFD.cs` and the two
`GLibSharp.Source*Native.cs` files are mostly field declarations and native
callback shims that exist to describe a layout, not to be called.

**Adjusted, the reachable hand-written rate is closer to 71%** — 14 966 of about
21 100 lines. That is the number worth moving.

---

## Where the work actually is

The twenty-five hand-written files with the most uncovered lines. The right-hand
column is the judgement, not the tool's.

| file | covered / total | uncovered | what it is |
|:--|--:|--:|:--|
| `Shared/FuncLoader.cs` | 246 / 792 | 546 | platform-split; not a gap |
| `Shared/GLibrary.cs` | 810 / 1 224 | 414 | platform-split; not a gap |
| `CairoSharp/Context.cs` | 562 / 850 | 288 | **real** — the drawing API's breadth |
| `GLibSharp/Value.cs` | 574 / 806 | 232 | **real** — GValue conversions per type |
| `GtkSharp/TreeStore.cs` | 178 / 366 | 188 | **real**, but deprecated in Gtk 4 |
| `GdkSharp/Pixbuf.cs` | 206 / 386 | 180 | **real** — save/load formats |
| `GLibSharp/Object.cs` | 1 038 / 1 216 | 178 | **real** — property and vfunc plumbing |
| `GtkSharp/SignalConnector.cs` | 0 / 178 | 178 | dead; see above |
| `GLibSharp/Source.cs` | 152 / 310 | 158 | **real** — custom GSource subclassing |
| `GLibSharp/KeyFile.cs` | 614 / 756 | 142 | mostly covered already |
| `GtkSharp/ListStore.cs` | 152 / 294 | 142 | **real**, deprecated |
| `GLibSharp/Date.cs` | 206 / 344 | 138 | **real** — calendar arithmetic |
| `GLibSharp/DateTime.cs` | 250 / 382 | 132 | **real** — timezones, formatting |
| `GtkSharp/TreeModelFilter.cs` | 50 / 182 | 132 | **real**, deprecated |
| `GtkSharp/TreeModelSort.cs` | 38 / 164 | 126 | **real**, deprecated |
| `GLibSharp/Log.cs` | 88 / 206 | 118 | **real** — handlers, fatal masks |
| `GioSharp/GioStream.cs` | 188 / 298 | 110 | **real** — Stream adapter seek/length |
| `GLibSharp/Spawn.cs` | 106 / 196 | 90 | **real** — process spawning |
| `GLibSharp/IOChannel.cs` | 216 / 304 | 88 | mostly covered |
| `GLibSharp/Signal.cs` | 268 / 350 | 82 | mostly covered |
| `GtkSharp/TextBuffer.cs` | 18 / 100 | 82 | **real** — serialise/deserialise |
| `CairoSharp/Surface.cs` | 128 / 206 | 78 | **real** — surface types |
| `GLibSharp/TimeVal.cs` | 0 / 78 | 78 | deprecated in GLib itself |
| `GtkSharp/NodeStore.cs` | 344 / 416 | 72 | mostly covered |

### The three worth doing next

1. **`GLibSharp/Value.cs`** — 232 lines. Every property read and write in the
   binding goes through `GValue`, and the conversions are per-type and
   hand-written. A wrong one is silent: you get a default instead of your value.
   `ObjectAndValueTests` covers the common types; the boxed, flags, pointer and
   `GType` paths are thin.

2. **`GLibSharp/Source.cs`** — 158 lines. Subclassing `GSource` from C# means a
   managed object driving the main loop's dispatch. `MainLoopTests` covers idles
   and timeouts, which are the *built-in* sources; the custom-source path is the
   one with a vtable in it.

3. **`CairoSharp/Context.cs`** — 288 lines. Entirely hand-written, no codegen,
   and the pixel-reading technique in `testing.md` gives it real oracles cheaply.

### What to leave alone

The four `Tree*` files total **588 uncovered lines**, which makes them the
largest single block after `Shared`. They are also `GtkTreeView`'s model layer,
deprecated in Gtk 4 and replaced by the `GListModel` pipeline that
`ListViewTests` covers. Testing them raises the number without protecting
anything anyone should be writing. `TreeModelImplementorTests` already covers the
part that still matters — implementing a model from C#.

---

## Files with no coverage at all

51 files, 1 274 lines. Ordered by size; the triage above accounts for the top of
the list.

| file | lines | |
|:--|--:|:--|
| `GtkSharp/SignalConnector.cs` | 178 | dead |
| `GLibSharp/TimeVal.cs` | 78 | deprecated in GLib |
| `GLibSharp/GLibSharp.SourceDummyMarshalNative.cs` | 70 | callback shim |
| `GLibSharp/GLibSharp.SourceFuncNative.cs` | 70 | callback shim |
| `GtkSharp/HandlerNotFoundException.cs` | 64 | dead |
| `CairoSharp/XlibSurface.cs` | 52 | X11 only |
| `GtkSharp/Image.cs` | 46 | **worth testing** |
| `GdkSharp/PixbufAnimation.cs` | 44 | **worth testing** |
| `GtkSharp/PaperSize.cs` | 42 | **worth testing** |
| `GLibSharp/Cond.cs` | 38 | ABI declaration |
| `GLibSharp/Mutex.cs` | 32 | ABI declaration |
| `GLibSharp/PollFD.cs` | 32 | ABI declaration |
| `GLibSharp/RecMutex.cs` | 32 | ABI declaration |
| `GtkSharp/BindingAttribute.cs` | 22 | reached indirectly by `BuilderBindingTests` |
| `GtkSharp/CssProvider.cs` | 22 | **worth testing** |
| `GtkSharp/IconView.cs` | 22 | deprecated |
| `PangoSharp/Analysis.cs` | 20 | **worth testing** |
| `GdkSharp/DisplayManager.cs` | 18 | needs a display server |

A file at 0% is worth a minute of triage before it is worth a test. Roughly half
of these are unreachable, deprecated, or declarations — and the other half are
small enough to be cheap.

---

## Reproducing this

```sh
dotnet cake build.cake --BuildTarget=Build --Configuration=Release
dotnet test Source/Tests/GtkSharp.Tests -c Release --no-build \
  --collect:"XPlat Code Coverage" --results-directory BuildOutput/Coverage
```

Then read `BuildOutput/Coverage/*/coverage.cobertura.xml`, keeping only classes
whose `filename` starts with `Libs/` and does **not** contain `/Generated/`.

Note that cobertura's `filename` is relative to `Source/` and uses backslashes on
Windows — a filter written for forward slashes silently matches nothing and
reports a coverage of zero over zero lines.

# Coverage of the hand-written code

Measured at **1546 tests** (1531 passing, 15 skipped) on Windows, `Release`,
with generated code excluded by the collector.

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
| **hand-written** | **14 492 / 21 970** | **66.0%** |
| generated, still reported | 1 190 / 1 282 | 92.8% |

**The generated code is now excluded by the collector itself**, so these are the
only numbers a run produces. Before that exclusion the report carried 233 316
generated lines at 14.2%, which dragged the headline figure from 66% to 17% and
buried the one that can be acted on. `Generated/` is a mechanical rendering of
the `api.xml`: a wrapper for every function in every bound library, most of which
no application will ever call. Covering it would mean calling all of Gtk.

The 1 282 lines still reported are explained under
[what still gets through](#what-still-gets-through) below.

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
| `GLibSharp` | 6 866 / 9 518 | 72.1% | **2 652** |
| `GtkSharp` | 2 412 / 4 340 | 55.6% | **1 928** |
| `Shared` | 1 056 / 2 016 | 52.4% | **960** |
| `CairoSharp` | 2 242 / 3 136 | 71.5% | **894** |
| `GdkSharp` | 488 / 886 | 55.1% | 398 |
| `PangoSharp` | 672 / 1 040 | 64.6% | 368 |
| `GioSharp` | 280 / 458 | 61.1% | 178 |
| `GskSharp` | 330 / 410 | 80.5% | 80 |
| `GtkSourceSharp` | 2 / 12 | 16.7% | 10 |
| `AdwaitaSharp` | 36 / 42 | 85.7% | 6 |
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

## What 66.0% does not mean

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

### Dead code, about 240 lines

`GtkSharp/SignalConnector.cs` (174 lines, 0%) and
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

**Adjusted, the reachable hand-written rate is closer to 71%**: 14 492 of about
20 500 lines. That is the number worth moving.

---

## Where the work actually is

The twenty-five hand-written files with the most uncovered lines. The right-hand
column is the judgement, not the tool's.

| file | covered / total | uncovered | what it is |
|:--|--:|--:|:--|
| `Shared/FuncLoader.cs` | 246 / 792 | 546 | platform-split; not a gap |
| `Shared/GLibrary.cs` | 810 / 1 224 | 414 | platform-split; not a gap |
| `CairoSharp/Context.cs` | 560 / 848 | 288 | **real**: the drawing API's breadth |
| `GLibSharp/Value.cs` | 574 / 806 | 232 | **real**: GValue conversions per type |
| `GtkSharp/TreeStore.cs` | 148 / 336 | 188 | **real**, but deprecated in Gtk 4 |
| `GLibSharp/Object.cs` | 1 024 / 1 202 | 178 | **real**: property and vfunc plumbing |
| `GdkSharp/Pixbuf.cs` | 166 / 342 | 176 | **real**: save and load formats |
| `GtkSharp/SignalConnector.cs` | 0 / 174 | 174 | dead; see above |
| `GLibSharp/Source.cs` | 150 / 308 | 158 | **real**: custom GSource subclassing |
| `GLibSharp/KeyFile.cs` | 610 / 752 | 142 | mostly covered already |
| `GtkSharp/ListStore.cs` | 144 / 286 | 142 | **real**, deprecated |
| `GLibSharp/Date.cs` | 204 / 342 | 138 | **real**: calendar arithmetic |
| `GLibSharp/DateTime.cs` | 248 / 380 | 132 | **real**: timezones, formatting |
| `GtkSharp/TreeModelFilter.cs` | 42 / 174 | 132 | **real**, deprecated |
| `GtkSharp/TreeModelSort.cs` | 34 / 160 | 126 | **real**, deprecated |
| `GLibSharp/Log.cs` | 88 / 200 | 112 | **real**: handlers, fatal masks |
| `GioSharp/GioStream.cs` | 188 / 298 | 110 | **real**: Stream adapter seek and length |
| `GLibSharp/Spawn.cs` | 102 / 192 | 90 | **real**: process spawning |
| `GLibSharp/IOChannel.cs` | 214 / 298 | 84 | mostly covered |
| `GtkSharp/TextBuffer.cs` | 8 / 90 | 82 | **real**: serialise and deserialise |
| `GLibSharp/Signal.cs` | 262 / 342 | 80 | mostly covered |
| `GLibSharp/TimeVal.cs` | 0 / 78 | 78 | deprecated in GLib itself |
| `CairoSharp/Surface.cs` | 126 / 202 | 76 | **real**: surface types |
| `GtkSharp/NodeStore.cs` | 344 / 416 | 72 | mostly covered |
| `GLibSharp/ValueArray.cs` | 92 / 160 | 68 | **real**, and small |

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

53 files, 1 272 lines. Ordered by size; the triage above accounts for the top of
the list.

| file | lines | |
|:--|--:|:--|
| `GtkSharp/SignalConnector.cs` | 174 | dead |
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

## How the exclusion works

Three mechanisms, and conflating them is the usual mistake. Each does one thing
and none of them does the others' job.

### 1. `// <auto-generated />` — for Roslyn

Every file `GapiCodegen` writes begins with

```csharp
// <auto-generated />
// This file was generated by the Gtk# code generator.
// Any changes made will be lost if regenerated.
```

Roslyn decides a file is generated from that leading comment and skips
**analyser** diagnostics for it. Worth having on its own: 2 100 files nobody can
edit should not produce suggestions.

It does **not** suppress compiler warnings — which is why the
`#pragma warning disable CS0612, CS0618` underneath it is still load-bearing —
and it does **not** affect coverage. No collector reads it.

### 2. `[ExcludeFromCodeCoverage]` — for dotCover and anything attribute-driven

`System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage` is the portable
mechanism: dotCover, coverlet and Visual Studio all honour it, and unlike a path
glob it travels with the assembly.

**Where it goes is the whole problem.** An attribute on *any* part of a partial
type applies to the *whole* type, and hand-written partial classes are this
binding's main customisation mechanism — **143 types have one**. Putting the
attribute on the generated half of `Gtk.Button` would silently stop measuring
`Source/Libs/GtkSharp/Button.cs` too, which is the code most worth measuring.

So `Source/Tools/GapiCodegen/CoverageExclusion.cs` splits it:

| the generated type | gets the attribute on |
|:--|:--|
| nobody has extended it by hand | the **type** — which also covers its static field initialisers |
| a hand-written partial shares it | each generated **member**, leaving the hand-written members measured |

Which is which is worked out from the tree rather than configured. The
hand-written partials live in the parent of the output directory, so adding one
later flips that type to per-member marking on the next build, with nothing to
remember. If that directory cannot be read, everything falls back to per-member,
because that answer is always *correct* — it is the type-level attribute that can
over-reach.

```csharp
// Gtk/Scale.cs — nothing extends it
[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public partial class Scale : Gtk.Range {

// Gtk/Button.cs — Source/Libs/GtkSharp/Button.cs extends it
public partial class Button : Gtk.Widget, Gtk.IActionable {
        [global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        public Button (IntPtr raw) : base(raw) {}
```

### 3. `coverlet.runsettings` — for the collector used here

```xml
<ExcludeByFile>**/Generated/**/*.cs</ExcludeByFile>
<ExcludeByAttribute>GeneratedCodeAttribute,CompilerGeneratedAttribute,ExcludeFromCodeCoverageAttribute</ExcludeByAttribute>
<SkipAutoProps>true</SkipAutoProps>
```

`GtkSharp.Tests.csproj` points `RunSettingsFilePath` at it, so any `dotnet test`
run picks it up without a command-line flag. That is what "automatically" means
here: there is nothing to remember.

`SkipAutoProps` is why the hand-written totals are smaller than they were before
this change (21 970 rather than 22 512). Trivial property accessors were being
counted, and counting them flatters the number.

> **Editing that file**: an XML comment may not contain two consecutive hyphens.
> VSTest rejects the whole settings file if it does, reporting "Settings file
> provided does not conform to required format" — and then runs the tests anyway
> with **no coverage configuration at all**, which looks exactly like the
> exclusion silently not working. This repository has now been bitten by the same
> rule three times: a `.props` file, a `.metadata` file, and this one.

### What still gets through

1 282 lines, and they are all one thing: **static field initialisers in
hand-extended types**.

```
Libs/GioSharp/Generated/GLib/FileAdapter.cs   143 lines, every one in .cctor
    static d_g_file_append_to g_file_append_to = FuncLoader.LoadFunction<...>(...);
```

Those initialisers compile into the type's *implicit class constructor*, and no
member attribute can reach it — a `.cctor` belongs to the type, and the type
deliberately does not carry the attribute here, because a hand-written partial
shares it.

**It could be closed, and deliberately is not.** Emitting an explicit
`[ExcludeFromCodeCoverage] static Button () {}` would pull those initialisers into
an attributed constructor — but the hand-written half's initialisers compile into
the *same* constructor and would stop being measured too, and declaring a static
constructor removes `beforefieldinit`, which changes *when* type initialisation
runs. Neither is worth trading for a cosmetic number.

The residue is confined to the 35 hand-extended types that have generated static
initialisers, it is counted separately from the hand-written figure above, and it
does not move the number anyone acts on.

## Reproducing this

```sh
dotnet cake build.cake --BuildTarget=Build --Configuration=Release
dotnet test Source/Tests/GtkSharp.Tests -c Release --no-build \
  --collect:"XPlat Code Coverage" --results-directory BuildOutput/Coverage
```

The runsettings is applied automatically, so the report already excludes
`Generated/`. Read `BuildOutput/Coverage/*/coverage.cobertura.xml`; to split
hand-written from the partial-class residue described above, keep classes whose
`filename` starts with `Libs/` and does not contain `/Generated/`.

Note that cobertura's `filename` is relative to `Source/` and uses backslashes on
Windows. A filter written for forward slashes silently matches nothing and
reports a coverage of zero over zero lines, which is how the first version of
this analysis died.

# Coverage of the hand-written code

Measured at **1569 tests** (1533 passing, 36 skipped) on Windows, `Release`,
with generated code excluded by the collector.

> **Every line count below is half what it was**, and no rate moved. Cobertura
> lists each line twice — once under `<class><lines>` and again under
> `<class><methods><method><lines>` — so a flat walk of `line` elements counts
> everything exactly twice, which is what the earlier revisions of this document
> did. The tell was that every single figure in every table was even. Ratios were
> never affected, so the conclusions were right for the wrong arithmetic; the
> absolute numbers now say what they claim to. Count `<class><lines>` only.

```sh
dotnet test Source/Tests/GtkSharp.Tests -c Release --collect:"XPlat Code Coverage" \
  --results-directory BuildOutput/Coverage
```

For how the suite is built and what a test in it is expected to be, see
[testing.md](testing.md). Coverage here is a **map of where to look next**, never
a target — every defect this suite has found came from asking what a piece of
code promises, not from raising a percentage.

The two most recent tests are a case in point. `WidgetBehaviourTests` gained
`An_entry_delegates_keys_to_an_internal_text_child` and
`A_controller_propagates_in_the_bubble_phase_unless_told_otherwise`, which pin
why an `EventControllerKey` attached to an `Entry` never fires — the `Entry` is a
shell around a `GtkText` that takes the focus and consumes the key, and a
controller defaults to the `Bubble` phase, which runs outwards from that target.
Everything they touch is generated and therefore excluded from every table below,
so they barely register as coverage. They were still worth writing: the test that
*did* exist for `EventControllerKey` emits the signal on the controller directly,
which skips propagation entirely, and it passed throughout.

---

## Why only the hand-written code

| | covered / total | rate |
|:--|--:|--:|
| **hand-written** | **7 247 / 11 010** | **65.8%** |
| generated, still reported | 595 / 641 | 92.8% |

**The generated code is now excluded by the collector itself**, so these are the
only numbers a run produces. Before that exclusion the report carried about
116 700 generated lines at 14.2%, which dragged the headline figure from 66% to
17% and buried the one that can be acted on. `Generated/` is a mechanical
rendering of the `api.xml`: a wrapper for every function in every bound library,
most of which no application will ever call. Covering it would mean calling all
of Gtk.

The 641 lines still reported are explained under
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
| `GLibSharp` | 3 433 / 4 759 | 72.1% | **1 326** |
| `GtkSharp` | 1 206 / 2 170 | 55.6% | **964** |
| `Shared` | 529 / 1 008 | 52.5% | **479** |
| `CairoSharp` | 1 121 / 1 568 | 71.5% | **447** |
| `GdkSharp` | 244 / 443 | 55.1% | 199 |
| `PangoSharp` | 336 / 520 | 64.6% | 184 |
| `GioSharp` | 140 / 229 | 61.1% | 89 |
| `GskSharp` | 165 / 205 | 80.5% | 40 |
| `JavaScriptCoreSharp` | 1 / 26 | *n/a* | 25 |
| `GtkSourceSharp` | 1 / 6 | *n/a* | 5 |
| `AdwaitaSharp` | 18 / 21 | *n/a* | 3 |
| `WebkitGtkSharp` | 1 / 3 | *n/a* | 2 |
| `GrapheneSharp` | 52 / 52 | 100.0% | 0 |

**Four of these rates are marked *n/a* because they are not measurements.**
`GtkSourceSharp` has six hand-written lines, `WebkitGtkSharp` three. Those
assemblies are almost entirely generated, and generated code is excluded from
this table by design — so their number is computed over a handful of lines and
**cannot move however many tests are added**. `GtkSourceTests` added twenty tests
and `WebKitTests` thirteen without shifting either figure by a line, which is the
clearest possible demonstration that the figure is not about them.

`JavaScriptCoreSharp` is the same shape but no longer trivial: it went from two
hand-written lines to twenty-six when `JavaScriptCoreSharp/Value.cs` was added,
and that file is untested. Twenty-five uncovered lines in an assembly that size
is a rate of 3.8%, which says nothing — but the file is small enough to be cheap,
and it is listed under [files with no coverage](#files-with-no-coverage-at-all).

For those assemblies, see [the generated surface](#the-generated-surface) below.
It is the only lens that shows their tests at all.

`GrapheneSharp` at 100% is real but small: 52 lines, all of it
`FixedVertexArrays.cs`, covered by `GrapheneVertexArrayTests`.

---

## What 65.8% does not mean

About **a fifth of the uncovered lines cannot be covered by this suite at all**,
and reading the number without that is how a coverage target turns into busywork.

### Platform-split code — roughly 479 lines

`Shared/FuncLoader.cs` (31%, 273 uncovered) and `Shared/GLibrary.cs` (66%, 206
uncovered) are the entire cross-platform loading layer. They branch on the
operating system, so on any one run the other platforms' branches are dead by
construction. A Windows run cannot execute `dlopen`; a Linux run cannot execute
`LoadLibrary`.

The honest measurement would union a Windows run with a Linux one. Until that
exists, treat `Shared` as **not a gap** — and note that its code *is* exercised,
constantly, by every other test in the suite.

### Dead code, about 119 lines

`GtkSharp/SignalConnector.cs` (87 lines, 0%) and
`GtkSharp/HandlerNotFoundException.cs` (32 lines, 0%) are unreachable.

Gtk 4 removed `gtk_builder_connect_signals_full`, so both public entry points of
`SignalConnector` throw `NotSupportedException` immediately. Its `ConnectFunc` —
the reflection machinery that used to match handler names to events — **has no
caller anywhere in the tree**, and `HandlerNotFoundException` is thrown only from
inside it.

This is not untested code. It is code that cannot run, and it will stay at 0%
until either `GtkBuilderScope` is implemented (see `testing.md`) or it is
deleted. Counting it as a gap invites somebody to write tests for a dead path.

### Backends that need a display server — 35 lines

`CairoSharp/XlibSurface.cs` (26) and `XcbSurface.cs` (9) are X11-only surface
types. Nothing on Windows can reach them, and the suite draws to image surfaces
by design.

### ABI declarations — 133 lines

`Cond.cs` (19), `Mutex.cs` (16), `RecMutex.cs` (16), `PollFD.cs` (16) and the two
`GLibSharp.Source*Native.cs` files (33 each) are mostly field declarations and
native callback shims that exist to describe a layout, not to be called.

**Adjusted, the reachable hand-written rate is closer to 71%**: 7 247 of about
10 244 lines. That is the number worth moving.

---

## Where the work actually is

The twenty-five hand-written files with the most uncovered lines. The right-hand
column is the judgement, not the tool's.

| file | covered / total | uncovered | what it is |
|:--|--:|--:|:--|
| `Shared/FuncLoader.cs` | 123 / 396 | 273 | platform-split; not a gap |
| `Shared/GLibrary.cs` | 406 / 612 | 206 | platform-split; not a gap |
| `CairoSharp/Context.cs` | 280 / 424 | 144 | **real**: the drawing API's breadth |
| `GLibSharp/Value.cs` | 287 / 403 | 116 | **real**: GValue conversions per type |
| `GtkSharp/TreeStore.cs` | 74 / 168 | 94 | **real**, but deprecated in Gtk 4 |
| `GLibSharp/Object.cs` | 512 / 601 | 89 | **real**: property and vfunc plumbing |
| `GdkSharp/Pixbuf.cs` | 83 / 171 | 88 | **real**: save and load formats |
| `GtkSharp/SignalConnector.cs` | 0 / 87 | 87 | dead; see above |
| `GLibSharp/Source.cs` | 75 / 154 | 79 | **real**: custom GSource subclassing |
| `GLibSharp/KeyFile.cs` | 305 / 376 | 71 | mostly covered already |
| `GtkSharp/ListStore.cs` | 72 / 143 | 71 | **real**, deprecated |
| `GLibSharp/Date.cs` | 102 / 171 | 69 | **real**: calendar arithmetic |
| `GLibSharp/DateTime.cs` | 124 / 190 | 66 | **real**: timezones, formatting |
| `GtkSharp/TreeModelFilter.cs` | 21 / 87 | 66 | **real**, deprecated |
| `GtkSharp/TreeModelSort.cs` | 17 / 80 | 63 | **real**, deprecated |
| `GLibSharp/Log.cs` | 44 / 100 | 56 | **real**: handlers, fatal masks |
| `GioSharp/GioStream.cs` | 94 / 149 | 55 | **real**: Stream adapter seek and length |
| `GLibSharp/Spawn.cs` | 51 / 96 | 45 | **real**: process spawning |
| `GLibSharp/IOChannel.cs` | 107 / 149 | 42 | mostly covered |
| `GtkSharp/TextBuffer.cs` | 4 / 45 | 41 | **real**: serialise and deserialise |
| `GLibSharp/Signal.cs` | 131 / 171 | 40 | mostly covered |
| `GLibSharp/TimeVal.cs` | 0 / 39 | 39 | deprecated in GLib itself |
| `CairoSharp/Surface.cs` | 63 / 101 | 38 | **real**: surface types |
| `GtkSharp/NodeStore.cs` | 172 / 208 | 36 | mostly covered |
| `GLibSharp/ValueArray.cs` | 46 / 80 | 34 | **real**, and small |

### The three worth doing next

1. **`GLibSharp/Value.cs`** — 116 lines. Every property read and write in the
   binding goes through `GValue`, and the conversions are per-type and
   hand-written. A wrong one is silent: you get a default instead of your value.
   `ObjectAndValueTests` covers the common types; the boxed, flags, pointer and
   `GType` paths are thin.

2. **`GLibSharp/Source.cs`** — 79 lines. Subclassing `GSource` from C# means a
   managed object driving the main loop's dispatch. `MainLoopTests` covers idles
   and timeouts, which are the *built-in* sources; the custom-source path is the
   one with a vtable in it.

3. **`CairoSharp/Context.cs`** — 144 lines. Entirely hand-written, no codegen,
   and the pixel-reading technique in `testing.md` gives it real oracles cheaply.

### What to leave alone

The four `Tree*` files total **294 uncovered lines**, which makes them the
largest single block after `Shared`. They are also `GtkTreeView`'s model layer,
deprecated in Gtk 4 and replaced by the `GListModel` pipeline that
`ListViewTests` covers. Testing them raises the number without protecting
anything anyone should be writing. `TreeModelImplementorTests` already covers the
part that still matters — implementing a model from C#.

---

## Files with no coverage at all

71 files, 692 lines. Ordered by size; the triage above accounts for the top of
the list.

| file | lines | |
|:--|--:|:--|
| `GtkSharp/SignalConnector.cs` | 87 | dead |
| `GLibSharp/TimeVal.cs` | 39 | deprecated in GLib |
| `GLibSharp/GLibSharp.SourceDummyMarshalNative.cs` | 33 | callback shim |
| `GLibSharp/GLibSharp.SourceFuncNative.cs` | 33 | callback shim |
| `GtkSharp/HandlerNotFoundException.cs` | 32 | dead |
| `CairoSharp/XlibSurface.cs` | 26 | X11 only |
| `JavaScriptCoreSharp/Value.cs` | 25 | **worth testing**; newly added |
| `GtkSharp/Image.cs` | 23 | **worth testing** |
| `GtkSharp/PaperSize.cs` | 21 | **worth testing** |
| `GdkSharp/PixbufAnimation.cs` | 20 | **worth testing** |
| `GLibSharp/Cond.cs` | 19 | ABI declaration |
| `GLibSharp/Mutex.cs` | 16 | ABI declaration |
| `GLibSharp/PollFD.cs` | 16 | ABI declaration |
| `GLibSharp/RecMutex.cs` | 16 | ABI declaration |
| `GLibSharp/FileUtils.cs` | 11 | **worth testing** |
| `GtkSharp/BindingAttribute.cs` | 11 | reached indirectly by `BuilderBindingTests` |
| `GtkSharp/CssProvider.cs` | 11 | **worth testing** |
| `GtkSharp/IconView.cs` | 10 | deprecated |
| `PangoSharp/Analysis.cs` | 10 | **worth testing** |
| `CairoSharp/XcbSurface.cs` | 9 | X11 only |

A file at 0% is worth a minute of triage before it is worth a test. Roughly half
of these are unreachable, deprecated, or declarations — and the other half are
small enough to be cheap.

---

## The generated surface

The table above is deliberately blind to generated code, and for most assemblies
that is right — the hand-written layer is where the defects are. For the four
assemblies that are *almost entirely* generated it leaves nothing to look at, so
this is the other lens: a diagnostic run with the exclusions turned off.

```sh
# not how coverage is normally collected -- see below
cat > diag.runsettings <<'XML'
<RunSettings><DataCollectionRunSettings><DataCollectors>
  <DataCollector friendlyName="XPlat code coverage">
    <Configuration><Format>cobertura</Format></Configuration>
  </DataCollector>
</DataCollectors></DataCollectionRunSettings></RunSettings>
XML

dotnet test Source/Tests/GtkSharp.Tests -c Release --no-build \
  --collect:"XPlat Code Coverage" --settings diag.runsettings
```

Measured on Windows at 1569 tests:

| assembly | generated lines covered | rate |
|:--|--:|--:|
| `GrapheneSharp` | 1 204 / 1 986 | 61% |
| `PangoSharp` | 620 / 1 679 | 37% |
| `GskSharp` | 251 / 864 | 29% |
| `GtkSharp` | 3 570 / 13 766 | 26% |
| `AdwaitaSharp` | 93 / 494 | 19% |
| `GdkSharp` | 282 / 2 310 | 12% |
| `GioSharp` | 505 / 4 629 | 11% |
| `GtkSourceSharp` | 99 / 1 142 | 9% |
| `WebkitGtkSharp` | 0 / 1 448 | **0%, and not for the reason it looks** |
| `JavaScriptCoreSharp` | 0 / 362 | **0%, same** |

**Those two zeroes are an artefact of where the run happened.** gvsbuild ships no
WebKit, so on Windows the thirteen `WebKitTests` and the two older
`OptionalLibraryTests` *skip* — the assembly is never loaded, and a line that
never runs on a machine that cannot run it is not an untested line. On Linux all
thirteen pass against WebKit 2.52.5. Measuring that surface needs a coverage run
on Linux, which this analysis does not yet have: `dotnet vstest` runs the suite
there but does not resolve the coverage collector, and `dotnet test` produces no
output at all under WSL against a tree restored on Windows. Worth fixing; not
fixed here, and stated rather than papered over.

`GtkSourceSharp` at 9% *is* a real measurement, and it did move: those 99 lines
are what `GtkSourceTests` and `SatelliteAssemblyTests` reach between them, out of
102 bound types.

**Do not turn this into a target.** 13 766 generated lines in `GtkSharp` is a
wrapper for every function in Gtk, and driving that number up means calling
functions for the sake of calling them — which is the assertion-free sweep
`testing.md` bans, wearing a percentage. Read it as *which assemblies are barely
touched*, and pick the ones an application would actually use.

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
this change — roughly 270 lines smaller. Trivial property accessors were being
counted, and counting them flatters the number.

> **Editing that file**: an XML comment may not contain two consecutive hyphens.
> VSTest rejects the whole settings file if it does, reporting "Settings file
> provided does not conform to required format" — and then runs the tests anyway
> with **no coverage configuration at all**, which looks exactly like the
> exclusion silently not working. This repository has now been bitten by the same
> rule three times: a `.props` file, a `.metadata` file, and this one.

### What still gets through

641 lines, and they are all one thing: **static field initialisers in
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

Two things about reading that file, both of which have already produced a wrong
answer here:

- **Count `<class><lines>` only.** Coverlet writes every line twice — once there
  and once under `<class><methods><method><lines>` — so a flat walk of all `line`
  elements returns exactly double. It divides out of any ratio, which is why the
  error survived several revisions: the rates were right and only the absolute
  counts were wrong. If every number in your table is even, this is why.
- **`filename` is relative to `Source/` and uses backslashes on Windows.** A
  filter written for forward slashes silently matches nothing and reports a
  coverage of zero over zero lines, which is how the first version of this
  analysis died.

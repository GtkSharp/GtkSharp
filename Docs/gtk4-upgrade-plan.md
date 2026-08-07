# Plan — Upgrade GtkSharp to GTK 4.22.4

**Status:** V1–V5 passed; gates 1 and 2 passed; **Phases 1–8 complete** — all 11 assemblies build clean, and the three GLib fundamental-type hierarchies (`GskRenderNode`, `GdkEvent`, `GtkExpression` — 57 types) are now bound, which unblocks `GtkSnapshot` drawing. **Phase 6 compiles: 0 errors across all 11 assemblies and Samples**, and the samples now run. Porting them exposed ten silent library defects, including one that prevented any Gtk 4 application from starting. Phase 7 packs templates and workload clean. **Phase 8 complete, verified against a real Gtk 4 runtime**: an xunit project (`Source/Tests/GtkSharp.Tests`) replaces the planned smoke flag — 83 tests, all passing and with no GLib diagnostics left, which found a wrong ABI on all 724 `throws` methods and 13 more removed-symbol null delegates. Phase 9 complete: all eight open items resolved (§15), and four further defects found while doing them. **Phase 10 complete — gate 7 passed**: the suite now runs on Linux (Debian trixie, Gtk 4.18.6, real WebKit), 226 of 227 passing with one documented skip, which found three more defects that Windows structurally could not show — including one where a single class missing from an older installed library disabled an entire assembly (§16). **Phase 11 complete**: 403 tests, hand-written coverage 30.5% → 43.6%, four further defects fixed — among them `GLib.PtrArray`, which had never worked at all because its symbols were looked up in the wrong library (§17). **Phase 12 complete**: 484 tests, 50.3%, six more defects — including `Cairo.Context.FontMatrix`, whose `out`-on-a-class ABI corrupted the stack and killed the test host outright (§18). **Phase 13 complete**: 556 tests, 52.7%, four more defects — among them `TreeModelSort.AppendValues` recursing into itself forever, and a constructor hazard that only reaches consumers of the package (§19). **Phase 14 complete**: 603 tests, and a defect in `GirToGapi` itself — a type-normalisation rule dropped a level of indirection on `const char* const*`, so 67 string-array parameters and returns across seven assemblies bound as a single string (§20). **Phases 15–19 complete**: five further sweeps — the rest of Cairo, hand-written GLib, the display-free Gdk, the text stack and the three satellite assemblies — taking the suite to 765 tests, the largest find being that codegen had emitted **no getter for any of the 889 public fields in the tree**. **Phase 21 complete**: 797 tests, still green on both platforms, from the first sweep over *authoring* — what happens when a C# program subclasses a Gtk type instead of calling one. Two defects: `Gtk.Widget.ConnectActivate` still wrote the activation signal's id into a `GtkWidgetClass` field Gtk 4 made private, so every managed subclass overriding `OnActivate` threw at class-init and could not be constructed at all; and codegen released a `(transfer full)` opaque argument before checking whether it was null, so `Widget.Allocate (w, h, b, null)` — the identity transform, which is what a layout manager passes for a child at the origin — threw `NullReferenceException` before reaching Gtk. **Phase 22 complete**: 835 tests, green on both platforms, from the first sweep over `GtkExpression` — the hierarchy the Gtk 4 list stack reads item values through, and the one nothing had called. Four defects: `Expression.Evaluate` and `ExpressionWatch.Evaluate` wrote the answer into unmanaged memory and freed it without reading it back, so every evaluation returned true and no value; `gtk_expression_bind` takes its receiver `(transfer full)` and the wrapper kept unreffing it too; `gtk_closure_expression_new` and `gtk_try_expression_new` took a single expression where an array was expected; and `gtk_cclosure_expression_new` was dropped by codegen entirely, leaving the type a Gtk 4 list view derives display values with unreachable. **Phase 23 complete**: 871 tests, green on both platforms, from the first sweep over the half of Gio that needs a filesystem and a main loop — GSettings against a schema the test compiles itself, GFileMonitor, the async pattern, GCancellable, and GFile's copy/move/delete. Six defects, the headline being that `GSettings::changed` and `GFileMonitor::changed` both wanted a class called `GLib.ChangedArgs`, so the file monitor's only signal handed its handler an object that read a `GFile` as a string and named neither the file nor the event. **Phase 24 complete**: 915 tests, green on both platforms, from the first sweep over graphene and the arithmetic half of Gsk — which turned out to be barely bound at all. Four defects, each of which had made a whole category unreachable or unsafe: graphene's gir spells a predicate's return type `bool` rather than `gboolean`, so codegen had **silently dropped all 49 methods returning one** (`matrix_inverse`, `matrix_decompose`, `rect_contains_point`, `rect_intersection`, every `_equal` and `_near`); a fixed-size array parameter came out as a single element, so `graphene_matrix_to_float` wrote sixty-four bytes through a float passed in a vector register, and `gsk_border_node_new` read four `GdkRGBA`s out of one; `graphene_simd4f_t`'s sixteen-byte alignment was invisible to the ABI description, so `graphene_plane_t`, `_euler_t` and `_sphere_t` were measured as 20 bytes against C's 32 and every caller-allocates out parameter of them overran by twelve; and the block those parameters allocate came from `g_malloc` while graphene frees it with `_aligned_free`, which is a heap corruption that lands minutes later on an unrelated test (`0xC0000374`). Four pieces of graphene behaviour are pinned because the obvious expectation is wrong, the headline being that `graphene_matrix_determinant` returns the **negative** determinant — the identity's is -1. **Phase 25 complete**: 951 tests, green on Windows and on Debian forky, from the first sweep over the controls an application is built out of and the two subsystems Gtk 4 replaced wholesale — the clipboard and drag-and-drop, neither of which had ever been called. Five defects: codegen emitted the `GError` check **after** converting the return value, so every failing `*_finish` that returns a struct by pointer threw `NullReferenceException` out of `Marshal.PtrToStructure` instead of the exception that says what went wrong, and `DropTarget.Value` — NULL whenever no drag is in progress, which is nearly always — threw on simply being read; `gtk_drop_target_set_gtypes` and `gdk_content_provider_new_union` are two more array-plus-count parameters bound as a single element, and each is the only way to do the thing it exists for; `GtkSelectionModel`'s `<prerequisite name="Gio.ListModel"/>` is dropped by `GirToGapi`, so `Gtk.Stack.Pages` came back as an adapter that could not be enumerated at all; and `GLib.Signal.Emit` could build neither a null object argument nor a `G_TYPE_VALUE` one, which between them are `GtkDropTarget`'s `accept` and `drop`. Three pieces of behaviour pinned because the obvious expectation is wrong, the headline being that `gtk_spin_button_spin` reads its `increment` argument for a step and ignores it for a page. **Phase 20 complete**: 766 tests, green on Windows *and* on Debian forky, with a CI workflow that can actually start — forky no longer packages `dotnet-sdk-8.0`, and neither glycin's image loaders nor WebKit's sandbox survive a container without a session bus, both of which abort the test host under a "Passed!" line rather than failing anything.
**Target:** GTK 4.22.4 (latest stable), replacing GTK 3.22/3.24 support
**Branch:** `gtk4` (cut from `develop` @ `c01f5f97d`)
**Package version line:** `4.22.4.x`
**Author:** drafted 2026-08-04; verification results folded in 2026-08-05

> Findings live in [gir-gapi-coverage.md](gir-gapi-coverage.md). Where measurement contradicted
> this plan, the section below is corrected in place and marked **[corrected]**.

---

## 0. Decisions taken (and their consequences)

These were settled before drafting. They are not up for re-litigation inside the plan; each one is load-bearing for everything below.

| # | Decision | Consequence |
|---|---|---|
| D1 | **New `GirToGapi` converter tool.** GTK4 api.xml is generated from upstream `.gir` files by a new tool in `Source/Tools/`. | The existing `GapiFixup` + `*.metadata` XPath mechanism survives intact. The converter becomes the single largest work item and the single largest correctness risk. |
| D2 | **Replace in place on a branch.** Same assembly names (`GtkSharp`, `GdkSharp`, …), same `Gtk`/`Gdk` namespaces, version `4.22.4.x`. | GTK3 support ends at the `3.24.24.x` package line on `develop`. Hard break for consumers — no side-by-side, which matches every other GTK4 binding effort. `develop` must be frozen or maintained separately. |
| D3 | **Full library scope.** Core (GLib, Gio, Cairo, Pango, **Graphene**, Gdk, **Gsk**, Gtk) **plus** GtkSourceView 5, WebKitGTK 6.0, and a **new** libadwaita-1 binding. `AtkSharp` is deleted. | 11 wrapper assemblies instead of 9. Three api.xml files (GtkSource, Webkit, Adwaita) are effectively new, not migrated. |
| D4 | **Full port depth.** Hand-written partial classes, all 37 sample sections, and all 12 template projects (including `.glade` → GTK4 `.ui`) are in scope. | Samples are the only verification mechanism this repo has (no test project — see `CLAUDE.md` §Tests). Porting them is not optional polish; it *is* the acceptance test. |

---

## 1. BLOCKING pre-flight verifications — ✅ ALL RESOLVED

**Do not write code until every item in this section is answered.** Each one can invalidate a whole phase below.

**Status as of 2026-08-05 — full evidence in [gir-gapi-coverage.md](gir-gapi-coverage.md):**

| | Question | Answer |
|:--|:---------|:-------|
| **V1** | Does GIR carry what the gapi schema needs? | **Yes.** Two gaps, neither blocking. `signal_vm` is not carried directly but is derivable — the rule scores **328/329 with zero false negatives** against GTK 3. **R1 is retired.** |
| **V2** | Where do the `.gir` files come from? | **Debian `forky`**, gtk4 exactly `4.22.4+ds-1`. Vendored in [../Source/Gir/](../Source/Gir/), sha256-pinned, re-fetchable with `Source/Gir/fetch-gir.py`. |
| **V3** | Are GDK/GSK inside `libgtk-4`? | **Confirmed** — `Gdk-4.0.gir` and `Gsk-4.0.gir` both declare `shared-library="libgtk-4.so.1"`. |
| **V4** | Windows runtime | **gvsbuild `2026.6.0`** (GTK4 4.22.4 + libadwaita 1.9.1 + GtkSourceView 5). No push access needed. **R6 resolved.** |
| **V5** | Does `GapiCodegen` need changes? | **No.** One dead symbol (`AtkFunction`) may be removed as cleanup. |

### V1 — Does GIR carry everything the gapi schema needs?

`Source/Libs/Shared/Gapi.xsd` describes constructs that GObject-Introspection does **not** obviously express. Before building the converter, take `gtk-4.0.gir` and confirm each of these has a faithful source:

| Gapi construct | Gapi.xsd | Suspected GIR source | Risk if absent |
|---|---|---|---|
| `<class_struct>` with `<method vm=… padding=… signal_vm=…>` | [Gapi.xsd:181-194](../Source/Libs/Shared/Gapi.xsd#L181) | `<record name="WidgetClass" glib:is-gtype-struct-for="Widget">` | **High** — drives `GObjectVM.cs` / `MethodABIField.cs`. Without it, virtual-method overriding and the `--abi-cs-usings` ABI check both break. |
| `<field bits="N">` | [Gapi.xsd:224](../Source/Libs/Shared/Gapi.xsd#L224) | `<field bits="N">` | Medium — struct layout. |
| `<childprop>` | [Gapi.xsd:161](../Source/Libs/Shared/Gapi.xsd#L161) | *none* — GTK4 removed child properties with `GtkContainer` | **None** — expected to be genuinely gone. Confirm, then drop from converter. |
| `<return-type owned= elements_owned= null_term_array=>` | [Gapi.xsd:280-287](../Source/Libs/Shared/Gapi.xsd#L280) | `transfer-ownership="full\|container\|none"`, `<array zero-terminated="1">` | Low — direct mapping. |
| `<parameter pass_as="out\|ref">` | [Gapi.xsd:271-276](../Source/Libs/Shared/Gapi.xsd#L271) | `direction="out\|inout"`, `caller-allocates` | Low. |
| `<parameter scope="call\|async\|notify">` | [Gapi.xsd:266](../Source/Libs/Shared/Gapi.xsd#L266) | `scope="call\|async\|notified"` | Low — note GIR spells it `notified`. |
| `<static-string>` | [Gapi.xsd:249-253](../Source/Libs/Shared/Gapi.xsd#L249) | `<constant>` | Low. |
| `<symbol type= cname= marshal_type= call_fmt= from_fmt=>` | [Gapi.xsd:53-61](../Source/Libs/Shared/Gapi.xsd#L53) | *none* — hand-authored | **None** — `GtkSharp-symbols.xml` stays hand-written. |
| `<alias>` | [Gapi.xsd:66-73](../Source/Libs/Shared/Gapi.xsd#L66) | `<alias>` | Low. |
| method `deprecated` | [Gapi.xsd:213](../Source/Libs/Shared/Gapi.xsd#L213) | `deprecated="1" deprecated-version=` | Low — but see R3, GTK 4.10+ deprecated a *lot*. |

**How to verify:** obtain `gtk-4.0.gir` for 4.22.4 and grep for each. Write the findings into `Docs/gir-gapi-coverage.md` before starting Phase 2. If `glib:is-gtype-struct-for` records do not carry enough to rebuild `<class_struct>`, escalate — the fallback is emitting `<class_struct>` from the GIR `<record>` plus a hand-maintained padding table, which is a materially bigger job.

### V2 — Where do the `.gir` files come from, and are they vendored?

Today `Source/Libs/<Name>/<Name>-api.xml` is **checked in**, so the build is hermetic and needs no GTK installed. That property must be preserved — CI is `ubuntu-22.04` (`.github/workflows/main.yml:9`) which cannot supply GTK 4.22 packages.

**Proposal:** vendor **both**. Check in `Source/Gir/<name>-<version>.gir` as the upstream input *and* the converted `Source/Libs/<Name>/<Name>-api.xml` as today. `GirToGapi` runs on demand (`--BuildTarget=RegenerateApi`), **not** on every build.

Confirm before coding:
- Exact GIR versions shipped with GTK 4.22.4 and companions (`gtk-4.0.gir`, `gdk-4.0.gir`, `gsk-4.0.gir`, `graphene-1.0.gir`, `pango-1.0.gir`, `gio-2.0.gir`, `glib-2.0.gir`, `gobject-2.0.gir`, `gdkpixbuf-2.0.gir`, `cairo-1.0.gir`, `gtksourceview-5.gir`, `webkitgtk-6.0.gir`, `libadwaita-1.gir`).
- Redistribution: the `.gir` files are LGPL-licensed alongside GTK. Vendoring them in an LGPL repo is fine, but record provenance (upstream tarball URL + sha256) in `Source/Gir/README.md`.
- `cairo-1.0.gir` is notoriously incomplete (Cairo has no real introspection). **`CairoSharp` has no `.metadata` and generates nothing today** — it stays 100 % hand-written. Do not attempt to generate it.

### V3 — GDK and GSK are *inside* `libgtk-4`, not separate shared libraries

This is the single most easily-missed runtime fact. In GTK 3 there are `libgdk-3.so.0` and `libgtk-3.so.0`. **In GTK 4 there is only `libgtk-4.so.1`** — GDK and GSK are compiled into it and export `gdk_*` / `gsk_*` symbols from that one object.

`Source/Libs/Shared/GLibrary.cs:28` currently maps `Library.Gdk` → `libgdk-3-0.dll`. If `Library.Gdk` and the new `Library.Gsk` are not both pointed at the GTK4 library filenames, **every generated GDK/GSK P/Invoke fails at first call with `DllNotFoundException`**, and it will look like a codegen bug.

Verify by inspecting a real 4.22.4 install: `nm -D /usr/lib/x86_64-linux-gnu/libgtk-4.so.1 | grep gdk_surface_new` should hit, and no `libgdk-4.so*` should exist.

### V4 — Windows runtime acquisition

[GtkSharp.targets:4-5](../Source/Libs/GtkSharp/GtkSharp.targets#L4) downloads `https://github.com/pieroviano/Dependencies/raw/master/gtk-3.24.24.zip` into `%LOCALAPPDATA%\Gtk\3.24.24`. **There is no `gtk-4.22.4.zip` in that repository.** This blocks Windows builds of Samples and every consumer of the `GtkSharp` package.

Decide before Phase 8:
1. Build a GTK 4.22.4 Windows bundle with [gvsbuild](https://github.com/wingtk/gvsbuild) and publish it to `GtkSharp/Dependencies` (needs push rights to that repo — **confirm access**), or
2. Point `GtkUrl` at a gvsbuild GitHub release asset directly (no repo rights needed; adds an external dependency on someone else's release cadence), or
3. Drop the auto-download and document MSYS2 (`pacman -S mingw-w64-x86_64-gtk4`) as a prerequisite.

The bundle must contain gtk4 **plus** graphene, gtksourceview-5, webkitgtk-6.0 and libadwaita-1 if those assemblies are to be runnable on Windows. WebKitGTK in particular has **no supported Windows build** — see R5.

### V5 — Does `GapiCodegen` itself need changes?

Read `Source/Tools/GapiCodegen/Parser.cs` and confirm it is version-agnostic. Specifically check:
- `SymbolTable.cs` for hardcoded `Gtk`/`Gdk` type names that GTK4 removed.
- `Options.cs` `--abi-cs-usings` handling — `Settings.cake:22` and `:41` pass `Atk` and `Gtk,GLib`; the Atk one disappears, and `Gsk`/`Graphene` may need adding.
- `Constant.cs` is the only file in the tool mentioning "gir" — check what that reference is; it may already contain a partial GIR affordance.

---

## 2. Phase 1 — Branch, versioning, scaffolding

### 1.1 Branch

```sh
git checkout -b gtk4 develop
```

`develop` stays as the GTK3 maintenance line. Add a note to `README.md` stating that `develop` is 3.24.x and `gtk4` is 4.22.x.

### 1.2 Version defaults

`build.cake:11`:
```diff
-Settings.Version = Argument("BuildVersion", "3.24.24.1");
+Settings.Version = Argument("BuildVersion", "4.22.4.1");
```

`build.cake:24-30` — the CI branch logic:
```diff
     if (!string.IsNullOrEmpty(EnvironmentVariable("GITHUB_ACTIONS")))
     {
-        Settings.Version = "3.24.24." + EnvironmentVariable("GITHUB_RUN_NUMBER");
+        Settings.Version = "4.22.4." + EnvironmentVariable("GITHUB_RUN_NUMBER");

-        if (EnvironmentVariable("GITHUB_REF") != "refs/heads/master")
+        if (EnvironmentVariable("GITHUB_REF") != "refs/heads/gtk4")
             Settings.Version += "-develop";
     }
```

### 1.3 SDK feature bands

`build.cake:17` lists bands down to `6.0.100`. GTK4 bindings targeting `net8.0` should not advertise 6.x workload manifests:
```diff
-var supportedVersionBands = new List<string>() {"6.0.100", "6.0.200", "6.0.300", "6.0.400", "7.0.400", "8.0.100", "8.0.200"};
+var supportedVersionBands = new List<string>() {"8.0.100", "8.0.200", "8.0.300", "8.0.400"};
```

### 1.4 Target frameworks

`Source/Libs/Directory.Build.props:6` is `net$(_GtkSharpNetVersion);netstandard2.0` with `LangVersion 9`.

**Keep both TFMs and keep `LangVersion 9`.** Nothing about GTK4 requires newer, and the workload ref pack (`Microsoft.DotNet.SharedFramework.Sdk`) depends on the current shape. Revisit only if generated GTK4 code needs `nint`/function pointers — `LPGen.cs`/`LPUGen.cs` already handle native ints without C# 9+ syntax.

### 1.5 New directories

```
Source/Gir/                        # vendored .gir inputs + README.md with provenance
Source/Tools/GirToGapi/            # the converter (Phase 2)
Source/Libs/GrapheneSharp/         # new
Source/Libs/GskSharp/              # new
Source/Libs/AdwaitaSharp/          # new
Docs/gir-gapi-coverage.md          # output of V1
```

---

## 3. Phase 2 — `GirToGapi` converter tool

The largest single deliverable. Lives at `Source/Tools/GirToGapi/`, added to `Source/Tools/Tools.sln`, built by the existing `Prepare` task (`build.cake:52-57`) into `BuildOutput/Tools`.

### 2.1 Shape

```
Source/Tools/GirToGapi/
  GirToGapi.csproj          # net8.0, matches GapiCodegen.csproj
  Program.cs                # --gir=… --out=… --namespace=… --library=… --include=…
  Gir/
    GirDocument.cs          # XDocument load, namespace resolution across --include girs
    GirNamespace.cs
    GirType.cs              # class / interface / record / union / enum / bitfield / callback / alias
    GirCallable.cs          # method / function / constructor / virtual-method / signal
    GirParameter.cs
    GirTypeRef.cs           # <type name= c:type=> and <array>
  Emit/
    ApiWriter.cs            # writes <api parser_version="3">  [corrected: Parser.cs:32 is at 3]
    ObjectEmitter.cs        # <object> + <class_struct> + <implements>
    InterfaceEmitter.cs
    StructEmitter.cs        # <struct> / <boxed> discrimination
    EnumEmitter.cs          # <enum type="enum"|"flags">
    CallbackEmitter.cs
    CTypeMapper.cs          # GIR c:type → gapi type string
  Rules/
    NameMangler.cs          # gtk_widget_set_visible → SetVisible (must match gapi2xml.pl)
    OwnershipMapper.cs      # transfer-ownership → owned / elements_owned
```

### 2.2 Mapping table (the core contract)

| GIR | gapi api.xml | Notes |
|---|---|---|
| `<class>` | `<object cname= parent=>` | `parent` = fully-qualified `c:type` of `<class parent=>` |
| `<class>` + its `glib:type-struct` `<record>` | `<class_struct cname=>` child | Emit `<field>` per record field; emit `<method vm="Name">` for each field whose type is a callback matching a `<virtual-method>` |
| `<interface>` | `<interface>` | `consume_only` when no `glib:type-struct` |
| `<record>` with `glib:type-name` + copy/free funcs | `<boxed opaque="true">` | `opaque=false` only where `.metadata` says so (existing rules already do this, e.g. `GtkSharp.metadata:11` for `GtkBorder`) |
| `<record>` without GType | `<struct>` | |
| `<record disguised="1">` | `<struct opaque="true">` | |
| `<enumeration>` | `<enum type="enum">` | |
| `<bitfield>` | `<enum type="flags">` | |
| `<callback>` | `<callback>` | |
| `<alias>` | `<alias>` | |
| `<constant>` | `<static-string>` for strings; **emit numeric too** | **[corrected]** — `Constant.cs` handles numeric constants and widens oversized values to `long` ([Constant.cs:48](../Source/Tools/GapiCodegen/Constant.cs#L48)). Do not skip them. |
| `<method>` / `<function>` | `<method>` / `<method shared="true">` | `shared` = static (GIR `<function>` on a type) |
| `<constructor>` | `<constructor>` | Mark `preferred` on the one matching `<type>_new` exactly |
| `<virtual-method>` | `<virtual_method>` | |
| `<glib:signal>` | `<signal when=>` | `when` from `<glib:signal when="first\|last\|cleanup">`, uppercased |
| `<property>` | `<property readable= writeable= construct= construct-only=>` | |
| `<doc>` | *dropped* | api.xml carries no docs today |

### 2.3 Type mapping — `CTypeMapper.cs`

gapi type strings are **C type strings**, not GIR names. `GapiCodegen`'s `SymbolTable.cs` keys off exactly these. Emit `c:type` verbatim where present; where GIR omits `c:type` (common on `<array>` inner types), synthesise from `<type name=>` via a table:

```csharp
// Emit/CTypeMapper.cs
static readonly Dictionary<string, string> GirNameToCType = new()
{
    ["none"]     = "void",
    ["gboolean"] = "gboolean",
    ["gint"]     = "gint",
    ["guint"]    = "guint",
    ["gint64"]   = "gint64",
    ["gsize"]    = "gsize",       // LPUGen — native-sized, see Source/Tools/GapiCodegen/LPUGen.cs
    ["gssize"]   = "gssize",      // LPGen
    ["glong"]    = "glong",
    ["gdouble"]  = "gdouble",
    ["gfloat"]   = "gfloat",
    ["utf8"]     = "const-gchar*",
    ["filename"] = "const-gchar*",
    ["gpointer"] = "gpointer",
    ["GType"]    = "GType",
};
```

> ⚠️ `gsize`/`gssize`/`glong` must land on the same strings the existing api.xml uses, or the native-int marshalling fixed in `17512f6ec` regresses. Diff-check this specifically (see 2.5).

### 2.4 Name mangling parity

`NameMangler.cs` must reproduce `gapi2xml.pl`'s `name` attribute derivation exactly, otherwise **every XPath rule in the `.metadata` files that selects by `@name` breaks** — and there are 1279 lines of them in `GtkSharp.metadata` alone, many keyed on `@name` (e.g. `GtkSharp.metadata:14`: `boxed[@cname='GtkIconSet']/method[@name='GetSizes']`).

Rule (from `Source/OldStuff/parser/gapi2xml.pl`): strip the type prefix from the `cname`, split on `_`, upper-case each segment's first letter, join. `gtk_icon_set_get_sizes` on `GtkIconSet` → `GetSizes`.

**Verification gate:** run `GirToGapi` against a **GTK 3.24 gir** and diff the output against the checked-in `Source/Libs/GtkSharp/GtkSharp-api.xml`. This is the highest-value test available and it costs one afternoon. It will not be a clean diff, but every difference must be explained. Record the triage in `Docs/gir-gapi-coverage.md`.

### 2.5 Wire into the build

`CakeScripts/GAssembly.cake` — add a `Gir` property and a `RegenerateApi()` method that runs *before* the existing `Prepare()` copy at [GAssembly.cake:37-40](../CakeScripts/GAssembly.cake#L37):

```csharp
public string Gir { get; set; }   // e.g. "Source/Gir/gtk-4.0.gir"

// New: only run under --BuildTarget=RegenerateApi. Writes the CHECKED-IN api.xml.
public void RegenerateApi()
{
    if (string.IsNullOrEmpty(Gir)) return;

    var includes = string.Join(" ", Deps
        .Select(d => Settings.AssemblyList.First(a => a.Name == d))
        .Where(a => !string.IsNullOrEmpty(a.Gir))
        .Select(a => "--include=" + a.Gir));

    Cake.DotNetExecute("BuildOutput/Tools/GirToGapi.dll",
        $"--gir={Gir} --out={RawApi} --assembly-name={Name} {includes}");
}
```

`build.cake` — new task, deliberately **not** in the `Default` chain:

```csharp
Task("RegenerateApi")
    .IsDependentOn("Init")
    .Does(() =>
{
    DotNetBuild("Source/Tools/Tools.sln", new DotNetBuildSettings {
        Verbosity = DotNetVerbosity.Minimal, Configuration = configuration });

    foreach(var gassembly in list)
        gassembly.RegenerateApi();
});
```

Consequence: `dotnet cake build.cake` stays hermetic and offline. Regenerating api.xml is an explicit, reviewable, committed act — the same discipline the repo has today.

### 2.6 Make `GapiFixup` fail loudly

`Source/Tools/GapiFixup/GapiFixup.cs:115,129,144,167,183,204,222` print `Warning: … matched no nodes` and continue. During a migration where the api.xml tree changes wholesale, a silently-unmatched rule is indistinguishable from a rule that worked — and there are ~1900 rules across all metadata files.

Add a `--strict` flag that counts warnings and exits non-zero:

```csharp
// GapiFixup.cs — near the top of Main
static int warnings = 0;
static bool strict = false;
// … each Console.WriteLine("Warning: …") site gains a preceding: warnings++;
// … at end of Main:
if (strict && warnings > 0) {
    Console.WriteLine($"gapi-fixup: {warnings} unmatched rule(s), failing due to --strict");
    return 1;
}
return 0;
```

Pass `--strict` from `GAssembly.cake:47-50` **once each assembly's metadata has been triaged** (Phase 4), not before.

---

## 4. Phase 3 — Assembly graph, native library map

### 3.1 `CakeScripts/Settings.cake` — full replacement of `Init()`

Current graph at [Settings.cake:14-52](../CakeScripts/Settings.cake#L14) becomes:

```csharp
AssemblyList = new List<GAssembly>()
{
    new GAssembly("GLibSharp")     { Gir = "Source/Gir/glib-2.0.gir" },       // hand-written; gir for --include only
    new GAssembly("GioSharp")      { Deps = new[] { "GLibSharp" },
                                     Gir  = "Source/Gir/gio-2.0.gir" },
    new GAssembly("CairoSharp"),                                              // hand-written, no gir (see V2)
    new GAssembly("GrapheneSharp") { Deps = new[] { "GLibSharp" },
                                     Gir  = "Source/Gir/graphene-1.0.gir" },  // NEW
    new GAssembly("PangoSharp")    { Deps = new[] { "GLibSharp", "CairoSharp" },
                                     Gir  = "Source/Gir/pango-1.0.gir" },
    new GAssembly("GdkSharp")      { Deps = new[] { "GLibSharp", "GioSharp", "CairoSharp", "PangoSharp" },
                                     Gir  = "Source/Gir/gdk-4.0.gir" },
    new GAssembly("GskSharp")      { Deps = new[] { "GLibSharp", "GioSharp", "CairoSharp", "PangoSharp",
                                                    "GrapheneSharp", "GdkSharp" },
                                     Gir  = "Source/Gir/gsk-4.0.gir" },       // NEW
    new GAssembly("GtkSharp")      { Deps = new[] { "GLibSharp", "GioSharp", "CairoSharp", "PangoSharp",
                                                    "GrapheneSharp", "GdkSharp", "GskSharp" },
                                     Gir  = "Source/Gir/gtk-4.0.gir",
                                     ExtraArgs = "--abi-cs-usings=Gtk,GLib" },
    new GAssembly("AdwaitaSharp")  { Deps = new[] { "GLibSharp", "GioSharp", "CairoSharp", "PangoSharp",
                                                    "GrapheneSharp", "GdkSharp", "GskSharp", "GtkSharp" },
                                     Gir  = "Source/Gir/libadwaita-1.gir" },  // NEW
    new GAssembly("GtkSourceSharp"){ Deps = new[] { "GLibSharp", "GioSharp", "CairoSharp", "PangoSharp",
                                                    "GrapheneSharp", "GdkSharp", "GskSharp", "GtkSharp" },
                                     Gir  = "Source/Gir/gtksourceview-5.gir" },
    new GAssembly("WebkitGtkSharp"){ Deps = new[] { "GLibSharp", "GioSharp", "CairoSharp", "PangoSharp",
                                                    "GrapheneSharp", "GdkSharp", "GskSharp", "GtkSharp" },
                                     Gir  = "Source/Gir/webkitgtk-6.0.gir",
                                     ExtraArgs = "--abi-cs-usings=WebKit,Gtk,GLib,Gdk,Pango,Cairo" },
};
```

Deltas: `AtkSharp` **gone**; `GrapheneSharp`, `GskSharp`, `AdwaitaSharp` **new**; every `Atk` reference dropped from `--abi-cs-usings` (`Settings.cake:22`, `:47`).

### 3.2 `Source/Libs/Shared/Library.cs` — full replacement

```csharp
enum Library
{
    GLib,
    GObject,
    Cairo,
    Gio,
    Pango,
    PangoCairo,
    Graphene,     // NEW
    GdkPixbuf,
    Gtk,          // GDK and GSK live inside this library — see V3
    GtkSource,
    Webkit,
    Adwaita,      // NEW
}
```

`Atk` is removed (GTK4 has no ATK). `Gdk` is removed as a *separate* enum value — but see 3.3 for how generated `gdk_*`/`gsk_*` calls resolve.

### 3.3 `Source/Libs/Shared/GLibrary.cs` — library filename map **[corrected]**

> **The Windows names below are the *verified* gvsbuild basenames, not the original guesses.**
> gvsbuild builds with MSVC and emits **no `lib` prefix** (`gtk-4-1.dll`, not `libgtk-4-1.dll`).
> The `lib`-prefixed forms are kept as later candidates so MSYS2 installs keep working — the
> fallback loop at [GLibrary.cs:83-90](../Source/Libs/Shared/GLibrary.cs#L83) makes that free.
> Verified against the bundle's central directory; see gir-gapi-coverage.md §4.1.

Replace the static-ctor block at [GLibrary.cs:20-32](../Source/Libs/Shared/GLibrary.cs#L20):

```csharp
_libraryDefinitions[Library.GLib]       = new[] {"glib-2.0-0.dll",       "libglib-2.0.so.0",       "libglib-2.0.0.dylib",       "libglib-2.0-0.dll"};
_libraryDefinitions[Library.GObject]    = new[] {"gobject-2.0-0.dll",    "libgobject-2.0.so.0",    "libgobject-2.0.0.dylib",    "libgobject-2.0-0.dll"};
_libraryDefinitions[Library.Cairo]      = new[] {"cairo-2.dll",          "libcairo.so.2",          "libcairo.2.dylib",          "libcairo-2.dll"};
_libraryDefinitions[Library.Gio]        = new[] {"gio-2.0-0.dll",        "libgio-2.0.so.0",        "libgio-2.0.0.dylib",        "libgio-2.0-0.dll"};
_libraryDefinitions[Library.Pango]      = new[] {"pango-1.0-0.dll",      "libpango-1.0.so.0",      "libpango-1.0.0.dylib",      "libpango-1.0-0.dll"};
_libraryDefinitions[Library.PangoCairo] = new[] {"pangocairo-1.0-0.dll", "libpangocairo-1.0.so.0", "libpangocairo-1.0.0.dylib", "libpangocairo-1.0-0.dll"};
_libraryDefinitions[Library.Graphene]   = new[] {"graphene-1.0-0.dll",   "libgraphene-1.0.so.0",   "libgraphene-1.0.0.dylib",   "libgraphene-1.0-0.dll"};
_libraryDefinitions[Library.GdkPixbuf]  = new[] {"gdk_pixbuf-2.0-0.dll", "libgdk_pixbuf-2.0.so.0", "libgdk_pixbuf-2.0.dylib",   "libgdk_pixbuf-2.0-0.dll"};
// GTK4 ships ONE library: gdk_*, gsk_* and gtk_* symbols all resolve here. Verified (V3).
_libraryDefinitions[Library.Gtk]        = new[] {"gtk-4-1.dll",          "libgtk-4.so.1",          "libgtk-4.1.dylib",          "libgtk-4-1.dll"};
_libraryDefinitions[Library.GtkSource]  = new[] {"gtksourceview-5-0.dll","libgtksourceview-5.so.0","libgtksourceview-5.0.dylib","libgtksourceview-5-0.dll"};
// No Windows build exists — gvsbuild has no webkit project. Linux/macOS only. See R5.
_libraryDefinitions[Library.Webkit]     = new[] {"libwebkitgtk-6.0.so.4",  "libwebkitgtk-6.0.dylib"};
_libraryDefinitions[Library.Adwaita]    = new[] {"adwaita-1-0.dll",      "libadwaita-1.so.0",      "libadwaita-1.0.dylib",      "libadwaita-1-0.dll"};
```

And the Windows fallback path at [GLibrary.cs:66](../Source/Libs/Shared/GLibrary.cs#L66) — note
the **`bin` segment**: the gvsbuild zip is not flat (`bin/ lib/ include/ share/ etc/`), unlike the
GTK 3 bundle it replaces.

```diff
-SetDllDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Gtk", "3.24.24"));
+SetDllDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Gtk", "4.22.4", "bin"));
```

### 3.4 Codegen `library=` attribute **[corrected]**

There is **no mapping** from the `library` string to the `Library` enum.
[GenBase.cs:65-69](../Source/Tools/GapiCodegen/GenBase.cs#L65) reads `/api/namespace/@library` and
[Method.cs:201](../Source/Tools/GapiCodegen/Method.cs#L201) pastes it **verbatim** into
`GLibrary.Load(<value>)` — the attribute is a C# expression.

So the mechanical half of V3 costs one metadata line per assembly, not a converter feature. The
mechanism already exists: [GtkSourceSharp.metadata:67](../Source/Libs/GtkSourceSharp/GtkSourceSharp.metadata#L67)
already rewrites a raw `libgtksourceview-4.so` into `Library.GtkSource`. Do the same in
`GdkSharp.metadata` and a new `GskSharp.metadata`:

```xml
<attr path="/api/namespace" name="library">Library.Gtk</attr>
```

`GirToGapi` should emit the raw `shared-library` value and let metadata normalise it.

### 3.5 Project files

- **Delete** `Source/Libs/AtkSharp/` entirely (6 hand-written `.cs`, `AtkSharp-api.xml`, `AtkSharp.metadata`, `AtkSharp.csproj`).
- **Create** `Source/Libs/GrapheneSharp/GrapheneSharp.csproj`, `GskSharp/GskSharp.csproj`, `AdwaitaSharp/AdwaitaSharp.csproj` modelled on [GdkSharp.csproj](../Source/Libs/GdkSharp/GdkSharp.csproj) — each with the `..\Shared\*.cs` link ItemGroup and the right `ProjectReference` set.
- **Edit** [GtkSharp.csproj:26-28](../Source/Libs/GtkSharp/GtkSharp.csproj#L26): drop the `AtkSharp` reference, add `GrapheneSharp` and `GskSharp`.
- **Edit** `Source/GtkSharp.sln`: remove the `AtkSharp` project (`{12A721AA-8E7F-459A-A62D-F7372350E5F1}`) and its 12 `ProjectConfigurationPlatforms` lines; add three new projects with fresh GUIDs.
- **Edit** [Samples.csproj:23](../Source/Samples/Samples.csproj#L23): drop `AtkSharp`, add `GrapheneSharp`, `GskSharp`, `AdwaitaSharp`.

---

## 5. Phase 4 — api.xml regeneration and metadata triage

Per assembly, in dependency order: `GLibSharp` → `GioSharp` → `GrapheneSharp` → `PangoSharp` → `GdkSharp` → `GskSharp` → `GtkSharp` → `AdwaitaSharp` → `GtkSourceSharp` → `WebkitGtkSharp`.

For each:

1. `dotnet cake build.cake --BuildTarget=RegenerateApi --Assembly=<Name>` → new `<Name>-api.xml`.
2. Run `GapiFixup` **without** `--strict`, capture every `Warning: … matched no nodes`.
3. Triage each warning into one of three buckets:
   - **Obsolete** — the rule targets a GTK3 type that no longer exists → delete the rule.
   - **Moved** — the type survived but the XPath changed (renamed, re-parented, `boxed`→`struct`) → rewrite the XPath.
   - **Converter bug** — the type exists in GIR but `GirToGapi` emitted it wrong → fix the converter, not the metadata.
4. Once warning-free, add `--strict` to that assembly's `GapiFixup` invocation.
5. `dotnet cake build.cake --BuildTarget=Build --Assembly=<Name>`, fix compile errors in the hand-written partials (Phase 5).

Expected metadata attrition, by inspection of the current files:

| Metadata | Lines now | Expected survival | Why |
|---|---|---|---|
| `GtkSharp.metadata` | 1279 | **~30 %** | Huge blocks target `GtkIconSet`/`GtkIconSource` (`:14-17`), `GtkActionEntry`/`GtkRadioActionEntry`/`GtkToggleActionEntry` (`:8-10`), `GtkGradient`/`GtkSymbolicColor` (`:12`, `:35`), `GtkTargetEntry` (`:36`), `GtkWidgetPath` (`:58-61`), `GtkSelectionData` (`:26-33`) — **all deleted in GTK4**. |
| `GdkSharp.metadata` | 200 | **~25 %** | `GdkWindow`→`GdkSurface`, `GdkScreen`/`GdkColor`/`GdkDeviceManager` deleted, the whole `GdkEvent*` hierarchy restructured into opaque event types. |
| `GioSharp.metadata` | 199 | **~90 %** | GIO is stable across the GTK3→4 boundary. Lowest-risk assembly; **do this one first** as the converter's smoke test. |
| `PangoSharp.metadata` | 116 | **~90 %** | Pango is stable. |
| `AtkSharp.metadata` | 44 | **0 %** | Deleted. |
| `GtkSourceSharp.metadata` | 82 | **~20 %** | GtkSourceView 4→5 is itself a breaking rewrite. |
| `WebkitGtkSharp.metadata` | 8 | **~0 %** | Namespace `WebKit2`→`WebKit`; effectively new. |
| `AdwaitaSharp.metadata` | — | new | Written from scratch as build errors demand. |

`Source/Libs/GtkSharp/GtkSharp-symbols.xml` is hand-written and must be re-audited against GTK4 types by the same process.

> **Sequencing advice:** do `GioSharp` end-to-end *first*, in isolation. It exercises the whole converter → fixup → codegen → compile loop against a library that barely changed, so any failure is unambiguously a converter bug rather than a GTK4 API change. Do not start `GtkSharp` until `GioSharp` builds clean.

---

## 6. Phase 5 — Hand-written layer port

117 hand-written `.cs` in `Source/Libs/GtkSharp/` (10 174 lines), 41 in `GdkSharp`, 45 in `PangoSharp`, 73 in `GLibSharp`, 57 in `CairoSharp`.

### 5.1 Files to DELETE (type removed from GTK4)

`Source/Libs/GtkSharp/`:
`Bin.cs`, `Container.cs`, `Container.Forall.cs`, `HBox.cs`, `VBox.cs`, `HScale.cs`, `VScale.cs`*, `Action.cs`, `ActionEntry.cs`, `ActionGroup.cs`, `UIManager.cs`, `Stock.cs`, `StockManager.cs`, `IconFactory.cs`, `IconSet.cs`, `ImageMenuItem.cs`, `StatusIcon.cs`, `ColorSelection.cs`, `Accel.cs`, `AccelKey.cs`, `ChildAttribute.cs`, `ChildPropertyAttribute.cs`, `Drag.cs`, `CellRenderer.GetSize.cs`

`Source/Libs/GdkSharp/`:
`Window.cs` (→ `Surface`), `Screen.cs`, `Color.cs`, `Property.cs`

\* verify against the actual file list at port time; the list above is derived from the grep in §5.3 plus known GTK4 removals.

**Consequence:** `Gtk.Container` disappearing is the single most user-visible break. `Add()`/`Remove()`/`Children` are gone; GTK4 uses per-widget child APIs (`Box.Append`, `Window.Child`, `Grid.Attach`, `ListBox.Append`). There is no compatibility shim and **none should be added** — a fake `Container` that silently misbehaves is worse than a compile error.

### 5.2 Files needing substantial REWRITE

| File | Lines | Change |
|---|---|---|
| [Application.cs](../Source/Libs/GtkSharp/Application.cs) | 203 | `gtk_init(&argc,&argv)` → `gtk_init(void)`; **`gtk_main`/`gtk_main_quit`/`gtk_main_iteration`/`gtk_events_pending` all removed** — `Run()` becomes `GLib.Application.Run()` or a `GLib.MainLoop`; `gtk_get_current_event` removed (events are delivered to `EventController`s). See 5.4. |
| [Widget.cs](../Source/Libs/GtkSharp/Widget.cs) | 599 | Drop the `GdkWindow` shim at [:34-38](../Source/Libs/GtkSharp/Widget.cs#L34); drop `AddAccelerator` at [:57-60](../Source/Libs/GtkSharp/Widget.cs#L57) (`GtkAccelGroup` gone → `GtkShortcutController`); the `g_signal_newv`/closure machinery at [:75-110](../Source/Libs/GtkSharp/Widget.cs#L75) is GObject-level and **survives unchanged**; template binding (`TemplateData`, [:41-55](../Source/Libs/GtkSharp/Widget.cs#L41)) survives — `gtk_widget_class_bind_template_child_full` still exists in GTK4. |
| [Global.cs](../Source/Libs/GtkSharp/Global.cs) | ~40 | `ShowUri(Gdk.Screen, …)` at [:30-33](../Source/Libs/GtkSharp/Global.cs#L30) → `Gtk.UriLauncher` / `gtk_show_uri(GtkWindow*, uri, timestamp)`. `CurrentEventTime` gone. |
| `StyleContext.cs` | 116 | Most `GtkStyleContext` API deprecated in 4.10; keep only `AddProvider`/`RemoveProvider` equivalents on `GtkWidget`/display. |
| `Clipboard.cs` | 129 | `GtkClipboard` → `GdkClipboard` (async, `GdkContentProvider`-based). Full rewrite. |
| `Image.cs`, `IconTheme.cs` | 130+131 | `IconSize.Button` etc. removed; `GtkIconTheme` is now per-`GdkDisplay` and returns `GtkIconPaintable`. |
| `TreeView.cs`, `ListStore.cs`, `TreeStore.cs`, `NodeStore.cs`, `NodeView.cs`, `TreeModel*.cs`, `NodeSelection.cs`, `TreeNode.cs`, `CellRenderer*.cs` | ~2000 | **Still present in 4.22 but deprecated since 4.10** in favour of `GtkListView`/`GtkColumnView`/`GListModel`. See R3 for the keep-vs-drop call. |
| `Menu.cs`, `MenuItem.cs` | — | `GtkMenu`/`GtkMenuItem` deleted; menus are `GMenuModel` + `GtkPopoverMenu`. |
| `Dialog.cs`, `FileChooserDialog.cs`, `FileChooserNative.cs` | 316+ | `gtk_dialog_run()` **removed** (no nested main loops). All dialogs are async — `GtkFileDialog`, `GtkAlertDialog` in 4.10+. |
| `Builder.cs` | 202 | `.glade` (GTK3 GtkBuilder format) → GTK4 `.ui`. `gtk_builder_add_from_string` survives; the XML does not. |
| `SignalConnector.cs` | 204 | Survives — it is GObject-level. Verify against template-signal changes. |

### 5.3 Files that merely reference removed types

The grep that produced the port list:
```sh
grep -rlE "\b(Container|Bin|VBox|HBox|Table|Alignment|Misc|Stock|IconFactory|IconSet|UIManager|StatusIcon|EventBox|ShowAll|Gdk\.Window|Gdk\.Screen|Gdk\.Color|StyleContext)\b" Source/Libs/*/*.cs
```
→ 25 files. Re-run it after each assembly is ported; it should converge to empty.

### 5.4 `Application` — concrete shape

GTK4 has no `gtk_main`. The current [Application.cs:105-160](../Source/Libs/GtkSharp/Application.cs#L105) block (`Run`, `EventsPending`, `RunIteration`, `Quit`, `CurrentEvent`) must go. Replacement:

```csharp
// Source/Libs/GtkSharp/Application.cs (GTK4)
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
delegate void d_gtk_init();
static d_gtk_init gtk_init = FuncLoader.LoadFunction<d_gtk_init>(
    FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_init"));

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
delegate bool d_gtk_init_check();
static d_gtk_init_check gtk_init_check = FuncLoader.LoadFunction<d_gtk_init_check>(
    FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_init_check"));

public static void Init()
{
    SetPrgname();
    gtk_init();
    SynchronizationContext.SetSynchronizationContext(new GLib.GLibSynchronizationContext());
}

// gtk_main() is gone. Applications run via GApplication:
//     var app = new Gtk.Application("org.example.App", GLib.ApplicationFlags.None);
//     app.Activated += (_, __) => { new MainWindow(app).Present(); };
//     return app.Run(args.Length, args);
```

`Init(string, ref string[])` and `InitCheck(string, ref string[])` ([Application.cs:63-110](../Source/Libs/GtkSharp/Application.cs#L63)) lose their reason to exist — GTK4's init takes no argv. Keep the overloads as `[Obsolete]` thin wrappers that ignore `args` **only if** `Init.cs:27-30` (`Gtk.Init.Check`) is worth preserving; otherwise delete both files.

**Decision recorded:** keep `Application.Init()` (no-arg) as the one supported entry point; delete `Init.cs` and the argv-taking overloads. Rationale — a GTK4 `Init(ref string[] args)` that silently drops `args` is a trap.

---

## 7. Phase 6 — Samples port

37 files under [Source/Samples/Sections/](../Source/Samples/Sections/). This is the acceptance test for the whole upgrade.

### 6.1 `Program.cs`

Current [Program.cs:16-45](../Source/Samples/Program.cs#L16) uses `Application.Init()` + `App.Register()` + `Win.ShowAll()` + `Application.Run()` and sets `App.AppMenu` (removed in GTK4). New shape:

```csharp
public static int Main(string[] args)
{
    var app = new Application("org.Samples.Samples", GLib.ApplicationFlags.None);

    app.Activated += (_, __) =>
    {
        Win = new MainWindow(app);
        // GTK4: no ShowAll — widgets are visible by default; Present() shows the window.
        Win.Present();
    };

    // App menu is gone in GTK4; use a GMenu on a PopoverMenuBar / MenuButton instead.
    return app.Run(args.Length, args);
}
```

`AboutDialog.Run()` at [Program.cs:66](../Source/Samples/Program.cs#L66) — `gtk_dialog_run` is removed; use `dialog.Present()` and handle the response signal, or `Gtk.AboutDialog` + `Present()`.

### 6.2 `MainWindow.cs`

Current [MainWindow.cs:23-77](../Source/Samples/MainWindow.cs#L23) is dense with removed API. Mapping:

| Current | GTK4 |
|---|---|
| `base(WindowType.Toplevel)` [:23](../Source/Samples/MainWindow.cs#L23) | `base()` — `GtkWindowType` removed |
| `WindowPosition = WindowPosition.Center` [:26](../Source/Samples/MainWindow.cs#L26) | removed; the compositor places windows |
| `DefaultSize = new Gdk.Size(800,600)` [:27](../Source/Samples/MainWindow.cs#L27) | `SetDefaultSize(800, 600)` |
| `_headerBar.ShowCloseButton` / `.Title` [:30-31](../Source/Samples/MainWindow.cs#L30) | `ShowTitleButtons`; `TitleWidget = new Label(...)` |
| `btnClickMe.Image = Image.NewFromIconName(..., IconSize.Button)` [:35](../Source/Samples/MainWindow.cs#L35) | `new Button { IconName = "document-new-symbolic" }` |
| `Titlebar = _headerBar` [:37](../Source/Samples/MainWindow.cs#L37) | survives |
| `new HPaned()` / `Pack1(w, false, true)` [:39-44](../Source/Samples/MainWindow.cs#L39) | `new Paned(Orientation.Horizontal)`; `StartChild = w; ResizeStartChild = false; ShrinkStartChild = true` |
| `_boxContent.Margin = 8` [:48](../Source/Samples/MainWindow.cs#L48) | `MarginTop/Bottom/Start/End` (the aggregate `Margin` is gone) |
| `scroll1.Child = vpanned` [:51](../Source/Samples/MainWindow.cs#L51) | survives (`GtkScrolledWindow.Child`) |
| `new Label { Expand = true }` [:52](../Source/Samples/MainWindow.cs#L52) | `Hexpand`/`Vexpand` |
| `Destroyed += …` [:76](../Source/Samples/MainWindow.cs#L76) | `OnDestroy`/`CloseRequest`; with `GtkApplication` the app quits when its last window closes |
| `_treeView.Selection.Changed` [:75](../Source/Samples/MainWindow.cs#L75) | survives (deprecated) — or port to `ColumnView` + `SelectionModel`, see R3 |

### 6.3 Section-by-section

Group the 37 sections by porting difficulty so the work can be parallelised and partially landed:

- **Mechanical** (Button, Label, Switch, ToggleButton, Spinner, ProgressBar, LevelBar, LinkButton, Entry, SpinButton, Range, ColorButton, ComboBox): container calls and `ShowAll` only.
- **Moderate** (Image, Pixbuf, Monitor, Seat, Timer, CssName, StyleContext, AboutDialog, FileChooserDialog): touch removed subsystems; `FileChooserDialog` → `Gtk.FileDialog` (async).
- **Structural** (DrawingArea, ImageDrawn, PolarFixed, PolarFixedSection, CustomCellRenderer, CompositeWidget, CellRenderer, EditableCells, ListStore, TreeView): custom drawing moves from the `Draw`/Cairo signal to `GtkDrawingArea.SetDrawFunc` or `GtkWidget.Snapshot` + `GtkSnapshot`/`GskRenderNode`; custom layout moves from `size_allocate` to `GtkLayoutManager`.
- **Delete or replace** (`ContainerChildPropertiesSection.cs`): child properties do not exist in GTK4. Replace with a `GtkGrid`/`GtkBox` layout-property section.
- **Blocked on native availability** (`WebviewSection.cs`): see R5.

`Samples.csproj:6-11` embeds `**\*.glade`; change the glob to `**\*.ui` once the files are converted (6.4).

### 6.4 `.glade` → `.ui`

GTK3 Glade files are not loadable by GTK4's `GtkBuilder`. Run `gtk4-builder-tool simplify --3to4 <file>.glade > <file>.ui` on each, then hand-fix what the tool flags (it does not handle `GtkContainer` packing properties or removed widgets). Affects the Samples embedded resources and all 12 template projects (§7.2).

---

## 8. Phase 7 — Templates and workload

### 7.1 Standalone templates — `Source/Templates/`

Three languages × four templates (Application, Dialog, Widget, Window) = 12 projects, each with a `.glade` and a code file. All use GTK3 idioms.

Port `Program.cs`/`MainWindow.cs` (and `.fs`/`.vb` equivalents) to the `GtkApplication` shape from §6.1, convert `.glade` → `.ui` per §6.4, and update `.template.config/template.json` descriptions to say GTK 4.

### 7.2 Workload — `Source/Workload/`

- `GtkSharp.Runtime/GtkSharp.Runtime.csproj:17` uses `<ProjectReference Include="$(_GtkSharpSourceDirectory)Libs\**\*.csproj" />` — a glob, so it picks up the three new assemblies automatically and drops `AtkSharp` automatically once the directory is deleted. **No edit needed**, but verify the resulting pack contents.
- `GtkSharp.Ref`, `GtkSharp.Sdk` — check for hardcoded assembly lists.
- `GtkSharp.NET.Sdk.Gtk/WorkloadManifest.in.json` — version-band references; align with the new `supportedVersionBands` from §1.3.
- `Source/Workload/GtkSharp.Workload.Template.*` — same content port as §7.1.

---

## 9. Phase 8 — Native runtime, CI

### 8.1 `GtkSharp.targets` **[corrected — V4 resolved]**

V4 chose the gvsbuild release asset. [GtkSharp.targets:4-10](../Source/Libs/GtkSharp/GtkSharp.targets#L4):

```diff
-    <GtkUrl Condition=" '$(GtkUrl)' == '' ">https://github.com/pieroviano/Dependencies/raw/master/gtk-3.24.24.zip</GtkUrl>
-    <GtkDir Condition=" '$(GtkDir)' == '' ">$(LOCALAPPDATA)\Gtk\3.24.24</GtkDir>
+    <GtkUrl Condition=" '$(GtkUrl)' == '' ">https://github.com/wingtk/gvsbuild/releases/download/2026.6.0/GTK4_Gvsbuild_2026.6.0_x64.zip</GtkUrl>
+    <GtkDir Condition=" '$(GtkDir)' == '' ">$(LOCALAPPDATA)\Gtk\4.22.4</GtkDir>
```
```diff
-  <Target Name="InstallGtk" BeforeTargets="Build" Condition=" … and !Exists('$(GtkDir)/libgtk-3-0.dll') ">
+  <Target Name="InstallGtk" BeforeTargets="Build" Condition=" … and !Exists('$(GtkDir)/bin/gtk-4-1.dll') ">
```

Three things this bundle changes versus the GTK 3 one, all verified against its central directory
(gir-gapi-coverage.md §4):

1. **It is not flat.** Contents unzip to `bin/ lib/ include/ share/ etc/ python/ wheels/`. The
   sentinel path and `GLibrary`'s `SetDllDirectory` (§3.3) both need the `bin` segment.
2. **No `lib` prefix** on DLL basenames — `gtk-4-1.dll`. See the corrected table in §3.3.
3. **It is 299.5 MB** versus the GTK 3 zip's ~60 MB, and includes `python/` and `wheels/` that
   GtkSharp has no use for. Consider extracting only `bin/`, `lib/`, `share/glib-2.0/`,
   `share/icons/` rather than the whole archive.

The sentinel filename must match `_libraryDefinitions[Library.Gtk][0]` from §3.3 **and** the actual
name inside the bundle. Both are now `gtk-4-1.dll`.

### 8.2 CI

`.github/workflows/main.yml` currently only builds on `ubuntu-22.04`. Two changes worth making:

1. **Runner bump.** ubuntu-22.04 ships GTK 3.24 / no GTK4 dev packages recent enough. The build itself does not need GTK installed (api.xml is vendored), so the *build* still passes — but if a `RegenerateApi` CI job is ever added, or if a headless smoke-run of Samples is added, it needs `ubuntu-24.04` and `apt install libgtk-4-dev libadwaita-1-dev libgtksourceview-5-dev libwebkitgtk-6.0-dev`.
2. **Headless smoke test** — the cheapest real verification this repo can get, and it partly compensates for having no test project:
   ```yaml
   - name: Smoke-run Samples headless
     run: |
       sudo apt-get install -y libgtk-4-1 xvfb
       xvfb-run -a dotnet BuildOutput/Samples/Samples.dll --smoke-exit
   ```
   Requires adding a `--smoke-exit` argument to `Source/Samples/Program.cs` that constructs the main window, pumps one main-loop iteration, and exits 0.

Also update the branch check in `build.cake:27` (§1.2) so `gtk4` builds are not all tagged `-develop`.

### 8.3 `CLAUDE.md`

The project instructions describe the GTK3 world in several places that become wrong: "wrapper for Gtk 3.22+", the assembly graph line, `GtkSharp.targets` downloading 3.24.24, and "`GLibSharp` and `CairoSharp` have no `.metadata`". Update the §Assembly graph, §Code generation pipeline (add `GirToGapi` and the `RegenerateApi` target), and §Native interop (the one-library fact from V3) sections in the same commit that lands each change.

---

## 10. Verification

There is no test project (`CLAUDE.md` §Tests), so verification is layered and mostly manual.

| # | Gate | Command | Passes when |
|---|---|---|---|
| 1 | Converter parity | `GirToGapi --gir=gtk-3.24.gir --out=/tmp/gtk3.xml` then `diff` vs `Source/Libs/GtkSharp/GtkSharp-api.xml` (on `develop`) | Every difference is explained in `Docs/gir-gapi-coverage.md`. **This is the most important gate — run it before Phase 4.** |
| 2 | Schema validity | `GapiCodegen --schema=Source/Libs/Shared/Gapi.xsd` (already validates) | No schema errors for any of the 10 generated api.xml files |
| 3 | Metadata clean | `GapiFixup --strict` per assembly | Zero unmatched XPath rules |
| 4 | Codegen | `dotnet cake build.cake --BuildTarget=Prepare` | `Generated/*.cs` produced for all 10 |
| 5 | Compile | `dotnet cake build.cake --BuildTarget=Build` | Solution builds clean, both TFMs, `LangVersion 9` |
| 6 | Pack | `dotnet cake build.cake` | 11 nupkgs + workload + templates in `BuildOutput/NugetPackages` |
| 7 | Symbol resolution | Run Samples on Linux with GTK 4.22.4 | ✅ **passed** — the suite runs the sample sections on Debian trixie (Gtk 4.18.6, webkitgtk 2.52.5): 402 pass, 1 skip. No `DllNotFoundException` / `EntryPointNotFoundException`. Three defects only Linux could show — see §16. |
| 8 | Samples runtime | `dotnet cake build.cake --BuildTarget=RunSamples` | All 37 sections render and are interactive |
| 9 | Windows | Same on Windows with the V4 bundle | Samples runs; `GtkSharp.targets` downloads and unzips correctly |
| 10 | Templates | `dotnet new gtkapp` from the packed template, then build and run | Produces a running GTK4 app |
| 11 | Workload | `dotnet cake build.cake --BuildTarget=InstallWorkload` on a scratch SDK, then `dotnet new gtkapp` | ⚠️ mutates the machine SDK — use a container |

**Recommended per-assembly loop:** gates 3→4→5 for one assembly at a time, in the dependency order from §5. Do not batch.

---

## 11. Risks

| # | Risk | Likelihood | Impact | Mitigation |
|---|---|---|---|---|
| ~~R1~~ | ~~**GIR cannot express `<class_struct>` vm/padding faithfully**~~ | ~~Medium~~ | ~~**Critical**~~ | **RETIRED by V1.** 207 `glib:is-gtype-struct-for` records carry the full slot list; the `signal_vm` rule scores 328/329 with zero false negatives. No hand-maintained padding table needed. **One residual requirement:** GTK 4 pads with a *fixed-size array* field on 35 class structs, which must be expanded into N separate padding slots — see gir-gapi-coverage.md §1.1. Getting that wrong corrupts the vfunc table silently. |
| R2 | **`GirToGapi` name mangling diverges from `gapi2xml.pl`**, silently invalidating ~1900 metadata XPath rules and producing subtly different public C# API names. | High | High | Gate 1 (GTK3 round-trip diff) catches this. `--strict` GapiFixup (§2.6) catches the residue. |
| R3 | **GTK 4.10+ deprecated the entire TreeView/ListStore/CellRenderer stack** (~2000 lines of hand-written code in `TreeView.cs`, `ListStore.cs`, `TreeStore.cs`, `NodeStore.cs`, `NodeView.cs`, `TreeModel*.cs`) plus `GtkDialog`, `GtkComboBox`, `GtkFileChooserDialog`, `GtkAssistant`, `GtkInfoBar`. They still work in 4.22 but generate deprecation warnings and will be removed in GTK 5. | Certain | Medium | **Decision needed at Phase 5 start:** bind them (with `[Obsolete]`) for migration continuity, or omit them and force consumers onto `ListView`/`ColumnView` immediately. Recommendation: **bind + `[Obsolete]`** — it makes the 37 sample sections portable and keeps the diff reviewable. Revisit for a 5.0 line. |
| R4 | **`libadwaita` is a from-scratch binding** with no existing metadata, api.xml, or hand-written partials. | Certain | Medium | Sequence it **last**, after `GtkSharp` builds clean. It is the most droppable item if the schedule bites — say so explicitly rather than half-landing it. |
| R5 | **WebKitGTK 6.0 has no Windows build.** `WebkitGtkSharp` is Linux-only in practice; `WebviewSection.cs` cannot run on Windows. | Certain | Low | Already partly true today. Guard the section with `GLibrary.IsSupported(Library.Webkit)` (the pattern already exists — [Global.cs:35](../Source/Libs/GtkSharp/Global.cs#L35)) and document the platform limitation. |
| ~~R6~~ | ~~**No GTK 4.22.4 Windows bundle exists**~~ | ~~Certain~~ | ~~High~~ | **RESOLVED by V4.** gvsbuild `2026.6.0` ships GTK4 4.22.4 + libadwaita 1.9.1 + GtkSourceView 5. Residual risk is only wingtk's release cadence and the version skew noted in `Source/Gir/README.md` (Debian libadwaita 1.9.2 vs bundle 1.9.1). |
| R7 | **`Gtk.Container` removal is an unshimmable break** for every downstream consumer. | Certain | High (external) | Ship a `Docs/migrating-from-gtk3.md` with the container→child-API mapping table. Do not build a compatibility shim. |
| R8 | **`netstandard2.0` target** may not survive if generated GTK4 code needs newer BCL surface. | Low | Low | `LPGen`/`LPUGen` already handle native ints without `nint`. Drop `netstandard2.0` only if a concrete compile error demands it, and note it as a breaking change. |
| R9 | **GDK/GSK-in-libgtk-4** (V3) produces `DllNotFoundException` at first use, not at build, so it survives all of gates 1-6 and only appears at gate 7. | Medium | Medium | Verify V3 empirically against a real install *before* Phase 3; add gate 7 early rather than at the end. |
| R10 | Scope. This is a rewrite wearing an upgrade's clothes: ~10 000 lines of hand-written binding code, 37 samples, 12 templates, a new codegen front-end, and 3 new assemblies. | Certain | — | Phases 1-4 (through `GioSharp` building clean) are the go/no-go milestone. If gate 1 cannot be made to pass, stop and reconsider D1. |

---

## 12. Suggested commit sequence

1. `chore: branch gtk4, version 4.22.4.x, SDK bands` — §1
2. `build: vendor .gir inputs + Source/Gir/README.md provenance` — V2
3. `tools: add GirToGapi` — §2.1-2.4
4. `tools: RegenerateApi cake target; GapiFixup --strict` — §2.5-2.6
5. `docs: gir-gapi-coverage.md` (gate 1 results) — §10
6. `gio: regenerate api.xml, triage metadata` — §5, smoke assembly
7. `build: assembly graph — drop Atk, add Graphene/Gsk/Adwaita` — §3
8. `shared: GTK4 library map (gdk/gsk live in libgtk-4)` — §3.2-3.3
9. …one commit per assembly, dependency order… — §5
10. `gtk: port hand-written layer` (several commits) — §6
11. `samples: port to GTK4` (grouped by §6.3 difficulty tier)
12. `templates+workload: GTK4` — §8
13. `ci: ubuntu-24.04, headless smoke run` — §9.2
14. `docs: update CLAUDE.md, add migrating-from-gtk3.md` — §9.3, R7

---

## 13. Open items requiring an answer before Phase 5

- **R3** — bind the GTK 4.10-deprecated TreeView/Dialog/ComboBox stack with `[Obsolete]`, or omit it? (Recommendation: bind + `[Obsolete]`.) The scale is now measured: **1 086 `deprecated-version=` annotations** in `Gtk-4.0.gir`.
- ~~**V4** — which Windows GTK4 bundle, and is there push access to `GtkSharp/Dependencies`?~~ **Answered:** gvsbuild `2026.6.0` release asset; no push access needed.
- Should `develop` (GTK3) continue receiving fixes, or be archived at `3.24.24.x`?

## 14. Progress

| Phase | Scope | State |
|:------|:------|:------|
| **V1–V5** | Blocking pre-flight verifications | ✅ **complete** |
| **1** | Branch, versioning, scaffolding | ✅ **complete** |
| **2** | `GirToGapi` converter | ✅ **complete** |
| **3** | Assembly graph, native library map | ✅ **complete** |
| **4** | api.xml regeneration + metadata triage | ✅ **complete** |
| **5** | Hand-written layer port | ✅ **complete** |
| **6** | Samples port (37 sections) | ✅ **complete** |
| **7** | Templates and workload | ✅ **complete** |
| **8** | Native runtime, CI | ✅ **complete** |
| **9** | Open items from §15 | ✅ **complete** |
| **10** | First run on Linux (gate 7) | ✅ **complete** — 227 tests, 226 pass, 1 skip |
| **11** | Hand-written layer test sweep | ✅ **complete** — 403 tests; hand-written coverage 30.5% → 43.6%; four more defects fixed |
| **12** | Second sweep: the files with no coverage | ✅ **complete** — 484 tests; 43.6% → 50.3%; six more defects fixed, one of them a process-killing ABI |
| **13** | Third sweep: the tree wrappers, main loop, Object and Value | ✅ **complete** — 556 tests; 50.3% → 52.7%; four more defects, and the "intermittent" abort explained |
| **14** | Fourth sweep: the Gtk 4 application layer | ✅ **complete** — 603 tests; a converter defect affecting 67 sites across seven assemblies |
| **15** | Fifth sweep: the rest of Cairo | ✅ **complete** — +29 tests; joins, caps and compositing decided by arithmetic, and two hash functions that equality tests structurally could not see |
| **16** | Sixth sweep: the hand-written GLib layer | ✅ **complete** — five defects, three of them code that had never executed: `Signal.Emit`'s returning branch, and three ways a `GVariantType` lies about its own string |
| **17** | Seventh sweep: the Gdk that needs no display | ✅ **complete** — +36 tests; five defects, four of them memory-safety, including the receiver-eating `gdk_content_formats_union*` behind the one-run-in-three crash |
| **18** | Eighth sweep: the text stack | ✅ **complete** — +34 tests; two calls Gtk 4 reshaped underneath the binding, one of which took the process down when read |
| **19** | Ninth sweep: the satellite assemblies | ✅ **complete** — 765 tests; `GirToGapi` had emitted **no getter for any of the 889 public fields in the tree** |
| **20** | Green on both platforms, and a CI that can run | ✅ **complete** — 766 tests: Windows 763 passing / 3 skips, Debian forky 765 / 1. Four Linux-only failures fixed (glycin vs the classic pixbuf loaders, and a non-portable `Marshal.SizeOf`), and the workflow rebuilt and verified end to end in a `debian:forky` container |
| **21** | Tenth sweep: authoring types from the managed side | ✅ **complete** — 797 tests (Windows 794 / 3 skips, Debian forky 796 / 1); two defects, one of which made **every managed `Widget` subclass that overrode `OnActivate` unconstructible**, and one that crashed `Widget.Allocate` whenever a layout manager placed a child with no transform |
| **22** | Eleventh sweep: `GtkExpression` and property binding | ✅ **complete** — 835 tests (Windows 832 / 3 skips, Debian forky 834 / 1); four defects in the one fundamental-type hierarchy nothing had called: `Evaluate` **discarded its own result** on both `GtkExpression` and `GtkExpressionWatch`, `gtk_expression_bind` ate a reference the wrapper went on owning, the two array-plus-count constructors took a single expression, and `GtkCClosureExpression` had **no constructor emitted at all** |
| **23** | Twelfth sweep: the Gio that needs a filesystem and a main loop | ✅ **complete** — 871 tests (Windows 868 / 3 skips, Debian forky 870 / 1); six defects, headed by `GSettings::changed` and `GFileMonitor::changed` sharing one `GLib.ChangedArgs` class, which made a monitor's only signal throw `InvalidCastException` inside the marshaller and name nothing; plus a `gchar ***` read as a string, a codegen rule that suppressed five accessors it had no replacement for, a NULL `GVariant` returned as a wrapper around `IntPtr.Zero`, a `Dispose` that unreffed before disconnecting, and `GLib.FileFactory` leaking every `GFile` it made |

### Phase 1 — complete

| Item | State |
|:-----|:------|
| §1.1 branch `gtk4` from `develop`, README note | done (local; not pushed) |
| §1.2 `build.cake` version defaults + CI branch check | done — `Init` prints `Version: 4.22.4.1` |
| §1.3 SDK feature bands → `8.0.100`–`8.0.400` | done; `WorkloadManifest.in.json` carries no band references |
| §1.4 keep both TFMs and `LangVersion 9` | verified, unchanged |
| §1.5 `Source/Gir/` | done — populated, not just created: 13 vendored `.gir` + provenance + `fetch-gir.py` |
| §1.5 new assembly directories, `Docs/gir-gapi-coverage.md` | done |

### Phase 2 — complete

`Source/Tools/GirToGapi/` converts all ten assemblies' `.gir` into gapi api.xml,
wired to a `RegenerateApi` target deliberately outside the `Default` chain so
ordinary builds stay hermetic. `GapiFixup` gained `--strict`.

**Gate 1** (convert a Gtk 3 gir, compare against the checked-in Gtk 3 api.xml,
which is known `gapi2xml.pl` output): 99.79 % kind agreement, 96.37 % member
coverage, 98.46 % name agreement, and padding slot counts matching exactly. Every
difference is explained in `gir-gapi-coverage.md` §6. **Gate 2** (schema validity)
passes. R1 and R2 are retired.

### Phase 3 — complete

`AtkSharp` deleted; `GrapheneSharp`, `GskSharp`, `AdwaitaSharp` added, with
`Settings.cake`, the solution, `GtkSharp.csproj` and `Samples.csproj` following.
Library map rewritten with the gvsbuild DLL names verified in V4.

One deviation from §3.2, deliberately: `Library.Gdk` is kept and `Library.Gsk`
added, both pointed at the Gtk 4 filenames, rather than dropping `Gdk` and
redirecting through metadata. Runtime behaviour is identical — all three resolve
`libgtk-4.so.1` — but no metadata redirect is needed and the hand-written files
that call `GLibrary.Load(Library.Gdk)` keep compiling until Phase 5 reaches them.

### Phase 4 — complete

Every generating assembly reaches zero unmatched metadata rules with
`StrictMetadata = true`; `Prepare` completes all eleven and exits 0.

| Assembly | Rules left | Unmatched | Plan predicted survival |
|:---------|-----------:|----------:|:------------------------|
| `GioSharp` | 196 | 0 | ~90 % |
| `PangoSharp` | 107 | 0 | ~90 % |
| `GdkSharp` | 65 | 0 | ~25 % |
| `GtkSharp` | 403 | 0 | ~30 % |
| `GtkSourceSharp` | 48 | 0 | ~20 % |
| `WebkitGtkSharp` | 4 | 0 | ~0 % |
| `GrapheneSharp`, `GskSharp`, `AdwaitaSharp` | 1–7 | 0 | new |

Two systematic causes accounted for more of the churn than Gtk 4 attrition did:
function grouping (only the first `c:symbol-prefixes` entry was being tried, so
every Gio global function fell into `Global`) and out-parameter directions, which
GIR annotates natively — the rules that supplied them by hand are now redundant.
`scripts/triage-metadata.py` and `scripts/retire-rules.py` carry the method; 705
of GtkSharp's 732 unmatched rules were decidable mechanically.

### Phase 5 — complete

**All eleven assemblies build clean on `net8.0` and `netstandard2.0`.** The only
build errors left in the tree are 29 in `Source/Samples`, which is Phase 6.

| Assembly | Notes |
|:---------|:------|
| `GLibSharp`, `CairoSharp` | hand-written, untouched |
| `GrapheneSharp`, `GioSharp`, `PangoSharp` | clean |
| `GdkSharp` | 29 hand-written files deleted, the `Event` shim among them |
| `GskSharp` | clean; the 36-type `RenderNode` hierarchy is bound, see below |
| `GtkSharp` | 46 hand-written files deleted; the `Expression` hierarchy is bound |
| `AdwaitaSharp` | new binding, clean from scratch |
| `GtkSourceSharp`, `WebkitGtkSharp` | clean |

`Gtk.Container` is gone with no compatibility shim: **R7 discharged** as the plan
intended.

#### What the last three assemblies needed

All three were blocked by one thing. GtkSharp hides `GtkBuildable` and every
`implements` entry naming it (`GtkSharp.metadata:47`, `:93`), and an `implements`
entry pointing at a hidden interface makes codegen drop the **whole implementing
type**, not just the entry. That silently removed `AdwSidebarSection` and eleven
GtkSourceView types — they never appeared in the generated output at all, and the
only clue was a `WARN` about an unknown GInterface. Downstream assemblies need the
same rule GtkSharp has; it is now in all three.

The rest were name collisions in the three shapes this migration keeps producing,
resolved with the conventions already established: rename the function when it
exists to raise a signal (`Emit*`), rename the signal to a past participle when
the function performs the action, hide the property when a method already
provides it.

One long-standing latent bug surfaced: `GtkSourceSharp.metadata` set
`scope="notify"` — the spelling `Gapi.xsd` documents — while `MethodBody` has
always tested for GIR's `"notified"`. The mismatch was invisible while nothing was
hidden; once closure and destroy indices are emitted, the parameter gets hidden
without the branch that declares its local ever running.

#### Codegen fixes made during Phase 5

All were pre-existing bugs that Gtk 3 never reached.

| Fix | Why |
|:----|:----|
| Interface properties stop emitting `implementor.` into implementing classes | `Property.RawGetter` keyed off "the property belongs to an interface" — equally true when it is emitted into an implementor (`ObjectGen.cs:232`), which has no such field. |
| `sizeof(IntPtr)` → `IntPtr.Size` | `sizeof` on a pointer-sized type needs an unsafe context; these expressions land in ordinary signal-marshalling code. |
| Enum GType helper classes are public | A signal in another assembly carrying the enum needs its GType; `internal` put `Gdk.DragActionGType` out of GtkSharp's reach. |
| `StructField` honours `is_callback` | Function-pointer fields were emitted with an empty type. A struct's fields are its layout, so the slot stays, as an opaque pointer. |
| `StructField` declaration matches `EqualityName` | Private array fields were declared StudlyCaps but referenced lower-cased by the generated `Equals`. |
| `Ctor` skips hidden parameters and stops mis-indexing | It indexed `Parameters` by the filtered names index, which only lined up while every parameter contributed a name. |
| `Property` name-vs-type guard also checks the implementor | |

Two naming consequences worth knowing: `GtkText` is bound as **`Gtk.TextWidget`**,
because `GtkEditable`'s `Text` member cannot live on a class called `Text`; and
Gtk 4's `Gtk.EventArgs`/`Gtk.EventHandler` shadow the `System` ones inside
`namespace Gtk`, so the hand-written layer qualifies them explicitly.

**Automatic signal connection is unsupported.** Gtk 4 replaced
`gtk_builder_connect_signals_full`, `gtk_widget_class_set_connect_func` and
`GtkBuilderConnectFunc` with `GtkBuilderScope`, which is not bound.
`SignalConnector.ConnectSignals` throws rather than silently doing nothing — a
template whose handlers were never wired reads as a UI that ignores every click,
much harder to diagnose than an exception naming the cause.

### GLib fundamental types — bound

Gtk 4 uses GLib *fundamental* types — `glib:fundamental="1"`, GTypeInstance with
their own ref/unref rather than GObject descendants — for three whole
hierarchies. All three are now bound, generated rather than hand-written:

| Hierarchy | Types | Refcounting |
|:----------|------:|:------------|
| `GskRenderNode` | 36 | `gsk_render_node_ref`/`unref` |
| `GdkEvent` | 14 | `gdk_event_ref`/`unref` |
| `GtkExpression` | 7 | `gtk_expression_ref`/`unref` |

**The base is `GLib.Opaque`, not `GLib.Object`.** Opaque already wraps a bare
handle, tracks ownership and routes disposal through `Ref`/`Unref` hooks, so the
lifetime lands on the type's own refcounting rather than on `g_object_ref`. That
is what made this tractable: the alternative considered earlier — rooting them at
`GLib.Object` — would have compiled and then called `g_object_unref` on handles
GObject does not own.

GIR supplies everything needed. `glib:fundamental="1"` marks every member of a
hierarchy; `glib:ref-func`/`unref-func` appear on the root only, which is exactly
where the overrides belong. The converter emits these as `fundamental="true"`,
`ref_func` and `unref_func` on `<object>` (`ObjectEmitter.cs:54`), and codegen
keys off them:

| Site | Change |
|:-----|:-------|
| `ObjectGen.cs` — base | A fundamental root has no parent, so `GLib.Opaque` is substituted |
| `ObjectGen.cs` — `GenFundamentalRefcounting` | Emits `Ref`/`Unref`/`Copy` overrides and a finalizer on the root, matching `OpaqueGen`'s shape |
| `ObjectGen.cs` — `CallByName` | Transfer-full is `OwnedCopy`, Opaque's spelling |
| `ObjectBase.cs` — `FromNative` | Resolves via `GLib.Opaque.GetOpaque`, not `GLib.Object.GetObject` |
| `Ctor.cs` | Still chains to `base (IntPtr.Zero)`, but skips `CreateNativeObject`: a fundamental type is not a GObject and cannot be subclassed from managed code |
| `ReturnValue.cs` | Same `OwnedCopy` spelling on the callback return path |

Two ownership details are worth recording, because getting either wrong leaks or
double-frees silently:

- **`Copy` returns a *separate* owning wrapper**, rather than Opaque's default
  `return this`. That default is right for a plain boxed pointer and wrong for a
  refcounted one. A distinct wrapper makes both callers correct: `GetOpaque` on a
  transfer-none value yields a wrapper holding its own reference, and `OwnedCopy`
  hands a fresh reference to the callee while the original keeps — and still
  unrefs — its own.
- **Constructors set `Owned = true` before assigning `Raw`.** Opaque's `Raw`
  setter takes a reference via the `Ref` hook, which is right when wrapping a
  borrowed pointer and one too many for a transfer-full constructor result.
  Claiming ownership first makes the hook, guarded on `!Owned`, correctly do
  nothing. (This over-referencing is pre-existing for hand-written refcounted
  opaques such as `Pango.AttrList`, which is not fixed here.)

Consequences: `Gtk.Snapshot.ToNode` and `AppendNode` now exist, so **custom
widget drawing via `GtkSnapshot` is no longer blocked** for the §6.3 DrawingArea
samples. The hand-written `Source/Libs/GdkSharp/Event.cs` shim is deleted, and
with it the restriction that events could not be constructed from managed code.
`GdkSharp-symbols.xml` lost its Gtk 3 event entries, which routed `GdkEvent`
through that shim's factory and would have overridden the generated type.

Still unbound: `GtkParamSpecExpression`, which derives from `GParamSpec` rather
than from `GtkExpression`; `SymbolTable` maps `GParamSpec` to `IntPtr`.

### Phase 6 — in progress

Samples are the acceptance test: the repository has no test project (CLAUDE.md
§Tests), so a running Samples app is the only end-to-end verification available.

> **The error count understates the work, and did so from the start.** Roslyn
> binds declarations before method bodies and stops if the declaration phase has
> errors, so while any signature, base type or override is unresolved, *no body
> is ever type-checked*. Confirmed directly: a deliberately bogus type inserted
> into a method body produced no diagnostic, while one in a method signature
> did. The counts below therefore measure the **declaration surface only**.
> `new VBox()` and `Box.PackStart` are already known to be broken and have never
> appeared in a build log. A survey of the sources finds 46 `PackStart` calls,
> 16 `.Add(`, 17 `Container` references, plus `VBox`/`HBox` and `ShowAll`
> awaiting the body phase.

**Phase 6 compiles.** `dotnet cake build.cake --BuildTarget=Build` reports **0
errors** across all eleven assemblies and `Source/Samples`.

The body phase opened at **127 errors** once the declaration surface was clear —
the real size of the port, always there, with the earlier counts of 29, 14 and 6
measuring only what Roslyn binds before it gives up.

> **Compiling is not the acceptance test.** The samples have not been *run*: that
> needs the Gtk 4 runtime and is Phase 8's headless smoke run. Everything below
> is verified by the compiler only.

#### Library defects found by porting the samples

Every one of these was silent — nothing in a build log would have shown them,
and they are exactly what the plan expected Phase 6 to surface.

| Defect | Consequence |
|:-------|:------------|
| `Application` loaded `gtk_main`, `gtk_main_quit`, `gtk_events_pending`, `gtk_main_iteration`, `gtk_main_iteration_do` — all removed in Gtk 4 | `FuncLoader.LoadFunction` returns `default(T)` for a missing export, so `Application.Run()` was a null delegate: **no Gtk 4 application could start**. Now drives a `GLib.MainLoop`, which is what `gtk_main` did anyway. |
| `gtk_init`/`gtk_init_check` became niladic in Gtk 4 | Declared `(ref int argc, ref IntPtr argv)`, passing two arguments to a function taking none, then trying to recover options never consumed. |
| `Widget.Dispose`/`Destroy` called `gtk_widget_destroy` | Also removed; disposing any window threw `NullReferenceException`. Now `gtk_window_destroy` for toplevels, unparenting otherwise. |
| `Widget.Destroyed` surfaced `GtkWidget::destroy` | Signal removed in Gtk 4, so it could never fire. Deleted, so users get a compile error instead of a window that ignores being closed. |
| `CellRenderer.GetSize.cs` wrote to `class_abi.GetFieldOffset("get_size")` | No such field in `GtkCellRendererClass`; the lookup returns null and throws for any subclass overriding `OnGetSize`. |
| `Widget.InitTemplateForType` always called `ConnectSignals` | That throws under Gtk 4, so **every** `[Template]` widget failed. Now called only when the template declares a `<signal>`. |
| `JSCValue` unresolvable, so codegen silently dropped every member naming it | The finish half of the async javascript calls vanished, leaving `evaluate_javascript` startable but never collectable. Mapped to `gpointer`. |

#### What the samples needed

`PackStart` was the largest single pattern at 38 calls, and
`ContainerChildPropertiesSection` the largest single file at 33 errors — its
premise, GtkContainer child properties, was replaced by *three* separate
mechanisms, so it now demonstrates all three rather than being deleted.

Several changes record capabilities Gtk 4 dropped rather than renamed:
`Monitor.IsPrimary` and `Workarea` (Wayland has no primary monitor and cannot
report the area panels leave free), `gdk_device_get_position` (no root
coordinates exist), `Gdk.Threads` (the global GDK lock is gone), the app menu,
`gtk_dialog_run` (nested main loops are not allowed), and `StyleContext.GetPadding`.

`ImageDrawn`, `CustomCellRenderer` and `Gtk.Snapshot` drawing exercise the
render-node hierarchy bound earlier; `DrawingAreaSection`'s Gtk 3 code called
`cr.Dispose()`, which under Gtk 4 would be a double free.

Two pre-existing sample bugs surfaced: `ColorButtonSection` wrote `Blue = 255`
into a 0..1 component, and called `Parse` on the button's own `Rgba`, filling in
a copy and changing nothing.

### Phase 7 — complete

`PackageTemplates` and `PackageWorkload` both succeed, and the packs contain
exactly the eleven assemblies.

**Templates.** All 24 `.glade` documents across both template sets became Gtk 4
`.ui`, by script: `<requires lib="gtk" version="4.0"/>`, `<packing>` removed with
`expand` becoming the child's own `hexpand`/`vexpand` chosen by the containing
box's orientation, `visible`/`can-focus` dropped, and `margin_left`/`margin_right`
becoming `margin-start`/`margin-end` so they follow text direction. Code in all
three languages moved from `DeleteEvent` to `CloseRequest`, from `Show` to
`Present`, and the `GtkSharp` package pin went to `4.22.4.*`.

**Workload.** As §7.2 predicted, `GtkSharp.Runtime` and `GtkSharp.Ref` need no
edits — both glob rather than listing assemblies — and `WorkloadManifest.in.json`
carries substitution tokens rather than literal versions. All four SDK feature
bands pack.

**`Builder.Autoconnect` had the same defect as the template path** and is fixed
the same way. It does two independent jobs: binding `[Object]` fields, which
works, and connecting signals, which throws under Gtk 4. Every template calls it,
so every template would have thrown. `AddFromStream` now records whether the
document declared a `<signal>`, and the signal half only runs when it did.

**A packaging trap worth knowing about.** `GtkSharp.Ref` globs `*.dll` out of the
shared build output directory, so **any stale assembly there ships**. A pre-migration
`AtkSharp.dll` was still present and went into both packs — Gtk 4 has no separate
Atk binding, and the project had already been deleted, but `Clean` only clears
the assemblies it knows about, so nothing removed it. CI builds from clean and is
unaffected; an upgraded working copy needs `--BuildTarget=FullClean` at least once.

### Phase 8 — complete

`GtkSharp.targets` now fetches the gvsbuild Gtk 4 bundle into
`%LOCALAPPDATA%\Gtk.22.4`, with the sentinel at `bin/gtk-4-1.dll` — matching
both `GLibrary`'s `SetDllDirectory` and `_libraryDefinitions[Library.Gtk][0]`.
The bundle's `python/` and `wheels/` directories are removed after extraction:
`Unzip` cannot extract selectively, and they are build-time material worth about
a third of the 300 MB archive.

**The bundle was downloaded and extracted for real**, and §8.1's assumptions
hold: the tree is `bin/ etc/ include/ lib/ python/ share/ wheels/`, the DLLs
carry no `lib` prefix, and `bin/gtk-4-1.dll` is present.

CI moves to `ubuntu-24.04` and runs the tests under `xvfb-run`. `build.cake`'s
branch check already said `gtk4` from Phase 1.

### Phase 8b — test project (added beyond the original plan)

The plan called for a bespoke `--smoke-exit` flag on the samples. That was built,
run, and then **replaced by `Source/Tests/GtkSharp.Tests`, an xunit project**, so
the checks are repeatable, individually named, and extensible. The smoke flag is
gone; `--BuildTarget=Test` is the entry point, and CI calls it.

| | |
|:--|:--|
| `SampleSectionTests` | One test case per `[Section]` type — 31 of them — each asserting a live widget comes back, not merely that construction did not throw. The samples are the widest exercise of the bindings here. |
| `BindingTests` | 14 behavioural round-trips, each pinning something this migration fixed: the button label ctor, box append/reorder/unparent, `Grid.QueryChild`, `StackPage.Title`, `RGBA.Parse` components, `Snapshot.ToNode` returning a `GskRenderNode`, `ConstantExpression.ValueType`, and `Application.Run` returning after `Quit`. |
| `GtkFixture` | Owns the single thread Gtk is initialised on and marshals every test body onto it — Gtk may only be used from the thread that called `gtk_init`, and xunit promises no thread affinity. Parallelisation is disabled assembly-wide. Each work item drains pending main-loop work, so a failure in layout or a draw function is attributed to the test that caused it. |

**83 tests, all passing**, against a real Gtk 4 runtime, with **no GLib CRITICAL or WARNING output left**.

`MainWindowTests` closes the one gap between the tests and the smoke run they
replaced: the smoke run also built `MainWindow`, added it to the application and
presented it, and nothing else assembles the sections into a window or drives
`GtkSourceView` and `TreeStore` together. It asserts the Gtk 4 layout the port
produced — title on the window rather than the header bar, the header bar as
`Titlebar`, a single `Paned` child with the tree view as its `StartChild`, and a
populated section model — then presents and checks the window becomes visible.

#### What running it actually found

Compiling had proved very little, exactly as feared. Three sections failed on
first run, all `NullReferenceException` from a null delegate:

| Defect | Cause |
|:-------|:------|
| `Button(string)` threw for every caller | It called `gtk_button_new_from_stock`, removed in Gtk 4 along with the stock registry and the `use_stock` property. Gtk 3 read the string as a stock id; it is now simply the label, which is what nearly every caller already meant. |
| Every `[Template]` widget crashed | `InitTemplateForInstance` dereferenced `data.SignalConnector` unconditionally, and the Phase 6 fix only assigns it when the template declares a `<signal>`. |
| `DeclaresSignals` misread its own documentation | It searched for the text `<signal`, which also matches a *comment* explaining that a document deliberately has none — precisely the case that must not be misread. It now parses the XML (`BuilderXml.cs`), shared by `Builder` and the template path. |

Writing the window test then exposed a fifth defect, this one silent by
construction. `Program.EnsureApplication` discarded the result of
`Application.Register`, and registration fails wherever there is no session bus
— which is the normal case on Windows. An unregistered GApplication **silently
refuses `AddWindow`**: the window still works, but the application never tracks
it, and `Application.Windows` stays empty with nothing said. The result is now
checked and a warning printed. The test asserts `Visible` after `Present`
instead of asserting window tracking, because the latter is environment
dependent and would fail for reasons that are not defects.

Then the test project found one more that the smoke run had masked:
`ButtonSection` adds an action to the application, and the smoke run happened to
have built one in `Main` first. The bootstrap is now a shared
`Program.EnsureApplication()` used by both, so the sections are exercised against
the setup the real program gives them.

`CLAUDE.md` is updated for Gtk 4 throughout, including a §Tests section that
explains why calling matters more than compiling here.

---

### The biggest defect the tests found: every `throws` method had the wrong ABI

The GLib diagnostics the test run printed were not noise. Chasing
`gdk_pixbuf_loader_write: assertion 'error == NULL || *error == NULL' failed`
led to a bug affecting **724 methods across seven assemblies** — every method
marked `throws="1"`.

GapiCodegen decides whether a call needs a `GError` by looking for a trailing
`GError**` **parameter**: `Parameters.cs` turns it into an `ErrorParameter`, and
`MethodBody.ThrowsException` walks the parameter list looking for its `CType`.
The `throws` attribute only tells codegen to *hide* that parameter — it never
adds one. `gapi2xml.pl` emitted both, because the C header it parsed had the
argument written out; GIR states it as a flag on the callable instead.

`GirToGapi` emitted the attribute and not the parameter. So every generated
P/Invoke declared **one argument fewer than the C function takes**, and the
callee read whatever happened to be in the argument register. Two consequences,
neither of which a compiler could see:

- GLib's `g_return_if_fail (error == NULL || *error == NULL)` fired against
  garbage.
- **No `GException` was ever raised.** Failures returned `false` and looked like
  success.

A second, narrower case: a method whose *only* C argument is the `GError**` has
no `<parameters>` element in GIR at all, and the emitter returned early before
reaching the throws check. `gdk_pixbuf_loader_close` and its kind were left
without an error argument even after the first fix.

Both are fixed in `CallableEmitter.AddBody`, all api.xml regenerated, and the
behaviour is pinned by a test that asserts a failing call raises `GException`
with a message.

Two smaller fixes came from the same run: `ApplicationOutput.Widget` is a
singleton, so a second `MainWindow` was handed a widget that still belonged to
the first, and Gtk 4 refuses to re-parent in place — it is now detached first.
And `Program.EnsureApplication` no longer discards the result of
`Application.Register`.

### Pressing the buttons: 13 more dead symbols

Constructing a section never opens a dialog — dialogs appear on a press — so
`ChildWindowTests` presses every button each section contains and requires that
any window that opens is a live toplevel which can be closed again. Getting that
working took two corrections worth recording:

- **`Widget.Activate` does not reach the handler.** Gtk 4 routes a press through
  a gesture, and `gtk_button_clicked` is gone. The theory passed on all 31
  sections while pressing nothing. `GLib.Signal.Emit(button, "clicked")` works,
  and `Pressing_the_file_chooser_button_opens_a_toplevel` now guards against the
  same silence returning.
- **Two buttons must not be pressed by a test**, and neither is a defect:
  `PixbufDemo` is a manual leak-stress toggle whose first press enters a loop
  that only a second press ends, and `LinkButton` asks the desktop to open a URI,
  which launches a browser.

The first real press crashed the run: `StyleContext.GetProperty` was a null
delegate. Rather than chase these one at a time, a sweep compared **every symbol
the hand-written layer loads** against the vendored `.gir` files. It found 13
that Gtk 4 does not export, each a `NullReferenceException` waiting to be
called:

| Removed in Gtk 4 | Was |
|:-----------------|:----|
| `gtk_style_context_get_property` | `StyleContext.GetProperty` — style properties went with the Gtk 3 theming API |
| `gtk_get_current_event` | `Application.CurrentEvent` |
| `gtk_binding_set_by_class`, `gtk_binding_entry_add_signall` | the whole `[Binding]` machinery — `GtkBindingSet` is replaced by `GtkShortcutController` |
| `gtk_widget_class_find_style_property`, `gtk_widget_style_get_property` | `Widget.StyleGetProperty` |
| `gtk_cell_renderer_render` | `CellRenderer.Render` |
| `gtk_icon_theme_list_icons` | `IconTheme.ListIcons` |
| `gtk_icon_theme_{get,set}_search_path_utf8` | a Gtk 2/3-on-Windows artifact that never existed in Gtk 4 |
| `gtk_image_new_from_stock` | `Image(stock_id, size)` |
| `gtk_window_{get,set}_icon_list`, `..._default_icon_list` | `Window.IconList`, `Window.DefaultIconList` |

All are removed rather than left to throw, so callers get a compile error.

**JavaScriptCore was being loaded from the wrong library.** 27 `jsc_*` symbols
were looked up in the WebKit module handle, which cannot resolve them — a handle
only exposes its own exports. `Library.JavaScriptCore` is added to the enum and
the filename map, and the lookups retargeted.

Two sample bugs surfaced too: `StyleContextSection` asked for CSS properties by
name, which Gtk 4 cannot do, and now reads the typed getters that remain; and
`LinkButtonSection` passed a caption to `gtk_link_button_new`, whose argument is
the URI, so Gtk refused to follow it.

## 15. Phase 9 — decisions taken on the open items

All eight open items were put to the maintainer and decided. Recorded here
before any of it is built, so the reasoning survives the diff.

| # | Item | Decision | Consequence |
|:--|:-----|:---------|:------------|
| 1 | Caller-allocates out-parameters corrupt the stack (~154) | **Fix in GapiCodegen** | Allocate the caller's storage when an out-parameter is reference-typed, keeping the `out X` signature. Fixes the class, not instances. |
| 2 | `GLib.Opaque` over-references transfer-full ctor results | **Fix `GLib.Opaque` itself** | Constructors claim ownership before assigning `Raw`, as fundamental types already do. Changes lifetime behaviour repo-wide, so it wants tests pinning refcounts. |
| 3 | R3 — Gtk 4.10-deprecated TreeView/Dialog/ComboBox stack | **Mark `[Obsolete]`** | Emitted from the `deprecated="1"` already in the api.xml. Consumers porting from Gtk 3 get a warning. |
| 4 | Samples use the deprecated stack heavily | **Port to ColumnView/ListView** | `TreeViewSection`, `ListStoreSection` and `EditableCellsSection` are rewritten onto the Gtk 4 replacements rather than suppressing warnings. Substantial, and it removes the only worked TreeView examples — chosen deliberately over keeping them. |
| 5 | JavaScriptCore unbound; js results are opaque `IntPtr` | **Add a `JavaScriptCoreSharp` assembly** | A twelfth assembly. Cannot be exercised locally: gvsbuild ships no WebKit at all, so it is build-verified here and first runs under CI. |
| 6 | Its `.gir` has to come from somewhere | **Fetch from Debian forky** | Same provenance path as the other 13, via `fetch-gir.py`, with the sha256 recorded in `Source/Gir/README.md`. |
| 7 | `GtkParamSpecExpression` hidden | **Bind it, rooted at `GLib.Opaque`** | Ignores its `GParamSpec` inheritance. The declared hierarchy is therefore not the C one; taken knowingly, because `GParamSpec` is not bound as a type anywhere. |
| 8 | CI has never run | **Push the branch** | Publishes the work and triggers the NuGet step on the configured registry. Done last, so CI sees the finished state. |

Order of work follows the dependencies: the codegen fix and the `GLib.Opaque`
fix change generated output and lifetimes, so they come before anything that
builds on them; `[Obsolete]` precedes the samples rewrite that reacts to it; the
push comes last.

## 16. Remaining

- **WebKit and JavaScriptCore are never exercised locally.** gvsbuild ships
  neither DLL, so `WebkitGtkSharp` and `JavaScriptCoreSharp` are build-verified
  and pack correctly, but their first run against the real libraries is CI on
  ubuntu-24.04.
- **114 deprecation warnings remain in the samples**, deliberately.
  `ComboBoxSection`, `CellRendererSection`, `EntrySection` and others demonstrate
  APIs that Gtk 4.10 deprecated but still ships. Only the three sections chosen
  in Phase 9 were rewritten; the warnings are the honest record of the rest.
- **`GtkParamSpecExpression`'s declared hierarchy is not the C one** — it is a
  `GParamSpec` there and a bare `GLib.Opaque` here, with no `ref_func`.
- **CI has not been observed running.** The workflow now runs inside a
  `debian:forky` container rather than bare `ubuntu-24.04`, because the runner's
  Gtk 4.14 is eight minor versions behind the 4.22.4 the bindings are generated
  from — and a symbol the binding declares but the library lacks is a null
  delegate, not a link error, so that skew would have hidden exactly the class of
  bug the suite exists to find while still reporting green.
- **Boxed types with no C allocator cannot be constructed** — `new Gsk.RoundedRect()`
  yields a null handle through the inherited `GLib.Opaque()` constructor, and the
  `Init*` methods write through it. A fix would generate an allocating
  constructor sized from `abi_info`, as the caller-allocates fix does. One test
  is `Skip`ped pointing at it. See `Docs/testing.md`.
 The workflow moved to ubuntu-24.04 and
  gained the test step, but no run has been seen from this side.

---

## 16. Phase 10 — the first run on Linux

Gate 7 could not be met from Windows: gvsbuild ships no WebKit, so every
`WebkitGtkSharp` and `JavaScriptCoreSharp` path was skipped rather than
exercised, and `GLibrary`'s Linux filename slots were never read. The suite was
therefore run on Debian trixie in WSL — Gtk 4.18.6, libwebkitgtk-6.0-4 2.52.5,
libjavascriptcoregtk-6.0-1, gtksourceview 5.16, .NET 8.0.423.

Three defects surfaced that Windows structurally could not show. Each is written
up in `Docs/testing.md`; in brief:

1. **Two managed types claimed `JSCValue`'s GType.** The hand-written
   `WebKit`-side `JavaScript.Value` and the generated `JavaScriptCore.Value` were
   both registered, so which one a signal argument arrived as depended on
   registration order. The handler's cast threw inside the signal marshaller,
   where nothing can catch it, and the run aborted. The duplicate is deleted, and
   `WebkitGtkSharp-api.xml` regenerated so `script-message-received` carries a
   typed `JSCValue*` rather than the `gpointer` it fell back to when the
   converter could not resolve the type.

2. **Nothing registered `JSCValue` at all.** `ObjectManager.Initialize()` runs
   from the static constructor of its *own* assembly's types, and a WebKit
   program meets its first JavaScriptCore type as a signal argument — after the
   lookup has already missed. The name-based fallback cannot help: it splits a C
   name at the second capital, turning `JSCValue` into `J.SCValue`.
   `WebkitGtkSharp.ObjectManager.InitializeExtras` now chains
   `JavaScriptCoreSharp.ObjectManager.Initialize()`.

3. **One missing type killed a whole assembly.** `gtksourceview` 5.18 added
   `GtkSourceAnnotation`; trixie ships 5.16, which has no
   `gtk_source_annotation_get_type`. Reading its `GType` called a null delegate,
   the `NullReferenceException` aborted `ObjectManager.Initialize()` partway, and
   *no* GtkSource type ended up registered — 38 tests failed, none of them about
   annotations, all reporting `TypeInitializationException` on an unrelated
   class. `GapiCodegen` now routes each registration through a helper that
   catches `NullReferenceException` and skips that one type. This is not
   cosmetic: without it, any installed library even slightly older than the girs
   disables its entire binding.

The third is the general lesson of this upgrade restated once more. The api.xml
describes the library the girs came from, not the library that is installed, and
the gap is invisible until something calls. Guarding the registration is the
difference between "one class from a newer release is unavailable" and "this
assembly does not work."

Remaining gates: 10 (`dotnet new gtkapp` end-to-end), 11 (workload install), and
CI, which still has not been observed running.

---

## 17. Phase 11 — testing the hand-written layer

The 8% overall line rate quoted in earlier phases measured the wrong thing.
Nearly all of it is code under `Generated/` — property getters and P/Invoke
declarations emitted from a template, uniform by construction, where one
round-trip exercises the same emission path as the thousand like it. The figure
tracks how many bindings exist, and moves whenever the api.xml does.

The number that matters excludes `Generated/` and `Samples`: the hand-written
layer is where a defect can actually live, and it stood at **30.5%**. Ranked by
uncovered lines it is also a work queue, and working down it produced 176 new
tests across seven files (403 total, 402 passing on Linux, 400 on Windows with
three WebKit skips) and took the hand-written figure to **43.6%**.

Four more defects came off that queue, all of them in code nothing had ever
called:

1. **`GLib.PtrArray` had never worked.** All seven `g_ptr_array_*` symbols were
   loaded from `Library.GObject`; they live in GLib. Every constructor threw
   `NullReferenceException` on first use, naming nothing. The same null-delegate
   failure mode as the removed Gtk 3 functions, from the opposite cause — not a
   symbol that went away, but one looked for in the wrong place.

2. **`GioStream` could not open a file.** Both file constructors threw
   `NotImplementedException`, so the class could only wrap a stream the caller
   had already opened. They now go through Gio and map `System.IO.FileMode`.

3. **`GioStream.Read` overran its buffer.** The guard read
   `offset + count - 1 > buffer.Length`, admitting a request one byte too long;
   with `offset == 0` that length went straight to the native read. `Write`, ten
   lines below, had the same guard written correctly. Its offset path also
   copied the whole scratch buffer instead of the bytes read, so a short read
   overwrote the caller's data with zeroes.

4. **`GLib.Log.WriteLog` was the only instance method** on an otherwise entirely
   static class, and touched no instance state — writing a log line meant
   constructing a `Log` first.

Three further tests failed because the expectation was wrong rather than the
library, and each is now pinned as behaviour, because in every case the
plausible assumption is the one that produces silently wrong results:
`Date.DaysBetween` returns `date2 - date1` and so reads backwards from its name;
`KeyFile` discards translations on load without `KeepTranslations` and then falls
back to the untranslated value rather than failing; and a `TreeIter` identifies a
row rather than a position, so reordering code that treats it as an index moves
the wrong row.

Still untouched and worth the next pass, largest first: `Gtk/NodeStore.cs` (384
lines, none reached), `GLib/IOChannel.cs` (286, none), `Gtk/SignalConnector.cs`
(178, none), `GLib/Spawn.cs` (168, none), and the remaining 548 uncovered lines
of `Cairo/Context.cs`.

---

## 18. Phase 12 — the files with no coverage at all

Phase 11 left a ranked queue of hand-written files, and the top of it was four
files nothing had ever executed: `NodeStore.cs` (384 lines), `IOChannel.cs`
(286), `SignalConnector.cs` (178) and `Spawn.cs` (168), plus the 548 lines of
`Cairo/Context.cs` the first pass did not reach and PangoSharp at 14.5%.

Eighty-one more tests (484 total; 483 passing on Linux, 481 on Windows with
three WebKit skips) took the hand-written figure from 43.6% to **50.3%**. Six
more defects came out of it.

**`Cairo.Context.FontMatrix` corrupted the stack.** `Cairo.Matrix` is a class,
so it already marshals as `cairo_matrix_t*`; declaring the parameter `out` made
it `cairo_matrix_t**`, and Cairo wrote 48 bytes of doubles through the address
of an 8-byte reference slot. Three getters had it, while the setters directly
beside them — same type, same file — were right. The same shape as the
caller-allocates defect from Phase 5.

This one carries a lesson about the test harness itself: **a crash is quieter
than a failure.** An `AccessViolationException` takes the test host down rather
than failing a test, so the run reports whatever finished first, prints
"Passed!", and stops. Before the fix the class reported nine passing tests of
twenty-one, and the whole suite 68 of 484. When a run's *total* is lower than it
should be, that is the thing to chase — the pass count will look fine.

**Three more symbols loaded from the wrong library.** An audit of every
`g_`-prefixed lookup against the library that exports it found `PtrArray` also
loading `g_object_unref` from GLib, and all five `g_spawn_*_utf8` entry points —
the ones `GLib.Spawn` uses on Windows only — loaded from GObject. Every spawn on
Windows called a null delegate while Linux, which takes the plain names beside
them, worked. A defect on exactly one platform is the signature of this mistake.

**`Pango.AttrIterator` could not return its attributes.** It built its
`GLib.SList` with no element type, so `DataMarshal` treated each item as a
GObject — which a `PangoAttribute` is not — returned null, and unboxing to
`IntPtr` threw. Reading attributes back off an iterator is the only way to get
them out of an `AttrList`, and it failed every time.

**`Pango.FontDescription` compared by handle.** Two descriptions built from the
same string are equal by every measure Pango offers, but were unequal here and
hashed differently, so one could not find the other in a dictionary. Now
overridden onto Pango's own `Equal` and `Hash`.

**`GLib.IOChannel.LineTerminator` never read the pointer.** The getter allocated
a byte buffer and decoded it without copying from what GLib returned, so it
reported that many NULs whatever the terminator had been set to.

**`GLib.Process` exposed no pid.** `SpawnAsync` exists to hand back a handle on
the child and returned an object you could learn nothing from. It has a `Pid`
property now.

Two things were found and deliberately *not* changed. `Gtk.SignalConnector`'s
entry points throw `NotSupportedException` because Gtk 4 replaced
`gtk_builder_connect_signals_full` with `GtkBuilderScope`, which this binding
does not implement; its `ConnectFunc` is therefore unreachable from anywhere and
stays only as the shape of what an implementation would need. And
`pango_parse_markup` is `hidden="1"` in the api.xml, so the markup parser has no
bound entry point — the test reaches it through `Gtk.Label` instead.

One observation was recorded here as unexplained: a run under coverage
instrumentation aborting at 315 of 484. It is explained now, and it was not the
`GLib.Opaque` suspicion recorded at the time. A test in `CairoTextAndPathTests`
leaked a `Cairo.Path`, and finalising one takes the process down. Because the
crash happens whenever the GC gets round to it, it landed on an unrelated test
and looked intermittent. Phase 13 has the detail.

Remaining, largest first: `GLib/Value.cs` (314 uncovered), `Gtk/TreeStore.cs`
(244), `GLib/Object.cs` (200), `GLib/Source.cs` (206), `Cairo/Surface.cs` (142).

---

## 19. Phase 13 — the tree wrappers, the main loop, Object and Value

Seventy-two more tests (556 total, 553 passing on Windows with three WebKit
skips) took the hand-written figure from 50.3% to **52.7%**. The percentage moved
least of the three sweeps and the findings were the worst, which is the argument
for reading the queue rather than the number.

**`TreeModelSort.AppendValues` recursed into itself forever.** It read
`return AppendValues ((Array) values);`, and there is no `AppendValues (Array)`
overload, so the cast bound straight back to the same method with the array
wrapped in a fresh `object[]`. A stack overflow cannot be caught, so this took
the process down. A sort model has no rows of its own, so it now says so and
names the call that works.

**Fourteen `SetValue` overloads threw `NotImplementedException`.** Refusing is
correct — `GtkTreeModel` has no set operation, because writing a row is the
store's job — but the exception read as "unfinished" rather than "ask the child
model". They now throw `NotSupportedException` naming
`ConvertIterToChildIter`, and the tests follow that advice to prove it works.

**`GLib.Source` was almost unreachable.** It has properties for priority, name,
recursion, its context and whether it has been destroyed — 272 lines — and
`Idle.Add`/`Timeout.Add` return an id that nothing could turn into a `Source`.
`MainContext.FindSourceById` does that now. `Source.Destroy` was missing for a
related reason: `g_source_destroy` was already loaded for `Free`, but no public
method reached it, so `IsDestroyed` had nothing to pair with.

**A number can bind to the raw-pointer constructor.** Since .NET 7 `IntPtr` is
`nint` and `int` converts to it implicitly, so `new ValueArray(2)`,
`new Date(2)` and `new DateTime(2)` all reach the pointer overload and
dereference address 2. The wrapper libraries are `LangVersion 9`, where the
conversion does not exist — so this reaches only consumers of the package, which
is the worst place for a hazard to live and the reason it survived.
`Opaque.CheckRaw` rejects addresses in the first page, and returns the pointer so
it can be used in a base-call argument: a guard in the constructor body throws
*after* `base(raw)`, and the finalizer frees address 2 regardless.

### The abort recorded in §18 as unexplained

It was not the `GLib.Opaque` over-referencing suspected at the time. A test in
`CairoTextAndPathTests` leaked a `Cairo.Path`; finalising one takes the process
down, and because that happens whenever the GC gets round to it, the crash landed
on an unrelated test and looked intermittent. The suite now runs 556 stably
across three plain and three instrumented runs.

Three crashes across Phases 12 and 13 shared a shape worth stating once: **a run
whose total is lower than it should be, under a "Passed!" line**. The pass count
looks fine because the tests that finished did pass. Check the total. And when a
crash appears to move between runs, suspect a finalizer before suspecting
nondeterminism.

Remaining, largest first: `Cairo/Context.cs` (398 uncovered), `GLib/Marshaller.cs`
(188), `Gtk/SignalConnector.cs` (178, unreachable by design — see §18),
`GdkSharp/Pixbuf.cs` (172), `GLib/HookList.cs` (118, none).

**Verified on Linux**: 602 of 603 passing, the one skip being the documented
`Gsk.RoundedRect`. Both WebKit tests run there, so the skip count is 1 rather
than the 3 Windows reports.

---

## 20. Phase 14 — the Gtk 4 application layer, and a converter defect

Forty-seven tests aimed at what a Gtk 4 application actually does — widget
measurement and layout, event controllers, CSS, and the `GAction`/`GMenu` and
`GListModel` stacks that replaced `GtkAction`, `GtkUIManager`,
`GtkTreeModelFilter` and `GtkTreeModelSort`. Those two stacks were introduced
wholesale by this port, so neither had any history of working.

Writing them found a defect in **`GirToGapi` itself**, which is the first one at
that level since Phase 2.

### `const char* const*` lost a star

The C-type normaliser had a rule to tidy const qualifiers:

    (const\s+)?(\w+)\*\s+const\*  ->  const $2*

`const char* const*` is a pointer to const pointers to const char — `char**`
with both levels qualified — and the rule dropped one of the two stars. Every
parameter and return value spelled that way came through as a single string, and
the call handed GTK the bytes of that string to read as an array of pointers.

**Sixty-seven of them, across seven assemblies.** `gtk_string_list_new` is the
one to remember: `new StringList(text)` compiled, read correctly, and was wrong.

It was partial, which is why it survived. The strv detection already accepted
`gchar**`, `char**` and `const char**`, covering most of the girs; the
`const T* const*` spelling accounts for 85 more, and only those broke. String
arrays therefore worked in enough places to look fine.

One character in the replacement, then `RegenerateApi`. The affected sites now
carry `type="const-char**" null_term_array="true"` and bind as `string[]` —
which is also how this became testable: `StringList(string[])` did not exist
before the fix.

### Also fixed

**A signal carrying an unmapped boxed type could not be handled.**
`GtkCssProvider`'s `parsing-error` carries a `GError`, whose GType resolves by
name to `GLib.Error` — a type this binding does not have — so `GLib.Value.ToBoxed`
threw from inside the signal marshaller, where nothing can catch it. The one
signal that tells an application its stylesheet is broken killed the process.
`ToBoxed` now returns the raw pointer, which is what the generated argument
(`ParsingErrorArgs.Error`, an `IntPtr`) expects anyway.

### Behaviour pinned rather than left as folklore

- `SimpleAction.StateChanged` is the `change-state` signal, not a notification.
  `GSimpleAction`'s default handler applies the state and connecting *replaces*
  it, so a handler that only reads the value leaves the action unchanged.
- `Widget.Activate` does not reach a `Clicked` handler, because Gtk 4 routes a
  press through a gesture. This is what once made the sample button-press theory
  pass while pressing nothing.
- `GMenuModel`'s items-changed carries signed counts; `GListModel`'s carries
  unsigned ones.

Hand-written coverage is 53.0%. Remaining, largest first: `Cairo/Context.cs`
(398 uncovered), `GLib/Marshaller.cs` (186), `GdkSharp/Pixbuf.cs` (172),
`GLib/HookList.cs` (118, none).

**Verified on Linux**: 602 of 603 passing. The api.xml regeneration was also
re-run there and produced files byte-identical to the ones committed from
Windows, which is the check this phase actually needed — a converter change that
rewrote seven api.xml files has to be reproducible across platforms, or the
checked-in api.xml becomes whichever machine ran `RegenerateApi` last.

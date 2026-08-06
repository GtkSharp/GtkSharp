# Plan — Upgrade GtkSharp to GTK 4.22.4

**Status:** V1–V5 passed; gates 1 and 2 passed; **Phases 1–4 complete**. Phase 5 in progress — 7 of 11 assemblies compile. See §14.
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

[GtkSharp.targets:4-5](../Source/Libs/GtkSharp/GtkSharp.targets#L4) downloads `https://github.com/GtkSharp/Dependencies/raw/master/gtk-3.24.24.zip` into `%LOCALAPPDATA%\Gtk\3.24.24`. **There is no `gtk-4.22.4.zip` in that repository.** This blocks Windows builds of Samples and every consumer of the `GtkSharp` package.

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
-    <GtkUrl Condition=" '$(GtkUrl)' == '' ">https://github.com/GtkSharp/Dependencies/raw/master/gtk-3.24.24.zip</GtkUrl>
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
| 7 | Symbol resolution | Run Samples on Linux with GTK 4.22.4 | No `DllNotFoundException` / `EntryPointNotFoundException`. **This is where a wrong V3 answer surfaces.** |
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
| **V1–V5** | Blocking pre-flight verifications | ✅ **complete** — 2026-08-05 |
| **1** | Branch, versioning, scaffolding | ✅ **complete** — 2026-08-05 |
| **2** | `GirToGapi` converter | ✅ **complete** — 2026-08-05, gates 1 and 2 passed |
| **3** | Assembly graph, native library map | ✅ **complete** — 2026-08-05 |
| **4** | api.xml regeneration + metadata triage | ✅ **complete** — 2026-08-06, all nine assemblies at zero unmatched rules |
| **5** | Hand-written layer port | 🔶 **in progress** — 7 of 11 assemblies compile |
| **6** | Samples port (37 sections) | ⬜ not started |
| **7** | Templates and workload | ⬜ not started |
| **8** | Native runtime, CI | ⬜ not started |

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

### Phase 5 — in progress

| Assembly | Build state |
|:---------|:------------|
| `GLibSharp`, `CairoSharp` | ✅ clean (hand-written, untouched) |
| `GrapheneSharp` | ✅ **clean** |
| `GioSharp` | ✅ **clean** |
| `PangoSharp` | ✅ **clean** |
| `GdkSharp` | ✅ **clean** — 28 hand-written files deleted |
| `GskSharp` | ✅ **clean** — `RenderNode` hierarchy deferred, see below |
| `GtkSharp` | 🔶 in progress — generated side compiles; hand-written layer is next |
| `AdwaitaSharp`, `GtkSourceSharp`, `WebkitGtkSharp` | ⬜ blocked behind `GtkSharp` |

**GtkSharp status.** The generated side is down to a handful of issues; the
remaining 185 errors are overwhelmingly the 117-file hand-written layer, which
had never been compiled against Gtk 4 until now. The distribution matches the
§5.1 delete list almost exactly — `NativeDialog`, `Clipboard`, `ColorSelection`,
`Accel`, `StatusIcon`, `Menu`, `Container.Forall`, `SelectionData` account for
most of it, and all of them bind types Gtk 4 removed. `FileChooserNative`,
`TextTag` and `MediaStream` are the generated-side remainder.

**GdkSharp is where the §5.1 deletions began.** Gone: `Window` (→ `Surface`),
`WindowAttr`, `Screen`, `Color`, `Property`, `Keymap`, `Atom`, `Selection`,
`TextProperty`, `Pixdata`, `PixbufFrame`, the whole `Event*` struct family
(17 files), plus `Device.cs` and `Display.cs`, which held nothing but removed
API. `Global.cs` shrank to a single member.

### Open: GLib fundamental types are not bound

Gtk 4 uses GLib *fundamental* types — `glib:fundamental="1"`, GTypeInstance with
their own ref/unref rather than GObject descendants — for two whole hierarchies:
`GdkEvent` and its ~20 event subclasses, and `GskRenderNode` and its 36 node
subclasses. `ObjectGen` has no notion of them: it emits `Handle`,
`CreateNativeObject`, a `base(IntPtr)` chain-up and
`GLib.Object.GetObject(raw) as T`, all of which assume GObject.

Current state:

- **`GdkEvent`** is carried by a small hand-written base supplying that surface
  over a plain handle. Constructing an event from managed code throws, since GDK
  delivers events to controllers and never accepts them.
- **`GskRenderNode` and its 36 subclasses are hidden.** The same trick does not
  work there because the generated code casts through `GLib.Object.GetObject`,
  which will not compile unless `RenderNode` derives from `GLib.Object` — and
  making it do so would put `g_object_ref`/`unref` on handles whose lifetime
  belongs to `gsk_render_node_ref`/`unref`. A binding that mismanages refcounts
  is worse than one that is missing. Cost: three GtkSharp members that reference
  `GskRenderNode` go with them; `Gsk.Renderer`, `Transform` and `RoundedRect` are
  unaffected.

Doing this properly means teaching `ObjectGen` about fundamental types: per-type
`GetObject` factories and ref/unref hooks. That is the largest single piece of
work left in Phase 5 after the Gtk layer itself, and it blocks custom widget
drawing via `GtkSnapshot`, which §6.3 needs for the DrawingArea samples.

Still untouched: the deletions and rewrites in §5.1 and §5.2 — `Container`,
`Menu`, `Application.Run`, `Clipboard`, `Dialog.Run`, the TreeView stack and the
rest of the hand-written Gtk layer. That work has not started, and it is the bulk
of Phase 5.

Seven converter and codegen fixes came out of compiling the output rather than
reading XML:

| Fix | Why |
|:----|:----|
| Skip namespace-level `<function>` with `moved-to` | GIR lists `graphene_box_empty` both on the record and again as a namespace alias; emitting both produced a spurious `<class name="Box">` colliding with the boxed type. 411 such aliases across the vendored set. |
| Fixed-size arrays as `type="X" array_len="N"` | Not `type="X*"`. `FieldBase` keys `IsArray` off `array_len`, so the pointer spelling made codegen reference a field it never declared. |
| `scope` only with a usable `closure` index | `g_bus_own_name` takes three callbacks against one `user_data`, and GIR annotates only the last; `MethodBody`'s i+1/i+2 fallback then lands on the next callback. |
| No `pass_as="out"` for caller-allocated array buffers | `g_input_stream_read`'s `void *buffer` is storage the caller supplies. Marking it out produced methods that never assign it. Struct out-parameters unaffected. |
| Infer `throws` from a trailing `GError**` | GIR sets `throws` on methods but not callbacks. gapi keys its whole GError treatment off it, so the parameter stayed visible and collided with the `error` local codegen declares. |
| `StructField` declaration vs `EqualityName` | Private array fields were declared StudlyCaps but referenced lower-cased by the generated `Equals`. Gtk 3 never had a private struct-level array; `graphene_quad_t` does. |
| `Ctor` skips hidden parameters | It also indexed `Parameters` by the filtered names index, which only lined up while every parameter contributed a name. |

**A recurring class of problem, worth expecting in Gdk and Gtk:** gapi2xml.pl
emitted bogus type names — `variant` where the type is `GVariant*` — that
`SymbolTable` never knew, so codegen dropped those members with a warning and
their name collisions never surfaced. Gtk 3 built partly *because* of that;
`GLib.IAction.State` was simply absent from the binding. GirToGapi emits the real
type, so they resolve now and the collisions are real. The fix is the one the
existing metadata already models: hide the property whose accessor comes from a
vfunc, or rename the function that emits a same-named signal.

### Phases 6–8 — not started

Samples (37 sections), templates and workload, native runtime and CI. The V4
decision (gvsbuild `2026.6.0`) and the corrected `GtkSharp.targets` paths in §8.1
are settled but not yet applied.

# GIR → gapi coverage

**Status:** V1–V5 complete. **Go** for Phase 2.
**Date:** 2026-08-05
**Evidence base:** the vendored `.gir` set in [../Source/Gir/](../Source/Gir/) (Debian forky,
gtk4 `4.22.4+ds-1`), plus `Gtk-3.0.gir` from [gtk-rs/gir-files](https://github.com/gtk-rs/gir-files)
used only as the GTK 3 control for the name-derivation experiment in §3.

This file is the deliverable of the blocking pre-flight verifications in
[gtk4-upgrade-plan.md](gtk4-upgrade-plan.md) §1, and remains the triage record for gate 1 and the
Phase 4 metadata work.

---

## Summary

| # | Verification | Result |
|:--|:-------------|:-------|
| V1 | Does GIR carry everything the gapi schema needs? | **Yes**, with two named gaps, neither blocking — see §1. R1 is retired. |
| V2 | Where do the `.gir` files come from, are they vendored? | **Resolved** — Debian forky, vendored, sha256-pinned. See [../Source/Gir/README.md](../Source/Gir/README.md). |
| V3 | Are GDK/GSK inside `libgtk-4`? | **Confirmed from the gir itself.** See §2. |
| V4 | Windows runtime acquisition | **Resolved** — gvsbuild `2026.6.0`. Bundle inspected; the plan's DLL-name table in §3.3 is **wrong** and is corrected in §4. |
| V5 | Does `GapiCodegen` need changes? | **Barely.** One dead symbol, one plan error. See §5. |

---

## 1. Construct coverage (V1)

Counts are `grep -c` over the vendored `Source/Gir/Gtk-4.0.gir` (gtk4 4.22.4) unless stated.

| Gapi construct | Gapi.xsd | GIR source | Verdict |
|:---------------|:---------|:-----------|:--------|
| `<class_struct>` container | [Gapi.xsd:181](../Source/Libs/Shared/Gapi.xsd#L181) | `<record glib:is-gtype-struct-for=>` — **207** present | ✅ present |
| `<method vm=>` | [Gapi.xsd:186](../Source/Libs/Shared/Gapi.xsd#L186) | class-struct `<field>` holding a `<callback>`; **323** `<virtual-method>` corroborate | ✅ present |
| `<method signal_vm=>` | [Gapi.xsd:186](../Source/Libs/Shared/Gapi.xsd#L186) | **not carried directly** — derivable, 99.7 % accurate, see §3 | ⚠️ heuristic, validated |
| `<method padding="true">` | [Gapi.xsd:186](../Source/Libs/Shared/Gapi.xsd#L186) | `_gtk_reserved1..8` fields (14–15 classes) **and** `<field name="padding" private="1">` array (35 classes) | ⚠️ two shapes, see §1.1 |
| `<field bits="N">` | [Gapi.xsd:224](../Source/Libs/Shared/Gapi.xsd#L224) | `<field bits="N">` — 0 in Gtk4 (those structs are gone), **21 in Pango-1.0**, **43 in GLib-2.0** | ✅ present |
| `<childprop>` | [Gapi.xsd:161](../Source/Libs/Shared/Gapi.xsd#L161) | **none** — and none in `Gtk-3.0.gir` either, though `GtkSharp-api.xml` has 64 | ✅ moot, see §1.2 |
| `<return-type owned= elements_owned=>` | [Gapi.xsd:280](../Source/Libs/Shared/Gapi.xsd#L280) | `transfer-ownership=` — 15 339 occurrences | ✅ direct |
| `<return-type null_term_array=>` | [Gapi.xsd:280](../Source/Libs/Shared/Gapi.xsd#L280) | `<array zero-terminated=>`, **absent ⇒ 1** per the GIR spec | ⚠️ see §1.3 |
| `<parameter pass_as="out\|ref">` | [Gapi.xsd:271](../Source/Libs/Shared/Gapi.xsd#L271) | `direction="out"` ×468, `direction="inout"` ×8, `caller-allocates="1"` ×110 | ✅ direct |
| `<parameter scope=>` | [Gapi.xsd:266](../Source/Libs/Shared/Gapi.xsd#L266) | `scope="call"` ×20, `"async"` ×71, **`"notified"` ×49** — GIR's spelling, must be mapped to gapi's `notify` | ✅ direct, rename needed |
| `<static-string>` | [Gapi.xsd:249](../Source/Libs/Shared/Gapi.xsd#L249) | `<constant>` ×98 | ✅ direct |
| `<symbol …>` | [Gapi.xsd:53](../Source/Libs/Shared/Gapi.xsd#L53) | none — hand-authored | ✅ expected; `GtkSharp-symbols.xml` stays hand-written |
| `<alias>` | [Gapi.xsd:66](../Source/Libs/Shared/Gapi.xsd#L66) | `<alias>` ×1 (`GtkAllocation`) | ✅ direct |
| method `deprecated` | [Gapi.xsd:213](../Source/Libs/Shared/Gapi.xsd#L213) | `deprecated="1" deprecated-version=` ×1 086 | ✅ direct — and confirms R3's scale |

### 1.1 Padding comes in two shapes

`gapi2xml.pl` marked a class-struct slot as padding by name
([gapi2xml.pl:573](../Source/OldStuff/parser/gapi2xml.pl#L573)): `reserved_?N`, `padding_?N`,
`recent_?N`. GTK 4 uses **both** of:

- `_gtk_reserved1` … `_gtk_reserved8` — individual `gpointer` slots, on 14–15 class structs. The
  existing name rule nearly covers these; it needs `_gtk_reservedN` added to the pattern.
- `<field name="padding" readable="0" private="1">` holding a **fixed-size array** — on 35 class
  structs, including `GtkWidgetClass`. This is one field standing for N pointers.

**Converter requirement:** an array-typed padding field must be expanded into `fixed-size`
separate `<method padding="true">` slots, not emitted as one. Getting this wrong silently shifts
every subsequent vfunc slot and corrupts the ABI — it will not fail at build, only at first
virtual-method dispatch. Cover it explicitly when writing `ObjectEmitter.cs`.

### 1.2 `<childprop>` was never a GIR construct

`Gtk-3.0.gir` contains **zero** child-property elements, yet the checked-in
`GtkSharp-api.xml` has 64. `gapi2xml.pl` recovered them by parsing
`gtk_container_class_install_child_property` calls in the C sources. GIR never carried them.

This is irrelevant for GTK 4 — child properties were removed with `GtkContainer` — so the
converter simply does not emit `<childprop>`. But it is worth recording as evidence for a general
point: **`gapi2xml.pl` read C implementation files, GIR is generated from annotations.** Where the
old parser recovered something from a `.c` body, GIR may not have it. §3 is the other instance.

### 1.3 `null_term_array` will over-produce, not under-produce

`GtkSharp-api.xml` carries `null_term_array` just **twice**, because the old parser rarely
detected it. In `Gtk-4.0.gir`, 47 `<array>` elements omit `zero-terminated` entirely — which the
GIR spec defines as *true*. A faithful converter will therefore mark far more arrays as
null-terminated than the current api.xml does.

This is a **correctness improvement**, but it is also a large, expected diff class at gate 1. Do
not treat it as a converter bug. It does change generated marshalling for those methods, so spot-
check a handful against the C headers before accepting the whole class.

---

## 2. V3 — GDK and GSK are inside `libgtk-4` (CONFIRMED)

Read directly off the `<namespace>` elements of the vendored 4.22.4 gir — no install needed:

| gir | `shared-library=` |
|:----|:------------------|
| `Gtk-4.0.gir` | `libgtk-4.so.1` |
| `Gdk-4.0.gir` | **`libgtk-4.so.1`** |
| `Gsk-4.0.gir` | **`libgtk-4.so.1`** |
| `Graphene-1.0.gir` | `libgraphene-1.0.so.0` |
| `GtkSource-5.gir` | `libgtksourceview-5.so.0` |
| `Adw-1.gir` | `libadwaita-1.so.0` |
| `WebKit-6.0.gir` | `libwebkitgtk-6.0.so.4,libjavascriptcoregtk-6.0.so.1` |

So `Library.Gdk` and a new `Library.Gsk` must both resolve to the GTK 4 library, exactly as the
plan's §3.2/§3.3 assume. R9 stands as a runtime risk but the underlying fact is now settled.

### 2.1 How the redirect is actually wired (corrects plan §3.4)

The plan says `GirToGapi` should emit `library="gtk-4"` and asks how `GapiCodegen` maps that
string to the `Library` enum. **There is no mapping.** [GenBase.cs:65-69](../Source/Tools/GapiCodegen/GenBase.cs#L65)
reads `/api/namespace/@library` and [Method.cs:201](../Source/Tools/GapiCodegen/Method.cs#L201)
pastes it **verbatim** into the generated call:

```csharp
FuncLoader.GetProcAddress(GLibrary.Load(<library attribute, literally>), "gtk_widget_show")
```

The attribute value is a C# expression. Today's api.xml files carry `library="Library.Gtk"`,
`library="Library.Gdk"` and so on — and where the raw value is wrong,
**the metadata already fixes it**: [GtkSourceSharp.metadata:67](../Source/Libs/GtkSourceSharp/GtkSourceSharp.metadata#L67)
rewrites the raw `libgtksourceview-4.so` to `Library.GtkSource`.

**Consequence:** the Gdk/Gsk → `Library.Gtk` redirect needs no converter feature at all. Add one
line to each of `GdkSharp.metadata` and a new `GskSharp.metadata`:

```xml
<attr path="/api/namespace" name="library">Library.Gtk</attr>
```

`GirToGapi` should emit `library` as the raw `shared-library` value and let metadata normalise it,
matching how `GtkSourceSharp` already behaves.

---

## 3. `signal_vm` is derivable from GIR — validated at 328/329

**This was risk R1, and it is the one finding that decided go/no-go.**

`gapi2xml.pl` does *not* get `signal_vm` from headers. It parses the C `*_class_init` body
([gapi2xml.pl:404](../Source/OldStuff/parser/gapi2xml.pl#L404) → `parseInitFunc`) looking for
`g_signal_new(..., G_STRUCT_OFFSET(XxxClass, field), ...)`, and marks that field
`signal_vm` ([gapi2xml.pl:548](../Source/OldStuff/parser/gapi2xml.pl#L548)). GIR carries
`<glib:signal>` elements but never links a signal to its class-struct slot.

**Proposed rule:** a class-struct callback field is `signal_vm` if its name matches a
`<glib:signal name>` on the same class with `-` → `_`; otherwise plain `vm`.

**Experiment** (`Gtk-3.0.gir` vs the `signal_vm`/`vm` attributes in the checked-in
`GtkSharp-api.xml`, which came from `gapi2xml.pl`):

```
classes compared:            202
predicted signal_vm matched: 328/329
false positives:             1   (GtkCellArea::apply_attributes)
false negatives:             0
```

**Zero false negatives** is the important half: the rule never mistakes a signal closure for a
plain vfunc, so `DefaultSignalHandler` emission is never silently lost. The single false positive
is a signal registered without a class-closure offset; it is correctable with one metadata rule or
a short exception list in the converter.

**R1 is retired.** No hand-maintained padding/vm table is needed, and the fallback plan in the
plan's §1/V1 escalation note does not have to be taken.

Reproduce with `scratchpad/v1_signalvm.py` (see §7).

---

## 4. V4 — Windows runtime (RESOLVED), and a correction to plan §3.3

**Decision: point `GtkUrl` at the gvsbuild release asset** (plan §1/V4 option 2). No push access
to `GtkSharp/Dependencies` is needed — that repo still contains only `gtk-3.24.20.zip`,
`gtk-3.24.24.zip`, `gtk-3.24.zip`.

Bundle: `https://github.com/wingtk/gvsbuild/releases/download/2026.6.0/GTK4_Gvsbuild_2026.6.0_x64.zip`
— 299.5 MB, 6 741 entries, containing **GTK4 4.22.4**, **libadwaita 1.9.1** and **GtkSourceView 5**.

Enumerated without downloading it, by HTTP-range-reading the zip central directory
(`scratchpad/zip_listing.py`).

### 4.1 The plan's DLL name table is wrong

Plan §3.3 guesses MSYS2-style `lib`-prefixed names. gvsbuild uses MSVC-style names **without** the
`lib` prefix. Since `_libraryDefinitions[…][0]` is the Windows fast path, this matters.

| `Library` | plan §3.3 guessed | **actual in bundle** |
|:----------|:------------------|:---------------------|
| `GLib` | `libglib-2.0-0.dll` | `glib-2.0-0.dll` |
| `GObject` | `libgobject-2.0-0.dll` | `gobject-2.0-0.dll` |
| `Cairo` | `libcairo-2.dll` | `cairo-2.dll` |
| `Gio` | `libgio-2.0-0.dll` | `gio-2.0-0.dll` |
| `Pango` | `libpango-1.0-0.dll` | `pango-1.0-0.dll` |
| `PangoCairo` | `libpangocairo-1.0-0.dll` | `pangocairo-1.0-0.dll` |
| `Graphene` | `libgraphene-1.0-0.dll` | `graphene-1.0-0.dll` |
| `GdkPixbuf` | `libgdk_pixbuf-2.0-0.dll` | `gdk_pixbuf-2.0-0.dll` |
| `Gtk` (also Gdk, Gsk) | `libgtk-4-1.dll` | **`gtk-4-1.dll`** |
| `GtkSource` | `libgtksourceview-5-0.dll` | `gtksourceview-5-0.dll` |
| `Adwaita` | `libadwaita-1-0.dll` | `adwaita-1-0.dll` |
| `Webkit` | `libwebkitgtk-6.0-4.dll` | **absent** |

Keep the `lib`-prefixed names as later fallback candidates — the multi-candidate loop at
[GLibrary.cs:83-90](../Source/Libs/Shared/GLibrary.cs#L83) makes that free, and it keeps MSYS2
installs working — but index 0 must be the gvsbuild name.

### 4.2 The bundle is not flat — `GtkSharp.targets` needs a path change

Top-level layout is `bin/ lib/ include/ share/ etc/ python/ wheels/`; **every DLL is under
`bin/`**. The current GTK 3 zip is flat, so
[GLibrary.cs:66](../Source/Libs/Shared/GLibrary.cs#L66) does
`SetDllDirectory(%LOCALAPPDATA%\Gtk\3.24.24)`. For GTK 4 that must become
`…\Gtk\4.22.4\bin`, and the `InstallGtk` sentinel in
[GtkSharp.targets](../Source/Libs/GtkSharp/GtkSharp.targets) must be `$(GtkDir)/bin/gtk-4-1.dll`.

### 4.3 R5 confirmed: no WebKit on Windows

The bundle contains no webkit DLL, and gvsbuild has no `webkit` project (its `projects/` directory
lists `gtk.py`, `gtksourceview.py`, `libadwaita.py`, `graphene.py`, but nothing for webkit).
`WebkitGtkSharp` is Linux-only in practice; guard `WebviewSection.cs` accordingly.

### 4.4 gvsbuild also ships gir

The bundle carries 39 `share/gir-1.0/*.gir`, including Windows-flavoured `GdkWin32-4.0.gir`. Not
used for vendoring (V2 chose Debian, which also covers webkit), but it is a useful cross-check if
a Windows-only marshalling difference is ever suspected.

---

## 5. V5 — `GapiCodegen` audit

| Check | Finding |
|:------|:--------|
| `Parser.cs` version-agnostic? | **Yes.** Only version logic is `curr_parser_version = 3` at [Parser.cs:32](../Source/Tools/GapiCodegen/Parser.cs#L32), refusing api.xml newer than itself. |
| Hardcoded GTK3 type names in `SymbolTable.cs` | **One**, [SymbolTable.cs:49](../Source/Tools/GapiCodegen/SymbolTable.cs#L49): `AddType (new SimpleGen ("AtkFunction", "IntPtr", …)) // function definition used for padding`. GTK 4 pads with `gpointer`, and AtkSharp is being deleted, so this becomes dead. Harmless to leave; cleaner to remove. |
| `--abi-cs-usings` handling | **Plan §1/V5 is wrong about the location.** `Options.cs` is vendored NDesk.Options and contains no GtkSharp options. The real definitions are in [CodeGenerator.cs:62-81](../Source/Tools/GapiCodegen/CodeGenerator.cs#L62) (`outdir=`, `abi-cs-usings=`, `schema=`). Nothing there is GTK-version-specific; dropping `Atk` and adding `Gsk`/`Graphene` is a `Settings.cake` edit only. |
| The `Constant.cs` "gir" mention | **Not a GIR affordance.** [Constant.cs:48](../Source/Tools/GapiCodegen/Constant.cs#L48) is a comment explaining that gir types all integer constants as `gint`, so oversized values are widened to `long`. Useful to know — it means numeric `<constant>` *is* supported, so plan §2.2's "skip numeric constants" row is wrong; emit them. |

**Verdict: `GapiCodegen` needs no changes for GTK 4.** One dead symbol may be removed as cleanup.

### 5.1 `parser_version` — emit 3, not 2

Plan §2.2 says `ApiWriter.cs` writes `<api parser_version="2">`. The checked-in files are six at
`parser_version="2"` and one at `"3"`, and the tool's current version is **3**. New output should
declare `3`.

---

## 6. Gate 1 — Gtk 3 round-trip diff (Phase 2, not yet run)

Still the most valuable test available, and §3 above is a partial down payment on it. When
running it, use a `Gtk-3.0.gir` from **Debian `libgtk-3-dev`** matching the GTK 3 version that
produced the checked-in `GtkSharp-api.xml`, not the gtk-rs copy — the gtk-rs files are derived
from the GNOME *nightly* SDK and post-processed by that repo's `fix.sh`/`reformat.sh`, which is
fine for the structural experiment in §3 but not for a byte-level diff.

Known diff classes to expect before starting, so they are not mistaken for converter bugs:

| Difference class | Cause | Action |
|:-----------------|:------|:-------|
| Many new `null_term_array="true"` | §1.3 — GIR default is true, old parser rarely detected it | Accept after spot-check |
| Missing `<childprop>` | §1.2 — never in GIR | Accept (GTK 4 removed them) |
| `GtkCellArea::apply_attributes` as `signal_vm` not `vm` | §3 — single heuristic false positive | Exception list or metadata |
| Padding slot count | §1.1 — array-vs-scalar padding fields | **Must match exactly**; any difference is a converter bug |

| Other difference | Count | Explanation | Action |
|:-----------------|:------|:------------|:-------|
| *to be filled at gate 1* | | | |

---

## 7. Reproducing the verification

All three scripts are checked in, so every claim above is re-runnable:

| Script | What it does |
|:-------|:-------------|
| [`Source/Gir/fetch-gir.py`](../Source/Gir/fetch-gir.py) | Re-fetches the vendored set: downloads each pinned `.deb`, verifies sha256, unpacks the `ar` container and `data.tar.xz` in pure Python (no `dpkg`, `ar`, or Linux host), extracts the `.gir`. Verified to reproduce the checked-in files byte-for-byte. |
| [`Source/Tools/GirToGapi/scripts/signal-vm-parity.py`](../Source/Tools/GirToGapi/scripts/signal-vm-parity.py) | The §3 experiment: predicts `signal_vm` from a gir and scores it against an existing api.xml. **Re-run this at gate 1.** Usage: `python signal-vm-parity.py <gir> <api.xml>`. |
| [`Source/Tools/GirToGapi/scripts/inspect-remote-zip.py`](../Source/Tools/GirToGapi/scripts/inspect-remote-zip.py) | Enumerates a remote zip via HTTP `Range` reads of its central directory — inspects the 300 MB gvsbuild bundle by fetching ~4 MB. Re-run when the bundle version moves, to re-check §4.1 and §4.2. |

To pin a newer upstream, read the `Version`/`Filename`/`SHA256` fields out of
`https://deb.debian.org/debian/dists/forky/main/binary-amd64/Packages.xz`, update `PACKAGES` in
`fetch-gir.py`, re-run it, and update the provenance table in `Source/Gir/README.md` in the same
commit.

---

## 8. Metadata triage log (Phase 4)

One row per `Warning: … matched no nodes` from a non-strict `GapiFixup` run, bucketed as
**obsolete** (delete the rule), **moved** (rewrite the XPath), or **converter bug** (fix
`GirToGapi`, not the metadata). An assembly gets `--strict` only once its section here is empty of
unresolved rows.

| Assembly | Rule | Bucket | Resolution |
|:---------|:-----|:-------|:-----------|
| *not started* | | | |

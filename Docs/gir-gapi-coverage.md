# GIR → gapi coverage

**Status:** V1–V5 complete; gates 1 and 2 passed; Phase 2 complete. **Go** for Phase 3.
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

## 6. Gate 1 — Gtk 3 round-trip diff ✅ PASSED

Run with `Source/Tools/GirToGapi/scripts/gate1-diff.py`: convert a **GTK 3** gir with `GirToGapi`
and compare against the checked-in `Source/Libs/GtkSharp/GtkSharp-api.xml`, which is known
`gapi2xml.pl` output. A byte diff is meaningless here — element order, attribute order and
whitespace all differ by construction — so the comparison is on what actually matters: which
types exist, which members they carry, and what everything is called.

```
types:  converted 758   reference 770   in both 479
kind agreement on shared types:          478/479   (99.79%)
members on shared types:                 6693 matched, 252 missing, 534 added
coverage of reference members:           96.37%
name agreement on shared C identifiers:  4719/4793 (98.46%)
```

The input was the gtk-rs `Gtk-3.0.gir`, which is GNOME-nightly-derived and so a somewhat different
GTK 3 revision from the one that produced the reference file. Part of the type delta is that
version skew rather than converter behaviour.

### Every difference class, explained

| Class | Count | Cause | Verdict |
|:------|------:|:------|:--------|
| Reference-only `struct` | 258 | Private C structs (`CacheEntry`, `Child`, `ClipboardRequest`, `CompareInfo`, …) that `gapi2xml.pl` scraped out of GTK's *internal* headers. Not introspectable, so GIR never mentions them. | **Improvement** — they were noise |
| Reference-only `object` | 16 | Same cause: GTK-internal classes (`GtkActionHelper`, `GtkActionMuxer`, `GtkMenuSectionBox`, …) | **Improvement** |
| Converted-only `object` | 62 | The a11y hierarchy (`GtkButtonAccessible`, `GtkCellAccessible`, …), which GIR exposes and the header scan missed | Harmless; moot in GTK 4 |
| Converted-only `struct` | 209 | Mostly the class structs of the above, plus records GIR exposes | Harmless |
| Name disagreements | 74 | **All** in namespace-level function grouping — `gtk_drag_begin` is `Drag.Begin` in the reference and `Global.DragBegin` here. See §6a.1. | Known deviation |
| `GtkStock` object→alias | 1 | GIR types it as an alias; gapi2xml made it an object | Cosmetic; GTK 4 deleted it |
| New `null_term_array="true"` | — | §1.3: GIR's default is true and the old parser rarely detected it | Contained — restricted to string arrays, see §6a.1 |
| Missing `<childprop>` | 64 | §1.2: never in GIR | Accept; GTK 4 removed child properties |
| Padding slot count | **0 differences** | §1.1 | **Exact match** — the ABI-critical case |

Member coverage of 96.37%, with the shortfall concentrated in non-introspectable private types,
and name agreement of 98.46%, with every disagreement inside one known subsystem, is a pass.
**R2 is retired.**

---

## 6a. Phase 2 results — the converter

`Source/Tools/GirToGapi/` converts all ten assemblies' `.gir` into gapi api.xml.

| Assembly | api.xml | Metadata rules | Unmatched | Survives | Plan predicted |
|:---------|--------:|---------------:|----------:|---------:|:---------------|
| `GLibSharp` | 599 KB | — | — | — | hand-written |
| `GioSharp` | 874 KB | 189 | 37 | **80 %** | ~90 % |
| `GrapheneSharp` | 100 KB | new | — | — | new |
| `PangoSharp` | 162 KB | 111 | 25 | **77 %** | ~90 % |
| `GdkSharp` | 180 KB | 191 | 169 | **11 %** | ~25 % |
| `GskSharp` | 98 KB | new | — | — | new |
| `GtkSharp` | 1350 KB | 1112 | 732 | **34 %** | ~30 % |
| `AdwaitaSharp` | 383 KB | new | — | — | new |
| `GtkSourceSharp` | 202 KB | 71 | 23 | **67 %** | ~20 % |
| `WebkitGtkSharp` | 252 KB | 4 | 1 | **75 %** | ~0 % |

`GdkSharp` is the one materially worse than predicted, which is no surprise: `GdkWindow` →
`GdkSurface` plus the event restructure invalidates most of that file. `GtkSourceSharp` and
`WebkitGtkSharp` came out far better than predicted.

**End-to-end proof** on `GioSharp`, the plan's designated smoke assembly: convert → `GapiFixup`
with the existing metadata → `GapiCodegen` yields **467 files / 67 941 lines** of C# with no
unhandled exception, against the GTK 3 baseline's 398 files / 50 403 lines. The generated code
contains correct `class_abi.GetFieldOffset("startup")` vfunc wiring and a
`[GLib.DefaultSignalHandler]` override — the `signal_vm` machinery from §3 working on real GTK 4
data, not only in the parity experiment.

`scripts/check-api.py` enforces the structural invariant GapiCodegen depends on: every
`<method vm=>` resolves to a `<virtual_method>`, every `<method signal_vm=>` to a
`<signal field_name=>`, the parent field comes first, and no slot name repeats. **2197 slots
across all ten files, all holding.** A violation is a NullReferenceException inside the generator
with no clue which type caused it, so this runs before codegen rather than after.

### 6a.1 Deliberate deviations from `gapi2xml.pl`

| Deviation | Why |
|:----------|:----|
| **Owner attribution follows GIR** rather than the C-prefix heuristic | `gtk_file_chooser_native_set_accept_label` becomes `FileChooserNative.SetAcceptLabel` here and was `FileChooser.NativeSetAcceptLabel` before. GIR is right. Affects ~3.5 % of members. |
| **Global functions group differently** | The old rule silently *dropped* functions whose cname had no third token — `gtk_init` had no home at all. Everything reaches `Global` here instead of vanishing. Accounts for all 74 gate-1 name disagreements. |
| **`element_type` is never emitted** | GapiCodegen handles it only on `GList`/`GSList`/`GPtrArray` returns and *throws* on anything else ([ReturnValue.cs:147-155](../Source/Tools/GapiCodegen/ReturnValue.cs#L147)). Emitting it for string arrays killed the generator mid-run. Left to metadata, as before — twelve hand-added occurrences in the whole tree. |
| **`array="true"` is never emitted on parameters** | Same reason: with no metadata-supplied count parameter it makes `Parameter.cs` throw. |
| **`null_term_array` restricted to string arrays** | `GLib.Marshaller.NullTermPtrToStringArray` is the only unaided null-terminated path. This also contains the §1.3 over-production. |
| **Namespace-level `<constant>` skipped** | gapi has no namespace-level slot for them — `Gapi.xsd` puts `static-string` inside `<object>` only. 129 in Gio, 98 in Gtk, 2459 in Gdk; the Gdk ones are keysyms, which have always been generated separately. Logged, never silent. |
| **`<function>` children of enums skipped** | `enumType` accepts only `<member>`. These are `*_error_quark`; GLibSharp binds error domains by hand. Logged. |

### 6a.2 One schema change

`Source/Libs/Shared/Gapi.xsd` gained a `throws` attribute on its three inline `parameters` complex
types. It is read by [Parameters.cs:58](../Source/Tools/GapiCodegen/Parameters.cs#L58) and already
appears in the checked-in api.xml files — the schema was simply behind. Without it every
GError-taking method is a validation error, and GIR marks far more of them than gapi2xml.pl did.

With that change the converted `GioSharp-api.xml` validates with **zero** schema errors beyond the
two the GTK 3 baseline also produces (a hand-written `<warning>` note at
`Source/Libs/GLibSharp/GLibSharp-api.xml:27`, unrelated to this work). **Gate 2 passes.**

### 6a.3 `GapiFixup --strict`

Added per plan §2.6: counts unmatched rules, exits non-zero. Verified both ways — exit 0 on the
GTK 3 file its metadata was written for, exit 1 with `gapi-fixup: 37 unmatched rule(s)` on the
GTK 4 one. Wired to a per-assembly `StrictMetadata` switch on `GAssembly`, **off everywhere**
until Phase 4 triages that assembly's rules.

---

## 7. Reproducing the verification

All three scripts are checked in, so every claim above is re-runnable:

| Script | What it does |
|:-------|:-------------|
| [`Source/Gir/fetch-gir.py`](../Source/Gir/fetch-gir.py) | Re-fetches the vendored set: downloads each pinned `.deb`, verifies sha256, unpacks the `ar` container and `data.tar.xz` in pure Python (no `dpkg`, `ar`, or Linux host), extracts the `.gir`. Verified to reproduce the checked-in files byte-for-byte. |
| [`Source/Tools/GirToGapi/scripts/signal-vm-parity.py`](../Source/Tools/GirToGapi/scripts/signal-vm-parity.py) | The §3 experiment: predicts `signal_vm` from a gir and scores it against an existing api.xml. **Re-run this at gate 1.** Usage: `python signal-vm-parity.py <gir> <api.xml>`. |
| [`Source/Tools/GirToGapi/scripts/inspect-remote-zip.py`](../Source/Tools/GirToGapi/scripts/inspect-remote-zip.py) | Enumerates a remote zip via HTTP `Range` reads of its central directory — inspects the 300 MB gvsbuild bundle by fetching ~4 MB. Re-run when the bundle version moves, to re-check §4.1 and §4.2. |
| [`Source/Tools/GirToGapi/scripts/name-parity.py`](../Source/Tools/GirToGapi/scripts/name-parity.py) | The R2 experiment: scores `StudlyCaps(gir @name)` against an existing api.xml's `@name`, per member kind. |
| [`Source/Tools/GirToGapi/scripts/gate1-diff.py`](../Source/Tools/GirToGapi/scripts/gate1-diff.py) | Gate 1: type, member and name agreement between a converted api.xml and a reference one. |
| [`Source/Tools/GirToGapi/scripts/check-api.py`](../Source/Tools/GirToGapi/scripts/check-api.py) | The class-struct invariants GapiCodegen assumes. **Run before every codegen**; a violation is an NRE inside the generator. |

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

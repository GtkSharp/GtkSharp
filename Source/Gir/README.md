# Vendored GObject-Introspection files

This directory holds the upstream `.gir` files that `GirToGapi` (`Source/Tools/GirToGapi/`)
converts into the checked-in `Source/Libs/<Name>/<Name>-api.xml` descriptions.

Both the `.gir` inputs **and** the converted `-api.xml` outputs are checked in. Conversion runs
on demand via `dotnet cake build.cake --BuildTarget=RegenerateApi`, never as part of a normal
build, so `dotnet cake build.cake` stays hermetic and needs no Gtk installed — a property CI
(`.github/workflows/main.yml`) depends on.

Regenerating an api.xml is an explicit, reviewable, committed act.

Files keep their upstream canonical names (`Gtk-4.0.gir`, not `gtk-4.0.gir`) so that the mapping
back to the source package is unambiguous.

## Provenance

Source: **Debian `forky`** binary packages from `https://deb.debian.org/debian/`. Chosen because
it is the only source carrying gtk4 at exactly the targeted 4.22.4 *and* covering libadwaita,
gtksourceview-5 and webkitgtk-6.0 in one consistently-built set. The sha256 below is of the
`.deb`, as recorded in `dists/forky/main/binary-amd64/Packages.xz`, and each was verified after
download.

| `.gir` files | Debian package | Version | sha256 of `.deb` |
|:-------------|:---------------|:--------|:-----------------|
| `GLib-2.0.gir`, `GObject-2.0.gir`, `Gio-2.0.gir` | `gir1.2-glib-2.0-dev` | `2.88.2-1` | `803e92b96b19038bd9a5ce8899fa42bf67ee947b3980ea6a8898184e88017143` |
| `Graphene-1.0.gir` | `libgraphene-1.0-dev` | `1.10.8-5+b2` | `c47e5f2ebf9947bdf9f3a8bbd5e8b19195dda5ba5e7e2641993ec5094656051c` |
| `Pango-1.0.gir`, `PangoCairo-1.0.gir` | `libpango1.0-dev` | `1.58.0-1` | `3356a87b156a40d38049024c0cc6f5cf80c4a2aae22c0da9f7ecd22e8a61f59e` |
| `GdkPixbuf-2.0.gir` | `libgdk-pixbuf-2.0-dev` | `2.44.7+dfsg-1` | `88eecfa145fbc6b92739403610d9c5490ff93d39e834dbda0c74475a90ecbaf1` |
| `Gdk-4.0.gir`, `Gsk-4.0.gir`, `Gtk-4.0.gir` | `libgtk-4-dev` | `4.22.4+ds-1` | `fcf670a37d326bc89f4f9322341c6a00dd4d9ac351fa47b921ab08c5abfde604` |
| `Adw-1.gir` | `libadwaita-1-dev` | `1.9.2-1` | `b7e5a2d6497409de330c0b2fa13caa4a44b045d5280fc9cfc802f0d8bff51356` |
| `GtkSource-5.gir` | `libgtksourceview-5-dev` | `5.20.0-1` | `c80a34f41772757f53bbdc9a9217e5a141c54fc68202de8ed9f10581155f377e` |
| `JavaScriptCore-6.0.gir` | `libjavascriptcoregtk-6.0-dev` | `2.52.5-1` | `65c34b2f699c3696faf88b36b0345f32daa433be5c9665beedef9cded6bf73aa` |
| `WebKit-6.0.gir` | `libwebkitgtk-6.0-dev` | `2.52.5-1` | `0166e15d8b1a43b08ab8acffe489e669c07a9148c4ba2486cbc2e33b2dbafbdf` |

Within each `.deb` the files come from `usr/share/gir-1.0/`, except `GLib-2.0.gir`, which Debian
places at `usr/lib/x86_64-linux-gnu/gir-1.0/GLib-2.0.gir` and symlinks into `usr/share/gir-1.0/`
from `libgirepository1.0-dev`. It is the same file.

### Version skew to be aware of

The Windows runtime bundle chosen in V4 (gvsbuild `2026.6.0`) carries **libadwaita 1.9.1** where
Debian forky carries **1.9.2**. Gtk itself is 4.22.4 on both. Micro-version skew in libadwaita is
not expected to change ABI, but it means `AdwaitaSharp` is generated against a marginally newer
description than the Windows runtime provides. Re-check when either side moves.

## Deliberately not vendored

| File | Why |
|:-----|:----|
| `cairo-1.0.gir` | Cairo has no real introspection; the gir is a stub of `foreign="1"` records. `CairoSharp` has no `.metadata` and generates nothing today — it stays 100 % hand-written. Do not attempt to generate it. |
| `GdkX11-4.0.gir`, `GdkWayland-4.0.gir`, `GdkWin32-4.0.gir` | Platform backends; out of scope, and binding them would break the single-assembly-per-namespace model. |
| `GioUnix-2.0.gir`, `GLibUnix-2.0.gir`, `GModule-2.0.gir` | Not bound today. |
| `WebKitWebProcessExtension-6.0.gir` | Web-process-side API; loaded into WebKit's own process, not usable from the app process. |

## Licensing

The `.gir` files are LGPL-licensed alongside Gtk itself. Vendoring them in this LGPL repository
is fine, but the provenance table above must be kept accurate.

## Reproducing

`fetch-gir.py` in this directory re-fetches every file above from the pinned packages, checking
each download against the sha256 recorded here:

```sh
python fetch-gir.py
```

It is deliberately dependency-free and OS-independent — it unpacks the Debian `ar` container and
its `data.tar` payload in pure Python, so no `dpkg`, `ar` or Debian host is required.

To move to a newer upstream: read the `Filename`/`SHA256` fields for the new versions out of
`dists/forky/main/binary-amd64/Packages.xz`, update `PACKAGES` in `fetch-gir.py`, re-run it, and
update the provenance table above in the same commit.

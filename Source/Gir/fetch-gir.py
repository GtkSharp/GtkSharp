#!/usr/bin/env python3
"""Re-fetch the vendored .gir files from their pinned Debian packages.

Run from this directory:  python fetch-gir.py

Deliberately dependency-free and OS-independent: it unpacks the Debian `ar`
container and its data.tar payload in pure Python, so no dpkg, ar or Linux host
is required. Every download is checked against the sha256 recorded in
dists/forky/main/binary-amd64/Packages.xz at the time of vendoring.

To move to a newer upstream, update PACKAGES below (version, path, sha256 — read
them out of the Packages index), re-run, and update the provenance table in
README.md in the same commit.
"""
import hashlib
import io
import lzma
import os
import sys
import tarfile
import urllib.request

BASE = "https://deb.debian.org/debian/"
HERE = os.path.dirname(os.path.abspath(__file__))
CACHE = os.path.join(HERE, ".deb-cache")

# Debian forky. package -> (pool path, sha256, [gir basenames to keep])
PACKAGES = {
    "gir1.2-glib-2.0-dev": (
        "pool/main/g/glib2.0/gir1.2-glib-2.0-dev_2.88.2-1_amd64.deb",
        "803e92b96b19038bd9a5ce8899fa42bf67ee947b3980ea6a8898184e88017143",
        ["GLib-2.0.gir", "GObject-2.0.gir", "Gio-2.0.gir"]),
    "libgraphene-1.0-dev": (
        "pool/main/g/graphene/libgraphene-1.0-dev_1.10.8-5+b2_amd64.deb",
        "c47e5f2ebf9947bdf9f3a8bbd5e8b19195dda5ba5e7e2641993ec5094656051c",
        ["Graphene-1.0.gir"]),
    "libpango1.0-dev": (
        "pool/main/p/pango1.0/libpango1.0-dev_1.58.0-1_amd64.deb",
        "3356a87b156a40d38049024c0cc6f5cf80c4a2aae22c0da9f7ecd22e8a61f59e",
        ["Pango-1.0.gir", "PangoCairo-1.0.gir"]),
    "libgdk-pixbuf-2.0-dev": (
        "pool/main/g/gdk-pixbuf/libgdk-pixbuf-2.0-dev_2.44.7+dfsg-1_amd64.deb",
        "88eecfa145fbc6b92739403610d9c5490ff93d39e834dbda0c74475a90ecbaf1",
        ["GdkPixbuf-2.0.gir"]),
    "libgtk-4-dev": (
        "pool/main/g/gtk4/libgtk-4-dev_4.22.4+ds-1_amd64.deb",
        "fcf670a37d326bc89f4f9322341c6a00dd4d9ac351fa47b921ab08c5abfde604",
        ["Gdk-4.0.gir", "Gsk-4.0.gir", "Gtk-4.0.gir"]),
    "libadwaita-1-dev": (
        "pool/main/liba/libadwaita-1/libadwaita-1-dev_1.9.2-1_amd64.deb",
        "b7e5a2d6497409de330c0b2fa13caa4a44b045d5280fc9cfc802f0d8bff51356",
        ["Adw-1.gir"]),
    "libgtksourceview-5-dev": (
        "pool/main/g/gtksourceview5/libgtksourceview-5-dev_5.20.0-1_amd64.deb",
        "c80a34f41772757f53bbdc9a9217e5a141c54fc68202de8ed9f10581155f377e",
        ["GtkSource-5.gir"]),
    "libwebkitgtk-6.0-dev": (
        "pool/main/w/webkit2gtk/libwebkitgtk-6.0-dev_2.52.5-1_amd64.deb",
        "0166e15d8b1a43b08ab8acffe489e669c07a9148c4ba2486cbc2e33b2dbafbdf",
        ["WebKit-6.0.gir"]),
}


def ar_members(blob):
    """Yield (name, payload) for each member of a Unix ar archive."""
    if blob[:8] != b"!<arch>\n":
        raise ValueError("not an ar archive")
    pos = 8
    while pos + 60 <= len(blob):
        header = blob[pos:pos + 60]
        name = header[0:16].decode().strip().rstrip("/")
        size = int(header[48:58].decode().strip())
        pos += 60
        yield name, blob[pos:pos + size]
        pos += size + (size % 2)   # ar members are 2-byte aligned


def open_data_tar(name, payload):
    if name.endswith(".xz"):
        return tarfile.open(fileobj=io.BytesIO(lzma.decompress(payload)))
    if name.endswith(".gz"):
        return tarfile.open(fileobj=io.BytesIO(payload), mode="r:gz")
    if name.endswith(".zst"):
        try:
            from compression import zstd           # Python 3.14+
        except ImportError:
            try:
                import pyzstd as zstd              # pip install pyzstd
            except ImportError:
                raise RuntimeError(
                    f"{name}: no zstd decoder; use Python 3.14+ or pip install pyzstd")
        return tarfile.open(fileobj=io.BytesIO(zstd.decompress(payload)))
    raise RuntimeError("unrecognised data payload: " + name)


def fetch(path, sha256):
    os.makedirs(CACHE, exist_ok=True)
    local = os.path.join(CACHE, os.path.basename(path))
    if not os.path.exists(local):
        with urllib.request.urlopen(BASE + path, timeout=300) as r:
            with open(local, "wb") as f:
                f.write(r.read())
    blob = open(local, "rb").read()
    got = hashlib.sha256(blob).hexdigest()
    if got != sha256:
        raise SystemExit(f"sha256 mismatch for {path}\n  want {sha256}\n  got  {got}")
    return blob


def main():
    failures = 0
    for pkg, (path, sha256, wanted) in PACKAGES.items():
        blob = fetch(path, sha256)
        # Debian keeps GLib-2.0.gir under a multiarch lib path and symlinks it
        # into usr/share/gir-1.0, so match on basename rather than directory.
        remaining = set(wanted)
        for name, payload in ar_members(blob):
            if not name.startswith("data.tar"):
                continue
            with open_data_tar(name, payload) as tf:
                for member in tf.getmembers():
                    base = os.path.basename(member.name)
                    if member.isfile() and base in remaining:
                        with open(os.path.join(HERE, base), "wb") as out:
                            out.write(tf.extractfile(member).read())
                        remaining.discard(base)
        if remaining:
            print(f"  !! {pkg}: not found: {', '.join(sorted(remaining))}")
            failures += 1
        else:
            print(f"  ok {pkg}: {', '.join(wanted)}")
    return 1 if failures else 0


if __name__ == "__main__":
    sys.exit(main())

"""V4: list the gvsbuild GTK4 bundle's contents without downloading 300 MB.

A zip's central directory sits at the end of the file, so HTTP Range requests
over the last few MB are enough to enumerate every entry.
"""
import io
import struct
import urllib.request
import zipfile

URL = ("https://github.com/wingtk/gvsbuild/releases/download/2026.6.0/"
       "GTK4_Gvsbuild_2026.6.0_x64.zip")


def head_size(url):
    req = urllib.request.Request(url, method="HEAD")
    with urllib.request.urlopen(req, timeout=120) as r:
        return int(r.headers["Content-Length"]), r.url


def get_range(url, start, end):
    req = urllib.request.Request(url, headers={"Range": f"bytes={start}-{end}"})
    with urllib.request.urlopen(req, timeout=300) as r:
        if r.status != 206:
            raise RuntimeError(f"server ignored Range (status {r.status})")
        return r.read()


size, real = head_size(URL)
print(f"asset size: {size/1e6:.1f} MB")

tail = get_range(real, size - 4_000_000, size - 1)
print(f"tail fetched: {len(tail)/1e6:.1f} MB")

# Locate End Of Central Directory record.
eocd = tail.rfind(b"PK\x05\x06")
if eocd < 0:
    raise SystemExit("no EOCD in tail")
cd_size, cd_off = struct.unpack("<II", tail[eocd + 12:eocd + 20])
print(f"central directory: {cd_size/1e6:.1f} MB at offset {cd_off}")

# Build a sparse file object: real central directory at its true offset.
buf = bytearray(size)
tail_start = size - len(tail)
buf[tail_start:size] = tail
if cd_off < tail_start:
    extra = get_range(real, cd_off, tail_start - 1)
    buf[cd_off:tail_start] = extra
    print(f"extra fetched: {len(extra)/1e6:.1f} MB")

zf = zipfile.ZipFile(io.BytesIO(bytes(buf)))
names = zf.namelist()
print(f"entries: {len(names)}")

print("\n--- top-level layout ---")
tops = {}
for n in names:
    top = n.split("/")[0] if "/" in n else "(root file)"
    tops[top] = tops.get(top, 0) + 1
for k, v in sorted(tops.items(), key=lambda kv: -kv[1]):
    print(f"  {k:24s} {v}")

print("\n--- DLLs of interest ---")
KEY = ("gtk", "gdk", "gsk", "graphene", "gtksourceview", "adwaita", "webkit",
       "glib", "gobject", "gio-", "pango", "cairo", "gmodule")
for n in sorted(names):
    low = n.lower()
    if low.endswith(".dll") and any(k in low.rsplit("/", 1)[-1] for k in KEY):
        print("  ", n)

print("\n--- gir-1.0 present? ---")
girs = [n for n in names if "/gir-1.0/" in n]
print(f"  {len(girs)} gir files")
for n in sorted(girs)[:20]:
    print("  ", n)

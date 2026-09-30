"""Delete .metadata rules whose target no longer exists in the api.xml.

The Phase 4 triage sorts unmatched rules into obsolete / moved / converter bug.
This applies the obsolete verdict only -- a rule is removed when the type it
selects is gone from the api.xml, or when the type survived but the member the
rule names is gone. Anything else is left alone and reported, so that "moved"
rules get rewritten by hand rather than silently dropped.

    python retire-rules.py <file-api.xml> <file.metadata> <gapifixup-warnings>
                           [--apply]

Without --apply it only reports.
"""
import re
import sys
import xml.etree.ElementTree as ET

api_path, meta_path, warn_path = sys.argv[1], sys.argv[2], sys.argv[3]
apply_changes = "--apply" in sys.argv

root = ET.parse(api_path).getroot()

kind_of = {}
members = {}
for ns in root:
    for t in ns:
        cname = t.get("cname")
        if not cname:
            continue
        kind_of[cname] = t.tag
        got = members.setdefault(cname, set())
        for m in t.iter():
            for key in ("name", "cname"):
                if m.get(key):
                    got.add((m.tag, m.get(key)))

TYPE_RE = re.compile(r"/api/namespace/(\*|[a-z_]+)\[@cname='([^']+)'\]")
MEMBER_RE = re.compile(r"/(method|signal|property|constructor|virtual_method|field|interface)"
                       r"\[@(?:cname|name)='([^']+)'\]")

obsolete, keep = set(), []
for line in open(warn_path, encoding="utf-8"):
    m = re.search(r'path="([^"]+)"', line)
    if not m:
        continue
    path = m.group(1)

    t = TYPE_RE.search(path)
    if not t:
        keep.append((path, "selector not of the form /api/namespace/<kind>[@cname=...]"))
        continue

    selector_kind, cname = t.group(1), t.group(2)
    actual = kind_of.get(cname)

    if actual is None:
        obsolete.add(path)
        continue

    if selector_kind not in ("*", actual):
        keep.append((path, f"MOVED: {selector_kind} -> {actual}, rewrite the selector"))
        continue

    mem = MEMBER_RE.search(path[t.end():])
    if mem and (mem.group(1), mem.group(2)) not in members.get(cname, ()):
        obsolete.add(path)
        continue

    keep.append((path, "type and member both present -- needs a look"))

src = open(meta_path, encoding="utf-8").read().split("\n")
removed, out = 0, []
for line in src:
    m = re.search(r'path="([^"]+)"', line)
    if m and m.group(1) in obsolete:
        removed += 1
        continue
    out.append(line)

print(f"{meta_path}")
print(f"  obsolete rules removed : {removed}")
print(f"  left for manual review : {len(keep)}")
for path, why in keep:
    print(f"    {why}\n      {path}")

if apply_changes:
    open(meta_path, "w", encoding="utf-8", newline="").write("\n".join(out))
    print("  written")
else:
    print("  (dry run; pass --apply to write)")

"""Bucket unmatched .metadata rules into the plan's triage categories.

Phase 4 asks for every "matched no nodes" warning to be sorted into obsolete
(the target is gone), moved (the target survived but the selector no longer
finds it), or converter bug. The first two are decidable mechanically: pull the
target type out of the XPath and look for it in the api.xml.

    python triage-metadata.py <file-api.xml> <file.metadata> <gapifixup-output>

Reads the GapiFixup warning text on stdin if no third argument is given.
"""
import re
import sys
import xml.etree.ElementTree as ET
from collections import defaultdict

api_path, meta_path = sys.argv[1], sys.argv[2]
warnings = (open(sys.argv[3], encoding="utf-8").read() if len(sys.argv) > 3
            else sys.stdin.read())

root = ET.parse(api_path).getroot()

# cname -> element tag, for every type in the api.xml
kind_of = {}
member_names = defaultdict(set)
for ns in root:
    for t in ns:
        if t.get("cname"):
            kind_of[t.get("cname")] = t.tag
            for m in t.iter():
                if m.get("name"):
                    member_names[t.get("cname")].add((m.tag, m.get("name")))
                if m.get("cname"):
                    member_names[t.get("cname")].add((m.tag, m.get("cname")))

TYPE_RE = re.compile(r"/api/namespace/(\*|[a-z_]+)\[@cname='([^']+)'\]")
MEMBER_RE = re.compile(r"/(method|signal|property|constructor|virtual_method|field|interface)"
                       r"\[@(?:cname|name)='([^']+)'\]")

buckets = defaultdict(list)

for line in warnings.splitlines():
    m = re.search(r'path="([^"]+)"', line)
    if not m:
        continue
    path = m.group(1)

    t = TYPE_RE.search(path)
    if not t:
        buckets["unparsed selector"].append(path)
        continue

    selector_kind, cname = t.group(1), t.group(2)
    actual = kind_of.get(cname)

    if actual is None:
        buckets["OBSOLETE - type gone from api.xml"].append(path)
        continue

    if selector_kind not in ("*", actual):
        buckets[f"MOVED - kind changed ({selector_kind} -> {actual})"].append(path)
        continue

    mem = MEMBER_RE.search(path[t.end():])
    if mem:
        want = (mem.group(1), mem.group(2))
        if want not in member_names[cname]:
            buckets["OBSOLETE - member gone from a surviving type"].append(path)
            continue

    buckets["NEEDS REVIEW - type and member both present"].append(path)

total = sum(len(v) for v in buckets.values())
print(f"{total} unmatched rule(s)\n")
for name in sorted(buckets, key=lambda k: -len(buckets[k])):
    rows = buckets[name]
    print(f"  {len(rows):5d}  {name}")

for name in sorted(buckets, key=lambda k: -len(buckets[k])):
    rows = buckets[name]
    print(f"\n=== {name} ({len(rows)}) ===")
    types = defaultdict(int)
    for p in rows:
        t = TYPE_RE.search(p)
        types[t.group(2) if t else "?"] += 1
    for cname, n in sorted(types.items(), key=lambda kv: -kv[1])[:25]:
        print(f"  {n:4d}  {cname}")
    if len(types) > 25:
        print(f"  ... and {len(types) - 25} more types")

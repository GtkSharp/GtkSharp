"""Gate 1: how closely does GirToGapi reproduce gapi2xml.pl on the SAME input?

Converts a GTK 3 gir and compares the result with the checked-in GTK 3 api.xml,
which is known gapi2xml.pl output. A byte diff is useless here (element order,
attribute order, whitespace), so this compares the things that actually matter:
which types exist, which members exist on them, and what they are called.

    python gate1-diff.py <converted-api.xml> <reference-api.xml>
"""
import sys
import xml.etree.ElementTree as ET
from collections import Counter, defaultdict

TYPE_TAGS = ("object", "interface", "struct", "boxed", "enum", "callback",
             "alias", "class")


def index(path):
    root = ET.parse(path).getroot()
    types = {}
    members = defaultdict(set)
    names = {}

    for ns in root:
        for t in ns:
            if t.tag not in TYPE_TAGS:
                continue
            key = (t.tag, t.get("cname"))
            types[key] = t
            names[t.get("cname")] = t.get("name")

            for m in t:
                if m.tag in ("method", "constructor", "virtual_method"):
                    members[t.get("cname")].add((m.tag, m.get("cname")))
                    if m.get("cname"):
                        names[m.get("cname")] = m.get("name")
                elif m.tag in ("property", "signal", "field"):
                    members[t.get("cname")].add((m.tag, m.get("cname")))

    return types, members, names


new_types, new_members, new_names = index(sys.argv[1])
old_types, old_members, old_names = index(sys.argv[2])

print("=" * 66)
print("GATE 1 - GirToGapi vs gapi2xml.pl on GTK 3")
print("=" * 66)

new_c = {k[1] for k in new_types}
old_c = {k[1] for k in old_types}

print(f"\ntypes:  converted {len(new_c)}   reference {len(old_c)}"
      f"   in both {len(new_c & old_c)}")
print(f"  only in reference (converter would drop): {len(old_c - new_c)}")
print(f"  only in converted (converter adds):       {len(new_c - old_c)}")

# Kind agreement for types present in both.
kind_new = {k[1]: k[0] for k in new_types}
kind_old = {k[1]: k[0] for k in old_types}
kind_mismatch = [(c, kind_old[c], kind_new[c])
                 for c in sorted(new_c & old_c) if kind_old[c] != kind_new[c]]
print(f"\nkind agreement on shared types: "
      f"{len(new_c & old_c) - len(kind_mismatch)}/{len(new_c & old_c)}")
if kind_mismatch:
    kinds = Counter((o, n) for _, o, n in kind_mismatch)
    for (o, n), count in kinds.most_common(8):
        print(f"  {o:10s} -> {n:10s} {count:5d}   e.g. "
              f"{next(c for c, oo, nn in kind_mismatch if (oo, nn) == (o, n))}")

# Member coverage on shared types.
shared_members = missing = added = 0
for c in sorted(new_c & old_c):
    o, n = old_members[c], new_members[c]
    shared_members += len(o & n)
    missing += len(o - n)
    added += len(n - o)
print(f"\nmembers on shared types: matched {shared_members}"
      f"   missing {missing}   added {added}")
print(f"  coverage of reference members: "
      f"{100 * shared_members / max(1, shared_members + missing):.2f}%")

# Name agreement wherever both sides name the same C identifier.
agree = [c for c in new_names if c in old_names and new_names[c] == old_names[c]]
disagree = [c for c in new_names if c in old_names and new_names[c] != old_names[c]]
print(f"\nname agreement on shared C identifiers: "
      f"{len(agree)}/{len(agree) + len(disagree)}"
      f"  ({100 * len(agree) / max(1, len(agree) + len(disagree)):.2f}%)")
for c in sorted(disagree)[:10]:
    print(f"    {c}: reference '{old_names[c]}' vs converted '{new_names[c]}'")
if len(disagree) > 10:
    print(f"    ... and {len(disagree) - 10} more")

"""Does StudlyCaps(gir @name) reproduce gapi2xml.pl's @name? (risk R2)

gapi2xml.pl derives the gapi @name by stripping the C prefix off the cname and
running StudlyCaps over the remainder (gapi2xml.pl:846-891). GIR's @name
attribute is *already* the prefix-stripped snake_case identifier, so the claim
under test is that StudlyCaps(gir @name) == gapi @name, with no prefix
arithmetic needed at all.

Tested per element kind against GTK 3, where the checked-in api.xml is known
gapi2xml.pl output.

    python name-parity.py <Gtk-3.0.gir> <GtkSharp-api.xml>
"""
import re
import sys
import xml.etree.ElementTree as ET
from collections import Counter

GI = "http://www.gtk.org/introspection/core/1.0"
GLIB = "http://www.gtk.org/introspection/glib/1.0"
C = "http://www.gtk.org/introspection/c/1.0"


def studly_caps(symbol):
    """Port of gapi2xml.pl:StudlyCaps, statement for statement."""
    symbol = re.sub(r"^([a-z])", lambda m: m.group(1).upper(), symbol)
    symbol = re.sub(r"^(\d)", r"\1_", symbol)
    symbol = re.sub(r"[-_]([a-z])", lambda m: m.group(1).upper(), symbol)
    symbol = re.sub(r"[-_](\d)", r"\1", symbol)
    symbol = re.sub(r"^2", "Two", symbol)
    symbol = re.sub(r"^3", "Three", symbol)
    return symbol


gir = ET.parse(sys.argv[1]).getroot().find(f"{{{GI}}}namespace")
api = ET.parse(sys.argv[2]).getroot()

# --- expected: keyed by the C identifier, which both sides agree on ----------
# methods/constructors/functions -> c:identifier ; properties/signals -> owner+name
expect = {}
for el in api.iter():
    if el.tag in ("method", "constructor", "virtual_method") and el.get("cname"):
        expect[("call", el.get("cname"))] = el.get("name")
    elif el.tag == "member" and el.get("cname"):
        expect[("member", el.get("cname"))] = el.get("name")

owner_of = {}
for parent in api.iter():
    for child in parent:
        if child.tag in ("property", "signal", "field") and parent.get("cname"):
            owner_of[(child.tag, parent.get("cname"), child.get("cname"))] = child.get("name")

results = Counter()
mismatches = []


def check(kind, key, gir_name):
    want = expect.get(key)
    if want is None:
        results[kind + ":absent-from-api"] += 1
        return
    got = studly_caps(gir_name)
    results[kind + (":ok" if got == want else ":MISMATCH")] += 1
    if got != want:
        mismatches.append((kind, key[-1], gir_name, got, want))


for el in gir.iter():
    tag = el.tag.split("}")[1]
    ident = el.get(f"{{{C}}}identifier")
    if tag in ("method", "function", "constructor") and ident:
        check(tag, ("call", ident), el.get("name"))
    elif tag == "member" and el.get(f"{{{C}}}identifier"):
        check("member", ("member", el.get(f"{{{C}}}identifier")), el.get("name"))

# properties / signals / fields: match by (owner c:type, member name)
for owner in gir.iter():
    otype = owner.get(f"{{{C}}}type")
    if not otype:
        continue
    for child in owner:
        ctag = child.tag.split("}")[1]
        if ctag == "property":
            key = ("property", otype, child.get("name"))
        elif ctag == "signal":
            key = ("signal", otype, child.get("name"))
        elif ctag == "field":
            key = ("field", otype, child.get("name"))
        else:
            continue
        want = owner_of.get(key)
        if want is None:
            results[key[0] + ":absent-from-api"] += 1
            continue
        got = studly_caps(child.get("name"))
        results[key[0] + (":ok" if got == want else ":MISMATCH")] += 1
        if got != want:
            mismatches.append((key[0], otype, child.get("name"), got, want))

print("StudlyCaps(gir @name) vs gapi @name\n")
width = max(len(k) for k in results)
for k in sorted(results):
    print(f"  {k:{width}s} {results[k]:6d}")

total_ok = sum(v for k, v in results.items() if k.endswith(":ok"))
total_bad = sum(v for k, v in results.items() if k.endswith(":MISMATCH"))
print(f"\n  matched {total_ok}/{total_ok + total_bad}"
      f"  ({100 * total_ok / max(1, total_ok + total_bad):.3f}%)")

if mismatches:
    print(f"\nfirst {min(25, len(mismatches))} mismatches (kind, owner/cname, gir name, got, want):")
    for m in mismatches[:25]:
        print("   ", m)

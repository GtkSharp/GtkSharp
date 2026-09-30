"""V1: can `signal_vm` be reconstructed from GIR alone?

gapi2xml.pl derives signal_vm by parsing the C *_class_init body for
g_signal_new(..., G_STRUCT_OFFSET(XxxClass, field), ...). GIR does not carry
that link. Proposed heuristic: a class-struct callback field whose name matches
a <glib:signal name> (with '-' -> '_') IS the signal class closure.

This script tests the heuristic against GTK 3: predict signal_vm from
Gtk-3.0.gir, compare with the signal_vm attributes in the checked-in
GtkSharp-api.xml (which came from gapi2xml.pl).
"""
import re
import sys
import xml.etree.ElementTree as ET

GIR = sys.argv[1]
API = sys.argv[2]

GI = "http://www.gtk.org/introspection/core/1.0"
GLIB = "http://www.gtk.org/introspection/glib/1.0"
C = "http://www.gtk.org/introspection/c/1.0"

gir = ET.parse(GIR).getroot()
ns = gir.find(f"{{{GI}}}namespace")

# record name -> record element, for gtype-struct lookup
records = {r.get("name"): r for r in ns.findall(f"{{{GI}}}record")}

predicted = {}   # class c:type -> (set(signal_vm fields), set(plain vm fields))
for cls in ns.findall(f"{{{GI}}}class"):
    ts = cls.get(f"{{{GLIB}}}type-struct")
    if not ts or ts not in records:
        continue
    signals = {s.get("name").replace("-", "_")
               for s in cls.findall(f"{{{GLIB}}}signal")}
    sig_vm, plain_vm = set(), set()
    for f in records[ts].findall(f"{{{GI}}}field"):
        if f.find(f"{{{GI}}}callback") is None:
            continue
        name = f.get("name")
        (sig_vm if name in signals else plain_vm).add(name)
    predicted[cls.get(f"{{{C}}}type")] = (sig_vm, plain_vm)

api = ET.parse(API).getroot()
actual = {}
for obj in api.iter("object"):
    cs = obj.find("class_struct")
    if cs is None:
        continue
    sig_vm, plain_vm = set(), set()
    for m in cs.findall("method"):
        if m.get("padding") == "true":
            continue
        if m.get("signal_vm"):
            sig_vm.add(m.get("signal_vm"))
        elif m.get("vm"):
            plain_vm.add(m.get("vm"))
    actual[obj.get("cname")] = (sig_vm, plain_vm)

common = sorted(set(predicted) & set(actual))
tot_sig = tot_ok = 0
false_pos = []   # predicted signal_vm, actually plain vm
false_neg = []   # predicted plain vm, actually signal_vm
missing_field = 0

for cname in common:
    p_sig, p_plain = predicted[cname]
    a_sig, a_plain = actual[cname]
    known = a_sig | a_plain
    for f in p_sig & known:
        tot_sig += 1
        if f in a_sig:
            tot_ok += 1
        else:
            false_pos.append((cname, f))
    for f in p_plain & known:
        if f in a_sig:
            false_neg.append((cname, f))
    missing_field += len((p_sig | p_plain) - known)

print(f"classes compared:            {len(common)} "
      f"(gir {len(predicted)}, api.xml {len(actual)})")
print(f"vm fields not in api.xml:    {missing_field}  (hidden by metadata / gtk3 gir skew)")
print(f"predicted signal_vm matched: {tot_ok}/{tot_sig}")
print(f"false positives (vm marked signal_vm): {len(false_pos)}")
for x in false_pos[:15]:
    print("   ", x)
print(f"false negatives (signal_vm marked vm): {len(false_neg)}")
for x in false_neg[:15]:
    print("   ", x)

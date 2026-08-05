"""Structural invariants an api.xml must satisfy before GapiCodegen sees it.

GapiCodegen resolves every class_struct <method vm="X"/> against a
<virtual_method cname="X"> and every <method signal_vm="X"/> against a
<signal field_name="X"> on the same type (ObjectBase.cs:118-132). A missing
counterpart is a NullReferenceException inside the generator, with no hint as
to which type caused it -- so check it here, where the message can be useful.

    python check-api.py <file-api.xml> [more-api.xml ...]

Exits non-zero if any invariant is violated.
"""
import sys
import xml.etree.ElementTree as ET

problems = []
checked = 0


def check(path):
    global checked
    root = ET.parse(path).getroot()

    for owner in root.iter():
        if owner.tag not in ("object", "interface"):
            continue

        cs = owner.find("class_struct")
        if cs is None:
            continue

        cname = owner.get("cname")
        vms = {vm.get("cname") for vm in owner.findall("virtual_method")}
        sigs = {s.get("field_name") for s in owner.findall("signal")}

        for m in cs.findall("method"):
            checked += 1
            vm, signal_vm = m.get("vm"), m.get("signal_vm")

            if vm is not None and vm not in vms:
                problems.append(
                    f"{path}: {cname}: class_struct <method vm='{vm}'> has no "
                    f"matching <virtual_method cname='{vm}'>")
            elif signal_vm is not None and signal_vm not in sigs:
                problems.append(
                    f"{path}: {cname}: class_struct <method signal_vm='{signal_vm}'> has no "
                    f"matching <signal field_name='{signal_vm}'>")
            elif vm is None and signal_vm is None:
                problems.append(
                    f"{path}: {cname}: class_struct <method> has neither vm nor signal_vm")

        # The first class_struct child must be the parent-class field: ObjectBase.cs
        # skips exactly one leading <field> as the parent, and miscounting it
        # shifts every ABI slot after it.
        children = list(cs)
        if children and children[0].tag != "field":
            problems.append(
                f"{path}: {cname}: class_struct does not start with the parent <field>")

        # Duplicate slot names would make the vm lookup ambiguous.
        slots = [m.get("vm") or m.get("signal_vm") for m in cs.findall("method")]
        dupes = {s for s in slots if slots.count(s) > 1}
        for d in sorted(dupes):
            problems.append(f"{path}: {cname}: duplicate class_struct slot '{d}'")


for path in sys.argv[1:]:
    check(path)

print(f"checked {checked} class-struct slots across {len(sys.argv) - 1} file(s)")

if problems:
    print(f"\n{len(problems)} problem(s):")
    for p in problems[:40]:
        print("  " + p)
    if len(problems) > 40:
        print(f"  ... and {len(problems) - 40} more")
    sys.exit(1)

print("all invariants hold")

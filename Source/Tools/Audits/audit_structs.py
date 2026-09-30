"""Compare every hand-written sequential struct against the C record it stands for.

GskRoundedRect was 40 bytes where GSK reads 48. GLib.SourceFuncs is 16 where GLib
reads 48. Both were found by reading the C declaration beside the C# one; this
does that comparison for every hand-written struct at once.

Field *counts* only. A count that matches can still have the wrong types, but a
count that does not match cannot possibly be right.
"""
import glob
import os
import re
import xml.etree.ElementTree as ET

CORE = '{http://www.gtk.org/introspection/core/1.0}'
C = '{http://www.gtk.org/introspection/c/1.0}'

# ---------------------------------------------------------------- the gir side

records = {}   # simple name -> (field count, field names, gir)

for path in glob.glob('Source/Gir/*.gir'):
    try:
        root = ET.parse(path).getroot()
    except Exception:
        continue
    for el in root.iter():
        tag = el.tag.split('}')[-1]
        if tag not in ('record', 'union'):
            continue
        name = el.get('name')
        if not name:
            continue
        fields = []
        for child in el:
            ctag = child.tag.split('}')[-1]
            if ctag in ('field', 'callback'):
                fields.append(child.get('name'))
        if name not in records:
            records[name] = (len(fields), fields, os.path.basename(path))

# ------------------------------------------------------------ the binding side

STRUCT = re.compile(
    r'\[StructLayout\s*\(\s*LayoutKind\.(Sequential|Explicit)[^\]]*\)\s*\]\s*'
    r'(?:public|internal|private|\s)*\s*(?:partial\s+)?struct\s+(\w+)',
    re.S)

FIELD = re.compile(
    r'^\s*(?:\[[^\]]*\]\s*)*(?:public|internal|private|protected)\s+'
    r'(?!const\b|static\b|readonly\s+static\b)'
    r'(?:readonly\s+)?[\w\.\[\]<>\?]+\s+(\w+)\s*(?:;|=)', re.M)


def body_of(src, start):
    """The text between the struct's braces."""
    i = src.find('{', start)
    if i < 0:
        return ''
    depth, j = 0, i
    while j < len(src):
        if src[j] == '{':
            depth += 1
        elif src[j] == '}':
            depth -= 1
            if depth == 0:
                return src[i + 1:j]
        j += 1
    return src[i + 1:]


rows = []
for path in glob.glob('Source/Libs/*/*.cs'):
    if os.sep + 'Generated' + os.sep in path:
        continue
    src = open(path, encoding='utf-8', errors='replace').read()
    for m in STRUCT.finditer(src):
        name = m.group(2)
        if name not in records:
            continue
        body = body_of(src, m.end())
        # only top-level fields: drop anything inside a nested brace
        depth, flat = 0, []
        for line in body.split('\n'):
            if depth == 0:
                flat.append(line)
            depth += line.count('{') - line.count('}')
        cs_fields = FIELD.findall('\n'.join(flat))

        want, want_names, gir = records[name]
        rows.append((name, len(cs_fields), want, cs_fields, want_names, gir,
                     path.replace('Source/Libs/', '')))

print('hand-written sequential structs matched to a gir record: %d\n' % len(rows))

bad = [r for r in rows if r[1] != r[2]]
print('MISMATCHED FIELD COUNTS: %d\n' % len(bad))
for name, got, want, cs, gir_names, gir, path in sorted(bad, key=lambda r: r[2] - r[1]):
    print('%-22s C# has %2d, %s says %2d   [%s]' % (name, got, gir, want, path))
    print('     C#  : %s' % ', '.join(cs) if cs else '     C#  : (none)')
    print('     gir : %s' % ', '.join(n or '?' for n in gir_names))
    print()

print('matching counts: %s'
      % ', '.join(sorted(r[0] for r in rows if r[1] == r[2])))

print("""
Known, and not regressions:

  Value                 a FALSE POSITIVE. Its fields are declared with no access
                        modifier, which the FIELD regex above requires, so it
                        reports zero fields. The struct itself is correct.
  SourceFuncs           genuinely wrong, and deliberately left so: the members
  SourceCallbackFuncs   that would use them now throw rather than corrupting the
                        main loop. See Docs/testing.md.

MarkupParser, PollFD and TimeVal matched on count and have since been read field
by field against the C declaration. Two were right; TimeVal was not -- glong is
32 bits on win-x64 and the binding used IntPtr -- and is now fixed. A matching
count is still only half a check: compare the TYPES by hand for anything new.""")

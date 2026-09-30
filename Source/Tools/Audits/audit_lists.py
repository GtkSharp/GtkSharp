"""Untyped GLib.List/GSList returns, classified by what the elements actually are.

An untyped list falls back to GLib.Object.IsObject for every element. That is
fine when the elements are GObjects and fatal when they are not: a boxed type
gets dereferenced as a GObject (access violation), a plain struct pointer comes
back null. pango_itemize was the fatal kind.

For each site, find the C function it calls and ask the gir what the list holds.
"""
import glob
import os
import re
import xml.etree.ElementTree as ET

CORE = '{http://www.gtk.org/introspection/core/1.0}'
C = '{http://www.gtk.org/introspection/c/1.0}'

# ---- what the gir says each C function's return list holds -------------------

returns = {}     # c identifier -> element type name
kind = {}        # gir type name -> 'class' | 'record' | 'interface' | ...

for path in glob.glob('Source/Gir/*.gir'):
    try:
        root = ET.parse(path).getroot()
    except Exception:
        continue

    for el in root.iter():
        tag = el.tag.split('}')[-1]
        if tag in ('class', 'record', 'interface', 'union', 'boxed'):
            if el.get('name'):
                kind.setdefault(el.get('name'), tag)

    for fn in root.iter():
        if fn.tag.split('}')[-1] not in ('function', 'method'):
            continue
        ident = fn.get(C + 'identifier')
        if not ident:
            continue
        rv = fn.find(CORE + 'return-value')
        if rv is None:
            continue
        t = rv.find(CORE + 'type')
        if t is None or t.get('name') not in ('GLib.List', 'GLib.SList'):
            continue
        inner = t.find(CORE + 'type')
        returns[ident] = inner.get('name') if inner is not None else None

# ---- the binding sites ------------------------------------------------------

SITE = re.compile(r'new GLib\.S?List\(raw_ret\)')

rows = []
for path in glob.glob('Source/Libs/*/Generated/**/*.cs', recursive=True):
    src = open(path, encoding='utf-8', errors='replace').read()
    lines = src.split('\n')
    for i, line in enumerate(lines):
        if not SITE.search(line):
            continue
        # the nearest delegate invocation above names the C function
        cfn = None
        for j in range(i, max(0, i - 12), -1):
            m = re.search(r'=\s*(\w+)\(', lines[j])
            if m and m.group(1).startswith(('g_', 'gtk_', 'gdk_', 'pango_', 'gsk_',
                                            'adw_', 'webkit_', 'jsc_', 'gtk_source_')):
                cfn = m.group(1)
                break
        elem = returns.get(cfn)
        rows.append((path.replace('Source/Libs/', '').replace(os.sep, '/'),
                     i + 1, cfn, elem, kind.get(elem, '?') if elem else '?'))

print('untyped list returns: %d\n' % len(rows))

fatal = [r for r in rows if r[4] not in ('class', 'interface') ]
ok = [r for r in rows if r[4] in ('class', 'interface')]

print('ELEMENTS ARE NOT GOBJECTS -- unsafe to iterate: %d\n' % len(fatal))
for path, line, cfn, elem, k in sorted(fatal, key=lambda r: str(r[2])):
    print('  %-58s %-38s -> %s (%s)' % (path + ':' + str(line), cfn, elem, k))

print('\nelements are GObjects or interfaces on one -- iterate safely: %d' % len(ok))
for path, line, cfn, elem, k in sorted(ok, key=lambda r: str(r[2]))[:40]:
    print('  %-58s %-38s -> %s' % (path + ':' + str(line), cfn, elem))

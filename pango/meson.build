snk = join_paths(meson.current_source_dir(), '..', 'gtk-sharp.snk')
pkg = 'pango'
assembly_name = pkg + '-sharp'
glueincludes = 'pango/pango.h'
raw_api_fname = join_paths(meson.current_source_dir(), pkg + '-api.raw')
metadata_fname = join_paths(meson.current_source_dir(), 'Pango.metadata')
gluefile = join_paths(meson.current_build_dir(), 'generate.c')

configure_file(input: assembly_name + '.dll.config.in',
        output: assembly_name + '.dll.config',
        configuration : remap_dl_data)

subdir('generated')

sources = [
    'Analysis.cs',
    'AttrBackground.cs',
    'AttrColor.cs',
    'AttrFallback.cs',
    'AttrFamily.cs',
    'AttrFloat.cs',
    'AttrFontDesc.cs',
    'AttrForeground.cs',
    'AttrGravity.cs',
    'AttrGravityHint.cs',
    'Attribute.cs',
    'AttrInt.cs',
    'AttrIterator.cs',
    'AttrLanguage.cs',
    'AttrLetterSpacing.cs',
    'AttrList.cs',
    'AttrRise.cs',
    'AttrScale.cs',
    'AttrShape.cs',
    'AttrSize.cs',
    'AttrStretch.cs',
    'AttrStrikethrough.cs',
    'AttrStrikethroughColor.cs',
    'AttrStyle.cs',
    'AttrUnderline.cs',
    'AttrUnderlineColor.cs',
    'AttrVariant.cs',
    'AttrWeight.cs',
    'Context.cs',
    'Coverage.cs',
    'FontFamily.cs',
    'FontMap.cs',
    'Global.cs',
    'GlyphItem.cs',
    'GlyphString.cs',
    'Item.cs',
    'Layout.cs',
    'LayoutLine.cs',
    'LayoutRun.cs',
    'Matrix.cs',
    'Scale.cs',
    'ScriptIter.cs',
    'TabArray.cs',
    'Units.cs',
]

deps = [glib_sharp, cairo_sharp]
pango_sharp = library(assembly_name, source_gen, sources,
        cs_args: ['-unsafe', '-keyfile:' + snk],
        link_with: deps,
        install: true,
        install_dir: lib_install_dir
)

gluegen = custom_target('pangogluegen',
        input: raw_api_fname,
        output: 'generate.c',
        command: [generate_api, '--fakeglue'],
        depends: [source_gen])

library('pangosharpglue-3', gluegen,
        dependencies: [glib_dep, gio_dep, pango_dep],
        install: true)

install_infos += [assembly_name, pango_sharp.full_path()]
pango_sharp_dep = declare_dependency(link_with: deps + [pango_sharp])

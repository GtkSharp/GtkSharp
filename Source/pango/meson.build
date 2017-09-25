pkg = 'pango'
assembly_name = pkg + '-sharp'
raw_api_fname = join_paths(meson.current_source_dir(), pkg + '-api.raw')
metadata_fname = join_paths(meson.current_source_dir(), 'Pango.metadata')

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
pango_sharp = library(assembly_name, source_gen, sources, assemblyinfo,
        cs_args: ['-unsafe'],
        link_with: deps,
        install: install,
        install_dir: lib_install_dir
)

nuget_infos += [['PangoSharp', pango_sharp, ['GlibSharp', 'GioSharp']]]
install_infos += [assembly_name, pango_sharp.full_path()]
pango_sharp_dep = declare_dependency(link_with: deps + [pango_sharp])

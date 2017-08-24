snk = join_paths(meson.current_source_dir(), '..', 'gtk-sharp.snk')
assembly_name = 'atk-sharp'

raw_api_fname = join_paths(meson.current_source_dir(), 'atk-api.raw')
metadata_fname = join_paths(meson.current_source_dir(), 'Atk.metadata')
glueincludes = 'atk/atk.h'
gluefile = join_paths(meson.current_build_dir(), 'generate.c')

configure_file(input: assembly_name + '.dll.config.in',
        output: assembly_name + '.dll.config',
        configuration : remap_dl_data)

subdir('generated')

sources = [
    'Global.cs',
    'Hyperlink.cs',
    'Misc.cs',
    'Object.cs',
    'SelectionAdapter.cs',
    'TextAdapter.cs',
    'TextChangedDetail.cs',
    'Util.cs',
]

atk_sharp = library(assembly_name, source_gen, sources,
        cs_args: ['-unsafe', '-keyfile:' + snk],
        link_with: glib_sharp,
)

install_infos += [assembly_name, atk_sharp.full_path()]
atk_sharp_dep = declare_dependency(link_with: atk_sharp)


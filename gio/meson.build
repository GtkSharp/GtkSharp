snk = join_paths(meson.current_source_dir(), '..', 'gtk-sharp.snk')
assembly_name = 'gio-sharp'

raw_api_fname = join_paths(meson.current_source_dir(), 'gio-api.raw')
metadata_fname = join_paths(meson.current_source_dir(), 'Gio.metadata')
glueincludes = 'gio/gio.h'
gluefile = join_paths(meson.current_build_dir(), 'generate.c')

configure_file(input: assembly_name + '.dll.config.in',
        output: assembly_name + '.dll.config',
        configuration : remap_dl_data)

configure_file(input: assembly_name + '-3.0.pc.in',
        output: assembly_name + '-3.0.pc',
        configuration : version_data)

subdir('generated')

sources = [
    'Application.cs',
    'AppInfoAdapter.cs',
    'FileAdapter.cs',
    'FileEnumerator.cs',
    'FileFactory.cs',
    'GioGlobal.cs',
    'GioStream.cs',
    'IFile.cs'
]

gio_sharp = library(assembly_name, source_gen, sources,
        cs_args: ['-unsafe', '-keyfile:' + snk],
        link_with: glib_sharp,
)

install_infos += [assembly_name, gio_sharp.full_path()]
gio_sharp_dep = declare_dependency(link_with: gio_sharp)

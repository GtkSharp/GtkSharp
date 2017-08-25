snk = join_paths(meson.current_source_dir(), '..', 'gtk-sharp.snk')
pkg = 'gio'
assembly_name = pkg + '-sharp'

raw_api_fname = join_paths(meson.current_source_dir(), pkg + '-api.raw')
metadata_fname = join_paths(meson.current_source_dir(), 'Gio.metadata')
glueincludes = 'gio/gio.h'
gluefile = join_paths(meson.current_build_dir(), 'generate.c')

configure_file(input: assembly_name + '.dll.config.in',
        output: assembly_name + '.dll.config',
        configuration : remap_dl_data)

configure_file(input: assembly_name + '-3.0.pc.in',
        output: assembly_name + '-3.0.pc',
        configuration : version_data,
        install_dir: pkg_install_dir,
        install: true)

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
        install: true,
        install_dir: lib_install_dir
)

gluegen = custom_target('giogluegen',
        input: raw_api_fname,
        output: 'generate.c',
        command: [generate_api, '--fakeglue'],
        depends: [source_gen])

library('giosharpglue-3', gluegen,
        dependencies: [glib_dep, gio_dep],
        install: true)

install_infos += [assembly_name, gio_sharp.full_path()]
gio_sharp_dep = declare_dependency(link_with: [glib_sharp, gio_sharp])

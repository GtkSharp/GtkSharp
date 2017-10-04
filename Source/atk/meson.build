pkg = 'atk'

assembly_name = pkg + '-sharp'
raw_api_fname = join_paths(meson.current_source_dir(), pkg + '-api.raw')
metadata_fname = join_paths(meson.current_source_dir(), 'Atk.metadata')

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

atk_sharp = library(assembly_name, source_gen, sources, assemblyinfo,
        cs_args: ['-unsafe'],
        link_with: glib_sharp,
        install: install,
        install_dir: lib_install_dir
)

nuget_infos += [['AtkSharp', atk_sharp, ['GlibSharp', 'GioSharp']]]
install_infos += [assembly_name, atk_sharp.full_path()]
atk_sharp_dep = declare_dependency(link_with: [glib_sharp, atk_sharp])

if add_languages('c', required: false) and csc.get_id() == 'mono'
    c_abi_exe = executable(assembly_name + '_c_abi', c_abi,
            dependencies: [glib_dep, atk_dep])

    cs_abi_exe = executable(assembly_name + '_cs_abi', cs_abi,
            cs_args: ['-nowarn:169', '-nowarn:108', '-nowarn:114', '-nowarn:0618', '-unsafe'],
            dependencies: [atk_sharp_dep])

    env = environment()
    env.prepend('MONO_PATH', mono_path)
    test(assembly_name + 'abi', diff, args: [c_abi_exe.full_path(), cs_abi_exe.full_path()],
            env: env)
else
    message('Not running tests ' + csc.get_id())
endif

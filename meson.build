project('GtkSharp', ['cs'], version: '3.24.0', meson_version: '>=0.47')

if host_machine.system() == 'windows'
  if host_machine.cpu() == 'amd64'
    add_project_arguments('-define:WIN64LONGS', language: 'cs')
  endif
endif


version = meson.project_version()
apiversion = '3.0.0.0'
mono_required_version = '>=3.2.0'
gtk_required_version='>=3.22.0'
glib_required_version='>=2.32.0'

csc = meson.get_compiler('cs')

runtime = ''
runtime_debug_flags = ''
if get_option('buildtype') == 'debug' or get_option('buildtype') == 'debugoptimized'
  runtime_debug_flags=' --debug'
endif

if csc.get_id() == 'mono'
    if not csc.version().version_compare(mono_required_version)
        error('Mono required version @0@ not found (@1@)'.format(
            mono_required_version, csc.version()))
    endif

    mono_runtime_dep = dependency('mono', required: false)
    if mono_runtime_dep.found()
        runtime = 'mono' + runtime_debug_flags
    endif

endif

install = get_option('install')
#assemblyinfo='/AssemblyInfo.cs'

gacutil = find_program('gacutil', required: install)
#diff = find_program('audit/test_abi.py')

glib_dep = dependency('glib-2.0', version: glib_required_version,
        fallback: ['glib', 'libglib_dep'])
gio_dep = dependency('gio-2.0', version: glib_required_version,
        fallback: ['glib', 'libgio_dep'])

# FIXME Check how to enabled debug flags (if at all needed).

# TODO monodoc

prefix = get_option('prefix')
assembly_data = configuration_data()
assembly_data.set('API_VERSION', apiversion)
#assemblyinfo = configure_file(input: 'AssemblyInfo.cs.in', output: 'AssemblyInfo.cs', configuration : assembly_data)

#policy_config = files('policy.config.in')
if host_machine.system() == 'darwin'
  lib_prefix=''
  lib_suffix='.dylib'
else
  lib_prefix='.so'
  lib_suffix=''
endif

remap_dl_data = configuration_data()
remap_dl_data.set('LIB_PREFIX', lib_prefix)
remap_dl_data.set('LIB_SUFFIX', lib_suffix)

pkg_version = meson.project_name() + '-3.0'
version_data = configuration_data()
version_data.set('VERSION', version)
version_data.set('PACKAGE_VERSION', pkg_version)

install_infos = []
nuget_infos = []
lib_install_dir = join_paths(get_option('libdir'), 'mono', pkg_version)
pkg_install_dir = join_paths(get_option('libdir'), 'pkgconfig')
gapi_xml_installdir = join_paths(get_option('datadir'), 'gapi-3.0')

schema = join_paths(meson.current_source_dir(), 'gapi.xsd')

if host_machine.system() == 'windows'
    pathsep = ';'
else
    pathsep = ':'
endif
mono_path = ''
foreach d: [ 'GLibSharp',  'GioSharp' ]
    mono_path += pathsep + join_paths(meson.current_build_dir(), d)
endforeach

#subdir('Source')

source = join_paths(meson.current_source_dir(), 'build.sh')
message('Building @0@ ...'.format(source))
r = run_command('sh', source)


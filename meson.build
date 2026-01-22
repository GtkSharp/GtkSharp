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

#policy_config = dependency('policy.config.in')
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

subdir('Source/Tools/GapiCodegen')
subdir('Source/Tools/GapiFixup')

source = join_paths(meson.current_source_dir(), 'build.sh')
message('Building @0@ ...'.format(source))
r = run_command('sh', source)

has_gtk = true


#glib_sharp = csc.find_library('GLibSharp.dll',  dirs : [join_paths(meson.current_source_dir(), 'BuildOutput/Release/')])
#pango_sharp = csc.find_library('PangoSharp.dll',  dirs : [join_paths(meson.current_source_dir(), 'BuildOutput/Release/')])
#cairo_sharp = csc.find_library('CAiroSharp.dll',  dirs : [join_paths(meson.current_source_dir(), 'BuildOutput/Release/')])
#atk_sharp = csc.find_library('AthSharp.dll',  dirs : [join_paths(meson.current_source_dir(), 'BuildOutput/Release/')])
#gio_sharp = csc.find_library('GioSharp.dll',  dirs : [join_paths(meson.current_source_dir(), 'BuildOutput/Release/')])
#gdk_sharp = csc.find_library('GdkSharp.dll',  dirs : [join_paths(meson.current_source_dir(), 'BuildOutput/Release/')])
#gtk_sharp = csc.find_library('GtkSharp.dll',  dirs : [join_paths(meson.current_source_dir(), 'BuildOutput/Release/')])

glib_sharp = custom_target('GLibSharp.dll', input: join_paths(meson.current_source_dir(), 'BuildOutput/Release/GLibSharp.dll'), output: 'GLibSharp.dll', command: ['ls'])
pango_sharp = custom_target('PangoSharp.dll', input: join_paths(meson.current_source_dir(), 'BuildOutput/Release/PangoSharp.dll'), output: 'PangoSharp.dll', command: ['ls'])
cairo_sharp = custom_target('CairoSharp.dll', input: join_paths(meson.current_source_dir(), 'BuildOutput/Release/CairoSharp.dll'), output: 'CairoSharp.dll', command: ['ls'])
atk_sharp = custom_target('AtkSharp.dll', input: join_paths(meson.current_source_dir(), 'BuildOutput/Release/AtkSharp.dll'), output: 'AtkSharp.dll', command: ['ls'])
gio_sharp = custom_target('GioSharp.dll', input: join_paths(meson.current_source_dir(), 'BuildOutput/Release/GioSharp.dll'), output: 'GioSharp.dll', command: ['ls'])
gdk_sharp = custom_target('GdkSharp.dll', input: join_paths(meson.current_source_dir(), 'BuildOutput/Release/GdkSharp.dll'), output: 'GdkSharp.dll', command: ['ls'])
gtk_sharp = custom_target('GtkSharp.dll', input: join_paths(meson.current_source_dir(), 'BuildOutput/Release/GtkSharp.dll'), output: 'GtkSharp.dll', command: ['ls'])

glib_sharp_dep = declare_dependency(link_with: glib_sharp)
gio_sharp_dep = declare_dependency(link_with: [glib_sharp, gio_sharp])
pango_sharp_dep = declare_dependency(link_with: pango_sharp)
cairo_sharp_dep = declare_dependency(link_with: cairo_sharp)
atk_sharp_dep = declare_dependency(link_with: atk_sharp)
gdk_sharp_dep = declare_dependency(link_with: gdk_sharp)
gtk_sharp_dep = declare_dependency(link_with: [glib_sharp, pango_sharp, cairo_sharp, gio_sharp, atk_sharp, gdk_sharp, gtk_sharp])

glib_api_includes = join_paths(meson.current_source_dir(), 'Source/Libs/GLibSharp/Generated/GLibSharp-api.xml')
gio_api_includes = join_paths(meson.current_source_dir(), 'Source/Libs/GioSharp/Generated/GioSharp-api.xml')
cairo_api_includes = join_paths(meson.current_source_dir(), 'Source/Libs/CairoSharp/Generated/CairoSharp-api.xml')
gtk_api_includes = join_paths(meson.current_source_dir(), 'Source/Libs/GtkSharp/Generated/GtkSharp-api.xml')
gdk_api_includes = join_paths(meson.current_source_dir(), 'Source/Libs/GdkSharp/Generated/GdkSharp-api.xml')
pango_api_includes = join_paths(meson.current_source_dir(), 'Source/Libs/PangoSharp/Generated/PangoSharp-api.xml')
atk_api_includes = join_paths(meson.current_source_dir(), 'Source/Libs/AtkSharp/Generated/AtkSharp-api.xml')

#gtk_sharp_dep = [gtk_sharp.get_variable('gtk_sharp_dep'), gtk_sharp.get_variable('atk_sharp_dep')]
#gtk_sharp_dep = custom_target('gtk-sharp-3.0', version: gtk_sharp_required_version,



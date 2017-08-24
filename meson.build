project('gtk-sharp', ['cs', 'c'], version: '3.22.6')

if host_machine.system() == 'windows'
  if h.get(0) == host_machine.cpu()
    add_project_arguments('-define:WIN64LONGS', language: 'cs')
  endif
endif


apiversion = '3.0.0.0'
mono_required_version = '>=3.2.0'
gtk_required_version='>=3.0.0'
glib_required_version='>=2.32.0'

# FIXME Handle .net
mono = meson.get_compiler('cs')
if not mono.version().version_compare(mono_required_version)
  error('Mono required version @0@ not found (@1@)'.format(
    mono_required_version, mono.version()))
endif

assemblyinfo='/AssemblyInfo.cs'

gacutil = find_program('gacutil')
al = find_program('al')

glib_dep = dependency('glib-2.0', version: glib_required_version)
gio_dep = dependency('gio-2.0', version: glib_required_version)
mono_runtime_dep = dependency('mono', required: false)

# FIXME Check how to enabled debug flags (if at all needed).

# TODO monodoc

prefix = get_option('prefix')
assembly_data = configuration_data()
assembly_data.set_quoted('API_VERSION', apiversion)
configure_file(input: 'AssemblyInfo.cs.in', output: 'AssemblyInfo.cs', configuration : assembly_data)

policy_config = files('policy.config.in')
runtime = ''
if get_option('buildtype') == 'debug' or get_option('buildtype') == 'debugoptimized'
  runtime_debug_flags=' --debug'
endif
if mono_runtime_dep.found()
  runtime = 'mono' + runtime_debug_flags
endif

if host_machine.system() == 'osx'
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
version_data.set('VERSION', meson.project_version())
version_data.set('PACKAGE_VERSION', pkg_version)

install_infos = []
lib_install_dir = join_paths(get_option('libdir'), 'mono', pkg_version)
pkg_install_dir = join_paths(get_option('libdir'), 'pkgconfig')
gapi_xml_installdir = join_paths(get_option('datadir'), 'gapi-3.0')

schema = join_paths(meson.current_source_dir(), 'gapi.xsd')
subdir('parser')
subdir('generator')
subdir('glib')
subdir('gio')

cairo_dep = dependency('cairo', required: false)
if cairo_dep.found()
    subdir('cairo')
else
    message('Cairo not found, not building')
endif

pango_dep = dependency('pango', required: false)
if pango_dep.found()
    subdir('pango')
else
    message('Pango not found, not building')
endif

atk_dep = dependency('atk', required: false)
if atk_dep.found()
    subdir('atk')
else
    message('Atk not found, not building')
endif

gdk_dep = dependency('gdk-3.0', version: gtk_required_version, required: false)
if gdk_dep.found() and atk_dep.found() and pango_dep.found()
    subdir('gdk')
    has_gdk = true
else
    message('Gdk not found, not building')
    has_gdk = false
endif

gtk_dep = dependency('gtk+-3.0', version: gtk_required_version, required: false)
if gtk_dep.found() and atk_dep.found() and pango_dep.found()
    subdir('gtk')
    subdir('sample/GtkDemo')
    has_gtk = true
else
    has_gtk = false
    message('Gtk not found, not building')
endif

gacutil_install = join_paths(meson.current_source_dir(), 'gacutil_install.py')
meson.add_install_script(gacutil_install, install_infos)

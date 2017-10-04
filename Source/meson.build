if host_machine.system() == 'windows'
  if host_machine.cpu() == 'amd64'
    add_project_arguments('-define:WIN64LONGS', language: 'cs')
  endif
endif


version = meson.project_version()
apiversion = '3.0.0.0'
mono_required_version = '>=3.2.0'
gtk_required_version='>=3.0.0'
glib_required_version='>=2.32.0'

csc = meson.get_compiler('cs')

runtime = ''
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

    add_project_arguments('-keyfile:' + join_paths(meson.current_source_dir(), 'gtk-sharp.snk'),
        language: ['cs'])
endif

install = get_option('install')
assemblyinfo='/AssemblyInfo.cs'

gacutil = find_program('gacutil')
al = find_program('al')
diff = find_program('audit/test_abi.py')

glib_dep = dependency('glib-2.0', version: glib_required_version,
        fallback: ['glib', 'libglib_dep'])
gio_dep = dependency('gio-2.0', version: glib_required_version,
        fallback: ['glib', 'libgio_dep'])

# FIXME Check how to enabled debug flags (if at all needed).

# TODO monodoc

prefix = get_option('prefix')
assembly_data = configuration_data()
assembly_data.set('API_VERSION', apiversion)
assemblyinfo = configure_file(input: 'AssemblyInfo.cs.in', output: 'AssemblyInfo.cs', configuration : assembly_data)

policy_config = files('policy.config.in')
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
version_data.set('VERSION', version)
version_data.set('PACKAGE_VERSION', pkg_version)

install_infos = []
nuget_infos = []
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
    subdir('sample/valtest')
    has_gtk = true
else
    has_gtk = false
    message('Gtk not found, not building')
endif

nuget = find_program('nuget.py')
license_path = 'https://github.com/gtk-sharp'
project_uri = 'https://github.com/gtk-sharp'
icon_uri = 'https://upload.wikimedia.org/wikipedia/en/5/5f/Gtk_Sharp_Logo.png'
license_uri = 'https://github.com/gtk-sharp/gtk-sharp/blob/master/COPYING'

deps = []
foreach nugetinfo: nuget_infos
    # FIXME - Pass proper '--owner' and '--author'
    cmd = [nuget, '--package-name', nugetinfo[0], '--assembly', nugetinfo[1].full_path(),
            '--project-url', project_uri, '--icon-url', icon_uri, '--license-url',
            license_uri, '--version', version, '--tags', 'gtk bindings',
            '--builddir', meson.current_build_dir()]

    foreach dep: nugetinfo[2]
        cmd += ['--dependency=' + dep + ':' + version]
    endforeach

    deps += [custom_target(nugetinfo[0] + '-nugget', command: cmd,
        depends: [nugetinfo[1]] + deps,
        output: nugetinfo[0] + '.' + version + '.nupkg')]
endforeach

if install
    gacutil_install = join_paths(meson.current_source_dir(), 'gacutil_install.py')
    meson.add_install_script(gacutil_install, install_infos)
endif

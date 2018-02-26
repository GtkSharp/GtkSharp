gapi_installdir = join_paths(get_option('libdir'), 'gapi-3.0')
pkg_install_dir = '@0@/pkgconfig'.format(get_option('libdir'))

gapi_parser = executable('gapi-parser', 'gapi-parser.cs',
    install_dir : gapi_installdir,
    install: install)

gapi_parser_data = configuration_data()
gapi_parser_data.set('prefix', prefix)
gapi_parser_data.set('RUNTIME', runtime)
pkg_gapi_parser_data = configuration_data()
pkg_gapi_parser_data .set('VERSION', meson.project_version())

if install
    configure_file(input: 'gapi3-parser.in',
            output: 'gapi3-parser',
            configuration : gapi_parser_data,
            install_dir: get_option('bindir'))
    configure_file(input: 'gapi-3.0.pc.in',
            output: 'gapi-3.0.pc',
            configuration : pkg_gapi_parser_data,
            install_dir: pkg_install_dir)
    install_data('gapi_pp.pl', 'gapi2xml.pl', install_dir: gapi_installdir)
else
    configure_file(input: 'gapi3-parser.in',
            output: 'gapi3-parser',
            configuration : gapi_parser_data)
    configure_file(input: 'gapi-3.0.pc.in',
            output: 'gapi-3.0.pc',
            configuration : pkg_gapi_parser_data)
endif

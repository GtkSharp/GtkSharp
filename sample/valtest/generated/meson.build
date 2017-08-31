generated_sources = [
    'Gtksharp_Valobj.cs',
]

source_gen = custom_target(assembly_name + 'codegen',
    input: raw_api_fname,
    output: generated_sources,
    command: [
        generate_api,
        '--api-raw', '@INPUT@',
        '--gapi-fixup', gapi_fixup.full_path(),
        '--metadata', metadata_fname,
        '--symbols', symbols,
        '--gapi-codegen', gapi_codegen.full_path(),
        '--extra-includes', glib_api_includes,
        '--extra-includes', pango_api_includes,
        '--extra-includes', gio_api_includes,
        '--extra-includes', cairo_api_includes,
        '--extra-includes', gdk_api_includes,
        '--extra-includes', atk_api_includes,
        '--extra-includes', gtk_api_includes,
        '--out', meson.current_build_dir(),
        '--files', ';'.join(generated_sources),
        '--assembly-name', assembly_name,
        '--schema', schema,
    ],
    depends: [gapi_codegen, gapi_fixup])

api_xml = custom_target(pkg + '_api_xml',
        input: raw_api_fname,
        output: pkg + '-api.xml',
        command: [generate_api, '--fakeglue'],
        depends: [source_gen])

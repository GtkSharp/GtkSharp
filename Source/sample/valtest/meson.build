pkg = 'valobj'
assembly_name = pkg + '-sharp'
symbols = ''

raw_api_fname = join_paths(meson.current_source_dir(), pkg + '-api.xml')
metadata_fname = ''

vallib = library('valobj',
		'valobj.c',
		dependencies: [gtk_dep])

subdir('generated')
executable('valtest', 'Valtest.cs', source_gen,
		cs_args: ['-unsafe'],
		dependencies: [gtk_sharp_dep])

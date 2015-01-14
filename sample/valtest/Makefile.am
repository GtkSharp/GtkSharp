noinst_SCRIPTS = valtest.exe
lib_LTLIBRARIES = libvalobj.la

assemblies=../../glib/glib-sharp.dll ../../gio/gio-sharp.dll ../../cairo/cairo-sharp.dll ../../pango/pango-sharp.dll ../../atk/atk-sharp.dll ../../gdk/gdk-sharp.dll ../../gtk/gtk-sharp.dll
references=$(addprefix -r:, $(assemblies))

valtest.exe: generated-stamp Valtest.cs $(assemblies)
	$(CSC) $(CSFLAGS) -out:valtest.exe $(references) $(srcdir)/Valtest.cs $(GENERATED_SOURCES_OPTION)

libvalobj_la_SOURCES =	\
	valobj.c	\
	valobj.h

libvalobj_la_LDFLAGS = -module -avoid-version -no-undefined

libvalobj_la_LIBADD = $(GTK_LIBS)

AM_CPPFLAGS = $(GTK_CFLAGS)

generated-stamp: valobj-api.xml
	rm -rf generated/* && \
	$(RUNTIME) ../../generator/gapi_codegen.exe --generate $(srcdir)/valobj-api.xml	\
	--include=../../gtk/gtk-api.xml --include=../../gdk/gdk-api.xml					\
	--outdir=generated --assembly-name=valobj-sharp											\
	--schema=$(top_srcdir)/gapi.xsd && \
	touch generated-stamp

api:
	PATH=../../parser:$(PATH) $(RUNTIME) ../../parser/gapi-parser.exe valobj-sources.xml

install:

CLEANFILES =		\
	valtest.exe	\
	valtest.exe.mdb	\
	generated-stamp \
	$(GENERATED_SOURCES_FILES)

EXTRA_DIST =		\
	Valtest.cs	\
	valobj-api.xml	\
	valobj-sources.xml

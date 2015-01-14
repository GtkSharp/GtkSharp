noinst_SCRIPTS = opaquetest.exe
lib_LTLIBRARIES = libopaque.la

assemblies=../../glib/glib-sharp.dll ../../gio/gio-sharp.dll ../../pango/pango-sharp.dll ../../atk/atk-sharp.dll ../../gdk/gdk-sharp.dll ../../gtk/gtk-sharp.dll
references=$(addprefix -r:, $(assemblies))

opaquetest.exe: generated-stamp OpaqueTest.cs $(assemblies)
	$(CSC) $(CSFLAGS) -out:opaquetest.exe $(references) $(srcdir)/OpaqueTest.cs $(GENERATED_SOURCES_OPTION)

libopaque_la_SOURCES =	\
	opaques.c	\
	opaques.h

libopaque_la_LDFLAGS = -module -avoid-version -no-undefined

libopaque_la_LIBADD = $(GTK_LIBS)

AM_CPPFLAGS = $(GTK_CFLAGS)

generated-stamp: opaque-api.xml
	rm -rf generated/* &&	\
	$(RUNTIME) ../../generator/gapi_codegen.exe --generate $(srcdir)/opaque-api.xml	\
	--include=../../gtk/gtk-api.xml --include=../../gdk/gdk-api.xml			\
	--outdir=generated --assembly-name=opaque-sharp					\
	--schema=$(top_srcdir)/gapi.xsd							\
	&& touch generated-stamp

api:
	PATH=../../parser:$(PATH) $(RUNTIME) ../../parser/gapi-parser.exe opaque-sources.xml
	$(RUNTIME) ../../generator/gapi-fixup.exe --metadata=Opaque.metadata --api=opaque-api.xml

install:

CLEANFILES =			\
	opaquetest.exe		\
	opaquetest.exe.mdb	\
	generated-stamp		\
	$(GENERATED_SOURCES_FILES)

EXTRA_DIST =		\
	OpaqueTest.cs	\
	opaque-api.xml	\
	opaque-sources.xml

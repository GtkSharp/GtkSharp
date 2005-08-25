noinst_SCRIPTS = opaquetest.exe
lib_LTLIBRARIES = libopaque.la

assemblies=../../glib/glib-sharp.dll ../../pango/pango-sharp.dll ../../atk/atk-sharp.dll ../../gdk/gdk-sharp.dll ../../gtk/gtk-sharp.dll
references=$(addprefix /r:, $(assemblies))

opaquetest.exe: OpaqueTest.cs generated/*.cs $(assemblies)
	$(CSC) /out:opaquetest.exe $(references) $(srcdir)/OpaqueTest.cs $(GENERATED_SOURCES)

libopaque_la_SOURCES =	\
	opaques.c	\
	opaques.h

libopaque_la_LDFLAGS = -module -avoid-version -no-undefined

libopaque_la_LIBADD = $(GTK_LIBS)

INCLUDES = $(GTK_CFLAGS)

generated/*.cs: opaque-api.xml
	$(RUNTIME) ../../generator/gapi_codegen.exe --generate $(srcdir)/opaque-api.xml --include ../../gtk/gtk-api.xml ../../gdk/gdk-api.xml --outdir=generated --assembly-name=opaque-sharp

api:
	PATH=../../parser:$PATH $(RUNTIME) ../../parser/gapi-parser.exe opaque-sources.xml
	$(RUNTIME) ../../parser/gapi-fixup.exe --metadata=Opaque.metadata --api=opaque-api.xml

install:

CLEANFILES =			\
	opaquetest.exe		\
	opaquetest.exe.mdb	\
	generated/*.cs

EXTRA_DIST =		\
	OpaqueTest.cs	\
	opaque-api.xml	\
	opaque-sources.xml

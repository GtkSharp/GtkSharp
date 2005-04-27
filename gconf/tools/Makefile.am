assemblydir = $(prefix)/lib/gtk-sharp-2.0
assembly_DATA = gconfsharp-schemagen.exe
bin_SCRIPTS = gconfsharp2-schemagen
CLEANFILES = gconfsharp-schemagen.exe
DISTCLEANFILES = gconfsharp2-schemagen

EXTRA_DIST =				\
	schemagen.cs			\
	gconfsharp2-schemagen.in

gconfsharp-schemagen.exe: $(srcdir)/schemagen.cs
	$(CSC) /out:gconfsharp-schemagen.exe $(srcdir)/schemagen.cs


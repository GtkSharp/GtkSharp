assemblydir = $(bindir)
assembly_DATA = gconfsharp-schemagen.exe
bin_SCRIPTS = gconfsharp-schemagen
CLEANFILES = gconfsharp-schemagen.exe
DISTCLEANFILES = gconfsharp-schemagen

EXTRA_DIST =				\
	schemagen.cs			\
	gconfsharp-schemagen.in

gconfsharp-schemagen.exe: $(srcdir)/schemagen.cs
	$(CSC) /out:gconfsharp-schemagen.exe $(srcdir)/schemagen.cs


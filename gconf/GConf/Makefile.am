if ENABLE_GNOME
ASSEMBLY = gconf-sharp.dll
TARGETS = $(ASSEMBLY) $(ASSEMBLY).config
pkgconfigdir = $(libdir)/pkgconfig
pkgconfig_DATA = gconf-sharp.pc
else
ASSEMBLY = 
TARGETS = 
endif
assemblydir = $(libdir)
assembly_DATA = $(TARGETS)
CLEANFILES = $(ASSEMBLY)

DISTCLEANFILES = gconf-sharp.pc $(ASSEMBLY).config AssemblyInfo.cs

EXTRA_DIST =			\
	$(sources)		\
	$(ASSEMBLY).config.in	\
	AssemblyInfo.cs.in	\
	gconf-sharp.pc.in

references = 			\
	/r:../../glib/glib-sharp.dll

sources = 			\
	ClientBase.cs 		\
	Client.cs 		\
	ChangeSet.cs 		\
	_Entry.cs 		\
	NoSuchKeyException.cs 	\
	NotifyEventArgs.cs 	\
	NotifyEventHandler.cs 	\
	NotifyWrapper.cs 	\
	Value.cs

build_sources = $(addprefix $(srcdir)/, $(sources)) AssemblyInfo.cs

$(ASSEMBLY): $(build_sources)
	$(CSC) /out:$(ASSEMBLY) /target:library $(references) $(build_sources)


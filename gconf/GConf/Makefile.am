if ENABLE_GNOME
ASSEMBLY = gconf-sharp.dll
pkgconfigdir = $(libdir)/pkgconfig
pkgconfig_DATA = gconf-sharp.pc
else
ASSEMBLY = 
endif
assemblydir = $(libdir)
assembly_DATA = $(ASSEMBLY)
CLEANFILES = $(ASSEMBLY)

DISTCLEANFILES = gconf-sharp.pc

EXTRA_DIST =			\
	$(sources)		\
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

build_sources = $(addprefix $(srcdir)/, $(sources))

$(ASSEMBLY): $(build_sources)
	$(CSC) /out:$(ASSEMBLY) /target:library $(references) $(build_sources)


ASSEMBLY = gconf-sharp.dll
assemblydir = $(libdir)
assembly_DATA = $(ASSEMBLY)
CLEANFILES = $(ASSEMBLY)

EXTRA_DIST =			\
	$(sources)

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


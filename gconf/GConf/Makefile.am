if ENABLE_GNOME
TARGET = $(ASSEMBLY) $(ASSEMBLY).config
pkgconfigdir = $(libdir)/pkgconfig
pkgconfig_DATA = gconf-sharp.pc
else
TARGET = 
endif
noinst_DATA = $(TARGET)
CLEANFILES = $(ASSEMBLY) gtk-sharp.snk
DISTCLEANFILES = gconf-sharp.pc $(ASSEMBLY).config AssemblyInfo.cs

ASSEMBLY = $(ASSEMBLY_NAME).dll
ASSEMBLY_NAME = gconf-sharp

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

gtk-sharp.snk: $(top_srcdir)/gtk-sharp.snk
	cp $(top_srcdir)/gtk-sharp.snk .

$(ASSEMBLY): $(build_sources) gtk-sharp.snk
	$(CSC) /out:$(ASSEMBLY) /target:library $(references) $(build_sources)

install-data-local:
	@if test -n '$(TARGET)'; then						\
	  echo "$(GACUTIL) /i $(ASSEMBLY) /f $(GACUTIL_FLAGS)";	\
	  $(GACUTIL) /i $(ASSEMBLY) /f $(GACUTIL_FLAGS) || exit 1;	\
	fi

uninstall-local:
	@if test -n '$(TARGET)'; then						\
	  echo "$(GACUTIL) /u $(ASSEMBLY_NAME) $(GACUTIL_FLAGS)";	\
	  $(GACUTIL) /u $(ASSEMBLY_NAME) $(GACUTIL_FLAGS) || exit 1;	\
	fi


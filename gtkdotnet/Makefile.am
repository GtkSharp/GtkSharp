if ENABLE_DOTNET
TARGET = $(ASSEMBLY) $(ASSEMBLY).config
pkgconfigdir=$(libdir)/pkgconfig
pkgconfig_DATA=gtk-dotnet-2.0.pc
else
TARGET =
endif

ASSEMBLY = $(ASSEMBLY_NAME).dll
ASSEMBLY_NAME = gtk-dotnet
noinst_DATA = $(TARGET)
CLEANFILES = $(ASSEMBLY) gtk-sharp.snk
DISTCLEANFILES = $(ASSEMBLY).config

references = ../gdk/gdk-sharp.dll
build_references = $(addprefix -r:, $(references)) -r:System.Drawing

sources =		 			\
	Graphics.cs

build_sources = $(addprefix $(srcdir)/, $(sources)) ../AssemblyInfo.cs

EXTRA_DIST =				\
	$(sources)			\
	$(ASSEMBLY).config.in		\
	gtk-dotnet-2.0.pc.in		\
	makefile.win32

gtk-sharp.snk: $(top_srcdir)/gtk-sharp.snk
	cp $(top_srcdir)/gtk-sharp.snk .

$(ASSEMBLY): $(build_sources) $(references) gtk-sharp.snk
	$(CSC) /nowarn:0612 /out:$(ASSEMBLY) /target:library $(build_references) $(build_sources)

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


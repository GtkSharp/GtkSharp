if ENABLE_GLIBSHARP2_COMPAT
TARGET = $(ASSEMBLY)
else
TARGET = 
endif

ASSEMBLY = $(ASSEMBLY_NAME).dll
ASSEMBLY_NAME = gapi-2.0-compat
noinst_DATA = $(TARGET)

CLEANFILES = $(ASSEMBLY) $(ASSEMBLY).mdb gtk-sharp.snk

references = $(GLIBSHARP2_LIBS)

sources =		 			\
	AssemblyInfo.cs				\
	GTypeExtensions.cs

build_sources = $(addprefix $(srcdir)/, $(sources))
dist_sources = $(sources)

EXTRA_DIST =				\
	$(dist_sources)			\
	$(ASSEMBLY).config

gtk-sharp.snk: $(top_srcdir)/gtk-sharp.snk
	cp $(top_srcdir)/gtk-sharp.snk .

$(ASSEMBLY): $(build_sources) gtk-sharp.snk
	@rm -f $(ASSEMBLY).mdb
	$(CSC) $(CSFLAGS) -unsafe -nowarn:0169,0612,0618 -out:$(ASSEMBLY) -target:library $(references) $(build_sources)

install-data-local:
	@if test -n '$(TARGET)'; then							\
	  echo "$(GACUTIL) /i $(ASSEMBLY) /f $(GACUTIL_FLAGS)";				\
	  $(GACUTIL) /i $(ASSEMBLY) /f $(GACUTIL_FLAGS) || exit 1;			\
	fi

uninstall-local:
	@if test -n '$(TARGET)'; then							\
	  echo "$(GACUTIL) /u $(ASSEMBLY_NAME) $(GACUTIL_FLAGS)";			\
	  $(GACUTIL) /u $(ASSEMBLY_NAME) $(GACUTIL_FLAGS) || exit 1;			\
	fi


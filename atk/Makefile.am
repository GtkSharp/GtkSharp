SUBDIRS = . glue

pkg = atk
METADATA = Atk.metadata
SYMBOLS =
references = ../glib/glib-sharp.dll

glue_includes = atk/atk.h

sources = 			\
	TextChangedDetail.cs

customs =			\
	Global.custom		\
	Hyperlink.custom \
	Misc.custom		\
	Object.custom		\
	SelectionAdapter.custom	\
	TextAdapter.custom	\
	Util.custom

add_dist =

MOON_DIR=$(top_builddir)/moonbin/
MOON_ASSEMBLY=$(MOON_DIR)$(ASSEMBLY)
MOON_PRE_ASSEMBLY=$(MOON_DIR)MoonAtkBridge.dll

references_moon = $(MOON_DIR)glib-sharp.dll
build_references_moon = $(addprefix /r:, $(references_moon))

moonlight: $(MOON_ASSEMBLY)

$(MOON_ASSEMBLY): $(references_moon) $(ASSEMBLY)
	smcs -define:NET_2_1 $(build_references_moon) $(BUILD_PARAMS) -out:$(MOON_PRE_ASSEMBLY)
	mv $(MOON_PRE_ASSEMBLY) $@
	$(GAPI_CDECL_INSERT)

include ../Makefile.include



assemblydir = $(bindir)
noinst_DATA = updater.exe
CLEANFILES = updater.exe

references =

sources = 				\
	updater.cs			\
	TypeReflector.cs

build_sources = $(addprefix $(srcdir)/, $(sources))
dist_sources = $(sources)

EXTRA_DIST =					\
	$(dist_sources)

updater.exe: $(build_sources)
	$(CSC) /out:updater.exe $(references) $(build_sources)


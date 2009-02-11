TARGETS = MountOperation.exe 

DEBUGS = $(addsuffix .mdb, $(TARGETS))

assemblies = 					\
	$(top_builddir)/glib/glib-sharp.dll 	\
	$(top_builddir)/gtk/gtk-sharp.dll 	\
	$(top_builddir)/gio/gio-sharp.dll 

references=$(addprefix /r:, $(assemblies))

noinst_SCRIPTS = $(TARGETS)
CLEANFILES = $(TARGETS) $(DEBUGS)

.cs.exe: $(assemblies)
	$(CSC) /out:$@ $(references) $<

EXTRA_DIST = 				\
	MountOperation.cs


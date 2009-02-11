TARGETS = AppInfo.exe Volume.exe

DEBUGS = $(addsuffix .mdb, $(TARGETS))

assemblies = 					\
	$(top_builddir)/glib/glib-sharp.dll 	\
	$(top_builddir)/gio/gio-sharp.dll 

references=$(addprefix /r:, $(assemblies))

noinst_SCRIPTS = $(TARGETS)
CLEANFILES = $(TARGETS) $(DEBUGS)

.cs.exe: $(assemblies)
	$(CSC) /debug /out:$@ $(references) $<

EXTRA_DIST = 				\
	AppInfo.cs			\
	Volume.cs


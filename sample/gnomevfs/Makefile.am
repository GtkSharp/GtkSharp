if ENABLE_GNOMEVFS
TARGETS =			\
	TestAsync.exe		\
	TestAsyncStream.exe	\
	TestCallback.exe	\
	TestDirectory.exe	\
	TestInfo.exe		\
	TestMime.exe		\
	TestMonitor.exe		\
	TestSync.exe		\
	TestSyncCreate.exe	\
	TestSyncStream.exe	\
	TestSyncWrite.exe	\
	TestUnlink.exe		\
	TestVolumes.exe		\
	TestXfer.exe
else
TARGETS =
endif

assemblies=../../glib/glib-sharp.dll ../../gnomevfs/gnome-vfs-sharp.dll
references = $(addprefix /r:, $(assemblies)) /r:Mono.GetOptions

noinst_SCRIPTS = $(TARGETS)
CLEANFILES = $(TARGETS)

EXTRA_DIST = 			\
	TestAsync.cs		\
	TestAsyncStream.cs	\
	TestCallback.cs		\
	TestDirectory.cs	\
	TestInfo.cs		\
	TestMime.cs		\
	TestMonitor.cs		\
	TestSync.cs		\
	TestSyncCreate.cs	\
	TestSyncStream.cs	\
	TestSyncWrite.cs	\
	TestUnlink.cs		\
	TestVolumes.cs		\
	TestXfer.cs

TestAsync.exe: $(srcdir)/TestAsync.cs $(assemblies)
	$(CSC) /out:TestAsync.exe $(references) $(srcdir)/TestAsync.cs

TestAsyncStream.exe: $(srcdir)/TestAsyncStream.cs $(assemblies)
	$(CSC) /out:TestAsyncStream.exe $(references) $(srcdir)/TestAsyncStream.cs

TestCallback.exe: $(srcdir)/TestCallback.cs $(assemblies)
	$(CSC) /out:TestCallback.exe $(references) $(srcdir)/TestCallback.cs

TestDirectory.exe: $(srcdir)/TestDirectory.cs $(assemblies)
	$(CSC) /out:TestDirectory.exe $(references) $(srcdir)/TestDirectory.cs

TestInfo.exe: $(srcdir)/TestInfo.cs $(assemblies)
	$(CSC) /out:TestInfo.exe $(references) $(srcdir)/TestInfo.cs

TestMime.exe: $(srcdir)/TestMime.cs $(assemblies)
	$(CSC) /out:TestMime.exe $(references) $(srcdir)/TestMime.cs

TestMonitor.exe: $(srcdir)/TestMonitor.cs $(assemblies)
	$(CSC) /out:TestMonitor.exe $(references) $(srcdir)/TestMonitor.cs

TestSyncStream.exe: $(srcdir)/TestSyncStream.cs $(assemblies)
	$(CSC) /out:TestSyncStream.exe $(references) $(srcdir)/TestSyncStream.cs

TestSync.exe: $(srcdir)/TestSync.cs $(assemblies)
	$(CSC) /out:TestSync.exe $(references) $(srcdir)/TestSync.cs

TestSyncCreate.exe: $(srcdir)/TestSyncCreate.cs $(assemblies)
	$(CSC) /out:TestSyncCreate.exe $(references) $(srcdir)/TestSyncCreate.cs

TestSyncWrite.exe: $(srcdir)/TestSyncWrite.cs $(assemblies)
	$(CSC) /out:TestSyncWrite.exe $(references) $(srcdir)/TestSyncWrite.cs

TestUnlink.exe: $(srcdir)/TestUnlink.cs $(assemblies)
	$(CSC) /out:TestUnlink.exe $(references) $(srcdir)/TestUnlink.cs

TestVolumes.exe: $(srcdir)/TestVolumes.cs $(assemblies)
	$(CSC) /out:TestVolumes.exe $(references) $(srcdir)/TestVolumes.cs

TestXfer.exe: $(srcdir)/TestXfer.cs $(assemblies)
	$(CSC) /out:TestXfer.exe $(references) $(srcdir)/TestXfer.cs

SUBDIRS = .

if ENABLE_GNOMEVFS
TARGET = $(ASSEMBLY) $(ASSEMBLY).config
APIS = $(API) $(ADDITIONAL_API)
pkgconfigdir = $(libdir)/pkgconfig
pkgconfig_DATA = gnome-vfs-sharp-2.0.pc
else
TARGET=
APIS=
endif

API = gnomevfs-api.xml
ADDITIONAL_API = gnomevfs-symbols.xml
RAW_API = gnomevfs-api.raw
INCLUDE_API =
METADATA = Gnomevfs.metadata
ASSEMBLY_NAME = gnome-vfs-sharp
references = /r:../glib/glib-sharp.dll

ASSEMBLY = $(ASSEMBLY_NAME).dll
gapidir = $(datadir)/gapi-2.0
noinst_DATA = $(TARGET)
gapi_DATA = $(APIS)
CLEANFILES = $(ASSEMBLY) generated-stamp generated/*.cs $(API) glue/generated.c gtk-sharp.snk
DISTCLEANFILES = $(ASSEMBLY).config AssemblyInfo.cs gnome-vfs-sharp.pc

sources = 					\
	Async.cs				\
	AsyncCallback.cs			\
	AsyncCallbackNative.cs			\
	AsyncDirectoryLoadCallback.cs		\
	AsyncDirectoryLoadCallbackNative.cs	\
	AsyncReadCallback.cs			\
	AsyncReadCallbackNative.cs		\
	AsyncWriteCallback.cs			\
	AsyncWriteCallbackNative.cs		\
	Directory.cs				\
	FileInfo.cs				\
	Handle.cs				\
	MimeType.cs				\
	ModuleCallback.cs			\
	ModuleCallbackAuthentication.cs		\
	ModuleCallbackFullAuthentication.cs	\
	Monitor.cs				\
	Sync.cs					\
	Vfs.cs					\
	VfsException.cs				\
	VfsStream.cs				\
	VfsStreamAsyncResult.cs			\
	Xfer.cs					\
	XferProgressCallback.cs			\
	XferProgressCallbackNative.cs

build_sources = $(addprefix $(srcdir)/, $(sources)) AssemblyInfo.cs

customs =				\
	Uri.custom			\
	VolumeMonitor.custom

build_customs = $(addprefix $(srcdir)/, $(customs))

EXTRA_DIST =			\
	$(ASSEMBLY).config.in	\
	AssemblyInfo.cs.in	\
	$(RAW_API)		\
	$(ADDITIONAL_API)	\
	$(METADATA)		\
	$(customs)		\
	$(sources)

$(API): $(srcdir)/$(RAW_API) $(srcdir)/$(METADATA)
	cp $(srcdir)/$(RAW_API) $(API)
	chmod u+w $(API)
	$(RUNTIME) ../parser/gapi-fixup.exe --api=$(API) --metadata=$(srcdir)/$(METADATA)

generated-stamp: $(API) $(srcdir)/$(ADDITIONAL_API) ../generator/gapi_codegen.exe $(build_customs)
	rm -f generated/* &&	\
	$(RUNTIME) ../generator/gapi_codegen.exe --generate $(API) $(srcdir)/$(ADDITIONAL_API)	\
	--outdir=generated --customdir=$(srcdir) --assembly-name=$(ASSEMBLY_NAME)	\
	--gluelib-name=gtksharpglue --glue-filename=glue/generated.c			\
	&& touch generated-stamp

gtk-sharp.snk: $(top_srcdir)/gtk-sharp.snk
	cp $(top_srcdir)/gtk-sharp.snk .

$(ASSEMBLY): $(build_sources) generated-stamp gtk-sharp.snk
	$(CSC) /nowarn:0612 /unsafe /out:$(ASSEMBLY) /target:library $(references) $(build_sources) $(GENERATED_SOURCES)

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


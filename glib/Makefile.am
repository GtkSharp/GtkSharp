SUBDIRS = glue

TARGET = $(ASSEMBLY)
ASSEMBLY = $(ASSEMBLY_NAME).dll
ASSEMBLY_NAME = glib-sharp
noinst_DATA = $(ASSEMBLY) $(ASSEMBLY).config
CLEANFILES = $(ASSEMBLY) $(ASSEMBLY).mdb gtk-sharp.snk AssemblyInfo.cs
DISTCLEANFILES = $(ASSEMBLY).config

references = 

sources =		 			\
	Argv.cs					\
	Boxed.cs				\
	CDeclCallbackAttribute.cs		\
	ClassInitializerAttribute.cs		\
	ConnectBeforeAttribute.cs		\
	DefaultSignalHandlerAttribute.cs	\
	DelegateWrapper.cs			\
	DestroyNotify.cs			\
	EnumWrapper.cs				\
	FileUtils.cs				\
	GException.cs				\
	GString.cs				\
	GType.cs				\
	GTypeAttribute.cs			\
	Idle.cs					\
	IWrapper.cs				\
	ListBase.cs				\
	List.cs					\
	Log.cs					\
	MainContext.cs				\
	MainLoop.cs				\
	ManagedValue.cs				\
	Markup.cs				\
	Marshaller.cs				\
	MissingIntPtrCtorException.cs		\
	NotifyHandler.cs			\
	Object.cs				\
	ObjectManager.cs			\
	Opaque.cs				\
	PropertyAttribute.cs			\
	Signal.cs				\
	SignalArgs.cs				\
	SignalAttribute.cs			\
	SignalCallback.cs			\
	SList.cs				\
	Source.cs				\
	Thread.cs				\
	Timeout.cs				\
	TypeConverter.cs			\
	TypeFundamentals.cs			\
	UnwrappedObject.cs			\
	ValueArray.cs				\
	Value.cs				\
	WeakObject.cs

build_sources = $(addprefix $(srcdir)/, $(sources)) AssemblyInfo.cs
dist_sources = $(sources)

EXTRA_DIST =				\
	$(dist_sources)			\
	$(ASSEMBLY).config.in		\
	makefile.win32

gtk-sharp.snk: $(top_srcdir)/gtk-sharp.snk
	cp $(top_srcdir)/gtk-sharp.snk .

AssemblyInfo.cs: $(top_builddir)/AssemblyInfo.cs
	cp $(top_builddir)/AssemblyInfo.cs .

if PLATFORM_WIN32
GAPI_CDECL_INSERT=$(top_srcdir)/gapi-cdecl-insert --keyfile=gtk-sharp.snk $(ASSEMBLY)
else
GAPI_CDECL_INSERT=
endif

$(ASSEMBLY): $(build_sources) gtk-sharp.snk AssemblyInfo.cs
	@rm -f $(ASSEMBLY).mdb
	$(CSC) $(CSFLAGS) -nowarn:0169,0612,0618 -out:$(ASSEMBLY) -target:library $(references) $(build_sources)
	$(GAPI_CDECL_INSERT)

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


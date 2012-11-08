SUBDIRS =

SNK = $(top_srcdir)/gtk-sharp.snk
TARGET = $(ASSEMBLY)
ASSEMBLY = $(ASSEMBLY_NAME).dll
ASSEMBLY_NAME = glib-sharp
ASSEMBLY_NAME_VERSION = $(ASSEMBLY_NAME),Version=$(API_VERSION)
noinst_DATA = $(ASSEMBLY) $(ASSEMBLY).config $(POLICY_ASSEMBLIES)
pkgconfigdir = $(libdir)/pkgconfig
pkgconfig_DATA = glib-sharp-3.0.pc
gapidir = $(datadir)/gapi-3.0
gapi_DATA = glib-api.xml

CLEANFILES = $(ASSEMBLY) $(ASSEMBLY).mdb $(POLICY_ASSEMBLIES) $(POLICY_CONFIGS)
DISTCLEANFILES = $(ASSEMBLY).config

POLICY_ASSEMBLIES = $(addsuffix .$(ASSEMBLY), $(addprefix policy., $(POLICY_VERSIONS)))
POLICY_CONFIGS = $(addsuffix .config, $(addprefix policy., $(POLICY_VERSIONS)))

references = 

sources =		 			\
	Argv.cs					\
	ConnectBeforeAttribute.cs		\
	DefaultSignalHandlerAttribute.cs	\
	DestroyNotify.cs			\
	ExceptionManager.cs			\
	FileUtils.cs				\
	GException.cs				\
	GInterfaceAdapter.cs			\
	GInterfaceAttribute.cs			\
	GLibSynchronizationContext.cs		\
	Global.cs				\
	GString.cs				\
	GType.cs				\
	GTypeAttribute.cs			\
	Idle.cs					\
	InitiallyUnowned.cs			\
	IOChannel.cs				\
	IWrapper.cs				\
	KeyFile.cs				\
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
	ParamSpec.cs				\
	Priority.cs				\
	PropertyAttribute.cs			\
	PtrArray.cs				\
	Signal.cs				\
	SignalArgs.cs				\
	SignalAttribute.cs			\
	SignalClosure.cs			\
	SList.cs				\
	Source.cs				\
	Spawn.cs				\
	Thread.cs				\
	Timeout.cs				\
	ToggleRef.cs				\
	TypeFundamentals.cs			\
	TypeInitializerAttribute.cs		\
	ValueArray.cs				\
	Value.cs				\
	Variant.cs				\
	VariantType.cs

build_sources = $(addprefix $(srcdir)/, $(sources)) $(top_builddir)/AssemblyInfo.cs
dist_sources = $(sources)

EXTRA_DIST =				\
	$(dist_sources)			\
	$(ASSEMBLY).config.in		\
	glib-sharp-3.0.pc.in		\
	glib-api.xml

$(ASSEMBLY): $(build_sources) $(SNK)
	@rm -f $(ASSEMBLY).mdb
	$(CSC) $(CSFLAGS) -keyfile:$(SNK) -unsafe -out:$(ASSEMBLY) -target:library $(references) $(build_sources)

policy.%.config: $(top_builddir)/policy.config
	sed -e "s/@ASSEMBLY_NAME@/$(ASSEMBLY_NAME)/" -e "s/@POLICY@/$*/" $(top_builddir)/policy.config > $@

$(POLICY_ASSEMBLIES) : policy.%.$(ASSEMBLY): policy.%.config $(SNK)
	$(AL) -link:policy.$*.config -out:$@ -keyfile:$(SNK)

install-data-local:
	@if test -n '$(TARGET)'; then							\
	  echo "$(GACUTIL) -i $(ASSEMBLY) -f $(GACUTIL_FLAGS)";				\
	  $(GACUTIL) -i $(ASSEMBLY) -f $(GACUTIL_FLAGS) || exit 1;			\
	  if test -n '$(POLICY_VERSIONS)'; then						\
	    for i in $(POLICY_VERSIONS); do						\
	      echo "$(GACUTIL) -i policy.$$i.$(ASSEMBLY) -f $(GACUTIL_FLAGS)";		\
	      $(GACUTIL) -i policy.$$i.$(ASSEMBLY) -f $(GACUTIL_FLAGS) || exit 1;	\
	    done									\
	  fi										\
	fi

uninstall-local:
	@if test -n '$(TARGET)'; then							\
	  echo "$(GACUTIL) -u $(ASSEMBLY_NAME_VERSION) $(GACUTIL_FLAGS)";		\
	  $(GACUTIL) -u $(ASSEMBLY_NAME_VERSION) $(GACUTIL_FLAGS) || exit 1;		\
	  if test -n '$(POLICY_VERSIONS)'; then						\
	    for i in $(POLICY_VERSIONS); do						\
	      echo "$(GACUTIL) -u policy.$$i.$(ASSEMBLY_NAME) $(GACUTIL_FLAGS)";	\
	      $(GACUTIL) -u policy.$$i.$(ASSEMBLY_NAME) $(GACUTIL_FLAGS) || exit 1;	\
	    done									\
	  fi										\
	fi


SUBDIRS = glue

TARGET = $(ASSEMBLY)
ASSEMBLY = $(ASSEMBLY_NAME).dll
ASSEMBLY_NAME = glib-sharp
noinst_DATA = $(ASSEMBLY) $(ASSEMBLY).config
CLEANFILES = $(ASSEMBLY) gtk-sharp.pub
DISTCLEANFILES = $(ASSEMBLY).config AssemblyInfo.cs

references = 

sources =		 			\
	Boxed.cs				\
	ConnectBeforeAttribute.cs		\
	DefaultSignalHandlerAttribute.cs	\
	DelegateWrapper.cs			\
	EnumWrapper.cs				\
	FileUtils.cs				\
	GException.cs				\
	Idle.cs					\
	IWrapper.cs				\
	ListBase.cs				\
	List.cs					\
	Log.cs					\
	MainContext.cs				\
	ManagedValue.cs				\
	Markup.cs				\
	Marshaller.cs				\
	MissingIntPtrCtorException.cs		\
	Object.cs				\
	ObjectManager.cs			\
	Opaque.cs				\
	SignalArgs.cs				\
	SignalAttribute.cs			\
	SignalCallback.cs			\
	SList.cs				\
	Source.cs				\
	Thread.cs				\
	Timeout.cs				\
	TypeConverter.cs			\
	Type.cs					\
	TypeFundamentals.cs			\
	UnwrappedObject.cs			\
	ValueArray.cs				\
	Value.cs

build_sources = $(addprefix $(srcdir)/, $(sources)) AssemblyInfo.cs
dist_sources = $(sources)

EXTRA_DIST =				\
	$(dist_sources)			\
	$(ASSEMBLY).config.in		\
	AssemblyInfo.cs.in		\
	makefile.win32

gtk-sharp.pub: $(top_srcdir)/gtk-sharp.pub
	cp $(top_srcdir)/gtk-sharp.pub .

$(ASSEMBLY): $(build_sources) gtk-sharp.pub
	$(CSC) /out:$(ASSEMBLY) /target:library $(references) $(build_sources)

install-data-local:
	@if test -n '$(TARGET)'; then						\
	  echo "$(GACUTIL) /i $(ASSEMBLY) /f /package $(PACKAGE) /root $(DESTDIR)$(libdir)";	\
	  $(GACUTIL) /i $(ASSEMBLY) /f /package $(PACKAGE) /root $(DESTDIR)$(libdir) || exit 1;	\
	fi

uninstall-local:
	@if test -n '$(TARGET)'; then						\
	  echo "$(GACUTIL) /u $(ASSEMBLY_NAME) /package $(PACKAGE) /root $(DESTDIR)$(libdir)";	\
	  $(GACUTIL) /u $(ASSEMBLY_NAME) /package $(PACKAGE) /root $(DESTDIR)$(libdir) || exit 1;	\
	fi


SUBDIRS = glue

ASSEMBLY = glib-sharp.dll
assemblydir = $(libdir)
assembly_DATA = $(ASSEMBLY)
CLEANFILES = $(ASSEMBLY)

references = 

sources =		 			\
	Boxed.cs				\
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
	time_t_CustomMarshaler.cs		\
	TypeConverter.cs			\
	Type.cs					\
	TypeFundamentals.cs			\
	UnwrappedObject.cs			\
	ValueArray.cs				\
	Value.cs

build_sources = $(addprefix $(srcdir)/, $(sources))
dist_sources = $(sources)

EXTRA_DIST =					\
	$(dist_sources)				\
	makefile.win32

$(ASSEMBLY): $(build_sources)
	$(CSC) /out:$(ASSEMBLY) /target:library $(references) $(build_sources)


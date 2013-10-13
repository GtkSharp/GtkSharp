assemblydir = $(prefix)/lib/gapi-3.0
assembly_DATA = gapi-fixup.exe gapi_codegen.exe
bin_SCRIPTS = gapi3-fixup gapi3-codegen
CLEANFILES = gapi-fixup.exe gapi_codegen.exe
DISTCLEANFILES = gapi3-fixup gapi3-codegen

references =

fixup_sources =				\
	gapi-fixup.cs

sources = 				\
	AliasGen.cs			\
	ArrayParameter.cs		\
	BoxedGen.cs			\
	ByRefGen.cs			\
	CallbackGen.cs			\
	ChildProperty.cs		\
	ClassBase.cs			\
	ClassField.cs			\
	ClassGen.cs			\
	CodeGenerator.cs		\
	ConstFilenameGen.cs		\
	ConstStringGen.cs		\
	Constant.cs			\
	Ctor.cs				\
	DefaultSignalHandler.cs		\
	EnumGen.cs			\
	FieldBase.cs			\
	GenBase.cs			\
	GenerationInfo.cs		\
	GObjectVM.cs			\
	HandleBase.cs			\
	IAccessor.cs			\
	IGeneratable.cs			\
	IManualMarshaler.cs		\
	InterfaceGen.cs			\
	InterfaceVM.cs			\
	IOwnable.cs			\
	LogWriter.cs			\
	LPGen.cs			\
	LPUGen.cs			\
	ManagedCallString.cs		\
	ManualGen.cs			\
	MarshalGen.cs			\
	MethodBase.cs			\
	MethodBody.cs			\
	Method.cs			\
	NativeStructGen.cs		\
	ObjectField.cs			\
	ObjectBase.cs			\
	ObjectGen.cs			\
	OpaqueGen.cs			\
	Options.cs			\
	OwnableGen.cs			\
	Parameter.cs			\
	Parameters.cs			\
	Parser.cs			\
	Property.cs			\
	PropertyBase.cs			\
	ReturnValue.cs			\
	Signal.cs			\
	Signature.cs			\
	SimpleBase.cs			\
	SimpleGen.cs			\
	Statistics.cs			\
	StructBase.cs			\
	StructField.cs			\
	StructGen.cs			\
	SymbolTable.cs			\
	UnionGen.cs			\
	VirtualMethod.cs		\
	VMSignature.cs			\
	XmlElementExtensions.cs

build_sources = $(addprefix $(srcdir)/, $(sources))
build_fixup_sources = $(addprefix $(srcdir)/, $(fixup_sources))

EXTRA_DIST =					\
	$(sources)				\
	$(fixup_sources)

gapi-fixup.exe: $(build_fixup_sources)
	$(CSC) $(CSFLAGS) -out:gapi-fixup.exe $(srcdir)/gapi-fixup.cs

gapi_codegen.exe: $(build_sources)
	$(CSC) $(CSFLAGS) -out:gapi_codegen.exe $(OFF_T_FLAGS) $(references) $(build_sources)


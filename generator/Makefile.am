assemblydir = $(libdir)/gtk-sharp-2.0
assembly_DATA = gapi_codegen.exe
bin_SCRIPTS = gapi2-codegen
CLEANFILES = gapi_codegen.exe
DISTCLEANFILES = gapi2-codegen

references =

sources = 				\
	AliasGen.cs			\
	BoxedGen.cs			\
	ByRefGen.cs			\
	CallbackGen.cs			\
	ChildProperty.cs		\
	ClassBase.cs			\
	ClassGen.cs			\
	CodeGenerator.cs		\
	ConstStringGen.cs		\
	Ctor.cs				\
	CustomMarshalerGen.cs		\
	EnumGen.cs			\
	Field.cs			\
	GenBase.cs			\
	GenerationInfo.cs		\
	GObjectGen.cs			\
	GStringGen.cs			\
	GUnicharGen.cs			\
	IGeneratable.cs			\
	ImportSignature.cs		\
	InterfaceGen.cs			\
	ManagedCallString.cs		\
	ManualGen.cs			\
	MethodBody.cs			\
	Method.cs			\
	ObjectGen.cs			\
	OpaqueGen.cs			\
	Parameters.cs			\
	Parser.cs			\
	Property.cs			\
	ReturnValue.cs			\
	Signal.cs			\
	SignalHandler.cs		\
	Signature.cs			\
	SimpleGen.cs			\
	SizeTGen.cs			\
	SSizeTGen.cs			\
	Statistics.cs			\
	StringGen.cs			\
	StructBase.cs			\
	StructGen.cs			\
	SymbolTable.cs			\
	TimeTGen.cs			\
	VirtualMethod.cs		\
	VMSignature.cs

build_sources = $(addprefix $(srcdir)/, $(sources))
dist_sources = $(sources)

EXTRA_DIST =					\
	makefile.win32				\
	$(dist_sources)

gapi_codegen.exe: $(build_sources)
	$(CSC) /out:gapi_codegen.exe $(references) $(build_sources)


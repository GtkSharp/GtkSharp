MCS=mcs
SOURCES=BoxedGen.cs \
	CallbackGen.cs \
	CodeGenerator.cs \
	EnumGen.cs \
	IGeneratable.cs \
	InterfaceGen.cs \
	ObjectGen.cs \
	Parser.cs \
	SignalHandler.cs \
	Statistics.cs \
	StructBase.cs \
	StructGen.cs \
	SymbolTable.cs

all: linux

windows: *.cs
	$(CSC) /unsafe /out:codegen.exe *.cs
	./codegen gtkapi.xml

linux: generated-stamp 

generated-stamp: codegen.exe gtkapi.xml
	mono ./codegen.exe gtkapi.xml && touch generated-stamp

unix:
	@echo "'make unix' is broken for now."

install: all
	@echo "Nothing to install in generator."

codegen.exe: *.cs
	$(MCS) --unsafe -o codegen.exe -r System.Xml.dll *.cs

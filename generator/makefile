MCS=mcs

all:
	@echo "You must use 'make windows' or 'make linux'."
	@echo "'make unix' is broken for now."

windows:
	$(CSC) /unsafe /out:codegen.exe *.cs
	./codegen gtkapi.xml

linux:
	$(MCS) --unsafe -o codegen.exe -r System.Xml.dll *.cs
	mono ./codegen.exe gtkapi.xml

unix:
	@echo "'make unix' is broken for now."

MCS=mcs
RUNTIME=mono

all: linux

windows: *.cs
	$(CSC) /unsafe /out:codegen.exe *.cs
	./codegen gtkapi.xml

linux: generated-stamp 

generated-stamp: codegen.exe gtkapi.xml
	$(RUNTIME) ./codegen.exe gtkapi.xml && touch generated-stamp

clean:
	rm -f *.exe

install: all
	@echo "Nothing to install in generator."

codegen.exe: *.cs
	$(MCS) --unsafe -o codegen.exe -r System.Xml *.cs

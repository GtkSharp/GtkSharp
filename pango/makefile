MCS=mcs

all:
	@echo "You must use 'make windows' or 'make unix'."
	@echo "'make unix' is broken for now."

windows:
	$(CSC) /unsafe /target:library /r:../glib/glib-sharp.dll /out:pango-sharp.dll /recurse:*.cs

linux:
	$(MCS) --unsafe --target library -r ../glib/glib-sharp.dll -o pango-sharp.dll --recurse *.cs

unix:
	@echo "'make unix' is broken for now."

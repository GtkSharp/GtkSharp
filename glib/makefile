MCS=mcs

all:
	@echo "You must use 'make windows' or 'make unix'."
	@echo "'make unix' is broken for now."

windows:
	$(CSC) /unsafe /target:library /out:glib-sharp.dll /recurse:*.cs

linux:
	$(MCS) --unsafe --target library -o glib-sharp.dll --recurse *.cs

unix:
	@echo "'make unix' is broken for now."

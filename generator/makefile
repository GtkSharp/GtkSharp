all:
	@echo "You must use 'make windows' or 'make unix'."
	@echo "'make unix' is broken for now."

windows:
	$(CSC) /unsafe /out:codegen.exe /recurse:*.cs
	./codegen api.xml

unix:
	@echo "'make unix' is broken for now."


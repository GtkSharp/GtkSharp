all:
	@echo "You must use 'make windows' or 'make unix'."
	@echo "'make unix' is broken for now."

windows:
	cat gdk-types.defs hardcoded.defs gtk-types.defs gtk-props.defs gtk.defs | ./defs-parse.pl

unix:
	@echo "'make unix' is broken for now."


all:
	@echo "You must use 'make windows' or 'make unix'."
	@echo "'make unix' is broken for now."

windows:
	cat pango-types.defs pango-structs.defs pango.defs gdk-types.defs gdk-structs.defs hardcoded.defs gtk-types.defs gtk-structs.defs gtk-signals.defs gtk-props.defs gtk.defs | ./defs-parse.pl

unix:
	@echo "'make unix' is broken for now."


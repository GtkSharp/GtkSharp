SUBDIRS = sources generator parser glib pango atk gdk gtk glade art gnomevfs gnome gtkhtml gconf rsvg vte gtkdotnet sample doc

EXTRA_DIST = 			\
	gtk-sharp.snk		\
	gapi-cdecl-insert	\
	makefile.win32		\
	policy.config.in	\
	AssemblyInfo.cs.in	\
	ChangeLog		\
	HACKING			\
	README			\
	README.generator

#DISTCLEANFILES = AssemblyInfo.cs

configure.in: bootstrap.status configure.in.in 
	$(SHELL) $<

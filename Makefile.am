SUBDIRS = sources generator parser glib gio cairo pango atk gdk gtk glade gtkdotnet sample doc

EXTRA_DIST = 			\
	mono.snk		\
	gtk-sharp.snk		\
	gapi-cdecl-insert	\
	makefile.win32		\
	policy.config.in	\
	AssemblyInfo.cs.in	\
	ChangeLog		\
	HACKING			\
	README			\
	README.generator

configure.in: bootstrap.status configure.in.in 
	$(SHELL) $<

win32-installer: all 
	candle.exe gtk-sharp-2.0-lib.wxs
	light.exe gtk-sharp-2.0-lib.wixobj
	candle.exe gtk-sharp-2.0-dev.wxs
	light.exe gtk-sharp-2.0-dev.wixobj

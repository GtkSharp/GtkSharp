SUBDIRS = sources generator parser glib gio cairo pango atk gdk gtk gtkdotnet doc

EXTRA_DIST = 			\
	gtk-sharp.snk		\
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

assembly_dirs = glib gio pango atk gdk gtk glade gtkdotnet

cross-bundle: all
	mkdir -p gtk-sharp-cross-$(VERSION)
	rm -rf gtk-sharp-cross-$(VERSION)/*
	cp *.wxs gtk-sharp-cross-$(VERSION)
	cp generator/gapi_codegen.exe gtk-sharp-cross-$(VERSION)
	cp */glue/.libs/*.dll gtk-sharp-cross-$(VERSION)
	@for a in $(assembly_dirs); do \
		mkdir -p gtk-sharp-cross-$(VERSION)/$$a; \
		cp $$a/*.dll gtk-sharp-cross-$(VERSION)/$$a; \
	done
	cp sample/GtkDemo/GtkDemo.exe gtk-sharp-cross-$(VERSION)
	zip -9r gtk-sharp-cross-$(VERSION).zip gtk-sharp-cross-$(VERSION)
	rm -rf gtk-sharp-cross-$(VERSION)


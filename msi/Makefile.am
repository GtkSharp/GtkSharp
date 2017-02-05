SUBDIRS = unmanaged .

assembly_dirs = glib gio pango atk gdk gtk gtkdotnet cairo

if ENABLE_MSI
TARGET=gtk-sharp-3.0.msi
else
TARGET=
endif

noinst_DATA = $(TARGET)

gtk-sharp-3.0.msi: gtk-sharp-3.0.wxs
	mkdir -p binaries
	rm -rf binaries/*
	cp $(top_builddir)/generator/gapi_codegen.exe binaries
	cp $(top_builddir)/*/glue/.libs/*.dll binaries
	for a in $(assembly_dirs); do \
		mkdir -p binaries/$$a; \
		cp $(top_builddir)/$$a/*.dll binaries/$$a; \
		cp $(top_builddir)/$$a/*.pdb binaries/$$a; \
	done
	mv binaries/libatksharpglue-3.dll   binaries/atksharpglue-3.dll
	mv binaries/libgiosharpglue-3.dll   binaries/giosharpglue-3.dll
	mv binaries/libgtksharpglue-3.dll   binaries/gtksharpglue-3.dll
	mv binaries/libpangosharpglue-3.dll binaries/pangosharpglue-3.dll

	cp $(top_builddir)/sample/GtkDemo/GtkDemo.exe binaries
	$(WIX_CANDLE) -ext WixUIExtension gtk-sharp-3.0.wxs
	$(WIX_LIGHT) -cultures:en-us -ext WixUIExtension -ext WixNetFxExtension -out gtk-sharp-$(VERSION).msi gtk-sharp-3.0.wixobj

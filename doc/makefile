LIBDIR=/usr/lib
BROWSER=../../monodoc/browser/browser.exe
ASSEMBLER=../../monodoc/browser/assembler.exe
GENERATOR=../../monodoc/generator/generator.exe
UPDATER=../../monodoc/generator/updater.exe

all: docs

b:
	mono --debug $(BROWSER)
tree:
	mono $(ASSEMBLER) --ecma en -o documentation

docs: glib pango atk gtk gdk gnome glade

glib:
	mono --debug $(UPDATER) $(LIBDIR)/glib-sharp.dll -o en
pango:
	mono --debug $(UPDATER) $(LIBDIR)/pango-sharp.dll -o en
atk:	
	mono --debug $(UPDATER) $(LIBDIR)/atk-sharp.dll -o en
gtk:
	mono --debug $(UPDATER) $(LIBDIR)/gtk-sharp.dll -o en
gdk:
	mono --debug $(UPDATER) $(LIBDIR)/gdk-sharp.dll -o en
gnome:
	mono --debug $(UPDATER) $(LIBDIR)/gnome-sharp.dll -o en
glade:	
	mono --debug $(UPDATER) $(LIBDIR)/glade-sharp.dll -o en

clean:
	rm *.tree
	rm *zip

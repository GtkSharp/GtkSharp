LIBDIR=/usr/lib
BROWSER=../../monodoc/browser/browser.exe
ASSEMBLER=../../monodoc/browser/assembler.exe
GENERATOR=../../monodoc/generator/generator.exe

all: tree

b:
	mono --debug $(BROWSER)
tree:
	mono $(ASSEMBLER) --ecma en/ -o documentation

docs: glib pango atk gtk gdk gnome glade

glib:
	mono --debug $(GENERATOR) $(LIBDIR)/glib-sharp.dll -o en
pango:
	mono --debug $(GENERATOR) $(LIBDIR)/pango-sharp.dll -o en
atk:	
	mono --debug $(GENERATOR) $(LIBDIR)/atk-sharp.dll -o en
gtk:
	mono --debug $(GENERATOR) $(LIBDIR)/gtk-sharp.dll -o en
gdk:
	mono --debug $(GENERATOR) $(LIBDIR)/gdk-sharp.dll -o en
gnome:
	mono --debug $(GENERATOR) $(LIBDIR)/gnome-sharp.dll -o en
glade:	
	mono --debug $(GENERATOR) $(LIBDIR)/glade-sharp.dll -o en

clean:
	rm *.tree
	rm *zip

LIBDIR=/usr/lib
BROWSER=../../monodoc/browser/browser.exe
ASSEMBLER=../../monodoc/browser/assembler.exe
GENERATOR=../../monodoc/generator/generator.exe

all: docs tree

b:
	mono --debug $(BROWSER)
tree:
	mono $(ASSEMBLER) --ecma en/ -o documentation

docs: glib pango atk gtk gdk gnome glade

glib:
	mono --debug $(GENERATOR) $(LIBDIR)/glib-sharp.dll -o en -f
pango:
	mono --debug $(GENERATOR) $(LIBDIR)/pango-sharp.dll -o en -f
atk:	
	mono --debug $(GENERATOR) $(LIBDIR)/atk-sharp.dll -o en -f
gtk:
	mono --debug $(GENERATOR) $(LIBDIR)/gtk-sharp.dll -o en -f
gdk:	
	mono --debug $(GENERATOR) $(LIBDIR)/gdk-sharp.dll -o en -f
gnome:
	mono --debug $(GENERATOR) $(LIBDIR)/gnome-sharp.dll -o en -f
glade:	
	mono --debug $(GENERATOR) $(LIBDIR)/glade-sharp.dll -o en -f

clean:
	rm *.tree
	rm *zip

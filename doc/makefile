prefix=/usr
BROWSER=../../monodoc/browser/browser.exe
ASSEMBLER=../../monodoc/browser/assembler.exe
UPDATER=../../monodoc/generator/updater.exe
DIRS=glib pango atk gdk gtk glade art gnome gda gnomedb gconf/GConf gconf/GConf.PropertyEditors rsvg gtkhtml

all: gtk-sharp-docs.zip gtk-sharp-docs.tree

b:
	mono --debug $(BROWSER)

gtk-sharp-docs.zip gtk-sharp-docs.tree: en/*/*.xml en/*.xml
	mono $(ASSEMBLER) --ecma en -o gtk-sharp-docs

update:
	for i in $(DIRS); do                    		\
		mono --debug $(UPDATER) ../$$i/*.dll -o ./en -f || exit 1;	\
        done

install: gtk-sharp-docs.zip gtk-sharp-docs.tree 
	install -m 644 gtk-sharp-docs.zip gtk-sharp-docs.tree `monodoc --get-sourcesdir`
	install -m 644 gtk-sharp-docs.source `monodoc --get-sourcesdir`

clean:
	rm -f *.tree
	rm -f *zip

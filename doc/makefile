prefix=/usr
BROWSER=../../monodoc/browser/browser.exe
ASSEMBLER=../../monodoc/browser/assembler.exe
GENERATOR=../../monodoc/generator/generator.exe
UPDATER=../../monodoc/generator/updater.exe

all: gtk-sharp-docs.zip gtk-sharp-docs.tree

b:
	mono --debug $(BROWSER)

gtk-sharp-docs.zip gtk-sharp-docs.tree: en/*/*.xml
	mono $(ASSEMBLER) --ecma en -o gtk-sharp-docs

install: gtk-sharp-docs.zip gtk-sharp-docs.tree 
	install -m 644 gtk-sharp-docs.zip gtk-sharp-docs.tree `monodoc --get-sourcesdir`
	install -m 644 gtk-sharp-docs.source `monodoc --get-sourcesdir`

clean:
	rm *.tree
	rm *zip

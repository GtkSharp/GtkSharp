prefix=/usr
BROWSER=../../monodoc/browser/browser.exe
ASSEMBLER=../../monodoc/browser/assembler.exe
GENERATOR=../../monodoc/generator/generator.exe
UPDATER=../../monodoc/generator/updater.exe

all: gtk-sharp-docs.zip gtk-sharp-docs.tree

b:
	mono --debug $(BROWSER)

gtk-sharp-docs.zip gtk-sharp-docs.tree:
	mono $(ASSEMBLER) --ecma en -o gtk-sharp-docs

install: gtk-sharp-docs.zip gtk-sharp-docs.tree 
	cp gtk-sharp-docs.zip gtk-sharp-docs.tree $(prefix)/lib/monodoc/sources
	cp gtk-sharp-docs.source $(prefix)/lib/monodoc/sources
clean:
	rm *.tree
	rm *zip

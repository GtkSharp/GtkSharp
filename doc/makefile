BROWSER=../../monodoc/browser/browser.exe
ASSEMBLER=../../monodoc/browser/assembler.exe
GENERATOR=generator.exe

all: tree

b:
	mono --debug $(BROWSER)

tree:
	mono $(ASSEMBLER) --ecma en/ -o documentation
clean:
	rm *.tree
	rm *zip

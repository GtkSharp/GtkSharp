all: generated-stamp

generated-stamp: formatXml gtkapi.xml
	./formatXml gtkapi.xml ../generator/gtkapi.xml && touch generated-stamp

formatXml: formatXml.c
	cc -o formatXml formatXml.c `pkg-config --cflags --libs libxml-2.0 glib-2.0`

gtkapi.xml: build.pl gapi_pp.pl gapi2xml.pl
	./build.pl

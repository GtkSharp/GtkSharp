SCRIPTS = \
	gapi.pl \
	gapi_pp.pl \
	gapi2xml.pl

MODULES=GAPI/Metadata.pm

all: $(SCRIPTS) $(MODULES) gapi_format_xml

gapi_format_xml: formatXml.c
	cc -o gapi_format_xml formatXml.c `pkg-config --cflags --libs libxml-2.0 glib-2.0`

clean:
	rm -f gapi_format_xml

prefix=/usr

install: all
	for i in $(SCRIPTS) gapi_format_xml; do \
		cp $$i $(DESTDIR)$(prefix)/bin; \
	done
	for i in $(MODULES); do \
		../mkinstalldirs $(DESTDIR)$(prefix)/share/perl5/GAPI && \
		cp $$i $(DESTDIR)$(prefix)/share/perl5/GAPI; \
	done
	

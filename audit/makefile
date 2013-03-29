MCS=dmcs

COMMON_SOURCES = \
	AssemblyResolver.cs	\
	Util.cs	\
	WellFormedXmlWriter.cs

APIINFO_SOURCES = mono-api-info.cs $(COMMON_SOURCES)

all: extract-missing.exe mono-api-info.exe mono-api-diff.exe gen-apidiff-html.exe

check: all
	rm -rf curr diff
	./get-apiinfo.pl .. curr
	./get-apidiff.pl base curr diff
	./get-missing.pl diff

mono-api-diff.exe: mono-api-diff.cs
	$(MCS) mono-api-diff.cs

mono-api-info.exe: $(APIINFO_SOURCES)
	$(MCS) `pkg-config --libs mono-cecil` -out:$@ $^

extract-missing.exe: extract-missing.cs
	$(MCS) extract-missing.cs

gen-apidiff-html.exe: gen-apidiff-html.cs
	$(MCS) -out:$@ $^

clean:
	rm -f *.exe
	rm -rf curr
	rm -rf diff


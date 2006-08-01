MCS=mcs

all: extract-missing.exe mono-api-info.exe mono-api-diff.exe

check: all
	rm -rf curr diff
	./get-apiinfo.pl .. curr
	./get-apidiff.pl base curr diff
	./get-missing.pl diff

mono-api-diff.exe: mono-api-diff.cs
	$(MCS) mono-api-diff.cs

mono-api-info.exe: mono-api-info.cs
	$(MCS) mono-api-info.cs

extract-missing.exe: extract-missing.cs
	$(MCS) extract-missing.cs

clean:
	rm -f *.exe
	rm -rf curr
	rm -rf diff


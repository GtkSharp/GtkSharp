if ENABLE_MSI
TARGET=unmanaged.msm
else
TARGET=
endif

noinst_DATA = $(TARGET)

DOWNLOADS = downloads.win32

download-stamp: $(DOWNLOADS)
	rm -rf source
	mkdir -p source
	cd source && for i in `cat ../$(DOWNLOADS)`; do	wget --no-check-certificate $$i; done && for j in `ls *.zip`; do unzip $$j; rm $$j; done
	touch download-stamp

unmanaged.wixobj: unmanaged.wxs redirector.exe download-stamp
	candle unmanaged.wxs

unmanaged.msm: unmanaged.wixobj
	light unmanaged.wixobj

bundle-scanner.exe: bundle-scanner.cs
	$(CSC) bundle-scanner.cs

redirector.exe: redirector.cs
	$(CSC) redirector.cs

scan: download-stamp bundle-scanner.exe
	$(RUNTIME) bundle-scanner.exe --wix=unmanaged.wxs --bundle=source

CLEANFILES = source
EXTRA_DIST = redirector.cs unmanaged.wxs downloads.win32 ignores

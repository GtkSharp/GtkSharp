CSC  =	mcs

DLLS =	-r glib-sharp.dll     \
	-r gtk-sharp.dll      \
	-r gdk-sharp.dll      \
	-r glib-sharp.dll     \
	-r System.Drawing.dll

all:
	$(CSC) /unsafe $(DLLS) packing.cs
clean: 
	rm -f *.exe

CSC  =	mcs

DLLS =	-r glib-sharp.dll     \
	-r gtk-sharp.dll      \
	-r gdk-sharp.dll      \
	-r System.Drawing.dll

all:
	$(CSC) /unsafe $(DLLS) eventbox.cs
clean: 
	rm -f *.exe

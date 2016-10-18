# Generating Sources

## Overview

This is a quick overview of some of the commands to run when updating the sources for a new gtk version

### Linux

You may need to install the following package for generating the source under Linux
```
sudo apt-get install libxml-libxml-perl
```

### Windows

So far I've only managed to get this working in ubuntu, not Windows due to the way the .net app launches the perl script via libc
i.e. [DllImport ("libc")]

It looks like we need to use the 32bit version of MinGW if we do try it this way.
The following path statements are needed in the console at the very least
```
PATH=$PATH:/c/Program\ Files\ \(x86\)/Microsoft\ SDKs/Windows/v10.0A/bin/NETFX\ 4.6\ Tools/
PATH=$PATH:/c/Windows/Microsoft.NET/Framework/v4.0.30319/
```

Also the parser can be rebuilt via
```
./autogen.sh --prefix=/tmp/install
cd parser
make clean
make
```

Also it's important to make sure MSYS2 is uptodate with
```
pacman -Syuu
```

To search for a package that's been install (to see what version it is for example)
```
pacman -Ss gtk3
```


## Editing Files for Downloaded Source

### Configure.ac version number

First change the version number in configure.ac to match that of the gtk version we're moving to
```
AC_INIT(gtk-sharp, 3.22.1)
```

### Sources/Makefile.am

Next change the version number in sources/Makefile.am to match
```
TARGET_GTK_VERSION=3.22.1
TARGET_GTK_API=3.22
```

Next update the orher url's in Makefile.am, the version numbers should match those in use on the system (such as MSYS2)
```
GTK_DOWNLOADS = \
	http://ftp.gnome.org/pub/GNOME/sources/glib/2.50/glib-2.50.0.tar.xz 			\
	http://ftp.gnome.org/pub/GNOME/sources/pango/1.40/pango-1.40.3.tar.xz			\
	http://ftp.gnome.org/pub/GNOME/sources/atk/2.22/atk-2.22.0.tar.xz			\
	http://ftp.gnome.org/pub/GNOME/sources/gdk-pixbuf/2.36/gdk-pixbuf-2.36.0.tar.xz	\
	http://ftp.gnome.org/pub/GNOME/sources/gtk+/$(TARGET_GTK_API)/gtk+-$(TARGET_GTK_VERSION).tar.xz
```

### Patches

As part of the source code download, some of the files will be patched
so you need to look at and check that all the patches apply correctly to the downloaded source when running make get-source-code


## Download the sources

Next we're going to download the source
```
./autogen.sh --prefix=/tmp/install
cd sources
make get-source-code
```

At this stage the sources should now be extracted within the sources sub directory

### Update sources.xml

One last file to update is the sources/sources.xml file
all directories in this file need to match the extracted directories

## Generate the API Code

### Generate the XML Files

Next to generate the xml files needed for the generation of code
```
make api
```

This should result in the following files

  * gdk/gdk-api.raw
  * gio/gio-api.raw
  * gtk/gtk-api.raw
  * pango/pango-api.raw

### Generate the API Code from the XML Files

TODO we need to use generator/gapi_codegen.exe on each of the xml files to generate the .cs code within the generated sub directories

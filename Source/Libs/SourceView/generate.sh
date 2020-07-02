#!/usr/bin/bash

wget http://ftp.acc.umu.se/pub/GNOME/sources/gtksourceview/4.0/gtksourceview-4.0.0.tar.xz
tar xf gtksourceview-4.0.0.tar.xz

gapi3-parser SourceView.source
gapi3-fixup --api=SourceView-api.xml --metadata=SourceView.metadata
gapi3-codegen --outdir=Generated `pkg-config --cflags gtk-sharp-3.0` --generate SourceView-api.xml
dotnet build

rm gtksourceview-4.0.0.tar.xz
rm -rf gtksourceview-4.0.0
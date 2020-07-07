#!/usr/bin/env bash
outdir=Generated
sourcever=4.0.0

clear
wget http://ftp.acc.umu.se/pub/GNOME/sources/gtksourceview/4.0/gtksourceview-$sourcever.tar.xz
tar xf gtksourceview-$sourcever.tar.xz

if [ -d $outdir ];
then
    rm -rf $outdir
fi

../../OldStuff/parser/gapi3-parser SourceView.source

dotnet ../../../BuildOutput/Tools/GapiFixup.dll --api=SourceView-api.xml --metadata=SourceView.metadata
dotnet ../../../BuildOutput/Tools/GapiCodegen.dll --outdir=$outdir --assembly-name=SourceView `pkg-config --cflags gtk-sharp-3.0` --generate SourceView-api.xml
dotnet build -v m

rm gtksourceview-$sourcever.tar.xz
rm -rf gtksourceview-$sourcever

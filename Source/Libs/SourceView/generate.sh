#!/usr/bin/env bash
outdir=Generated
sourcever=4.0.0

wget http://ftp.acc.umu.se/pub/GNOME/sources/gtksourceview/4.0/gtksourceview-$sourcever.tar.xz
tar xf gtksourceview-$sourcever.tar.xz

if [ -d $outdir ];
then
    rm -rf $outdir
fi

../../OldStuff/parser/gapi3-parser SourceView.source

rm gtksourceview-$sourcever.tar.xz
rm -rf gtksourceview-$sourcever

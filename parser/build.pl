#!/usr/bin/perl -w

$file = "../generator/gtkapi.xml";

unlink ($file);

%srcs = ( "atk-1.0.0/atk" => "Atk:atk-1.0",
	  "pango-1.0.0/pango" => "Pango:pango-1.0",
	  "gtk+-2.0.0/gdk" => "Gdk:gdk-x11-2.0",
	  "gtk+-2.0.0/gdk-pixbuf" => "Gdk:gdk_pixbuf-2.0",
	  "gtk+-2.0.0/gtk" => "Gtk:gtk-x11-2.0");

foreach $dir (keys %srcs) {
	($ns, $lib) = split (/:/, $srcs{$dir});
	system ("./gapi_pp.pl $dir | ./gapi2xml.pl $ns $file $lib");
}


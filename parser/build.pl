#!/usr/bin/perl -w

$file = "gtkapi.xml";

unlink ($file);

%srcs = ( "atk-1.0.2/atk" => "Atk:atk-1.0",
	  "pango-1.0.3/pango" => "Pango:pango-1.0",
	  "gtk+-2.0.5/gdk" => "Gdk:gdk-x11-2.0",
	  "gtk+-2.0.5/gdk-pixbuf" => "Gdk:gdk_pixbuf-2.0",
	  "gtk+-2.0.5/gtk" => "Gtk:gtk-x11-2.0",
	  "libgnome-2.0.1/libgnome" => "Gnome:gnome-2",
	  "libgnomecanvas-2.0.1/libgnomecanvas" => "Gnome:gnomecanvas-2",
	   "libgnomeui-2.0.1/libgnomeui" => "Gnome:gnomeui-2",
	   "gtkhtml/src" => "Gtk:gtkhtml-3.0");

foreach $dir (keys %srcs) {
	($ns, $lib) = split (/:/, $srcs{$dir});
	system ("./gapi_pp.pl $dir | ./gapi2xml.pl $ns $file $lib");
}


#!/usr/bin/perl -w

$file = "../generator/gtkapi.xml";

unlink ($file);

%ns = ( "Atk"   => "atk-1.0.2/atk",
	"Pango" => "pango-1.0.2/pango",
	"Gdk"   => "gtk+-2.0.3/gdk",
	"Gtk"   => "gtk+-2.0.3/gtk");

foreach $key (keys %ns) {
	$dir = $ns{$key};
	system ("./gapi_pp.pl $dir | ./gapi2xml.pl $key $file");
}


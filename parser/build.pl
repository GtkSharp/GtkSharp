#!/usr/bin/perl -w

$file = "../generator/gtkapi.xml";

unlink ($file);

%ns = ( "Atk"   => "atk-1.0.2/atk",
	"Pango" => "pango-1.0.2/pango",
	"Gdk"   => "gtk+-2.0.3/gdk",
	"Gdk.Imaging" => "gtk+-2.0.3/gdk-pixbuf",
	"Gtk"   => "gtk+-2.0.3/gtk");

%c_ns = ( "Gdk.Imaging" => "Gdk");

foreach $key (keys %ns) {
	$dir = $ns{$key};
	if (not ($c_key = $c_ns{$key})) {
		$c_key = $key;
	}
	system ("./gapi_pp.pl $dir | ./gapi2xml.pl $c_key $file --out-ns $key");
}


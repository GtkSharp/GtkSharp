#!/usr/bin/perl
#
# mapdllnames.pl : remaps the DllImport libnames for a specified source dir.
#
# Author: Mike Kestner <mkestner@speakeasy.net>
#
# <c> 2002 Mike Kestner
#############################################################################

%map = (
	'glib-2.0', "libglib-2.0-0.dll",
	'gobject-2.0', "libgobject-2.0-0.dll",
	'pango-1.0', "libpango-1.0-0.dll",
	'atk-1.0', "libatk-1.0-0.dll",
	'gdk-x11-2.0', "libgdk-win32-2.0-0.dll",
	'gdk-pixbuf-2.0', "libgdk_pixbuf-2.0-0.dll",
	'gtk-x11-2.0', "libgtk-win32-2.0-0.dll"
);

foreach $filename (@ARGV) {

	chomp($filename);
	open(INFILE, $filename) || die "Couldn't open $filename\n";
	open(OUTFILE, ">$filename.tmp") || die "Couldn't open $filename.tmp\n";

	while ($line = <INFILE>) {
		if ($line =~ /DllImport\(\"(.*)\"/ && exists($map{$1})) {
			$line =~ s/\"(.*)\"/\"$map{$1}\"/;
		}

		print OUTFILE $line;
	}
	close(INFILE);
	close(OUTFILE);
	`mv $filename.tmp $filename`;
}
		

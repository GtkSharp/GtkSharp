#!/usr/bin/perl -w
#
# gapi.pl - frontend script for old style non-xml sources files.
#
# Author: Mike Kestner <mkestner@speakeasy.net>
#
# Copyright (c) 2001-2003 Mike Kestner
#
# This program is free software; you can redistribute it and/or
# modify it under the terms of version 2 of the GNU General Public
# License as published by the Free Software Foundation.
#
# This program is distributed in the hope that it will be useful,
# but WITHOUT ANY WARRANTY; without even the implied warranty of
# MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
# General Public License for more details.
#
# You should have received a copy of the GNU General Public
# License along with this program; if not, write to the
# Free Software Foundation, Inc., 59 Temple Place - Suite 330,
# Boston, MA 02111-1307, USA.

if ($ARGV[0] && $ARGV[1]) {
	open INFILE, $ARGV[0];
	$outdir = $ARGV[1];
} else {
	print "Usage: gapi.pl <sources_file> <out_dir>\n";
	exit 0;
}

@srcs = ();
%files = ();

while (<INFILE>)
{
	chomp;
	my @entry = split /\s+/;
	push @srcs, \@entry;
	
	$ns = $entry[1];
	if (not $files{$ns}) {
		$files{$ns} = lc ($ns) . "-api.xml";
	}
}

foreach $ns (keys (%files)) {
	unlink $files{$ns};
}

foreach $entry (@srcs) {
	($dir, $ns, $lib) = @$entry;
	print "hi $ns\n";
	$file = $files{$ns};
	system ("gapi_pp.pl $dir | gapi2xml.pl $ns $file $lib");
}

foreach $ns (keys (%files)) {
	$file = $files{$ns};
	system ("gapi_format_xml $file $outdir/$file");
	unlink $file;
}


#!/usr/bin/perl -w
# gapi-parser - parser frontend for XML-based sources file format.
#
# Author:  Mike Kestner  <mkestner@speakeasy.net>
#
# Copyright (c) 2003  Mike Kestner
# Copyright (c) 2003  Novell, Inc.
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

use XML::LibXML;

die "Usage: gapi-parser <xml_sources_file>\n" if (!$ARGV[0]);

my $parser = new XML::LibXML;
my $doc = $parser->parse_file($ARGV[0]);
die "Unable to parse input file $ARGV[0].\n" if (!$doc);
my $root = $doc->documentElement;
die "Improperly formatted input file $ARGV[0].\n" if (!$root || $root->nodeName ne "gapi-parser-input");

for ($apinode = $root->firstChild; $apinode; $apinode = $apinode->nextSibling ()) {
	next if ($apinode->nodeName ne "api");
	@attrs = $apinode->attributes;
	my $outfile = "";
	foreach $attr (@attrs) {
		if ($attr->name eq "filename") {
			$outfile = $attr->value;
		} else {
			die "Unexpected attribute $attr->name\n";
		}
	}

	unlink "$outfile.pre";

	for ($libnode = $apinode->firstChild; $libnode; $libnode = $libnode->nextSibling ()) {
		next if ($libnode->nodeName ne "library");
		@attrs = $libnode->attributes;
		my ($lib);
		foreach $attr (@attrs) {
			if ($attr->name eq "name") {
				$lib = $attr->value;
			} else {
				die "Unexpected attribute $attr->name\n";
			}
		}

		for ($nsnode = $libnode->firstChild; $nsnode; $nsnode = $nsnode->nextSibling ()) {
			next if ($nsnode->nodeName ne "namespace");
			@attrs = $nsnode->attributes;
			my ($ns);
			foreach $attr (@attrs) {
				if ($attr->name eq "name") {
					$ns = $attr->value;
				} else {
					die "Unexpected attribute $attr->name\n";
				}
			}

			my @files = ();
			my @realfiles = ();
			my %excludes = ();
			for ($srcnode = $nsnode->firstChild; $srcnode; $srcnode = $srcnode->nextSibling ()) {
				next if ($srcnode->nodeName ne "dir" && $srcnode->nodeName ne "file" && $srcnode->nodeName ne "exclude");

				if ($srcnode->nodeName eq "dir") {
					my ($dir);
					$dir = $srcnode->firstChild->nodeValue;
					print "<dir $dir> ";
					@files = (@files, `ls $dir/*.c`);
					@files = (@files, `ls $dir/*.h`);
				} elsif ($srcnode->nodeName eq "file") {
					$incfile = $srcnode->firstChild->nodeValue;
					print "<file $incfile> ";
					@files = (@files, $srcnode->firstChild->nodeValue);
				} elsif ($srcnode->nodeName eq "exclude") {
					$excfile = $srcnode->firstChild->nodeValue;
					print "<exclude $excfile> ";
					$excludes{$srcnode->firstChild->nodeValue} = 1;
				} else {
					die "Unexpected source $srcnode->nodeName \n";
				}

			}
			print "\n";
			if ($#files >= 0) {
				foreach $file (@files) {
					chomp ($file);
					@realfiles = (@realfiles, $file) if (!exists($excludes{$file}));
				}
					
				$pp_args = join (" ", @realfiles);
				system ("gapi_pp.pl $pp_args | gapi2xml.pl $ns $outfile.pre $lib");
			}
		}
	}

	system ("gapi_format_xml $outfile.pre $outfile");
	unlink "$outfile.pre";
}


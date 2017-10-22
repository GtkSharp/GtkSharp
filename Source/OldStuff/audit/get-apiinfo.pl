#!/usr/bin/perl
#
# get-apiinfo.pl : gets apiinfo files for the assemblies in a directory tree.
#
# Authors: Mike Kestner <mkestner@novell.com>
#
# Copyright (c) 2005 Novell, Inc.
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

die "Usage: get-apiinfo.pl <root_dir> <outdir>" if (@ARGV != 2);

$root = $ARGV[0];
$outdir = $ARGV[1];

$cecildir = `pkg-config --variable=assemblies_dir mono-cecil`;
chomp ($cecildir);

`mkdir -p $outdir`;
`mkdir -p apitmp`;
`cp $root/*/*.dll apitmp`;
print "Getting api info: ";
foreach $assm (`ls apitmp/*.dll`) {
	chomp ($assm);
	$assm =~ /apitmp\/(.*)\.dll/;
	print "*";
	`MONO_PATH=$cecildir mono mono-api-info.exe $assm > $outdir/$1.apiinfo`;
}
`rm -rf apitmp`;
print " Completed\n\n";


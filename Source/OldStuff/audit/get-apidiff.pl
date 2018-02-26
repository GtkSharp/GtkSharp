#!/usr/bin/perl
#
# get-apidiff.pl : gets apidiff files for the apiinfo files in
# 		   base_dir that have corresponding files in revised_dir
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

die "Usage: get-apidiff.pl <base_dir> <revised_dir> <outdir>" if (@ARGV != 3);

$base_dir = $ARGV[0];
$revised_dir = $ARGV[1];
$outdir = $ARGV[2];

`mkdir -p $outdir`;
print "comparing api to base: ";
foreach $baseapi (`ls $base_dir/*.apiinfo`) {
	chomp ($baseapi);
	$baseapi =~ /$base_dir\/(.*)\.apiinfo/;
	if (-e "$revised_dir/$1.apiinfo") {
		print "*";
		`mono mono-api-diff.exe $base_dir/$1.apiinfo $revised_dir/$1.apiinfo  > $outdir/$1.apidiff`;
	} else {
		die "Warning: no $1 apiinfo file found in $revised_dir\n";
	}
}
print " Completed\n\n";


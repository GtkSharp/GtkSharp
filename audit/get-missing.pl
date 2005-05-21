#!/usr/bin/perl
#
# get-missing.pl : scans apidiff files for missing api
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

die "Usage: get-missing.pl <diff_dir>" if (@ARGV != 1);

$diff_dir = $ARGV[0];

foreach $diff (`ls $diff_dir/*.apidiff`) {
	chomp ($diff);
	print "\nchecking for missing members in $diff\n";
	@missing = `mono extract-missing.exe $diff`;
	if (@missing) {
		foreach $miss (@missing) {
			print "  - $miss";
		}
	} else {
		print "  - No missing api found\n";
	}
}
print "\n";

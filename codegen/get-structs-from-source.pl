#!/usr/bin/perl
#
# get-structs-from-source.pl : Extracts define-struct expressions
#
# Author: Mike Kestner (mkestner@speakeasy.net)
#
# <c> 2001 Mike Kestner

while ($line = <STDIN>) {

	if ($line =~ /typedef\s+struct\s+(\w+)\s+\**(\w+);/) {
		$types{$2} = $1;
	} elsif ($line =~ /typedef\s+(\w+)\s+(\w+);/) {
		$types{$2} = $1;
	} elsif ($line =~ /^struct\s+(\w+)/) {
		$sname = $1;
		$sdef = $line;
		while ($line = <STDIN>) {
			$sdef .= $line;
			last if ($line =~ /^}/);
		}
		$sdefs{$sname} = $sdef;
	}
}

foreach $key (sort (keys (%types))) {
	next if (($key =~ /Class$/) || ($key =~ "Private") ||
		 exists($types{$key."Class"}));

	if (exists($sdefs{$key})) {
		$def = $sdefs{$key};
	} else {
		$newkey = $types{$key};
		while ($newkey && !exists($sdefs{$newkey})) {
			$newkey = $types{$newkey};
		}
		warn "$key has no struct def\n" if ($newkey eq "");
		$def = $sdefs{$newkey};
	}

	$key =~ /$ARGV[0](\w+)/;
	print "(define-struct $1\n";
	print "  (in-module \"$ARGV[0]\")\n";
	print "  (c-name \"$key\")\n";
	print "  (fields\n";
	$def =~ s/\s+/ /g;
	$def =~ s/\n\s*//g;
	$def =~ s|/\*.*?\*/||g;
	$def =~ /\{(.+)\}/;
	foreach $mem (split (/;/, $1)) {
		$mem =~ s/\s+(\*+)/\1 /g;
		$mem =~ s/const /const\-/g;
		if ($mem =~ /(\S+\s+\(\*)\s*(.+\))/) {
			$type = $1; $fdesc = $2;
			$type =~ s/\s+\(\*/\*/;
			$fdesc =~ s/^(\w+)\)/\1/;
			print "    '(\"$type\" \"$fdesc\")\n";
		} elsif ($mem =~ /(\S+)\s+(.+)/) {
			$type = $1; $symb = $2;
			foreach $tok (split (/,\s*/, $symb)) {
				print "    '(\"$type\" \"$tok\")\n";
			}
		}
	}
	print "  )\n)\n\n";
}

#!/usr/bin/perl
#
# get-structs-from-source.pl : Extracts define-struct expressions
#
# Author: Mike Kestner (mkestner@speakeasy.net)
#
# <c> 2001 Mike Kestner

while ($line = <STDIN>) {

	if ($line =~ /typedef\s+struct\s+(\w+)\s+(\w+);/) {
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
	next if (($key =~ /Class$/) || exists($types{$key."Class"}));
	$key =~ /$ARGV[0](\w+)/;
	print "(define-struct $1\n";
	print "  (in-module \"$ARGV[0]\")\n";
	print "  (c-name \"$key\")\n";
	print "  (fields\n";
	$sdefs{$types{$key}} =~ s/\n\s*//g;
	$sdefs{$types{$key}} =~ /\{(.+)\}/;
	foreach $mem (split (/;/, $1)) {
		$mem =~ s?/\*.*\*/??;
		$mem =~ s/\s+(\*+)/\1 /;
		$mem =~ s/const /const\-/;
		if ($mem =~ /(\S+)\s+(.+)/) {
			$type = $1; $symb = $2;
			foreach $tok (split (/,\s*/, $symb)) {
				print "    '(\"$type\" \"$tok\")\n";
			}
		}
	}
	print "  )\n)\n\n";
}

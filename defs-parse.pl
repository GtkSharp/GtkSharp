#!/usr/bin/perl

while ($line = <STDIN>) {

	if ($line =~ /^\(define-(enum|flags) (Gtk|G|Gdk)(\w+)/) {
		if (!defined ($namespace)) {
			print "// Generated file: Do not modify\n\n";
			print "namespace $2 {\n\n\tusing System;\n\n";
		} elsif ($2 ne $namespace) {
			print "}\n\nnamespace $2 {\n\n";
		}

		$type = $1;
		$namespace = $2;
		$typename = $3;

		foreach $paren ($line =~ /[\(\)]/g) {
			($paren eq "(") ? $nest++ : $nest--;
		}

		if ($type eq "flags") {
			print "\t[Flags]\n";
		}
		print "\tpublic enum $typename {\n";

		$val = 1;
		while ($nest > 0) {
			$line = <STDIN>;
			if ($line =~ /\((.*) .*\)/) {
				$name = $1;
				$name =~ s/^([a-z])/\u\1/;
				$name =~ s/\-([a-z])/\u\1/g;
				$name =~ s/\-(\d)/\1/g;
				if ($type eq "enum") {
					print "\t\t$name,\n";
				} else {
					print "\t\t$name = $val,\n";
					$val *= 2;
				}
			}

			foreach $paren ($line =~ /[\(\)]/g) {
				($paren eq "(") ? $nest++ : $nest--;
			}
		}

		print "\t}\n\n";
	}

}

if (defined ($namespace)) {
	print "}\n";
}

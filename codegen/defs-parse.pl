#!/usr/bin/perl

while ($line = <STDIN>) {

	if ($line =~ /^\(define-(enum|flags)/) {
		parse_enum_flags ();
	}
}

sub parse_enum_flags ()
{
	$line =~ /^\(define-(enum|flags) (\w+)/;
	$type = $1;
	$typename = $2;

	$line = <STDIN>;
	$line =~ /\(in-module "(\w+)"/;
	$namespace = $1;

	do { $line = <STDIN>; } until ($line =~ /\(values/);

	@enums = ();
	while ($line = <STDIN>) {
		last if ($line =~ /^\s*\)/);

		if ($line =~ /\((.+)\)/) {
			($name, $dontcare, $val) = split (/ /, $1);
			$name =~ s/\"//g;
			$name =~ s/^([a-z])/\u\1/;
			$name =~ s/\-([a-z])/\u\1/g;
			$name =~ s/\-(\d)/\1/g;
			@enums = (@enums, "$name:$val");
		} else {
			die $line;
		}
	}

	$dir = lc ($namespace);
	if (! -e "../$dir") { 
		`mkdir ../$dir`; 
	}

	open (OUTFILE, ">../$dir/$typename.cs") || die "can't open file";
	
	print OUTFILE "// Generated file: Do not modify\n\n";
	print OUTFILE "namespace $namespace {\n\n";
	print OUTFILE "\t/// <summary> $typename Enumeration </summary>\n";
	print OUTFILE "\t/// <remarks>\n\t///\t Valid values:\n";
	foreach $enum (@enums) {
		($name) = split (/:/, $enum);
		print OUTFILE "\t///\t\t$name\n"
	}
	print OUTFILE "\t/// </remarks>\n\n";

	if ($type eq "flags") {
		print OUTFILE "\tusing System;\n\n\t[Flags]\n";
	}
	print OUTFILE "\tpublic enum $typename {\n";

	$flag = 1;
	foreach $enum (@enums) {
		($name, $val) = split (/:/, $enum);
		if ($val) {
			print OUTFILE "\t\t$name = $val,\n";
		} elsif ($type eq "enum") {
			print OUTFILE "\t\t$name,\n";
		} else {
			print OUTFILE "\t\t$name = $flag,\n";
			$flag *= 2;
		}
	}

	print OUTFILE "\t}\n\n}\n";

	close (OUTFILE);
}


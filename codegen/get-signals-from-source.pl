#!/usr/bin/perl

$hdrfile = $srcfile = $ARGV[0];
$hdrfile =~ s/c$/h/;

open (SRCFILE, $srcfile) || die "Could open $srcfile";
@lines = <SRCFILE>;

open (SRCFILE, $hdrfile) || die "Could open $hdrfile";
@hdrlines = <SRCFILE>;

$linenum = 0;

while ($linenum < @lines) {

	$line = $lines[$linenum];

	if ($line =~ /^(\w+)_class_init/) {
		$typename = StudCaps($1);
	} elsif ($line =~ /g_signal_new/) {
		$str = $line;
		do {
			$str .= $lines[++$linenum];
		} until ($lines[$linenum] =~ /;/);

		print_signal ($str, $typename);
	}

	$linenum++;
}

sub print_signal
{
	my ($spec, $class) = @_;
	$spec =~ s/\n\s*//g;

	$spec =~ /\(\"(\w+)\",.*G_SIGNAL_RUN_(\w+).*_OFFSET\s*\((.*)\),/;
	$signame = $1;
	$run = lc($2);
	$class_method = $3;

	($ret, $params) = lookup_method($class_method);

	print "(define-signal $signame\n";
	print "  (of-object \"$class\")\n";
	print "  (return-type \"$ret\")\n";
	print "  (when \"$run\")\n";
	print $params;
	print ")\n\n";
}

sub lookup_method
{
	my ($pstr) = @_;
	my $lineno = 0;
	($classname, $method) = split(/,\s*/, $pstr);

	while ($hdrlines[$lineno] !~ /^struct\s*_$classname/) {$lineno++;}
	
	do {
		if ($hdrlines[$lineno] =~ /$method/) {
			$sig = "";
			while ($hdrlines[$lineno] !~ /;/) {
				$sig .= $hdrlines[$lineno++];
			}
			$sig .= $hdrlines[$lineno];
			$sig =~ s/\n\s*//g;
			$sig =~ /(\S+)\s*\(\* $method\)\s*\((.*)\);/;
			$ret = $1;
			$parms = $2;
			$ret =~ s/void/none/;
			@plist = split(/,/, $parms);
			$parms = "  (parameters\n";
			foreach $parm (@plist) {
				$parm =~ s/\s+\*/\* /;
				$parm =~ s/(\S+)/"$1"/g;
				$parms .= "    '($parm)\n";
			}
			$parms .= "  )\n";
			return ($ret, $parms);
		}
	} until ($hdrlines[$lineno++] =~ /^}/);

	return ();

}

sub StudCaps
{
	my ($str) = @_;

	$str =~ s/^(\w)/\u\1/;
	$str =~ s/[_-]([a-z])/\u\1/g;
	$str =~ s/[_-](\d)/\1/g;
	return $str;
}

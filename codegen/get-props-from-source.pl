#!/usr/bin/perl

open (SRCFILE, $ARGV[0]) || die "Could open $ARGV[0]";

@lines = <SRCFILE>;
$linenum = 0;

while ($linenum < @lines) {

	$line = $lines[$linenum];

	if ($line =~ /^(\w+)_class_init/) {
		$typename = StudCaps($1);
		$fstr = "";
		
		do {
			$fstr .= $lines[$linenum++];
		} until ($lines[$linenum] =~ /^}/);

		parse_init_func ($typename, $fstr);
	}

	$linenum++;
}
		
sub parse_init_func
{
	my ($class, $func) = @_;
	my @init_lines = split (/\n/, $func);

	my $linenum = 0;
	while ($linenum < @init_lines) {

		my $line = $init_lines[$linenum];
			
		while ($linenum < @init_lines) {
			$line = $init_lines[$linenum];
			if ($line =~ /g_object_class_install_prop/) {
				my $prop = $line;
				do {
					$prop .= $init_lines[++$linenum];
				} until ($init_lines[$linenum] =~ /;/);
				print_prop ($prop, $class);
			} elsif ($line =~ /g_signal_new/) {
				my $sig = $line;
				do {
					$sig .= $init_lines[++$linenum];
				} until ($init_lines[$linenum] =~ /;/);
				print_signal ($sig, $class);
			}
			$linenum++;
		}

		$linenum++;
	}
}

sub print_signal
{
	my ($spec, $class) = @_;
	$spec =~ s/\n\s*//g;

	$spec =~ /\((.*)\);/;
	my @args = split (/,\s*/, $1);

	$args[0] =~ /\w+/;
	my $name = $&;

	my $ret = $args[8];
	if ($ret =~ /G_TYPE_(\w+)/) {
		$ret = lc ($1);
	}

	my $param_cnt = $args[9];

	my $pstr = "\t<signal name=\"$name\">\n";
	$pstr .= "\t\t<return> $ret </return>\n";
	if ($param_cnt) {
		$pstr .= "\t\t<parameters>\n";
		for ($i=0; $i < $param_cnt; $i++) {
			if ($args[$i+10] =~ /G_TYPE_(\w+)/) {
				$args[$i+10] = lc ($1);
			}
			$pstr .= "\t\t\t<param> $args[$i+10] </param>\n";
		}
		$pstr .= "\t\t</parameters>\n";
	}
	$pstr .= "\t</signal>\n\n";

	$signals{$name} = $pstr;
}

sub print_prop
{
	my ($spec, $class) = @_;

	$spec =~ /g_param_spec_(\w+)\s*\((.*)/;
	$type = $1;
	$params = $2;

	if ($type =~ /boolean|^u*int|pointer/) {
		$params =~ /\"(.+)\",.+\".+\".+\"(.+)\".*(,\s*G_PARAM_\w+.*)\)\s*\)/;
		$name = $1; $docs = $2; $mode = $3;
		$type = "g$type";
	} elsif ($type =~ /string/) {
		$params =~ /\"(.+)\",.+\".+\".+\"(.+)\".*(,\s*G_PARAM_\w+.*)\)\s*\)/;
		$name = $1; $docs = $2; $mode = $3;
		$type = "gchar*";
	} elsif ($type =~ /enum|flags/) {
		$params =~ /\"(.+)\",.+,.+\"(.+)\".*,\s+(\w+),.*,(\s*G_PARAM_\w+.*)\)\s*\)/;
		$name = $1; $docs = $2; $type = $3; $mode = $4;
		$type =~ s/TYPE_//;
		$type = StudCaps(lc($type));
	} elsif ($type =~ /object/) {
		$params =~ /\"(.+)\",.+,.+\"(.+)\".*,\s+(\w+),(\s*G_PARAM_\w+.*)\)\s*\)/;
		$name = $1; $docs = $2; $type = $3; $mode = $4;
		$type =~ s/TYPE_//;
		$type = StudCaps(lc($type));
	}


	print "(define-property $name\n";
	print "  (of-object \"$class\")\n";
	print "  (prop-type \"$type\")\n";
	print "  (doc-string \"$docs\")\n";

	if ($mode =~ /READ/) { print "  (readable #t)\n"; }
	if ($mode =~ /WRIT/) { print "  (writeable #t)\n"; }
	if ($mode =~ /CONS/) { print "  (construct-only #t)\n"; }

	print ")\n\n";

	$props{$name} = $pstr;
}

sub StudCaps
{
	my ($str) = @_;

	$str =~ s/^(\w)/\u\1/;
	$str =~ s/[_-]([a-z])/\u\1/g;
	$str =~ s/[_-](\d)/\1/g;
	return $str;
}

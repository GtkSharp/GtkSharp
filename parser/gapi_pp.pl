#!/usr/bin/perl
#
# gapi_pp.pl : A source preprocessor for the extraction of API info from a
#	       C library source directory.
#
# Author: Mike Kestner <mkestner@speakeasy.net>
#
# <c> 2001 Mike Kestner

$eatit_regex = "^#if.*(__cplusplus|DEBUG|DISABLE_(DEPRECATED|COMPAT)|ENABLE_BROKEN|COMPILATION)";
$ignoreit_regex = '^\s+\*|#\s*include|#\s*else|#\s*endif|#\s*undef|G_(BEGIN|END)_DECLS|extern|GDKVAR|GTKVAR|GTKMAIN_C_VAR|GTKTYPEUTILS_VAR|VARIABLE|GTKTYPEBUILTIN';

foreach $dir (@ARGV) {
	@hdrs = (@hdrs, `ls $dir/*.h`);
}

foreach $fname (@hdrs) {

	if ($fname =~ /test|private|internals|gtktextlayout|gtkmarshalers/) {
		@privhdrs = (@privhdrs, $fname);
		next;
	}

	open(INFILE, $fname) || die "Could open $fname\n";

	while ($line = <INFILE>) {

		next if ($line =~ /$ignoreit_regex/);
		next if ($line !~ /\S/);

		if ($line =~ /#\s*define\s+\w+\s*\D+/) {
			$def = $line;
			while ($line =~ /\\\n/) {$def .= ($line = <INFILE>);}
			if ($def =~ /_CHECK_\w*CAST|INSTANCE_GET_INTERFACE/) {
				$def =~ s/\\\n//g;
				print $def;
			}
		} elsif ($line =~ /^\s*\/\*/) {
			while ($line !~ /\*\//) {$line = <INFILE>;}
		} elsif ($line =~ /^#ifndef\s+\w+_H_*\b/) {
			while ($line !~ /#define/) {$line = <INFILE>;}
		} elsif ($line =~ /$eatit_regex/) {
			while ($line !~ /#else|#endif/) {$line = <INFILE>;}
		} elsif ($line =~ /^#\s*ifn?\s*\!?def/) {
			#warn "Ignored #if:\n$line";
		} elsif ($line =~ /typedef\s+struct\s+\w*\s*\{/) {
			while ($line !~ /^}\s*\w+;/) {$line = <INFILE>;}
		} elsif ($line =~ /^enum\s+\{/) {
			while ($line !~ /^};/) {$line = <INFILE>;}
		} else {
			print $line;
		}
	}
}

foreach $fname (`ls $ARGV[0]/*.c`, @privhdrs) {

	open(INFILE, $fname) || die "Could open $fname\n";

	if ($fname =~ /builtins_ids/) {
		while ($line = <INFILE>) {
			next if ($line !~ /\{/);

			chomp($line);
			$builtin = "BUILTIN" . $line;
			$builtin .= <INFILE>;
			print $builtin;
		}
		next;
	}

	while ($line = <INFILE>) {
		#next if ($line !~ /^(struct|\w+_class_init)|g_boxed_type_register_static/);
		next if ($line !~ /^(struct|\w+_class_init|\w+_base_init|\w+_get_type)/);

		if ($line =~ /^struct/) {
			# need some of these to parse out parent types
			print "private";
		}

		do {
			print $line;
		} until (($line = <INFILE>) =~ /^}/);
		print $line;
	}
}


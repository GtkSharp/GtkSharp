#!/usr/bin/perl -w

if ($ARGV[0] && $ARGV[1]) {
	open INFILE, $ARGV[0];
	$outdir = $ARGV[1];
} else {
	print "Usage: gapi.pl <sources_file> <out_dir>\n";
	exit 0;
}

@srcs = ();
%files = ();

while (<INFILE>)
{
	chomp;
	my @entry = split /\s+/;
	push @srcs, \@entry;
	
	$ns = $entry[1];
	if (not $files{$ns}) {
		$files{$ns} = lc ($ns) . "-api.xml";
	}
}

foreach $ns (keys (%files)) {
	unlink $files{$ns};
}

foreach $entry (@srcs) {
	($dir, $ns, $lib) = @$entry;
	print "hi $ns\n";
	$file = $files{$ns};
	system ("gapi_pp.pl $dir | gapi2xml.pl $ns $file $lib");
}

foreach $ns (keys (%files)) {
	$file = $files{$ns};
	system ("gapi_format_xml $file $outdir/$file");
	unlink $file;
}


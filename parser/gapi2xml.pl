#!/usr/bin/perl
#
# gapi2xml.pl : Generates an XML representation of GObject based APIs.
#
# Author: Mike Kestner <mkestner@speakeasy.net>
#
# <c> 2001 Mike Kestner
##############################################################

$debug=1;

use XML::LibXML;

if (!$ARGV[0]) {
	die "Usage: gapi_pp.pl <srcdir> | gapi2xml.pl <namespace> <outfile>\n";
}

$ns = $ARGV[0];

##############################################################
# If a filename was provided see if it exists.  We parse existing files into
# a tree and append the namespace to the root node.  If the file doesn't 
# exist, we create a doc tree and root node to work with.
##############################################################

if ($ARGV[1] && -e $ARGV[1]) {
	#parse existing file and get root node.
	$doc = XML::LibXML->new->parse_file($ARGV[1]);
	$root = $doc->getDocumentElement();
} else {
	$doc = XML::LibXML::Document->new();
	$root = $doc->createElement('api');
	$doc->setDocumentElement($root);
}

$ns_elem = $doc->createElement('namespace');
$ns_elem->setAttribute('name', $ns);
$root->appendChild($ns_elem);

##############################################################
# First we parse the input for typedefs, structs, enums, and class_init funcs
# and put them into temporary hashes.
##############################################################

while ($line = <STDIN>) {
	if ($line =~ /typedef\s+(struct\s+\w+\s+)\*+(\w+);/) {
		$ptrs{$2} = $1;
	} elsif ($line =~ /typedef\s+(struct\s+\w+)\s+(\w+);/) {
		$types{$2} = $1;
	} elsif ($line =~ /typedef\s+(\w+\s+\**)(\w+);/) {
		$types{$2} = $1;
	} elsif ($line =~ /typedef\s+enum/) {
		$ename = $1;
		$edef = $line;
		while ($line = <STDIN>) {
			$edef .= $line;
			last if ($line =~ /^}\s*(\w+);/);
		}
		$edef =~ s/\n\s*//g;
		$edef =~ s|/\*.*?\*/||g;
		$edef =~ /}\s*(\w+);/;
		$ename = $1;
		$edefs{$ename} = $edef;
	} elsif ($line =~ /typedef\s+\w+\s*\**\s*\(\*\s*(\w+)\)\s*\(/) {
		$fname = $1;
		$fdef = "";
		while ($line !~ /;/) {
			$fdef .= $line;
			$line = <STDIN>;
		}
		$fdef .= $line;
		$fdef =~ s/\n\s+//g;
		$fpdefs{$fname} = $fdef;
	} elsif ($line =~ /struct\s+(\w+)/) {
		$sname = $1;
		$sdef = $line;
		while ($line = <STDIN>) {
			$sdef .= $line;
			last if ($line =~ /^}/);
		}
		$sdef =~ s!/\*.*?(\*/|\n)!!g;
		$sdef =~ s/\n\s*//g;
		$sdefs{$sname} = $sdef;
	} elsif ($line =~ /^(\w+)_class_init\b/) {
		$class = StudlyCaps($1);
		$pedef = $line;
		while ($line = <STDIN>) {
			$pedef .= $line;
			last if ($line =~ /^}/);
		}
		$pedefs{$class} = $pedef;
	} elsif ($line =~ /^(const|G_CONST_RETURN)?\s*\w+\s*\**\s*(\w+)\s*\(/) {
		$fname = $2;
		$fdef = "";
		while ($line !~ /;/) {
			$fdef .= $line;
			$line = <STDIN>;
		}
		$fdef .= $line;
		$fdefs{$fname} = $fdef;
	} elsif ($line =~ /G_TYPE_CHECK_(\w+)_CAST.*,\s*(\w+),\s*(\w+)/) {
		if ($1 eq "INSTANCE") {
			$objects{$2} = $3 . $objects{$2};
		} else {
			$objects{$2} .= ":$3";
		}
	} elsif ($line =~ /GTK_CHECK_CAST.*,\s*(\w+),\s*(\w+)/) {
		$objects{$1} = $2 . $objects{$1};
	} elsif ($line =~ /GTK_CHECK_CLASS_CAST.*,\s*(\w+),\s*(\w+)/) {
		$objects{$1} .= ":$2";
	} elsif ($line =~ /INSTANCE_GET_INTERFACE.*,\s*(\w+),\s*(\w+)/) {
		$ifaces{$1} = $2;
	} else {
		print $line;
	}
}

##############################################################
# Produce the enum definitions.
##############################################################

foreach $cname (sort(keys(%edefs))) {
	$ecnt++;
	$enum_elem = addNameElem($ns_elem, 'enum', $cname, $ns);
	$def = $edefs{$cname};
	if ($def =~ /=\s*1\s*<<\s*\d+/) {
		$enum_elem->setAttribute('type', "flags");
	} else {
		$enum_elem->setAttribute('type', "enum");
	}
	$def =~ /\{(.*)\}/;
	@vals = split(/,\s*/, $1);
	@v0 = split(/_/, $vals[0]);
	if (@vals > 1) {
		$done = 0;
		for ($idx = 0, $regex = ""; $idx < @v0; $idx++) {
			$regex .= ($v0[$idx] . "_");
			foreach $val (@vals) {
				$done = 1 if ($val !~ /$regex/);
			}
			last if $done;
		}
		$common = join("_", @v0[0..$idx-1]);
	} else {
		$common = join("_", @v0[0..$#v0-1]);
	}
	
	foreach $val (@vals) {
		if ($val =~ /$common\_(\w+)\s*=\s*(\d+.*)/) {
			$name = $1;
			if ($2 =~ /1u?\s*<<\s*(\d+)/) {
				$enumval = "1 << $1";
			} else {
				$enumval = $2;
			}
		} elsif ($val =~ /$common\_(\w+)/) {
			$name = $1; $enumval = "";
		} else {
			die "Unexpected enum value: $val\n";
		}

		$val_elem = addNameElem($enum_elem, 'member');
		$val_elem->setAttribute('cname', "$common\_$name");
		$val_elem->setAttribute('name', StudlyCaps(lc($name)));
		if ($enumval) {
			$val_elem->setAttribute('value', $enumval);
		}
	}
}

##############################################################
# Parse the callbacks.
##############################################################

foreach $cbname (sort(keys(%fpdefs))) {
	next if ($cbname !~ /$ns/);
	$cbcnt++;
	$fdef = $cb = $fpdefs{$cbname};
	$cb_elem = addNameElem($ns_elem, 'callback', $cbname, $ns);
	$cb =~ /typedef\s+(.*)\(.*\).*\((.*)\);/;
	$ret = $1; $params = $2;
	addReturnElem($cb_elem, $ret);
	if ($params && ($params ne "void")) {
		addParamsElem($cb_elem, split(/,/, $params));
	}
}


##############################################################
# Parse the interfaces list.   
##############################################################

foreach $type (sort(keys(%ifaces))) {

	$iface = $ifaces{$type};
	($inst, $dontcare) = split(/:/, delete $objects{$type});
	$ifacetype = delete $types{$iface};
	delete $types{$inst};

	$ifacecnt++;
	$iface_el = addNameElem($ns_elem, 'interface', $inst, $ns);
	addFuncElems($iface_el, $inst);
}


##############################################################
# Parse the classes by walking the objects list.   
##############################################################

foreach $type (sort(keys(%objects))) {
	
	($inst, $class) = split(/:/, $objects{$type});
	$class = $inst . "Class" if (!$class);
	$initfunc = $pedefs{$inst};
	$insttype = delete $types{$inst};
	$classtype = delete $types{$class};

	$instdef = $classdef = "";
	$instdef = $sdefs{$1} if ($insttype =~ /struct\s+(\w+)/);
	$classdef = $sdefs{$1} if ($classtype =~ /struct\s+(\w+)/);
	$instdef =~ s/\s+(\*+)/\1 /g;
	warn "Strange Class $inst\n" if (!$instdef && $debug);

	$classcnt++;
	$obj_el = addNameElem($ns_elem, 'object', $inst, $ns);

	# Extract parent and fields from the struct
	if ($instdef =~ /^struct/) {
		$instdef =~ /\{(.*)\}/;
		@fields = split(/;/, $1);
		$fields[0] =~ /(\w+)/;
		$obj_el->setAttribute('parent', "$1");
		addFieldElems($obj_el, @fields[1..$#fields]);
	} elsif ($instdef =~ /privatestruct/) {
		# just get the parent for private structs
		$instdef =~ /\{\s*(\w+)/;
		$obj_el->setAttribute('parent', "$1");
	}

	# Get the props from the class_init func.
	if ($initfunc) {
		parseInitFunc($obj_el, $initfunc);
	} else {
		warn "Don't have an init func for $inst.\n" if $debug;
	}

	addFuncElems($obj_el, $inst);
}

##############################################################
# Parse the remaining types.
##############################################################

foreach $key (sort (keys (%types))) {

	$lasttype = $type = $key;
	while ($type && ($types{$type} !~ /struct/)) {
		$lasttype = $type;
		$type = $types{$type};
	}

	if ($types{$type} =~ /struct\s+(\w+)/) {
		$type = $1;
	} else {
		$elem = addNameElem($ns_elem, 'alias', $key, $ns);
		$elem->setAttribute('type', $lasttype);
		warn "alias $key to $lasttype\n" if $debug;
		next;
	}
		
	if (exists($sdefs{$type})) {
		$def = $sdefs{$type};
	} else {
		warn "Couldn't find $type\n" if $debug;
		next;
	}

	$struct_el = addNameElem($ns_elem, 'struct', $key, $ns);
	$def =~ s/\s+/ /g;
	$def =~ /\{(.+)\}/;
	addFieldElems($struct_el, split(/;/, $1));
	addFuncElems($struct_el, $key);
}

##############################################################
# Output the tree
##############################################################

if ($ARGV[1]) {
	open(XMLFILE, ">$ARGV[1]") || 
				die "Couldn't open $ARGV[1] for writing.\n";
	print XMLFILE $doc->toString();
	close(XMLFILE);
} else {
	print $doc->toString();
}

##############################################################
# Generate a few stats from the parsed source.
##############################################################

$scnt = keys(%sdefs); $fcnt = keys(%fdefs); $tcnt = keys(%types);
print "structs: $scnt  enums: $ecnt  callbacks: $cbcnt\n";
print "funcs: $fcnt types: $tcnt  classes: $classcnt\n";
print "props: $propcnt signals: $sigcnt\n";

sub addFieldElems
{
	my ($parent, @fields) = @_;

	foreach $field (@fields) {
		next if ($field !~ /\S/);
		$field =~ s/\s+(\*+)/\1 /g;
		$field =~ s/const /const\-/g;
		if ($field =~ /(\S+\s+\*?)\(\*\s*(.+)\)\s*\((.*)\)/) {
			$elem = addNameElem($parent, 'callback', $2);
			addReturnElem($elem, $1);
			addParamsElem($elem, $3);
		} elsif ($field =~ /(\S+)\s+(.+)/) {
			$type = $1; $symb = $2;
			foreach $tok (split (/,\s*/, $symb)) {
				if ($tok =~ /(\w+)\s*\[(.*)\]/) {
					$elem = addNameElem($parent, 'field', $1);
					$elem->setAttribute('array_len', "$2");
				} elsif ($tok =~ /(\w+)\s*\:\s*(\d+)/) {
					$elem = addNameElem($parent, 'field', $1);
					$elem->setAttribute('bits', "$2");
				} else {
					$elem = addNameElem($parent, 'field', $tok);
				}
				$elem->setAttribute('type', "$type");
			}
		} else {
			die "$field\n";
		}
	}
}

sub addFuncElems
{
	my ($obj_el, $inst) = @_;

	my $prefix = $inst;
	$prefix =~ s/([A-Z]+)/_\1/g;
	$prefix = lc($prefix);
	$prefix =~ s/^_//;
	$prefix .= "_";

	$fcnt = keys(%fdefs);

	foreach $mname (keys(%fdefs)) {
		next if ($mname !~ /$prefix/);

		if ($fdefs{$mname} =~ /\(\s*$inst\b/) {
			$el = addNameElem($obj_el, 'method', $mname, $prefix);
			$fdefs{$mname} =~ /(.*?)\w+\s*\(/;
			addReturnElem($el, $1);
			$drop_1st = 1;
		} elsif ($mname =~ /$prefix(new)/) {
			$el = addNameElem($obj_el, 'constructor', $mname); 
			$drop_1st = 0;
		} else {
			next;
		}

		$mdef = delete $fdefs{$mname};

		if (($mdef =~ /\(.*\)/) && ($1 ne "void")) {
			@parms = split(/,/, $1);
			($dump, @parms) = @params if $dump_1st;
			if (@parms > 0) {
				addParamsElem($el, @parms);
			}
		}
		
	}
}

sub addNameElem
{
	my ($node, $type, $cname, $prefix) = @_;

	my $elem = $doc->createElement($type);
	$node->appendChild($elem);
	if ($prefix) {
		$cname =~ /$prefix(\w+)/;
		$elem->setAttribute('name', $1);
	}
	if ($cname) {
		$elem->setAttribute('cname', $cname);
	}
	return $elem;
}

sub addParamsElem
{
	my ($parent, @params) = @_;

	my $parms_elem = $doc->createElement('parameters');
	$parent->appendChild($parms_elem);
	foreach $parm (@params) {
		$parm_elem = $doc->createElement('parameter');
		$parms_elem->appendChild($parm_elem);
		$parm =~ s/\s+\*/\* /g;
		$parm =~ s/const\s+/const-/g;
		$parm =~ /(\S+)\s+(\S+)/;
		$parm_elem->setAttribute('type', "$1");
		$parm_elem->setAttribute('name', "$2");
	}
}

sub addReturnElem
{
	my ($parent, $ret) = @_;

	$ret =~ s/const|G_CONST_RETURN/const-/g;
	$ret =~ s/\s+//g;
	my $ret_elem = $doc->createElement('return-type');
	$parent->appendChild($ret_elem);
	$ret_elem->setAttribute('type', $ret);
	return $ret_elem;
}

sub addPropElem
{
	my ($spec, $node) = @_;
	my ($name, $mode, $docs);
	$spec =~ /g_param_spec_(\w+)\s*\((.*)/;
	my $type = $1;
	my $params = $2;

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
		$type = StudlyCaps(lc($type));
	} elsif ($type =~ /object/) {
		$params =~ /\"(.+)\",.+,.+\"(.+)\".*,\s+(\w+),(\s*G_PARAM_\w+.*)\)\s*\)/;
		$name = $1; $docs = $2; $type = $3; $mode = $4;
		$type =~ s/TYPE_//;
		$type = StudlyCaps(lc($type));
	}


	$prop_elem = $doc->createElement('property');
	$node->appendChild($prop_elem);
	$prop_elem->setAttribute('name', $name);
	$prop_elem->setAttribute('type', $type);
	$prop_elem->setAttribute('doc-string', $docs);

	if ($mode =~ /READ/) {
		$prop_elem->setAttribute('readable', "true");
	}
	if ($mode =~ /WRIT/) {
		$prop_elem->setAttribute('writeable', "true");
	}
	if ($mode =~ /CONS/) {
		$prop_elem->setAttribute('construct-only', "true");
	}
}

sub addSignalElem
{
	my ($spec, $class, $node) = @_;
	$spec =~ s/\n\s*//g; $class =~ s/\n\s*//g;

	$sig_elem = $doc->createElement('signal');
	$node->appendChild($sig_elem);

	$sig_elem->setAttribute('name', $1) if ($spec =~ /\(\"(\w+)\"/);
	$sig_elem->setAttribute('when', $1) if ($spec =~ /_RUN_(\w+)/);

	my $method = "";
	if ($spec =~ /_OFFSET\s*\(\w+,\s*(\w+)\)/) {
		$method = $1;
	} else {
		@args = split(/,/, $spec);
		$args[7] =~ s/_TYPE//; $args[7] =~ s/\s+//g;
		addReturnElem($sig_elem, StudlyCaps(lc($args[7])));
		$parmcnt = ($args[8] =~ /\d+/);
		if ($parmcnt > 0) {
			$parms_elem = $doc->createElement('parameters');
			$sig_elem->appendChild($parms_elem);
			for (my $idx = 0; $idx < $parmcnt; $idx++) {
				$arg = $args[9+$idx];
				$arg =~ s/_TYPE//; $arg =~ s/\s+//g;
				$arg = StudlyCaps(lc($arg));
				$parm_elem = $doc->createElement('parameter');
				$parms_elem->appendChild($parm_elem);
				$parm_elem->setAttribute('name', "p$idx");
				$parm_elem->setAttribute('type', $arg);
			}
		}
		return;
	}

	if ($class =~ /;\s*(\S+\s*\**)\s*\(\*\s*$method\)\s*\((.*)\);/) {
		$ret = $1; $parms = $2;
		addReturnElem($sig_elem, $ret);
		if ($parms && ($parms ne "void")) {
			addParamsElem($sig_elem, split(/,/, $parms));
		}
	} else {
		die "$method $class";
	}

}


sub parseInitFunc
{
	my ($obj_el, $initfunc) = @_;

	my @init_lines = split (/\n/, $initfunc);

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
				addPropElem ($prop, $obj_el);
				$propcnt++;
			} elsif ($line =~ /g(tk)?_signal_new/) {
				my $sig = $line;
				do {
					$sig .= $init_lines[++$linenum];
				} until ($init_lines[$linenum] =~ /;/);
				addSignalElem ($sig, $classdef, $obj_el);
				$sigcnt++;
			}
			$linenum++;
		}

		$linenum++;
	}
}

##############################################################
# Converts a dash or underscore separated name to StudlyCaps.
##############################################################

%num2txt = ('1', "One", '2', "Two", '3', "Three", '4', "Four", '5', "Five",
	    '6', "Six", '7', "Seven", '8', "Eight", '9', "Nine", '0', "Zero");

sub StudlyCaps
{
	my ($symb) = @_;
	$symb =~ s/^([a-z])/\u\1/;
	$symb =~ s/^(\d)/\1_/;
	$symb =~ s/[-_]([a-z])/\u\1/g;
	$symb =~ s/[-_](\d)/\1/g;
	$symb =~ s/^2/Two/;
	$symb =~ s/^3/Three/;
	return $symb;
}


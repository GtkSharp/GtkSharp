#!/usr/bin/perl
#
# gapi2xml.pl : Generates an XML representation of GObject based APIs.
#
# Author: Mike Kestner <mkestner@speakeasy.net>
#
# <c> 2001-2003 Mike Kestner
##############################################################

$debug=0;

use XML::LibXML;
use GAPI::Metadata;

if (!$ARGV[2]) {
	die "Usage: gapi_pp.pl <srcdir> | gapi2xml.pl <namespace> <outfile> <libname>\n";
}

$ns = $ARGV[0];
$libname = $ARGV[2];

##############################################################
# Check if the filename provided exists.  We parse existing files into
# a tree and append the namespace to the root node.  If the file doesn't 
# exist, we create a doc tree and root node to work with.
##############################################################

if (-e $ARGV[1]) {
	#parse existing file and get root node.
	$doc = XML::LibXML->new->parse_file($ARGV[1]);
	$root = $doc->getDocumentElement();
} else {
	$doc = XML::LibXML::Document->new();
	$root = $doc->createElement('api');
	$doc->setDocumentElement($root);
	$warning_node = XML::LibXML::Comment->new ("\n\n        This file was automatically generated.\n        Please DO NOT MODIFY THIS FILE, modify .metadata files instead.\n\n");
	$root->appendChild($warning_node);
}

$ns_elem = $doc->createElement('namespace');
$ns_elem->setAttribute('name', $ns);
$ns_elem->setAttribute('library', $libname);
$root->appendChild($ns_elem);

##############################################################
# First we parse the input for typedefs, structs, enums, and class_init funcs
# and put them into temporary hashes.
##############################################################

while ($line = <STDIN>) {
	if ($line =~ /typedef\s+(struct\s+\w+\s+)\*+(\w+);/) {
		$ptrs{$2} = $1;
	} elsif ($line =~ /typedef\s+(struct\s+\w+)\s+(\w+);/) {
		next if ($2 =~ /Private$/);
		# fixme: siiigh
		$2 = "GdkDrawable" if ($1 eq "_GdkDrawable");
		$types{$2} = $1;
	} elsif ($line =~ /typedef\s+(\w+)\s+(\**)(\w+);/) {
		$types{$3} = $1 . $2;
	} elsif ($line =~ /(typedef\s+)?\benum\b/) {
		$edef = $line;
		while ($line = <STDIN>) {
			$edef .= $line;
			last if ($line =~ /^}\s*(\w+)?;/);
		}
		$edef =~ s/\n\s*//g;
		$edef =~ s|/\*.*?\*/||g;
		if ($edef =~ /typedef.*}\s*(\w+);/) {
			$ename = $1;
		} elsif ($edef =~ /^enum\s+(\w+)\s*{/) {
			$ename = $1;
		} else {
			print "Unexpected enum format\n$edef";
			next;
		}
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
	} elsif ($line =~ /^(private)?struct\s+(\w+)/) {
		next if ($line =~ /;/);
		$sname = $2;
		$sdef = $line;
		while ($line = <STDIN>) {
			$sdef .= $line;
			last if ($line =~ /^}/);
		}
		$sdef =~ s!/\*.*?(\*/|\n)!!g;
		$sdef =~ s/\n\s*//g;
		$sdefs{$sname} = $sdef;
	} elsif ($line =~ /^(\w+)_(class|base)_init\b/) {
		$class = StudlyCaps($1);
		$pedef = $line;
		while ($line = <STDIN>) {
			$pedef .= $line;
			last if ($line =~ /^}/);
		}
		$pedefs{lc($class)} = $pedef;
	} elsif ($line =~ /^(\w+)_get_type\b/) {
		$class = StudlyCaps($1);
		$pedef = $line;
		while ($line = <STDIN>) {
			$pedef .= $line;
			if ($line =~ /g_boxed_type_register_static/) {
				$boxdef = $line;
				while ($line !~ /;/) {
					$boxdef .= ($line = <STDIN>);
				}
				$boxdef =~ s/\n\s*//g;
				$boxdef =~ /\(\"(\w+)\"/;
				my $boxtype = $1;
				$boxtype =~ s/($ns)Type(\w+)/$ns$2/;
				$boxdefs{$boxtype} = $boxdef;
			}
			last if ($line =~ /^}/);
		}
		$typefuncs{lc($class)} = $pedef;
	} elsif ($line =~ /^(const|G_CONST_RETURN)?\s*\w+\s*\**\s*(\w+)\s*\(/) {
		$fname = $2;
		$fdef = "";
		while ($line !~ /;/) {
			$fdef .= $line;
			$line = <STDIN>;
		}
		$fdef .= $line;
		$fdef =~ s/\n\s*//g;
		if ($fdef !~ /^_/) {
			$fdefs{$fname} = $fdef;
		}
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
	} elsif ($line =~ /^BUILTIN\s*\{\s*\"(\w+)\".*GTK_TYPE_BOXED/) {
		$boxdefs{$1} = $line;
	} elsif ($line =~ /^BUILTIN\s*\{\s*\"(\w+)\".*GTK_TYPE_(ENUM|FLAGS)/) {
		# ignoring these for now.
	} elsif ($line =~ /^\#define/) {
		my $test_ns = uc ($ns);
		if ($line =~ /\#define\s+(\w+)\s+\"(.*)\"/) {
			$defines{$1} = $2;
		}
	} else {
		print $line;
	}
}

##############################################################
# Produce the enum definitions.
##############################################################
%enums = ();

foreach $cname (sort(keys(%edefs))) {
	$ecnt++;
	$enums{lc($cname)} = $cname;
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
		if ($val =~ /$common\_?(\w+)\s*=\s*(\-?\d+.*)/) {
			$name = $1;
			if ($2 =~ /1u?\s*<<\s*(\d+)/) {
				$enumval = "1 << $1";
			} else {
				$enumval = $2;
			}
		} elsif ($val =~ /$common\_?(\w+)/) {
			$name = $1; $enumval = "";
		} else {
			die "Unexpected enum value: $val for common value $common\n";
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
	$initfunc = $pedefs{lc($inst)};
	$ifacetype = delete $types{$iface};
	delete $types{$inst};
	
	$ifacecnt++;
	$iface_el = addNameElem($ns_elem, 'interface', $inst, $ns);

	$elem_table{lc($inst)} = $iface_el;

	$classdef = $sdefs{$1} if ($ifacetype =~ /struct\s+(\w+)/);
	if ($initfunc) {
		parseInitFunc($iface_el, $initfunc);
	} else {
		warn "Don't have an init func for $inst.\n" if $debug;
	}
}


##############################################################
# Parse the classes by walking the objects list.   
##############################################################

foreach $type (sort(keys(%objects))) {
	
	($inst, $class) = split(/:/, $objects{$type});
	$class = $inst . "Class" if (!$class);
	$initfunc = $pedefs{lc($inst)};
	$typefunc = $typefuncs{lc($inst)};
	$insttype = delete $types{$inst};
	$classtype = delete $types{$class};

	$instdef = $classdef = "";
	$instdef = $sdefs{$1} if ($insttype =~ /struct\s+(\w+)/);
	$classdef = $sdefs{$1} if ($classtype =~ /struct\s+(\w+)/);
	$instdef =~ s/\s+(\*+)/\1 /g;
	warn "Strange Class $inst\n" if (!$instdef && $debug);

	$classcnt++;
	$obj_el = addNameElem($ns_elem, 'object', $inst, $ns);

	$elem_table{lc($inst)} = $obj_el;

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

	# Get the interfaces from the class_init func.
	if ($typefunc) {
		parseTypeFunc($obj_el, $typefunc);
	} else {
		warn "Don't have a GetType func for $inst.\n" if $debug;
	}

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
		$def = "privatestruct";
	}


	# fixme: hack
	if ($key eq "GdkBitmap") {
		$struct_el = addNameElem($ns_elem, 'object', $key, $ns);
	} elsif (exists($boxdefs{$key})) {
		$struct_el = addNameElem($ns_elem, 'boxed', $key, $ns);
	} else {
		$struct_el = addNameElem($ns_elem, 'struct', $key, $ns);
	}

	$elem_table{lc($key)} = $struct_el;

	$def =~ s/\s+/ /g;
	if ($def =~ /privatestruct/) {
		$struct_el->setAttribute('opaque', 'true');
	} else {
		$def =~ /\{(.+)\}/;
		addFieldElems($struct_el, split(/;/, $1));
	}
}

# really, _really_ opaque structs that aren't even defined in sources. Lovely.
foreach $key (sort (keys (%ptrs))) {
	next if $ptrs{$key} !~ /struct\s+(\w+)/;
	$type = $1;
	$struct_el = addNameElem ($ns_elem, 'struct', $key, $ns);
	$struct_el->setAttribute('opaque', 'true');
	$elem_table{lc($key)} = $struct_el;
}

addFuncElems();
addStaticFuncElems();

# This should probably be done in a more generic way
foreach $define (sort (keys (%defines))) {
	next if $define !~ /[A-Z]_STOCK_/;
	if ($stocks{$ns}) {
		$stock_el = $stocks{$ns};
	} else {
		$stock_el = addNameElem($ns_elem, "object", $ns . "Stock", $ns);
		$stocks{$ns} = $stock_el;
	}
	$string_el = addNameElem ($stock_el, "static-string", $define);
	$string_name = lc($define);
	$string_name =~ s/\w+_stock_//;
	$string_el->setAttribute('name', StudlyCaps($string_name));
	$string_el->setAttribute('value', $defines{$define});
}

##############################################################
# Add metadata
##############################################################
GAPI::Metadata::fixup $doc;

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
print "props: $propcnt signals: $sigcnt\n\n";

sub addFieldElems
{
	my ($parent, @fields) = @_;

	foreach $field (@fields) {
		next if ($field !~ /\S/);
		$field =~ s/\s+(\*+)/\1 /g;
		$field =~ s/const /const\-/g;
		$field =~ s/struct /struct\-/g;
		$field =~ s/.*\*\///g;
		next if ($field !~ /\S/);

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
	my ($obj_el, $inst, $prefix);

	$fcnt = keys(%fdefs);

	foreach $mname (sort (keys (%fdefs))) {
		next if ($mname =~ /^_/);
		$obj_el = "";
		$prefix = $mname;
		$prepend = undef;
		while ($prefix =~ /(\w+)_/) {
			$prefix = $key = $1;
			$key =~ s/_//g;
			# FIXME: lame Gdk API hack
			if ($key eq "gdkdraw") {
				$key = "gdkdrawable";
				$prepend = "draw_";
			}
			if (exists ($elem_table{$key})) {
				$prefix .= "_";
				$obj_el = $elem_table{$key};
				$inst = $key;
				last;
			} elsif (exists ($enums{$key}) && ($mname =~ /_get_type/)) {
				delete $fdefs{$mname};
				last;
			}
		}
		next if (!$obj_el);

		$mdef = delete $fdefs{$mname};

		if ($mname =~ /$prefix(new)/) {
			$el = addNameElem($obj_el, 'constructor', $mname); 
			$drop_1st = 0;
		} else {
			$el = addNameElem($obj_el, 'method', $mname, $prefix, $prepend);
			$mdef =~ /(.*?)\w+\s*\(/;
			addReturnElem($el, $1);
			$mdef =~ /\(\s*(const)?\s*(\w+)/;
			if (lc($2) ne $inst) {
				$el->setAttribute("shared", "true");
				$drop_1st = 0;
			} else {
				$drop_1st = 1;
			}
		}

		parseParms ($el, $mdef, $drop_1st);

	}
}

sub parseParms
{
	my ($el, $mdef, $drop_1st) = @_;

	if (($mdef =~ /\((.*)\)/) && ($1 ne "void")) {
		@parms = ();
		$parm = "";
		$pcnt = 0;
		foreach $char (split(//, $1)) {
			if ($char eq "(") {
				$pcnt++;
			} elsif ($char eq ")") {
				$pcnt--;
			} elsif (($pcnt == 0) && ($char eq ",")) {
				@parms = (@parms, $parm);
				$parm = "";
				next;
			}
			$parm .= $char;
		}

		if ($parm) {
			@parms = (@parms, $parm);
		}
		# @parms = split(/,/, $1);
		($dump, @parms) = @parms if $drop_1st;
		if (@parms > 0) {
			addParamsElem($el, @parms);
		}
	}
}

sub addStaticFuncElems
{
	my ($global_el, $ns_prefix);

	@mnames = sort (keys (%fdefs));
	$mcount = @mnames;

	return if ($mcount == 0);

	$ns_prefix = "";
	$global_el = "";

	for ($i = 0; $i < $mcount; $i++) {
		$mname = $mnames[$i];
		$prefix = $mname;
		next if ($prefix =~ /^_/);

		if ($ns_prefix eq "") {
			my (@toks) = split(/_/, $prefix);
			for ($j = 0; $j < @toks; $j++) {
				if (join ("", @toks[0 .. $j]) eq lc($ns)) {
					$ns_prefix = join ("_", @toks[0 .. $j]);
					last;
				}
			}
			next if ($ns_prefix eq "");
		}
		next if ($mname !~ /^$ns_prefix/);

		if ($mname =~ /($ns_prefix)_([a-zA-Z]+)_\w+/) {
			$classname = $2;
			$key = $prefix = $1 . "_" . $2 . "_";
			$key =~ s/_//g;
			$cnt = 1;
			if (exists ($enums{$key})) {
				$cnt = 1; 
			} elsif ($classname ne "set" && $classname ne "get" &&
			    $classname ne "scan" && $classname ne "find" &&
			    $classname ne "add" && $classname ne "remove" &&
			    $classname ne "free" && $classname ne "register" &&
			    $classname ne "execute" && $classname ne "show" &&
			    $classname ne "parse" && $classname ne "paint" &&
			    $classname ne "string") {
				while ($mnames[$i+$cnt] =~ /$prefix/) { $cnt++; }
			}
			if ($cnt == 1) {
				$mdef = delete $fdefs{$mname};

				if (!$global_el) {
					$global_el = $doc->createElement('class');
					$global_el->setAttribute('name', "Global");
					$global_el->setAttribute('cname', $ns . "Global");
					$ns_elem->appendChild($global_el);
				}
				$el = addNameElem($global_el, 'method', $mname, $ns_prefix);
				$mdef =~ /(.*?)\w+\s*\(/;
				addReturnElem($el, $1);
				$el->setAttribute("shared", "true");
				parseParms ($el, $mdef, 0);
				next;
			} else {
				$class_el = $doc->createElement('class');
				$class_el->setAttribute('name', StudlyCaps($classname));
				$class_el->setAttribute('cname', StudlyCaps($prefix));
				$ns_elem->appendChild($class_el);

				for ($j = 0; $j < $cnt; $j++) {
					$mdef = delete $fdefs{$mnames[$i+$j]};

					$el = addNameElem($class_el, 'method', $mnames[$i+$j], $prefix);
					$mdef =~ /(.*?)\w+\s*\(/;
					addReturnElem($el, $1);
					$el->setAttribute("shared", "true");
					parseParms ($el, $mdef, 0);
				}
				$i += ($cnt - 1);
				next;
			}
		}
	}
}

sub addNameElem
{
	my ($node, $type, $cname, $prefix, $prepend) = @_;

	my $elem = $doc->createElement($type);
	$node->appendChild($elem);
	if ($prefix) {
		my $match;
		if ($cname =~ /$prefix(\w+)/) {
			$match = $1;
		} else {
			$match = $cname;
		}
		if ($prepend) {
			$name = $prepend . $match;
		} else {
			$name = $match;
		}
		$elem->setAttribute('name', StudlyCaps($name));
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
	my $parm_num = 0;
	foreach $parm (@params) {
		$parm_num++;
		$parm =~ s/\s+(\*+)/\1 /g;
		$parm =~ s/(\*+)\s*const/\1/g;
		$parm =~ s/const\s+/const-/g;
		if ($parm =~ /(.*)\(\s*\**\s*(\w+)\)\s+\((.*)\)/) {
			my $ret = $1; my $cbn = $2; my $params = $3;
			$cb_elem = addNameElem($parms_elem, 'callback', $cbn);
			addReturnElem($cb_elem, $ret);
			if ($params && ($params ne "void")) {
				addParamsElem($cb_elem, split(/,/, $params));
			}
			next;
		} elsif ($parm =~ /\.\.\./) {
			$parm_elem = $doc->createElement('parameter');
			$parms_elem->appendChild($parm_elem);
			$parm_elem->setAttribute('ellipsis', 'true');
			next;
		}
		$parm_elem = $doc->createElement('parameter');
		$parms_elem->appendChild($parm_elem);
		my $name = "";
		if ($parm =~ /(\S+)\s+(\S+)/) {
			$parm_elem->setAttribute('type', $1);
			$name = $2;
		} elsif ($parm =~ /(\S+)/) {
			$parm_elem->setAttribute('type', $1);
			$name = "arg" . $parm_num;
		}
		if ($name =~ /(\w+)\[.*\]/) {
			$name = $1;
			$parm_elem->setAttribute('array', "true");
		}
		$parm_elem->setAttribute('name', $name);
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
	$spec =~ /g_param_spec_(\w+)\s*\((.*)\s*\)\s*\)/;
	my $type = $1;
	my @params = split(/,/, $2);

	$name = $params[0];
	if ($defines{$name}) {
		$name = $defines{$name};
	} else {
		$name =~ s/\s*\"//g;
	}

	$mode = $params[$#params];

	if ($type =~ /boolean|float|double|^u?int|pointer/) {
		$type = "g$type";
	} elsif ($type =~ /string/) {
		$type = "gchar*";
	} elsif ($type =~ /boxed|object/) {
		$type = $params[$#params-1];
		$type =~ s/TYPE_//;
		$type =~ s/\s+//g;
		$type = StudlyCaps(lc($type));
	} elsif ($type =~ /enum|flags/) {
		$type = $params[$#params-2];
		$type =~ s/TYPE_//;
		$type =~ s/\s+//g;
		$type = StudlyCaps(lc($type));
	}

	$prop_elem = $doc->createElement('property');
	$node->appendChild($prop_elem);
	$prop_elem->setAttribute('name', StudlyCaps($name));
	$prop_elem->setAttribute('cname', $name);
	$prop_elem->setAttribute('type', $type);

	$prop_elem->setAttribute('readable', "true") if ($mode =~ /READ/);
	$prop_elem->setAttribute('writeable', "true") if ($mode =~ /WRIT/);
	$prop_elem->setAttribute('construct-only', "true") if ($mode =~ /CONS/);
}

sub addSignalElem
{
	my ($spec, $class, $node) = @_;
	$spec =~ s/\n\s*//g; $class =~ s/\n\s*//g;


	$sig_elem = $doc->createElement('signal');
	$node->appendChild($sig_elem);

	if ($spec =~ /\(\"([\w\-]+)\"/) {
		$sig_elem->setAttribute('name', StudlyCaps($1));
		$sig_elem->setAttribute('cname', $1);
	}
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

	if ($class =~ /;\s*(\S+\s*\**)\s*\(\*\s*$method\)\s*\((.*?)\);/) {
		$ret = $1; $parms = $2;
		addReturnElem($sig_elem, $ret);
		if ($parms && ($parms ne "void")) {
			addParamsElem($sig_elem, split(/,/, $parms));
		}
	} else {
		die "$method $class";
	}

}

sub addImplementsElem
{
	my ($spec, $node) = @_;
	$spec =~ s/\n\s*//g; 
	if ($spec =~ /,\s*(\w+)_TYPE_(\w+),/) {
		$impl_elem = $doc->createElement('interface');
		$name = StudlyCaps (lc ("$1_$2"));
		$impl_elem->setAttribute ("cname", "$name");
		$node->appendChild($impl_elem);
	}
}


sub parseInitFunc
{
	my ($obj_el, $initfunc) = @_;

	my @init_lines = split (/\n/, $initfunc);

	my $linenum = 0;
	while ($linenum < @init_lines) {

		my $line = $init_lines[$linenum];
			
		if ($line =~ /#define/) {
			# FIXME: This ignores the bool helper macro thingie.
		} elsif ($line =~ /g_object_class_install_prop/) {
			my $prop = $line;
			do {
				$prop .= $init_lines[++$linenum];
			} until ($init_lines[$linenum] =~ /\)\s*;/);
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
}

sub parseTypeFunc
{
	my ($obj_el, $typefunc) = @_;

	my @type_lines = split (/\n/, $typefunc);

	my $linenum = 0;
	$impl_node = undef;
	while ($linenum < @type_lines) {

		my $line = $type_lines[$linenum];
			
		if ($line =~ /#define/) {
			# FIXME: This ignores the bool helper macro thingie.
		} elsif ($line =~ /g_type_add_interface_static/) {
			my $prop = $line;
			do {
				$prop .= $type_lines[++$linenum];
			} until ($type_lines[$linenum] =~ /;/);
			if (not $impl_node) {
				$impl_node = $doc->createElement ("implements");
				$obj_el->appendChild ($impl_node);
			}
			addImplementsElem ($prop, $impl_node);
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


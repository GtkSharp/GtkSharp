#!/usr/bin/perl
# 
# defs-parse.pl : Gtk+ defs format parser and code generator.
#
# Author: Mike Kestner <mkestner@speakeasy.net>
#
# <c> 2001 Mike Kestner

%maptypes = (
	'none', "void", 'gboolean', "bool", 'gint', "int", 'guint', "uint",
	'guint32', "uint", 'const-gchar', "String", 'GObject', "GLib.Object",
	'gchar', "String", 'gfloat', "float", 'gdouble', "double");

%marshaltypes = (
	'none', "void", 'gboolean', "bool", 'gint', "int", 'guint', "uint",
	'guint32', "uint", 'const-gchar', "IntPtr", 'GObject', "IntPtr",
	'gchar', "IntPtr", 'gfloat', "float", 'gdouble', "double");


%usings = (
	'GLib', "System,System.Collections,System.Runtime.InteropServices,GLib",
	'Gdk', "System,System.Collections,System.Runtime.InteropServices,GLib,Gdk",
	'Gtk', "System,System.Collections,System.Runtime.InteropServices,GLib,Gdk,Gtk",
	'GtkSharp', "System,System.Collections,System.Runtime.InteropServices,GLib,Gdk,Gtk");

`mkdir -p ../glib/generated`;
`mkdir -p ../gdk/generated`;
`mkdir -p ../gtk/generated`;

while ($def = get_def()) {

	if ($def =~ /^\(define-(enum|flags)/) {
		gen_enum (split (/\n/, $def));
	} elsif ($def =~ /^\(define-struct (\w+)/) {
		$name = $1;
		$def =~ /c-name "(\w+)"/;
		$cname=$1;
		$def =~ s/\r?\n\s*//g;
		$structs{$cname} = $def;
		$maptypes{$cname} = $name;
		$marshaltypes{$cname} = $name;
	} elsif ($def =~ /^\(define-object (\w+)/) {
		$name = $1;
		$def =~ /c-name "(\w+)"/;
		$cname=$1;
		$def =~ s/\r?\n\s*//g;
		$objects{$cname} = $def;
		$maptypes{$cname} = $name;
		$marshaltypes{$cname} = "IntPtr";
	} elsif ($def =~ /^\(define-(prop|signal|method)/) {
		$def =~ /of-object "(\w+)"/;
		$cname=$1;
		$def =~ s/\r?\n\s*//g;
		$objects{$cname} .= "\n$def";
	} elsif ($def =~ /^\(define-function/) {
		if ($def =~ /is-constructor-of (\w+)\)/) {
			$cname=$1;
			$def =~ s/\r?\n\s*//g;
			$objects{$cname} .= "\n$def";
		}
	} elsif ($def =~ /^\(define-(interface)/) {
		# Nothing much to do here, I think.
	} elsif ($def =~ /^\(define-boxed/) {
		# Probably need to handle these though...
	} else {
		die "Unexpected definition $def\n";
	}

}

foreach $key (keys (%structs)) {
	gen_struct ($key, $structs{$key});
}

foreach $key (keys (%objects)) {
	next if ($key !~ /GdkPixbuf\b|GtkWindow\b|GtkAccelGroup|GtkBin\b/);
	gen_object (split (/\n/, $objects{$key}));
}

###############
# subroutines
###############

# Gets a single definition from the input stream.
sub get_def
{
	while ($line = <STDIN>) {
		next if ($line !~ /^\(define/);
		$expr = $line;
		do { 
			$line = <STDIN>; 
			$expr .= $line; 
		} until ($line =~ /^\)/);
		return $expr;
	}
	return;
}

# Converts a dash or underscore separated name to StudlyCaps.
sub StudCaps
{
	my ($symb) = @_;
	$symb =~ s/^([a-z])/\u\1/;
	$symb =~ s/[-_]([a-z])/\u\1/g;
	$symb =~ s/[-_](\d)/\1/g;
	return $symb;
}

# Code generation for the enum and flags definitions.
sub gen_enum 
{
	my (@lines) = @_;
	$line = $lines[$pos=0];
	$line =~ /^\(define-(enum|flags) (\w+)/;
	$type = $1;
	$typename = $2;

	$line = $lines[++$pos];
	$line =~ /\(in-module "(\w+)"/;
	$namespace = $1;

	$maptypes{"$namespace$typename"} = $typename;
	$marshaltypes{"$namespace$typename"} = "int";

	do { $line = $lines[++$pos]; } until ($line =~ /\(values/);

	@enums = ();
	while ($line = $lines[++$pos]) {
		last if ($line =~ /^\s*\)/);
		if ($line =~ /\((.+)\)/) {
			($name, $dontcare, $val) = split (/ /, $1);
			$name =~ s/\"//g;
			$name = StudCaps ($name);
			@enums = (@enums, "$name:$val");
		}
	}

	$dir = "../" . lc ($namespace) . "/generated";

	open (OUTFILE, ">$dir/$typename.cs") || die "can't open file";
	
	print OUTFILE "// Generated file: Do not modify\n\n";
	print OUTFILE "namespace $namespace {\n\n";
	print OUTFILE "\t/// <summary> $typename Enumeration </summary>\n";
	print OUTFILE "\t/// <remarks> Valid values:\n";
	print OUTFILE "\t///\t<list type = \"bullet\">\n";
	foreach $enum (@enums) {
		($name) = split (/:/, $enum);
		print OUTFILE "\t///\t\t<item> $name </item>\n"
	}
	print OUTFILE "\t///\t</list>\n\t/// </remarks>\n\n";

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

# Code generation for the structure definitions.
sub gen_struct
{
	my ($cname, $def) = @_;

	$def =~ /define-struct (\w+)/;
	$name = $1;
	$def =~ /in-module "(\w+)"/;
	$namespace = $1;

	$dir = "../" . lc ($namespace) . "/generated";
	open (OUTFILE, ">$dir/$name.cs") || die "can't open file";
	
	print OUTFILE "// Generated file: Do not modify\n\n";
	print OUTFILE "namespace $namespace {\n\n";
	foreach $ns (split (/,/, $usings{$namespace})) {
		print OUTFILE "\tusing $ns;\n";
	}
	print OUTFILE "\n\t/// <summary> $name Structure </summary>\n";
	print OUTFILE "\t/// <remarks>\n\t///\tFIXME: Add docs.\n";
	print OUTFILE "\t/// </remarks>\n\n";

	print OUTFILE "\t[StructLayout(LayoutKind.Sequential)]\n";
	print OUTFILE "\tpublic class $name {\n";

	if ($def =~ /fields'\((.*)\)\)\)/) {
		foreach $parm (split(/\)'\(/, $1)) {
			$parm =~ s/\*//g;
			$parm =~ /"(\S*)" "(\S*)"/;
			$ptype = $1;
			$pname = $2; 
			$pname =~ s/object/objekt/;
			print OUTFILE "\t\tpublic $maptypes{$ptype} $pname;\n";
		}
	}

	print OUTFILE "\t}\n\n}\n";
	close (OUTFILE);
}

# Code generation for objects.
sub gen_object
{
	my ($objdef, @defs) = @_;
	my ($key, $typename, $parent, $dir, $namespace, $abstract, $def);
		
	$objdef =~ /define-object (\w+)/;
	$typename = $1;

	$objdef =~ /parent "(\w+)"/;
	$parent = $maptypes{$1};

	$objdef =~ /in-module "(\w+)"/;
	$dir = "../" . lc ($namespace = $1) . "/generated";

	%props = ();
	%signals = ();
	%methods = ();
	@ctors = ();
	foreach $def (@defs) {
		if ($def =~ /define-property (\w+)/) {
			$props{StudCaps($1)} = $def;
		} elsif ($def =~ /define-signal (\w+)/) {
			$signals{StudCaps($1)} = $def;
		} elsif ($def =~ /define-method (\w+)/) {
			$methods{StudCaps($1)} = $def;
		} elsif ($def =~ /is-constructor-of/) {
			@ctors = (@ctors, $def);
		}
	}

	open (OUTFILE, ">$dir/$typename.cs") || die "can't open file";
	
	print OUTFILE "// Generated file: Do not modify\n\n";
	print OUTFILE "namespace $namespace {\n\n";
	foreach $ns (split (/,/, $usings{$namespace})) {
		print OUTFILE "\tusing $ns;\n";
	}
	print OUTFILE "\n\t/// <summary> $typename Class </summary>\n";
	print OUTFILE "\t/// <remarks>\n\t///\t FIXME: Generate docs\n";
	print OUTFILE "\t/// </remarks>\n\n";
	print OUTFILE "\tpublic ";
	if (@ctors == 0) {
		print OUTFILE "abstract ";
	}
	print OUTFILE "class $typename : $parent {\n\n";

	# Only generate the wrapper ctor if other ctors exist
	if (@ctors) {
		print OUTFILE "\t\t/// <summary>\n";
		print OUTFILE "\t\t///\t$typename Constructor\n";
		print OUTFILE "\t\t/// </summary>\n";
		print OUTFILE "\t\t/// <remarks>\n";
		print OUTFILE "\t\t///\tWraps a raw GObject reference.\n";
		print OUTFILE "\t\t/// </remarks>\n\n";
		print OUTFILE "\t\tpublic $typename (IntPtr o)\n\t\t{\n";
		print OUTFILE "\t\t\tRawObject = o;\n\t\t}\n\n";
	}

	foreach $ctor (@ctors) {
		print OUTFILE gen_ctor ($ctor, "gtk-1.3.dll");
	}

	foreach $key (sort (keys (%props))) {
		print OUTFILE gen_prop ($key, $props{$key}, "gtk-1.3.dll");
	}

	if (%signals) {
		print OUTFILE "\t\tprivate Hashtable Signals = new Hashtable ();\n\n";
	}

	foreach $key (sort (keys (%signals))) {
		print OUTFILE gen_signal ($key, $signals{$key}, "gtk-1.3.dll");
	}

	foreach $key (sort (keys (%methods))) {
		next if (($key =~ /^(Get|Set)(\w+)/) && exists($props{$2}));
		my $mod = "";
		if (exists($signals{$key})) {
			$mod = "Emit";
		}
		print OUTFILE gen_method ("$mod$key", $methods{$key}, "gtk-1.3.dll");
	}

	$custom = "../" . lc ($namespace) . "/$typename.custom";
	print OUTFILE `cat $custom` if -e $custom;
	print OUTFILE "\t}\n}\n";
	close (OUTFILE);
}

# Code generation for signal definitions.
sub gen_signal
{
	my ($name, $def, $dll) = @_;
	my ($marsh, $cname, @plist, $ret, $sret, $mret, $code);

	$def =~ /define-signal (\w+)/;
	$cname = "\"$1\"";

	$def =~ /return-type \"(\w+)\"/;
	$ret = $1;

	$def =~ /parameters\s*'\((.*)\)\)\)/;
	@plist = split(/\)'\(/, $1);

	$marsh = get_sighandler ($def);

	if (($ret eq "none") && (@plist == 1)) {
		$marsh = "SimpleSignal";
	} elsif (($ret eq "gboolean") && (@plist == 2) && 
		 ($plist[1] =~ /^\"GdkEvent\*\"/)) {
		$marsh = "SimpleEvent";
	} else {
		return "\t\t// FIXME: Need Marshaller for $name event\n\n";
	}

	$code = "\t\t/// <summary> $name Event </summary>\n";
	$code .= "\t\t/// <remarks>\n\t\t///\tFIXME: Get some docs.\n";
	$code .= "\t\t/// </remarks>\n\n";
	$code .= "\t\tpublic event EventHandler $name {\n";
	$code .= "\t\t\tadd {\n";
	$code .= "\t\t\t\tif (Events [$cname] == null)\n";
	$code .= "\t\t\t\t\tSignals [$cname] = new $marsh (this, RawObject, ";
	$code .= "$cname, value);\n";
	$code .= "\t\t\t\tEvents.AddHandler ($cname, value);\n\t\t\t}\n";
	$code .= "\t\t\tremove {\n";
	$code .= "\t\t\t\tEvents.RemoveHandler ($cname, value);\n";
	$code .= "\t\t\t\tif (Events [$cname] == null)\n";
	$code .= "\t\t\t\t\tSignals.Remove ($cname);\n\t\t\t}\n\t\t}\n\n";
	return $code;
}

# Code generation for property definitions.
sub gen_prop
{
	my ($name, $def, $dll) = @_;
	my ($cname, $mode, $sret, $mret, $docs, $code);

	$def =~ /define-property (\w+)/;
	$cname = $1;

	$def =~ /prop-type "(\w+)/;
	if (exists ($objects{$1}) || ($1 =~ /GObject/)) {
		$sret = $maptypes{$1};
		$mret = "GLib.Object";
	} elsif ($maptypes{$1} eq "String") {
		$sret = $mret = "String";
	} elsif (exists ($maptypes{$1})) {
		$sret = $maptypes{$1};
		$mret = $marshaltypes{$1};
	} else {
		$sret = $mret = $1;
	}

	$def =~ /doc-string "(.+)"\)/;
	$docs = $1;

	$mode = 0;
	if ($def =~ /\(readable #t\)/) {
		$mode = 1;
	}

	if (($def =~ /\(writeable #t\)/) && ($def !~ /\(construct-only #t\)/)) {
		$mode += 2;
	}

	$code = "\t\t/// <summary> $name Property </summary>\n";
	$code .= "\t\t/// <remarks>\n\t\t///\t$docs\n";
	$code .= "\t\t/// </remarks>\n\n";
	$code .= "\t\tpublic $sret $name {\n";
	if ($mode & 1) { 
		$code .= "\t\t\tget {\n\t\t\t\t$mret val;\n";
		$code .= "\t\t\t\tGetProperty (\"$cname\", out val);\n";
		$code .= "\t\t\t\treturn ";
		if ($sret ne $mret) { 
			$code .= "($sret)";
		}
		$code .= "val;\n\t\t\t}\n";
	}
	if ($mode & 2) { 
		$code .= "\t\t\tset {\n";
		$code .= "\t\t\t\tSetProperty (\"$cname\", ($mret) value);\n";
		$code .= "\t\t\t}\n";
	}
	$code .= "\t\t}\n\n";
	return $code;
}

# Generate the code for a constructor definition.
sub gen_ctor
{
	my ($def, $dll) = @_;
	my ($cname, $sret, $ret, $mret, $sig, $call, $pinv, $code);

	$def =~ /\(c-name "(\w+)"/;
	$cname = $1;

	$def =~ /is-constructor-of (\w+)\)/;
	if (exists ($maptypes{$1})) {
		$sret = $maptypes{$1};
		$mret = $marshaltypes{$1};
		$ret = $1;
	} else {
		die "Unexpected return type in constructor: $1\n";
	}

	($call, $pinv, $sig) = gen_param_strings($def);

	$code = "\t\t/// <summary> $sret Constructor </summary>\n";
	$code .= "\t\t/// <remarks>\n\t\t///\t FIXME: Generate docs\n";
	$code .= "\t\t/// </remarks>\n\n";
	$code .= "\t\t[DllImport(\"$dll\", CharSet=CharSet.Ansi,\n";
	$code .= "\t\t\t   CallingConvention=CallingConvention.Cdecl)]\n";
	$code .= "\t\tstatic extern $mret $cname ($pinv);\n\n";
	$code .= "\t\tpublic $sret ($sig)\n";
	$code .= "\t\t{\n\t\t\t";
	$code .= "RawObject = $cname ($call);\n\t\t}\n\n";
}

# Generate the code for a method definition.
sub gen_method
{
	my ($name, $def, $dll) = @_;
	my ($cname, $sret, $ret, $mret, $sig, $call, $pinv, $code);

	$def =~ /\(c-name "(\w+)"/;
	$cname = $1;

	$def =~ /return-type "(\w+)/;
	if (exists ($maptypes{$1})) {
		$sret = $maptypes{$1};
		$mret = $marshaltypes{$1};
		$ret = $1;
	} else {
		$sret = $mret = $ret = $1;
	}

	($call, $pinv, $sig) = gen_param_strings($def);

	$code = "\t\t/// <summary> $name Method </summary>\n";
	$code .= "\t\t/// <remarks>\n\t\t///\t FIXME: Generate docs\n";
	$code .= "\t\t/// </remarks>\n\n";
	$code .= "\t\t[DllImport(\"$dll\", CharSet=CharSet.Ansi,\n";
	$code .= "\t\t\t   CallingConvention=CallingConvention.Cdecl)]\n";
	$code .= "\t\tstatic extern $mret $cname (IntPtr obj";
	if ($pinv) {
		$code .= ", $pinv";
	}
	$code .= ");\n\n";
	$code .= "\t\tpublic $sret $name ($sig)\n";
	$code .= "\t\t{\n\t\t\t";
	if ($sret ne "void") { $code .= "return "; }
	if ($call) {
		$call = "$cname (RawObject, $call)";
	} else {
		$call = "$cname (RawObject)";
	}
	if ($sret eq $mret) { 
		$code .= "$call";
	} elsif ($sret eq "String") {
		$code .= "Marshal.PtrToStringAnsi($call)";
	} elsif ($mret eq "int") {
		$code .= "($sret) $call";
	} elsif (exists ($objects{$ret}) || ($ret =~ /GObject/)) {
		$code .= "($sret) GLib.Object.GetObject($call)";
	} else {
		die "Unexpected return type match $sret:$mret\n";
	}
	$code .= ";\n\t\t}\n\n";
	return $code;
}

# Generate the DllImport, signature, and call parameter strings.
sub gen_param_strings
{
	my ($def) = @_;
	my ($call, $parm, $pinv, $pname, $ptype, $sig);

	$call = $pinv = $sig = "";
	if ($def =~ /parameters'\((.*)\)\)\)/) {
		foreach $parm (split(/\)'\(/, $1)) {
			$parm =~ s/\*//g;
			$parm =~ /"(\S*)" "(\S*)"/;
			$ptype = $1;
			$pname = $2;
			$pname =~ s/object/objekt/;
			$pname =~ s/event/ev3nt/;
			if ($sig) { 
				$sig .= ', '; 
				$call .= ', '; 
				$pinv .= ', '; 
			}
			$pinv .= "$marshaltypes{$ptype} $pname";
			$sig .= "$maptypes{$ptype} $pname";
			if ($maptypes{$ptype} eq $marshaltypes{$ptype}) {
				$call .= "$pname";
			} elsif (exists ($objects{$ptype}) || 
				 ($ptype =~ /GObject/)) {
				$call .= "$pname.Handle";
			} elsif ($ptype =~ /gchar/) {
				$call .= "Marshal.StringToHGlobalAnsi($pname)";
			} elsif ($marshaltypes{$ptype} = "int") {
				$call .= "(int) $pname";
			} else {
				die "Unexpected type encountered $ptype\n";
			}
		}
	}
	return ($call, $pinv, $sig);
}

# Code generation for signal handlers.
sub get_sighandler
{
	my ($def) = @_;
	my ($key, $name, $dir, $ns, $nspace, $tok);

	$def =~ /return-type \"(\w+)\"/;
	my $ret = $1;

	$def =~ /parameters'\((.*)\)\)\)/;
	my @parms = split(/\)'\(/, $1);

	for ($i = 1; $i < @parms; $i++) {
		$parms[$i] =~ /^\"(\w+)/;
		$key .= ":$1";
	}
	$key = $ret . $key;

	if (exists($sighandlers{$key})) {
		return $sighandlers{$key};
	}

	my ($call, $pinv, $sig) = gen_param_strings($def);

	if ($key =~ /Gtk/) {
		$dir = "../gtk/generated";
		$nspace = "Gtk";
	} elsif ($key =~ /Gdk/) {
		$dir = "../gdk/generated";
		$nspace = "Gdk";
	} else {
		$dir = "../glib/generated";
		$nspace = "GLib";
	}

	$name = "";
	foreach $tok (split(/:/, $key)) {
		if (exists($objects{$tok})) {
			$name .= "Object";
		} elsif (exists($maptypes{$tok})) {
			$name .= "$maptypes{$tok}";
		} else {
			die "Whassup with $tok?";
		}
	}
	my $sname = $name . "Signal";
	my $dname = $name . "Delegate";
	my $cbname = $name . "Callback";

	$sighandlers{$key} = $name;

	open (SIGFILE, ">$dir/$sname.cs") || die "can't open file";
	
	print SIGFILE "// Generated file: Do not modify\n\n";
	print SIGFILE "namespace GtkSharp {\n\n";
	foreach $ns (split (/,/, $usings{$nspace})) {
		print SIGFILE "\tusing $ns;\n";
	}
	print SIGFILE "\n\tpublic delegate $marshaltypes{$ret} ";
	print SIGFILE "$dname($pinv, int key);\n\n";
	print SIGFILE "\tpublic class $sname : SignalCallback {\n\n";
	print SIGFILE "\t\tprivate static $dname _Delegate;\n\n";
	print SIGFILE "\t\tprivate static $maptypes{$ret} ";
	print SIGFILE "$cbname($pinv, int key)\n\t\t{\n";
	print SIGFILE "\t\t\tif (!_Instances.Contains(key))\n";
	print SIGFILE "\t\t\t\tthrow new Exception(\"Unexpected signal key\");";
	print SIGFILE "\n\n\t\t\t$sname inst = ($sname) _Instances[key];\n";
	print SIGFILE "\t\t\tSignalArgs args = new SignalArgs ();\n";
	if ($def =~ /parameters'\((.*)\)\)\)/) {
		my (@parms) = split(/\)'\(/, $1);
		print "$sname pcnt=$#parms\n";
		for ($idx=0; $idx < $#parms; $idx++) {
			$parms[$idx+1] =~ s/\*//g;
			$parms[$idx+1] =~ /"(\S*)" "(\S*)"/;
			$ptype = $1;
			$pname = $2;
			$pname =~ s/object/objekt/;
			$pname =~ s/event/ev3nt/;
			print "$ptype $pname\n";
			if (exists($objects{$ptype})) {
				print SIGFILE "\t\t\targs.Args[$idx] = GLib.Object.GetObject($pname);\n";
			} elsif (exists($maptypes{$ptype})) {
				print SIGFILE "\t\t\targs.Args[$idx] = $pname;\n";
			} else { warn "Whassup wit $ptype?"; }
		}
	}
	print SIGFILE "\t\t\tinst._handler (inst._obj, args);\n";
	if ($ret ne "none") {
		print SIGFILE "\t\t\treturn ($maptypes{$ret}) args.RetVal;\n";
	}
	print SIGFILE "\t\t}\n\n";
	print SIGFILE "\t\t[DllImport(\"gobject-1.3.dll\", ";
	print SIGFILE "CharSet=CharSet.Ansi, ";
	print SIGFILE "CallingConvention=CallingConvention.Cdecl)]\n";
	print SIGFILE "\t\tstatic extern void g_signal_connect_data(";
	print SIGFILE "IntPtr obj, IntPtr name, $dname cb, int key, IntPtr p,";
	print SIGFILE " int flags);\n\n";
	print SIGFILE "\t\tpublic $sname(GLib.Object obj, IntPtr raw, ";
	print SIGFILE "String name, EventHandler eh) : base(obj, eh)";
	print SIGFILE "\n\t\t{\n\t\t\tif (_Delegate == null) {\n";
	print SIGFILE "\t\t\t\t_Delegate = new $dname($cbname);\n\t\t\t}\n";
	print SIGFILE "\t\t\tg_signal_connect_data(raw, ";
	print SIGFILE "Marshal.StringToHGlobalAnsi(name), _Delegate, _key, ";
	print SIGFILE "new IntPtr(0), 0);\n\t\t}\n\n";
	print SIGFILE "\t\t~$sname()\n{\t\t\t_Instances.Remove(_key);\n";
	print SIGFILE "\t\t\tif(_Instances.Count == 0) {\n";
	print SIGFILE "\t\t\t\t_Delegate = null;\n\t\t\t}\n\t\t}\n";
	print SIGFILE "\t}\n}\n";
	close (SIGFILE);

	return $sighandlers{$key};
}

#!/usr/bin/perl -w
#
# netdoc_pp.pl: .NET documentation preprocessor
#
# Author: Rachel Hestilow <hestilow@ximian.com> 
#
# <c> 2002 Rachel Hestilow

die "Usage: netdoc_pp.pl <infile1> ...\n" if not $ARGV[0];
use XML::LibXML;

my $parser = new XML::LibXML;
my (%namespaces, $assembly);

use Class::Struct;
struct( Klass => {
	assembly => '$',
	ctors => '%',
	methods => '%',
	props => '%',
	events => '%',
	fields => '%',
	data => '@',
});

foreach $file (@ARGV) {
	my $doc = $parser->parse_file ($file);
	my $node;
	for ($node = $doc->documentElement->firstChild; $node; $node = $node->nextSibling ()) {
		if ($node->nodeName eq "assembly") {
			$assembly = parseAssembly ($node);
		} elsif ($node->nodeName eq "members") {
			%namespaces = parseMembers ($node, $assembly);
		}
	}
}

generate ($assembly, %namespaces);

sub parseMembers {
	my ($members, $assembly) = @_;
	my %namespaces = ();
	for ($member = $members->firstChild; $member; $member = $member->nextSibling ()) {
		next if $member->nodeName ne "member";
		my @attrs = $member->attributes;
		my $name = $attrs[0]->value;
		die "Invalid member $name\n" if not ($name =~ /^([TMPEF]):([\w\.]+)\.([\w\#]+)(\((.*)\))?(\~([\w\.]+))?$/);
			 
		my ($type, $ns, $mname, $args, $op) = ($1, $2, $3, $5, $7);
		my ($klasses, $klass);
		my @data = ('', '', '');

		$mname .= " -> $op" if $op;

		if ($type eq 'T') {
			$klass = Klass->new (assembly => $assembly);
			if (not $namespaces{$ns}) {
				$klasses = {};
				$namespaces{$ns} = $klasses;
			} else {
				$klasses = $namespaces{$ns};
			}
			${$klasses}{$mname} = $klass;
		} else {
			my $klass_name = $ns;
			if ($ns =~ /(.+)\.(.+)/) {
				$ns = $1;
				$klass_name = $2;
			}
			if (not $namespaces{$ns}) {
				$klasses = {};
				$namespaces{$ns} = $klasses;
			} else {
				$klasses = $namespaces{$ns};
			}
			$klass = ${$klasses}{$klass_name};
			if (not $klass) {
				$klass = Klass->new (assembly => $assembly);
				${$klasses}{$klass_name} = $klass;
			}
		}

		my $subnode;
		for ($subnode = $member->firstChild; $subnode; $subnode= $subnode->nextSibling ()) {
			my $ind;
			if ($subnode->nodeName eq "summary") {
				$ind = 1; 
			} elsif ($subnode->nodeName eq "remarks") {
				$ind = 2;
			} else {
				next;
			}
			
			if ($subnode->textContent) {
				$data[$ind] = $subnode->textContent;
				$data[$ind] =~ s/^([\n\s])+//;
				$data[$ind] =~ s/([\n\s])+$//;
			}
		}
		
		if ($type eq "T") {
			my $d;
			foreach $d (@data) {
				$d = "" if not $d;
				push @{$klass->data}, $d;
			}
		} elsif ($type eq "M") {
			if ($mname eq "#ctor") {
				$args = "" if not $args;
				${$klass->ctors}{$args} = \@data;
			} else {
				$args = "" if not $args;
				$data[0] = $args;
				${$klass->methods}{$mname} = \@data;
			}
		} elsif ($type eq "P") {
			${$klass->props}{$mname} = \@data;
		} elsif ($type eq "E") {
			${$klass->events}{$mname} = \@data;
		} elsif ($type eq "F") {
			${$klass->fields}{$mname} = \@data;
		}
	}
	return %namespaces;
}

sub parseAssembly
{
	my ($parent) = @_;
	for ($node = $parent->firstChild; $node; $node = $node->nextSibling ()) {
		next if $node->nodeName ne 'name';
		return $node->textContent;
	}
}

sub addComments {
	my ($doc, $node, @data) = @_;
	if (not ($data[1] =~ /^\s*$/)) {
		$elem = $doc->createElement ("summary");
		$node->appendChild ($elem);
		$elem->appendChild (XML::LibXML::Text->new ($data[1]));
	}
	$elem = $doc->createElement ("remarks");
	$node->appendChild ($elem);
	$elem->appendChild (XML::LibXML::Text->new ($data[2]));
}

sub generate {
	my ($asm, %namespaces) = @_;
	my $doc = XML::LibXML::Document->new ();
	my $root = $doc->createElement ('doc');
	$doc->setDocumentElement ($root);

	print STDERR "asm $asm\n";
	$root->setAttribute ('assembly', $asm);

	my ($ns, $ns_elem);
	foreach $ns (sort keys %namespaces) {
		$ns_elem = $doc->createElement ('namespace');
		$ns_elem->setAttribute ('name', $ns);
		$root->appendChild ($ns_elem);

		my ($klass_name, $klass, $klass_elem, $klasses);
		$klasses = $namespaces{$ns};
		foreach $klass_name (sort keys %$klasses)
		{
			$klass = ${$klasses}{$klass_name};
			$klass_elem = $doc->createElement ('class');
			$klass_elem->setAttribute ('name', $klass_name);
			$klass_elem->setAttribute ('assembly', $klass->assembly);

			my ($elem, $ctor, $prop, $method, $event);

			if ($klass->data and @{$klass->data}) {
				addComments ($doc, $klass_elem, @{$klass->data});
			}

			foreach $ctor (sort keys %{$klass->ctors}) {
				$elem = $doc->createElement ('constructor');
				$elem->setAttribute ('name', $klass_name);
				$elem->setAttribute ('args', $ctor);
				my @data = @{${$klass->ctors}{$ctor}};
				addComments ($doc, $elem, @data);
				$klass_elem->appendChild ($elem);
			}
			
			foreach $method (sort keys %{$klass->methods}) {
				$elem = $doc->createElement ('method');
				$method =~ s/\#/\./g;
				$elem->setAttribute ('name', $method);
				my @data = @{${$klass->methods}{$method}};
				$elem->setAttribute ('args', $data[0]);
				addComments ($doc, $elem, @data);
				$klass_elem->appendChild ($elem);
			}

			foreach $prop (sort keys %{$klass->props}) {
				$elem = $doc->createElement ('property');
				$elem->setAttribute ('name', $prop);
				my @data = @{${$klass->props}{$prop}};
				addComments ($doc, $elem, @data);
				$klass_elem->appendChild ($elem);
			}
			
			foreach $event (sort keys %{$klass->events}) {
				$elem = $doc->createElement ('event');
				$elem->setAttribute ('name', $event);
				my @data = @{${$klass->events}{$event}};
				addComments ($doc, $elem, @data);
				$klass_elem->appendChild ($elem);
			}
			
			foreach $field (sort keys %{$klass->fields}) {
				$elem = $doc->createElement ('field');
				$elem->setAttribute ('name', $field);
				my @data = @{${$klass->fields}{$field}};
				addComments ($doc, $elem, @data);
				$klass_elem->appendChild ($elem);
			}

			$ns_elem->appendChild ($klass_elem);
		}
	}
	
	print $doc->toString();	
}

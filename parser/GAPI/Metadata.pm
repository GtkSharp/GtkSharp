#
# Metadata.pm: Adds additional properties to a GApi tree. 
#
# Author: Rachel Hestilow  <hestilow@ximian.com> 
#
# <c> 2002 Rachel Hestilow
##############################################################

package GAPI::Metadata;

use XML::LibXML;

sub new {
	my $namespace = $_[1];
	my $file = "$namespace.metadata";
	my $self = {};
	@{$self->{rules}} = ();
	$self->{metadata} = $namespace;
	bless $self;
	$self->load ($file);
	return $self;
}

sub parseClass {
	my ($node, $classes) = @_;
	my %methods = ();
	my %signals = ();
	my %properties = ();
	my @attrs = $node->attributes;
	my $class_name = $attrs[0]->value;
	${$classes}{$class_name} = [\%methods, \%signals, \%properties];

	for ($method_node = $node->firstChild; $method_node != undef; $method_node = $method_node->nextSibling ()) {
		if ($method_node->nodeName eq "method" or $method_node->nodeName eq "constructor") {
			$methods{$method_node->firstChild->nodeValue} = 1;
		} elsif ($method_node->nodeName eq "signal") {
			$signals{$method_node->firstChild->nodeValue} = 1;
		} elsif ($method_node->nodeName eq "property") {
			$properties{$method_node->firstChild->nodeValue} = 1;
		}	
	}
}

sub parseData {
	my $node = $_[0];
	my @data = ();
	for ($data_node = $node->firstChild; $data_node != undef; $data_node = $data_node->nextSibling ()) {
		next if $data_node->nodeName ne "attribute";
		my @attrs = $data_node->attributes;
		my $target = $attrs[0]->value;
		my ($filter_level, $filter_value, $attr_name, $attr_value);
		my $directives = "";
		for ($attr_node = $data_node->firstChild; $attr_node != undef; $attr_node = $attr_node->nextSibling ()) {
			if ($attr_node->nodeName eq "filter") {
				my @filter_attrs = $attr_node->attributes;
				$filter_level = $filter_attrs[0]->value;
				$filter_value = $attr_node->firstChild->nodeValue;
			} elsif ($attr_node->nodeName eq "name") {
				$attr_name = $attr_node->firstChild->nodeValue;
			} elsif ($attr_node->nodeName eq "value") {
				$attr_value = $attr_node->firstChild->nodeValue;
			} elsif ($attr_node->nodeName eq "delete") {
				$directives = "delete";
			}
		}
		my @data_attr = ("attribute", $target, "filter", $filter_level, $filter_value, $attr_name, $attr_value, $directives);
		push @data, \@data_attr;
	}

	return @data;
}
				
sub load {
	my ($self, $file) = @_;
	my $parser = new XML::LibXML;
	my $doc = $parser->parse_file($file);
	my $root = $doc->documentElement;
	for ($rule_node = $root->firstChild; $rule_node != undef; $rule_node = $rule_node->nextSibling ()) {
		next if $rule_node->nodeName ne "rule";
		my %classes = ();
		my @data;
		for ($node = $rule_node->firstChild; $node != undef; $node = $node->nextSibling ()) {
			if ($node->nodeName eq "class") {
				parseClass ($node, \%classes);
			} elsif ($node->nodeName eq "data") {
				@data = parseData ($node);	
			}
		}

		push @{$self->{rules}}, [\%classes, \@data];
	}
}

sub fixupParams {
	my ($method_node, $data_list_ref) = @_;
	my ($params_node, $node);

	foreach $data (@$data_list_ref) {
		if ($$data[1] eq "method" or $$data[1] eq "signal") {
			$method_node->setAttribute ($$data[5], $$data[6]);
			next;
		}

		for ($node = $method_node->firstChild; $node; $node = $node->nextSibling ()) {
			if ($node->nodeName eq "parameters") {
				$params_node = $node;
				last;
			} elsif ($node->nodeName eq "return-type" and $$data[1] eq "return") {
				$node->setAttribute ($$data[5], $$data[6]);
				last;
			}
		}
		next if not $params_node;
		if ($$data[1] eq "parameters") {
			$params_node->setAttribute ($$data[5], $$data[6]);
			next;
		}


		for ($node = $params_node->firstChild; $node; $node = $node->nextSibling ()) {
			my $param_type;
			my $param_name;

			foreach $attr ($node->attributes) {
				if ($attr->name eq "type") {
					$param_type = $attr->value;
				} elsif ($attr->name eq "name") {
					$param_name = $attr->value;
				}
			}
	
			if ($$data[3] eq "type") {
				if ($param_type eq $$data[4]) {
					$node->setAttribute ($$data[5], $$data[6]);
				}
			} elsif ($$data[3] eq "name") {
				if ($param_name eq $$data[4]) {
					$node->setAttribute ($$data[5], $$data[6]);
				}
			}
		}
	}
}

sub myGetChildrenByTagName {
   my ($node, $tagname) = @_;
   my ($child);
   my (@nodes) = ();
   for ($child = $node->firstChild; $child; $child = $child->nextSibling ()) {
   	if ($child->nodeName eq $tagname) {
		push @nodes, $child;		
	}
   }
   return @nodes;
}

sub addClassData {
    my ($doc, $node, $class, $data_list_ref) = @_;

    foreach $data (@$data_list_ref) {
	if ($$data[1] eq "class") {
		if ($$data[7] eq "delete") {
			my $parent = $node->parentNode;
			$parent->removeChild ($node);
			return;
		}
	    # my copy of XML::LibXML doesn't have this method.
	    #my @nodes = $node->getChildrenByTagName ($$data[5]);
	    my @nodes = myGetChildrenByTagName ($node, $$data[5]);

	    if (0 == scalar @nodes) {
		$node->setAttribute ($$data[5], $$data[6]);
	    }
	    next;
	}
    }
}

sub fixupNamespace {
	my ($self, $doc, $ns_node) = @_;
	my $node;
	foreach $rule (@{$self->{rules}}) {
		my ($classes_ref, $data_list_ref) = @$rule;
		for ($node = $ns_node->firstChild; $node; $node = $node->nextSibling ()) {
			next if not ($node->nodeName eq "object" or $node->nodeName eq "interface" or $node->nodeName eq "struct" or $node->nodeName eq "boxed" or $node->nodeName eq "callback");
			my $class, $methods_ref, $attr;
			foreach $attr ($node->attributes) {
				if ($attr->name eq "cname") {
					$class = $attr->value;
					last;
				}
			}

			my %classes = %$classes_ref;
			$methods_ref = $classes{$class}[0];
			$signals_ref = $classes{$class}[1];
			$properties_ref = $classes{$class}[2];

			if ({%$classes_ref}->{$class}) {
			    addClassData ($doc, $node, $class, $data_list_ref);
			}

			next if not ($methods_ref or $signals_ref or $properties_ref);

			for ($method_node = $node->firstChild; $method_node; $method_node = $method_node->nextSibling ()) {
				next if not ($method_node->nodeName eq "method" or $method_node->nodeName eq "constructor");
				my $method;
				foreach $attr ($method_node->attributes) {
					if (($attr->name eq "name" and $method_node->nodeName eq "method") or ($attr->name eq "cname" and $method_node->nodeName eq "constructor")) {
						$method = $attr->value;
						last;
					}
				}
				next if not ${%$methods_ref}{$method};

				fixupParams ($method_node, $data_list_ref);
			}
			
			for ($signal_node = $node->firstChild; $signal_node; $signal_node = $signal_node->nextSibling ()) {
				next if $signal_node->nodeName ne "signal";
				my $signal;
				foreach $attr ($signal_node->attributes) {
					if ($attr->name eq "name") {
						$signal = $attr->value;
						last;
					}
				}
				next if not ${$signals_ref}{$signal};

				fixupParams ($signal_node, $data_list_ref);
			}

			for ($property_node = $node->firstChild; $property_node; $property_node = $property_node->nextSibling ()) {
				next if $property_node->nodeName ne "property";
				my $property;
				foreach $attr ($property_node->attributes) {
					if ($attr->name eq "name") {
						$property = $attr->value;
						last;
					}
				}
				next if not ${$properties_ref}{$property};

				foreach $data (@$data_list_ref) {
					if ($$data[1] eq "property") {
						$property_node->setAttribute ($$data[5], $$data[6]);
						next;
					}
				}

			}

		}
	}
}

sub fixup {
	my $doc = $_[0];
	my ($api_node, $ns_node);
	my $metadata = undef;

	$api_node = $doc->documentElement;
	return if not ($api_node and $api_node->nodeName eq "api");
	for ($ns_node = $api_node->firstChild; $ns_node; $ns_node = $ns_node->nextSibling ()) {
		next if $ns_node->nodeName ne "namespace";
		next if not $ns_node->attributes;
		my @attrs = $ns_node->attributes;
		next if not @attrs;
		my $namespace = $attrs[0]->value;
		if (-f "$namespace.metadata") {
			if (not ($metadata and $metadata->{namespace} eq $namespace)) {
				$metadata = new GAPI::Metadata ($namespace);
			}
			$metadata->fixupNamespace ($doc, $ns_node);
		}
	}
}

sub output {
	my $self = $_[0];
	$rule_num = 0;
	foreach $rule (@{$self->{rules}}) {
		print "Rule #$rule_num:\n";
		my ($classes_ref, $data_list_ref) = @$rule;
		my %classes = %$classes_ref;
		my @data_list = @$data_list_ref;
		foreach $class (keys (%classes)) {
			print "\tClass $class:\n";
			foreach $method (keys %{$classes{$class}}) {
				print "\t\tMethod $method\n";
			}
		}
		print "\tData:\n";
		foreach $data (@data_list) {
			printf "\t\tAdd an %s to all %s of %s %s: %s=%s\n",
			       $$data[0], $$data[1], $$data[3], $$data[4], $$data[5], $$data[6];
		}
		$rule_num++;
	}
}

1;

#
# Metadata.pm: Adds additional properties to a GApi tree. 
#
# Author: Rachel Hestilow  <hestilow@ximian.com> 
#
# <c> 2002 Rachel Hestilow
##############################################################

package Metadata;

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
	my @attrs = $node->attributes;
	my $class_name = $attrs[0]->value;
	${$classes}{$class_name} = \%methods;

	for ($method_node = $node->firstChild; $method_node != undef; $method_node = $method_node->nextSibling ()) {
		next if $method_node->nodeName ne "method";
		$methods{$method_node->firstChild->nodeValue} = 1;
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
		for ($attr_node = $data_node->firstChild; $attr_node != undef; $attr_node = $attr_node->nextSibling ()) {
			if ($attr_node->nodeName eq "filter") {
				my @filter_attrs = $attr_node->attributes;
				$filter_level = $filter_attrs[0]->value;
				$filter_value = $attr_node->firstChild->nodeValue;
			} elsif ($attr_node->nodeName eq "name") {
				$attr_name = $attr_node->firstChild->nodeValue;
			} elsif ($attr_node->nodeName eq "value") {
				$attr_value = $attr_node->firstChild->nodeValue;
			}
		}
		my @data_attr = ("attribute", $target, "filter", $filter_level, $filter_value, $attr_name, $attr_value);
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
	for ($node = $method_node->firstChild; $node; $node = $node->nextSibling ()) {
		if ($node->nodeName eq "parameters") {
			$params_node = $node;
			last;
		}
	}
	return if not $params_node;
	for ($node = $params_node->firstChild; $node; $node = $node->nextSibling ()) {
		my $param_type;
		foreach $attr ($node->attributes) {
			if ($attr->name eq "type") {
				$param_type = $attr->value;
				last;
			}
		}

		foreach $data (@$data_list_ref) {
			if ($param_type eq $$data[4]) {
				$node->setAttribute ($$data[5], $$data[6]);
			}
		}
	}
}

sub fixupNamespace {
	my ($self, $ns_node) = @_;
	my $node;
	foreach $rule (@{$self->{rules}}) {
		my ($classes_ref, $data_list_ref) = @$rule;
		for ($node = $ns_node->firstChild; $node; $node = $node->nextSibling ()) {
			next if $node->nodeName ne "object";
			my $class, $methods_ref, $attr;
			foreach $attr ($node->attributes) {
				if ($attr->name eq "cname") {
					$class = $attr->value;
					last;
				}
			}

			my %classes = %$classes_ref;
			$methods_ref = $classes{$class};
			next if not $methods_ref;

			for ($method_node = $node->firstChild; $method_node; $method_node = $method_node->nextSibling ()) {
				next if $method_node->nodeName ne "method";
				my $method;
				foreach $attr ($method_node->attributes) {
					if ($attr->name eq "name") {
						$method = $attr->value;
						last;
					}
				}
				next if not ${$methods_ref}{$method};
				fixupParams ($method_node, $data_list_ref);
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
		next if not ($ns_node->attributes and (scalar (@{$ns_node->attributes})) + 1);
		my @attrs = $ns_node->attributes;
		my $namespace = $attrs[0]->value;
		if (-f "$namespace.metadata") {
			if (not ($metadata and $metadata->{namespace} eq $namespace)) {
				$metadata = new Metadata ($namespace);
			}
			$metadata->fixupNamespace ($ns_node);
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

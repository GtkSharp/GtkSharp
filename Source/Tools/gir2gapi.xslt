<?xml version="1.0" encoding="UTF-8"?>

<!--
//
// gir2gapi.xslt
//
// This stylesheet converts gir to gapi format
//
//
//
//
// Author:
//   Andreia Gaita (shana@spoiledcat.net)
//   Stephan Sundermann (stephansundermann@gmail.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
-->

<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
	xmlns:exsl="http://exslt.org/common" xmlns:gir="http://www.gtk.org/introspection/core/1.0"
	xmlns:c="http://www.gtk.org/introspection/c/1.0"
	xmlns:glib="http://www.gtk.org/introspection/glib/1.0"
	exclude-result-prefixes="xsl exsl gir c glib">
	<xsl:output method="xml" version="1.0" encoding="UTF-8" indent="yes"/>
	<xsl:strip-space elements="*"/>

	<!--- maps svg names into xaml names -->
	<xsl:variable name="mappings">
		<mappings>
			<mapping from="default" to="_default"/>
			<mapping from="object" to="_object"/>
			<mapping from="GObjectObject" to="GObject"/>
			<mapping from="ret" to="_ret"/>
			<mapping from="result" to="_result"/>
		</mappings>
	</xsl:variable>

	<xsl:variable name="consttypes">
		<types>
			<type name="char*"/>
			<type name="gchar*"/>
			<type name="gchar**"/>
			<type name="gfilename*"/>

			<ctor>
				<type name="GType"/>
				<type name="gpointer"/>
			</ctor>
		</types>
	</xsl:variable>

	<xsl:variable name="typemappings">
		<mappings>
			<mapping from="gchararray" to="gchar*"/>
			<mapping from="any*" to="gpointer*"/>
			<mapping from="utf8" to="gchar*"/>
			<mapping from="none" to="void"/>
			<mapping from="any*" to="gpointer*"/>
			<mapping from="GPtrArray*" to="GPtrArray"/>
			<mapping from="filename" to="gfilename*"/>
		</mappings>
	</xsl:variable>
	<!-- START HERE -->

	<xsl:template match="/">
		<xsl:apply-templates select="gir:repository"/>
	</xsl:template>

	<xsl:template match="gir:repository">
		<xsl:variable name="external">
			<xsl:apply-templates select="gir:include"/>
		</xsl:variable>

		<!--
		<bla>
			<xsl:call-template name="map-gtype">
				<xsl:with-param name="type">Soup.Message</xsl:with-param>
				<xsl:with-param name="external" select="$external"/>
			</xsl:call-template>
		</bla>

		<bleh>
			<xsl:call-template name="output-type">
				<xsl:with-param name="nodename">return-type</xsl:with-param>
				<xsl:with-param name="typename">Soup.Message</xsl:with-param>
				<xsl:with-param name="type"/>
				<xsl:with-param name="transfer-ownership">none</xsl:with-param>
				<xsl:with-param name="doname">1</xsl:with-param>
				<xsl:with-param name="external" select="$external" />
			</xsl:call-template>
		</bleh>
-->
		<api parser_version="3">
			<xsl:apply-templates select="gir:namespace">
				<xsl:with-param name="external" select="$external"/>
			</xsl:apply-templates>
		</api>

	</xsl:template>

	<xsl:template match="gir:include">
		<include name="{@name}">
			<xsl:copy-of select="document(concat('/usr/share/gir-1.0/', @name, '-', @version, '.gir'), .)"/>
		</include>
	</xsl:template>

	<xsl:template match="gir:namespace">
		<xsl:param name="external"/>

		<xsl:variable name="split1">
			<xsl:value-of select="substring-after(@shared-library, 'lib')"/>
		</xsl:variable>
		<xsl:variable name="library">
			<xsl:value-of select="substring-before($split1, '.so')"/>
		</xsl:variable>
		<namespace name="{@name}" library="{$library}">
			<xsl:apply-templates select="gir:alias">
				<xsl:with-param name="external" select="$external"/>
			</xsl:apply-templates>
			<xsl:apply-templates select="gir:enumeration">
				<xsl:with-param name="external" select="$external"/>
			</xsl:apply-templates>
			<xsl:apply-templates select="gir:bitfield">
				<xsl:with-param name="external" select="$external"/>
			</xsl:apply-templates>
			<xsl:apply-templates select="gir:callback">
				<xsl:with-param name="external" select="$external"/>
			</xsl:apply-templates>
			<xsl:apply-templates select="gir:interface">
				<xsl:with-param name="external" select="$external"/>
			</xsl:apply-templates>
			<xsl:apply-templates select="gir:class">
				<xsl:with-param name="external" select="$external"/>
			</xsl:apply-templates>
			<xsl:apply-templates select="gir:union">
				<xsl:with-param name="external" select="$external"/>
			</xsl:apply-templates>
			<xsl:apply-templates select="gir:record[not(@glib:is-gtype-struct-for)]">
				<xsl:with-param name="external" select="$external"/>
			</xsl:apply-templates>


			<object name="Global" cname="{@name}Global" opaque="true">
				<xsl:for-each select="gir:function[not(@introspectable=0)]">
					<xsl:call-template name="output-method">
						<xsl:with-param name="shared">true</xsl:with-param>
						<xsl:with-param name="external" select="$external"/>
					</xsl:call-template>
				</xsl:for-each>
				<xsl:for-each select="gir:method[not(@introspectable=0)]">
					<xsl:call-template name="output-method">
						<xsl:with-param name="shared">true</xsl:with-param>
						<xsl:with-param name="external" select="$external"/>
					</xsl:call-template>
				</xsl:for-each>
			</object>

			<object name="Constants" cname="{@name}Constants" opaque="true">
				<xsl:for-each select="gir:constant">
					<constant value="{@value}">
						<xsl:call-template name="map-gtype">
							<xsl:with-param name="type" select="gir:type/@c:type"/>
							<xsl:with-param name="nopointer">1</xsl:with-param>
							<xsl:with-param name="external" select="$external"/>
						</xsl:call-template>
						<xsl:attribute name="name">
							<xsl:call-template name="sanitize">
								<xsl:with-param name="name">
									<xsl:call-template name="map-name">
										<xsl:with-param name="name" select="@name"/>
									</xsl:call-template>
								</xsl:with-param>
							</xsl:call-template>
						</xsl:attribute>
						<xsl:call-template name="set-version-and-deprecated">
							<xsl:with-param name="version" select="@version"/>
							<xsl:with-param name="deprecated" select="@deprecated"/>
							<xsl:with-param name="deprecated-version" select="@deprecated-version"/>
						</xsl:call-template>
					</constant>
				</xsl:for-each>
			</object>
		</namespace>
	</xsl:template>


	<xsl:template match="gir:class | gir:interface">
		<xsl:param name="external"/>

		<xsl:variable name="ptype">
			<type>
				<xsl:call-template name="map-gtype">
					<xsl:with-param name="type" select="@parent"/>
					<xsl:with-param name="external" select="$external"/>
				</xsl:call-template>
			</type>
		</xsl:variable>

		<xsl:variable name="parent">
			<xsl:value-of select="exsl:node-set($ptype)/type/@gtype"/>
		</xsl:variable>

		<xsl:variable name="type">
			<xsl:call-template name="map-type">
				<xsl:with-param name="type" select="@c:type"/>
				<xsl:with-param name="name" select="@name"/>
				<xsl:with-param name="external" select="$external"/>
			</xsl:call-template>
		</xsl:variable>

		<xsl:variable name="name">
			<xsl:call-template name="sanitize">
				<xsl:with-param name="name" select="@name"/>
			</xsl:call-template>
		</xsl:variable>

		<xsl:variable name="opaque">
			<xsl:choose>
				<xsl:when
					test="contains(gir:doc/text(), 'opaque') or @disguised='1' or @glib:fundamental='1'"
					>true</xsl:when>
				<xsl:otherwise>false</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>

		<xsl:variable name="hidden">
			<xsl:choose>
				<xsl:when test="@private='1'">true</xsl:when>
				<xsl:otherwise>false</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>

		<xsl:variable name="nodename">
			<xsl:choose>
				<xsl:when test="name()='interface'">interface</xsl:when>
				<xsl:when test="@glib:fundamental='1'">struct</xsl:when>
				<xsl:otherwise>object</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>

		<xsl:element name="{$nodename}">
			<xsl:attribute name="name">
				<xsl:value-of select="$name"/>
			</xsl:attribute>
			<xsl:attribute name="cname">
				<xsl:value-of select="exsl:node-set($type)/type/@gtype"/>
			</xsl:attribute>
			<xsl:if test="@abstract=1">
				<xsl:attribute name="defaultconstructoraccess">protected</xsl:attribute>
			</xsl:if>
			<xsl:attribute name="opaque">
				<xsl:value-of select="$opaque"/>
			</xsl:attribute>
			<xsl:attribute name="hidden">
				<xsl:value-of select="$hidden"/>
			</xsl:attribute>

			<xsl:if test="not(@glib:type-struct) and $nodename='interface'">
				<xsl:attribute name="consume_only">true</xsl:attribute>
			</xsl:if>

			<xsl:if test="@parent">
				<xsl:attribute name="parent">
					<xsl:value-of select="$parent"/>
				</xsl:attribute>
			</xsl:if>
			
			<xsl:call-template name="set-version-and-deprecated">
				<xsl:with-param name="version" select="@version"/>
				<xsl:with-param name="deprecated" select="@deprecated"/>
				<xsl:with-param name="deprecated-version" select="@deprecated-version"/>
			</xsl:call-template>

			<xsl:if test="gir:implements">
				<implements>
					<xsl:for-each select="gir:implements">
						<xsl:variable name="interfacetype">
							<xsl:call-template name="map-type">
								<xsl:with-param name="name" select="@name"/>
								<xsl:with-param name="external" select="$external"/>
							</xsl:call-template>
						</xsl:variable>
						<interface cname="{exsl:node-set($interfacetype)/type/@gtype}"/>
					</xsl:for-each>
				</implements>
			</xsl:if>

			<xsl:variable name="type-struct" select="@glib:type-struct"/>
			<xsl:if test="@glib:type-struct">
				<xsl:apply-templates select="//gir:record[@name=$type-struct]">
					<xsl:with-param name="external" select="$external"/>
					<xsl:with-param name="class-struct-for" select="current()"/>
				</xsl:apply-templates>
			</xsl:if>
			<method name="GetType" cname="{@glib:get-type}" shared="true">
				<return-type type="GType"/>
			</method>

			<xsl:if test="@glib:get-value-func">
				<constructor cname="{@glib:get-value-func}">
					<parameters>
						<parameter name="value" type="GValue*"/>
					</parameters>
				</constructor>
			</xsl:if>

			<xsl:if test="@glib:set-value-func">
				<method name="SetGValue" cname="{@glib:set-value-func}" shared="true">
					<return-type type="void"/>
					<parameters>
						<parameter name="value" type="GValue*" pass_as="ref"/>
						<parameter name="obj" type="{exsl:node-set($type)/type/@gtype}*"/>
					</parameters>
				</method>
			</xsl:if>

			<xsl:if test="@glib:ref-func">
				<method name="Ref" cname="{@glib:ref-func}">
					<return-type type="{exsl:node-set($type)/type/@gtype}*" owned="true"/>
					<parameters/>
				</method>
			</xsl:if>

			<xsl:if test="@glib:unref-func">
				<method name="Unref" cname="{@glib:unref-func}">
					<return-type type="void"/>
					<parameters/>
				</method>
			</xsl:if>

			<xsl:apply-templates>
				<xsl:with-param name="external" select="$external"/>
			</xsl:apply-templates>
		</xsl:element>
	</xsl:template>

	<xsl:template match="gir:union">
		<xsl:param name="external"/>

		<xsl:variable name="type">
			<xsl:call-template name="map-type">
				<xsl:with-param name="type" select="@c:type"/>
				<xsl:with-param name="name" select="@name"/>
				<xsl:with-param name="external" select="$external"/>
			</xsl:call-template>
		</xsl:variable>

		<xsl:variable name="name">
			<xsl:call-template name="sanitize">
				<xsl:with-param name="name" select="@name"/>
			</xsl:call-template>
		</xsl:variable>

		<union name="{$name}" cname="{exsl:node-set($type)/type/@ctype}">
			<xsl:call-template name="set-version-and-deprecated">
				<xsl:with-param name="version" select="@version"/>
				<xsl:with-param name="deprecated" select="@deprecated"/>
				<xsl:with-param name="deprecated-version" select="@deprecated-version"/>
			</xsl:call-template>
			<xsl:apply-templates>
				<xsl:with-param name="external" select="$external"/>
			</xsl:apply-templates>
		</union>
	</xsl:template>

	<xsl:template match="gir:record">
		<xsl:param name="external"/>
		<xsl:param name="class-struct-for"/>
		<xsl:choose>
			<xsl:when test="not(@glib:is-gtype-struct-for)">
				<xsl:variable name="type">
					<xsl:call-template name="map-type">
						<xsl:with-param name="type" select="@c:type"/>
						<xsl:with-param name="name" select="@name"/>
						<xsl:with-param name="external" select="$external"/>
					</xsl:call-template>
				</xsl:variable>

				<xsl:variable name="name">
					<xsl:call-template name="sanitize">
						<xsl:with-param name="name" select="@name"/>
					</xsl:call-template>
				</xsl:variable>

				<xsl:variable name="opaque">
					<xsl:choose>
						<xsl:when test="contains(gir:doc/text(), 'opaque') or @disguised='1'"
							>true</xsl:when>
						<xsl:otherwise>false</xsl:otherwise>
					</xsl:choose>
				</xsl:variable>

				<xsl:variable name="hidden">
					<xsl:choose>
						<xsl:when test="@private='1' or contains(@name, 'Private')">true</xsl:when>
						<xsl:otherwise>false</xsl:otherwise>
					</xsl:choose>
				</xsl:variable>

				<xsl:variable name="nodename">
					<xsl:choose>
						<xsl:when test="@glib:get-type">boxed</xsl:when>
						<xsl:otherwise>struct</xsl:otherwise>
					</xsl:choose>
				</xsl:variable>
				<xsl:element name="{$nodename}">
					<xsl:attribute name="name">
						<xsl:value-of select="$name"/>
					</xsl:attribute>
					<xsl:attribute name="cname">
						<xsl:value-of select="exsl:node-set($type)/type/@gtype"/>
					</xsl:attribute>
					<xsl:attribute name="opaque">
						<xsl:value-of select="$opaque"/>
					</xsl:attribute>
					<xsl:attribute name="hidden">
						<xsl:value-of select="$hidden"/>
					</xsl:attribute>
					<xsl:call-template name="set-version-and-deprecated">
						<xsl:with-param name="version" select="@version"/>
						<xsl:with-param name="deprecated" select="@deprecated"/>
						<xsl:with-param name="deprecated-version" select="@deprecated-version"/>
					</xsl:call-template>
					<xsl:if test="$nodename='boxed'">
						<method name="GetType" cname="{@glib:get-type}" shared="true">
							<return-type type="GType"/>
						</method>
					</xsl:if>
					<xsl:apply-templates>
						<xsl:with-param name="external" select="$external"/>
					</xsl:apply-templates>
				</xsl:element>
			</xsl:when>
			<xsl:otherwise>
				<class_struct>
					<xsl:attribute name="cname">
						<xsl:value-of select="@c:type"/>
					</xsl:attribute>
					<xsl:call-template name="set-version-and-deprecated">
						<xsl:with-param name="version" select="@version"/>
						<xsl:with-param name="deprecated" select="@deprecated"/>
						<xsl:with-param name="deprecated-version" select="@deprecated-version"/>
					</xsl:call-template>
					<xsl:for-each select="gir:field">
						<xsl:apply-templates select="current()">
							<xsl:with-param name="external" select="$external"/>
						</xsl:apply-templates>
						<xsl:if test="gir:callback">
							<method>
								<xsl:choose>
									<xsl:when
										test="$class-struct-for/glib:signal[current()/@name = translate(@name, '-', '_')]">
										<xsl:attribute name="signal_vm">
											<xsl:value-of select="@name"/>
										</xsl:attribute>
									</xsl:when>
									<xsl:otherwise>
										<xsl:attribute name="vm">
											<xsl:value-of select="@name"/>
										</xsl:attribute>
									</xsl:otherwise>
								</xsl:choose>
							</method>
						</xsl:if>
					</xsl:for-each>
				</class_struct>

				<!-- We have to convert all field/callback when there is no method in the original class to virtual_methods -->
				<xsl:for-each select="gir:field/gir:callback">
					<xsl:if test="not($class-struct-for/gir:virtual-method[@name=current()/@name])">
						<xsl:variable name="introspectable">
							<xsl:choose>
								<xsl:when test="../@introspectable">0</xsl:when>
								<xsl:otherwise>1</xsl:otherwise>
							</xsl:choose>
						</xsl:variable>

						<xsl:call-template name="output-method">
							<xsl:with-param name="cname" select="@name"/>
							<xsl:with-param name="nodename">virtual_method</xsl:with-param>
							<xsl:with-param name="shared">true</xsl:with-param>
							<xsl:with-param name="introspectable" select="$introspectable"/>
							<xsl:with-param name="external" select="$external"/>
						</xsl:call-template>
						
					</xsl:if>
				</xsl:for-each>

				<xsl:for-each select="gir:method[not(@introspectable=0)]">
					<xsl:call-template name="output-method">
						<xsl:with-param name="shared">true</xsl:with-param>
						<xsl:with-param name="external" select="$external"/>
					</xsl:call-template>
				</xsl:for-each>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template match="gir:namespace/gir:method[not(@introspectable=0)]"/>

	<xsl:template match="gir:method[not(@introspectable=0)]">
		<xsl:param name="external"/>

		<xsl:call-template name="output-method">
			<xsl:with-param name="external" select="$external"/>
		</xsl:call-template>
		
	</xsl:template>

	<xsl:template match="gir:virtual-method">
		<xsl:param name="external"/>

		<xsl:variable name="introspectable">
			<xsl:choose>
				<xsl:when test="@introspectable=0">0</xsl:when>
				<xsl:otherwise>1</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<xsl:if test="not(../glib:signal[@name = translate(current()/@name,'_','-')])">
			<xsl:call-template name="output-method">
				<xsl:with-param name="nodename">virtual_method</xsl:with-param>
				<xsl:with-param name="cname" select="@name"/>
				<xsl:with-param name="introspectable" select="$introspectable"/>
				<xsl:with-param name="external" select="$external"/>
			</xsl:call-template>
		</xsl:if>
		
	</xsl:template>

	<xsl:template match="gir:namespace/gir:function[not(@introspectable=0)]"/>

	<xsl:template match="gir:function[not(@introspectable=0)]">
		<xsl:param name="external"/>

		<xsl:call-template name="output-method">
			<xsl:with-param name="shared">true</xsl:with-param>
			<xsl:with-param name="external" select="$external"/>
		</xsl:call-template>
	</xsl:template>

	<xsl:template match="gir:callback">
		<xsl:param name="shared">false</xsl:param>
		<xsl:param name="nodename">callback</xsl:param>
		<xsl:param name="doname">1</xsl:param>
		<xsl:param name="external"/>

		<xsl:variable name="name">
			<xsl:call-template name="capitalize">
				<xsl:with-param name="string" select="@name"/>
			</xsl:call-template>
		</xsl:variable>
		<xsl:variable name="type">
			<xsl:choose>
				<xsl:when test="@c:type">
					<xsl:value-of select="@c:type"/>
				</xsl:when>
				<xsl:otherwise><xsl:value-of select="//gir:repository/gir:namespace/@name"
						/><xsl:value-of select="$name"/>Func</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<xsl:element name="{$nodename}">
			<xsl:if test="@introspectable">
				<xsl:attribute name="hidden">
					<xsl:value-of select="@introspectable"/>
				</xsl:attribute>
			</xsl:if>
			<xsl:attribute name="name">
				<xsl:value-of select="$name"/>
			</xsl:attribute>
			<xsl:attribute name="cname">
				<xsl:choose>
					<xsl:when test="$doname='0'">
						<xsl:value-of select="@name"/>
					</xsl:when>
					<xsl:otherwise>
						<xsl:value-of select="$type"/>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:attribute>
			<xsl:if test="$shared = 'true'">
				<xsl:attribute name="shared">true</xsl:attribute>
			</xsl:if>
			<xsl:call-template name="set-version-and-deprecated">
				<xsl:with-param name="version" select="@version"/>
				<xsl:with-param name="deprecated" select="@deprecated"/>
				<xsl:with-param name="deprecated-version" select="@deprecated-version"/>
			</xsl:call-template>

			<!-- xsl:if test="gir:return-value/gir:type/@name != 'none'" -->
			<xsl:for-each select="gir:return-value">
				<return-type>
					<xsl:call-template name="output-type">
						<xsl:with-param name="nodename">return-type</xsl:with-param>
						<xsl:with-param name="typename" select="gir:type/@name"/>
						<xsl:with-param name="type" select="gir:type/@c:type"/>
						<xsl:with-param name="transfer-ownership" select="@transfer-ownership"/>
						<xsl:with-param name="doname">0</xsl:with-param>
						<xsl:with-param name="external" select="$external"/>
					</xsl:call-template>

				</return-type>
			</xsl:for-each>
			<!-- /xsl:if -->

			<xsl:if test="gir:parameters">
				<!-- For some wtf reason, some callbacks inside fields in the gir file include
					the pointer to the #$$@#$ parent type, which is something that gets added
					automatically. Remove the first such parameter if it's stupid -->
				<xsl:variable name="parent">
					<xsl:call-template name="get-real-type-name">
						<xsl:with-param name="node" select="../.."/>
						<xsl:with-param name="external" select="$external"/>
					</xsl:call-template>
				</xsl:variable>
				<parameters>
					<xsl:if test="@throws">
						<xsl:attribute name="throws">
							<xsl:value-of select="@throws"/>
						</xsl:attribute>
					</xsl:if>
					<!-- xsl:for-each select="gir:parameters/gir:parameter[position()&gt;1 or gir:type/@name!=$parent]" -->
					<xsl:for-each select="gir:parameters/gir:parameter">

						<parameter>
							<xsl:if test="@scope">
								<xsl:attribute name="scope">
									<xsl:value-of select="@scope"/>
								</xsl:attribute>
							</xsl:if>

							<xsl:if test="@allow-none">
								<xsl:attribute name="allow-none">
									<xsl:value-of select="@allow-none"/>
								</xsl:attribute>
							</xsl:if>

							<xsl:if test="@closure">
								<xsl:attribute name="closure">
									<xsl:value-of select="@closure"/>
								</xsl:attribute>
							</xsl:if>

							<xsl:if test="@destroy">
								<xsl:attribute name="destroy">
									<xsl:value-of select="@destroy"/>
								</xsl:attribute>
							</xsl:if>

							<xsl:call-template name="output-type">
								<xsl:with-param name="name" select="@name"/>
								<xsl:with-param name="typename" select="gir:type/@name"/>
								<xsl:with-param name="type" select="gir:type/@c:type"/>
								<xsl:with-param name="transfer-ownership"
									select="@transfer-ownership"/>
								<xsl:with-param name="external" select="$external"/>
							</xsl:call-template>
						</parameter>
					</xsl:for-each>
					<xsl:if test="@throws">
						<parameter name="error" type="GError**"/>
					</xsl:if>
				</parameters>
			</xsl:if>
		</xsl:element>
	</xsl:template>

	<xsl:template match="gir:constructor[not(@introspectable=0)]">
		<xsl:param name="external"/>

		<xsl:variable name="t" select="gir:parameters/gir:parameter/gir:type/@name"/>

		<constructor cname="{@c:identifier}">

			<xsl:if test="count(gir:parameters/gir:parameter)=0">
				<xsl:attribute name="disable_void_ctor"/>
			</xsl:if>
			<xsl:if
				test="count(gir:parameters/gir:parameter) = 1 and exsl:node-set($consttypes)/types/ctor/type[@name=$t]">
				<xsl:attribute name="disable_raw_ctor"/>
			</xsl:if>
			<xsl:call-template name="set-version-and-deprecated">
				<xsl:with-param name="version" select="@version"/>
				<xsl:with-param name="deprecated" select="@deprecated"/>
				<xsl:with-param name="deprecated-version" select="@deprecated-version"/>
			</xsl:call-template>

			<xsl:if test="gir:parameters">
				<xsl:element name="parameters">
					<xsl:for-each select="gir:parameters/gir:parameter">

						<xsl:element name="parameter">
							<xsl:call-template name="output-type">
								<xsl:with-param name="name" select="@name"/>
								<xsl:with-param name="typename" select="gir:type/@name"/>
								<xsl:with-param name="type" select="gir:type/@c:type"/>
								<xsl:with-param name="transfer-ownership"
									select="@transfer-ownership"/>
								<xsl:with-param name="external" select="$external"/>
							</xsl:call-template>
						</xsl:element>

					</xsl:for-each>
				</xsl:element>
			</xsl:if>
		</constructor>
	</xsl:template>

	<xsl:template match="glib:signal">
		<xsl:param name="external"/>

		<xsl:variable name="name" >
			<xsl:call-template name="capitalize">
				<xsl:with-param name="string" select="@name"/>
				<xsl:with-param name="sep">
					<xsl:text>-</xsl:text>
				</xsl:with-param>
			</xsl:call-template>
			<xsl:if test="../gir:method[@name = translate(current()/@name,'-','_')]">Event</xsl:if>
		</xsl:variable>

		<signal name="{$name}" cname="{@name}" when="{@when}">
			
			<xsl:if test="../gir:virtual-method[@name = translate(current()/@name,'-','_')]">
				<xsl:attribute name="field_name">
					<xsl:value-of select="translate(current()/@name,'-','_')"/>
				</xsl:attribute>
			</xsl:if>
			<xsl:call-template name="set-version-and-deprecated">
				<xsl:with-param name="version" select="@version"/>
				<xsl:with-param name="deprecated" select="@deprecated"/>
				<xsl:with-param name="deprecated-version" select="@deprecated-version"/>
			</xsl:call-template>

			<!-- xsl:if test="gir:return-value/gir:type/@name != 'none'" -->
			<xsl:for-each select="gir:return-value">
				<return-type>
					<xsl:call-template name="output-type">
						<xsl:with-param name="typename" select="gir:type/@name"/>
						<xsl:with-param name="type" select="gir:type/@c:type"/>
						<xsl:with-param name="transfer-ownership" select="@transfer-ownership"/>
						<xsl:with-param name="doname">0</xsl:with-param>
						<xsl:with-param name="external" select="$external"/>
					</xsl:call-template>
				</return-type>
			</xsl:for-each>
			<!-- /xsl:if -->

			<parameters>
				<xsl:variable name="prtypemap">
					<xsl:call-template name="map-type">
						<xsl:with-param name="type" select="../@c:type"/>
						<xsl:with-param name="name" select="../@name"/>
						<xsl:with-param name="external" select="$external"/>
					</xsl:call-template>
				</xsl:variable>

				<xsl:for-each select="gir:parameters/gir:parameter">
					<parameter>
						<xsl:if test="@scope">
							<xsl:attribute name="scope">
								<xsl:value-of select="@scope"/>
							</xsl:attribute>
						</xsl:if>

						<xsl:if test="@allow-none">
							<xsl:attribute name="allow-none">
								<xsl:value-of select="@allow-none"/>
							</xsl:attribute>
						</xsl:if>

						<xsl:call-template name="output-type">
							<xsl:with-param name="name" select="@name"/>
							<xsl:with-param name="typename" select="gir:type/@name"/>
							<xsl:with-param name="type" select="gir:type/@c:type"/>
							<xsl:with-param name="transfer-ownership" select="@transfer-ownership"/>
							<xsl:with-param name="external" select="$external"/>
						</xsl:call-template>
					</parameter>
				</xsl:for-each>
			</parameters>
			<xsl:if test="../gir:method[@name = translate(current()/@name,'-','_')]">
				<warning>Signal renamed because of existing method with same name</warning>
			</xsl:if>
		</signal>

	</xsl:template>

	<xsl:template match="gir:enumeration">
		<xsl:param name="external"/>

		<xsl:variable name="name">
			<xsl:call-template name="sanitize">
				<xsl:with-param name="name" select="@name"/>
			</xsl:call-template>
		</xsl:variable>

		<enum name="{$name}" cname="{@c:type}" type="enum">
			<xsl:if test="@glib:get-type">
				<xsl:attribute name="gtype">
					<xsl:value-of select="@glib:get-type"/>
				</xsl:attribute>
			</xsl:if>
			<xsl:call-template name="set-version-and-deprecated">
				<xsl:with-param name="version" select="@version"/>
				<xsl:with-param name="deprecated" select="@deprecated"/>
				<xsl:with-param name="deprecated-version" select="@deprecated-version"/>
			</xsl:call-template>
			
			<xsl:for-each select="gir:member">
				<xsl:sort select="@value" data-type="number"/>

				<xsl:variable name="ename">
					<xsl:variable name="mname">
						<xsl:choose>
							<xsl:when test="@name = @value">
								<xsl:call-template name="sanitize">
									<xsl:with-param name="name" select="@c:identifier"/>
								</xsl:call-template>
							</xsl:when>
							<xsl:otherwise>
								<xsl:call-template name="sanitize">
									<xsl:with-param name="name" select="@name"/>
								</xsl:call-template>
							</xsl:otherwise>
						</xsl:choose>
					</xsl:variable>
					<xsl:call-template name="capitalize">
						<xsl:with-param name="string" select="$mname"/>
					</xsl:call-template>
				</xsl:variable>
				<xsl:variable name="mname">
					<xsl:call-template name="map-name">
						<xsl:with-param name="name" select="$ename"/>
					</xsl:call-template>
				</xsl:variable>
				<member cname="{@c:identifier}" name="{$mname}">
					<xsl:if test="@value">
						<xsl:attribute name="value">
							<xsl:value-of select="@value"/>
						</xsl:attribute>
					</xsl:if>
				</member>
			</xsl:for-each>
		</enum>
	</xsl:template>

	<xsl:template match="gir:bitfield">
		<xsl:param name="external"/>

		<xsl:variable name="name">
			<xsl:call-template name="sanitize">
				<xsl:with-param name="name" select="@name"/>
			</xsl:call-template>
		</xsl:variable>

		<enum name="{$name}" cname="{@c:type}" type="flags">
			<xsl:if test="@glib:get-type">
				<xsl:attribute name="gtype">
					<xsl:value-of select="@glib:get-type"/>
				</xsl:attribute>
			</xsl:if>
			<xsl:call-template name="set-version-and-deprecated">
				<xsl:with-param name="version" select="@version"/>
				<xsl:with-param name="deprecated" select="@deprecated"/>
				<xsl:with-param name="deprecated-version" select="@deprecated-version"/>
			</xsl:call-template>
			
			<xsl:for-each select="gir:member">
				<xsl:sort select="@value" data-type="number"/>

				<xsl:variable name="ename">
					<xsl:variable name="mname">
						<xsl:choose>
							<xsl:when test="@name = @value">
								<xsl:call-template name="sanitize">
									<xsl:with-param name="name" select="@c:identifier"/>
								</xsl:call-template>
							</xsl:when>
							<xsl:otherwise>
								<xsl:call-template name="sanitize">
									<xsl:with-param name="name" select="@name"/>
								</xsl:call-template>
							</xsl:otherwise>
						</xsl:choose>
					</xsl:variable>
					<xsl:call-template name="capitalize">
						<xsl:with-param name="string" select="$mname"/>
					</xsl:call-template>
				</xsl:variable>
				<xsl:variable name="mname">
					<xsl:call-template name="map-name">
						<xsl:with-param name="name" select="$ename"/>
					</xsl:call-template>
				</xsl:variable>
				<member cname="{@c:identifier}" name="{$mname}">
					<xsl:if test="@value">
						<xsl:attribute name="value">
							<xsl:value-of select="@value"/>
						</xsl:attribute>
					</xsl:if>
				</member>
			</xsl:for-each>
		</enum>
	</xsl:template>

	<xsl:template match="gir:field">
		<xsl:param name="external"/>
		<xsl:if test="not(gir:callback) or not(../@glib:is-gtype-struct-for)">

			<xsl:variable name="writable">
				<xsl:choose>
					<xsl:when test="@writable=1">
						<xsl:text>true</xsl:text>
					</xsl:when>
					<xsl:otherwise>
						<xsl:text>false</xsl:text>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:variable>

			<xsl:variable name="readable">
				<xsl:choose>
					<xsl:when test="@readable=0">
						<xsl:text>false</xsl:text>
					</xsl:when>
					<xsl:otherwise>
						<xsl:text>true</xsl:text>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:variable>

			<xsl:variable name="access">
				<xsl:choose>
					<xsl:when test="@private">private</xsl:when>
					<xsl:otherwise>public</xsl:otherwise>
				</xsl:choose>
			</xsl:variable>

			<xsl:variable name="name">
				<xsl:call-template name="capitalize">
					<xsl:with-param name="string" select="@name"/>
				</xsl:call-template>
				<xsl:if
					test="../gir:property[@name = translate(current()/@name,'_','-')] or ../gir:method[@name = current()/@name]">
					<xsl:choose>
						<xsl:when test="//gir:callback[@name = current()/gir:type/@name]"
							>Func</xsl:when>
						<xsl:otherwise>Field</xsl:otherwise>
					</xsl:choose>
				</xsl:if>
			</xsl:variable>

			<xsl:variable name="is_callback">
				<xsl:choose>
					<xsl:when test="not(gir:callback)">false</xsl:when>
					<xsl:otherwise>true</xsl:otherwise>
				</xsl:choose>
			</xsl:variable>

			<field cname="{@name}" access="{$access}" writeable="{$writable}" readable="{$readable}" is_callback="{$is_callback}">
				<xsl:if test="@bits">
					<xsl:attribute name="bits">
						<xsl:value-of select="@bits"/>
					</xsl:attribute>
				</xsl:if>
				<xsl:call-template name="output-type">
					<xsl:with-param name="name" select="$name"/>
					<xsl:with-param name="typename" select="gir:type/@name"/>
					<xsl:with-param name="type" select="gir:type/@c:type"/>
					<xsl:with-param name="transfer-ownership" select="@transfer-ownership"/>
					<xsl:with-param name="external" select="$external"/>
				</xsl:call-template>
			</field>

		</xsl:if>
	</xsl:template>

	<xsl:template match="gir:property[not(@private)]">
		<xsl:param name="external"/>
		<xsl:variable name="name">
			<xsl:call-template name="capitalize">
				<xsl:with-param name="string">
					<xsl:choose>
						<xsl:when test="../gir:method[@name = translate(current()/@name,'-','_')] or ../gir:virtual-method[@name = translate(current()/@name,'-','_')]">
							<xsl:value-of select="@name"/>Prop
						</xsl:when>
						<xsl:otherwise>
							<xsl:value-of select="@name"/>
						</xsl:otherwise>
					</xsl:choose>
				</xsl:with-param>
				<xsl:with-param name="sep">-</xsl:with-param>
			</xsl:call-template>
		</xsl:variable>
		<xsl:variable name="typemap">
			<xsl:call-template name="map-type">
				<xsl:with-param name="type" select="gir:type/@c:type"/>
				<xsl:with-param name="name" select="gir:type/@name"/>
				<xsl:with-param name="external" select="$external"/>
			</xsl:call-template>
		</xsl:variable>

		<xsl:variable name="type">
			<xsl:choose>
				<xsl:when test="//*/gir:namespace/gir:enumeration[@c:type=gir:type/@c:type]">
					<xsl:text>int</xsl:text>
				</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="exsl:node-set($typemap)/type/@ctype"/>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>

		<xsl:variable name="construct">
			<xsl:choose>
				<xsl:when test="@construct and @construct=1">true</xsl:when>
				<xsl:otherwise>false</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>

		<xsl:variable name="construct-only">
			<xsl:choose>
				<xsl:when test="@construct-only and @construct-only=1">true</xsl:when>
				<xsl:otherwise>false</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>

		<xsl:variable name="writable">
			<xsl:choose>
				<xsl:when test="@writable=1">
					<xsl:text>true</xsl:text>
				</xsl:when>
				<xsl:otherwise>
					<xsl:text>false</xsl:text>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>

		<xsl:variable name="readable">
			<xsl:choose>
				<xsl:when test="@readable=0">
					<xsl:text>false</xsl:text>
				</xsl:when>
				<xsl:otherwise>
					<xsl:text>true</xsl:text>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>

		<xsl:if test="$type!=''">
			<property name="{$name}" cname="{@name}" type="{$type}" readable="{$readable}"
				writeable="{$writable}" construct="{$construct}"
				construct-only="{$construct-only}">
				<xsl:call-template name="set-version-and-deprecated">
					<xsl:with-param name="version" select="@version"/>
					<xsl:with-param name="deprecated" select="@deprecated"/>
					<xsl:with-param name="deprecated-version" select="@deprecated-version"/>
				</xsl:call-template>
			</property>
		</xsl:if>
		
	</xsl:template>

	<xsl:template match="gir:*"/>

	<xsl:template match="text()" mode="#all"/>


	<!-- Helper templates -->

	<xsl:template name="output-method">
		<xsl:param name="shared">false</xsl:param>
		<xsl:param name="nodename">method</xsl:param>
		<xsl:param name="cname"/>
		<xsl:param name="introspectable"/>
		<xsl:param name="external"/>

		<xsl:variable name="name">
			<xsl:call-template name="capitalize">
				<xsl:with-param name="string" select="@name"/>
			</xsl:call-template>
		</xsl:variable>

		<xsl:element name="{$nodename}">
			<xsl:attribute name="name">
				<xsl:value-of select="$name"/>
			</xsl:attribute>
			<xsl:choose>
				<xsl:when test="$cname">
					<xsl:attribute name="cname">
						<xsl:value-of select="$cname"/>
					</xsl:attribute>
				</xsl:when>
				<xsl:otherwise>
					<xsl:attribute name="cname">
						<xsl:value-of select="@c:identifier"/>
					</xsl:attribute>
				</xsl:otherwise>
			</xsl:choose>
			<xsl:if test="$shared = 'true'">
				<xsl:attribute name="shared">true</xsl:attribute>
			</xsl:if>
			<xsl:if test="@private = '1'">
				<xsl:attribute name="hidden">true</xsl:attribute>
			</xsl:if>
			<xsl:if test="@disguised = '1'">
				<xsl:attribute name="opaque">true</xsl:attribute>
			</xsl:if>
			<xsl:call-template name="set-version-and-deprecated">
				<xsl:with-param name="version" select="@version"/>
				<xsl:with-param name="deprecated" select="@deprecated"/>
				<xsl:with-param name="deprecated-version" select="@deprecated-version"/>
			</xsl:call-template>
			<xsl:if test="$introspectable=0">
				<xsl:attribute name="hidden">true</xsl:attribute>
			</xsl:if>

			<!-- xsl:if test="gir:return-value/gir:type/@name != 'none'" -->
			<xsl:for-each select="gir:return-value">
				<return-type>
					<xsl:call-template name="output-type">
						<xsl:with-param name="typename" select="gir:type/@name"/>
						<xsl:with-param name="type" select="gir:type/@c:type"/>
						<xsl:with-param name="transfer-ownership" select="@transfer-ownership"/>
						<xsl:with-param name="external" select="$external"/>
					</xsl:call-template>
				</return-type>
			</xsl:for-each>
			<!-- / xsl:if -->

			<xsl:if test="gir:parameters">
				<parameters>
					<xsl:if test="@throws">
						<xsl:attribute name="throws">
							<xsl:value-of select="@throws"/>
						</xsl:attribute>
					</xsl:if>
					<xsl:for-each select="gir:parameters/gir:parameter">
						<parameter>

							<xsl:if test="@closure">
								<xsl:attribute name="closure">
									<xsl:value-of select="@closure"/>
								</xsl:attribute>
							</xsl:if>

							<xsl:if test="@destroy">
								<xsl:attribute name="destroy">
									<xsl:value-of select="@destroy"/>
								</xsl:attribute>
							</xsl:if>

							<xsl:if test="@scope">
								<xsl:attribute name="scope">
									<xsl:value-of select="@scope"/>
								</xsl:attribute>
							</xsl:if>

							<xsl:if test="@allow-none">
								<xsl:attribute name="allow-none">
									<xsl:value-of select="@allow-none"/>
								</xsl:attribute>
							</xsl:if>

							<xsl:call-template name="output-type">
								<xsl:with-param name="name" select="@name"/>
								<xsl:with-param name="typename" select="gir:type/@name"/>
								<xsl:with-param name="type" select="gir:type/@c:type"/>
								<xsl:with-param name="transfer-ownership"
									select="@transfer-ownership"/>
								<xsl:with-param name="external" select="$external"/>
							</xsl:call-template>
						</parameter>
					</xsl:for-each>
					<xsl:if test="@throws">
						<parameter name="error" type="GError**"/>
					</xsl:if>
				</parameters>
			</xsl:if>
		</xsl:element>
	</xsl:template>

	<xsl:template name="output-type">
		<xsl:param name="name"/>
		<xsl:param name="typename"/>
		<xsl:param name="type"/>
		<xsl:param name="transfer-ownership"/>
		<xsl:param name="dotype">1</xsl:param>
		<xsl:param name="external"/>


		<xsl:variable name="pname">
			<xsl:call-template name="map-name">
				<xsl:with-param name="name" select="$name"/>
			</xsl:call-template>
		</xsl:variable>
		<xsl:variable name="thetype">
			<xsl:choose>
				<xsl:when test="$type">
					<xsl:call-template name="map-ctype">
						<xsl:with-param name="ctype" select="$type"/>
					</xsl:call-template>
				</xsl:when>
				<xsl:when test="$typename">
					<xsl:value-of select="$typename"/>
				</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="$pname"/>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>

		<xsl:variable name="ptypemap">
			<xsl:call-template name="map-type">
				<xsl:with-param name="type" select="$thetype"/>
				<xsl:with-param name="name" select="$typename"/>
				<xsl:with-param name="transfer-ownership" select="@transfer-ownership"/>
				<xsl:with-param name="external" select="$external"/>
			</xsl:call-template>
		</xsl:variable>

		<xsl:variable name="ptype">
			<xsl:choose>
				<xsl:when
					test="$transfer-ownership='none' and exsl:node-set($consttypes)/types/type[@name=exsl:node-set($ptypemap)/type/@ctype]">
					<xsl:text>const-</xsl:text>
					<xsl:value-of select="exsl:node-set($ptypemap)/type/@ctype"/>
				</xsl:when>
				<xsl:otherwise>
					<xsl:choose>
						<xsl:when test="exsl:node-set($ptypemap)/type/@ctype!=''">
							<xsl:value-of select="exsl:node-set($ptypemap)/type/@ctype"/>
						</xsl:when>
						<xsl:otherwise>
							<xsl:choose>
								<xsl:when test="gir:callback[@name=$name]">
									<xsl:value-of select="//gir:repository/gir:namespace/@name"
										/><xsl:value-of select="@name"/>Func </xsl:when>
								<xsl:otherwise>
									<xsl:choose>
										<xsl:when test="gir:array">
											<xsl:value-of select="gir:array/*/@c:type"/>
											</xsl:when>
										<xsl:otherwise>none</xsl:otherwise>
									</xsl:choose>
								</xsl:otherwise>
							</xsl:choose>
						</xsl:otherwise>
					</xsl:choose>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>

		<xsl:if test="$pname!=''">
			<xsl:attribute name="name">
				<xsl:value-of select="$pname"/>
			</xsl:attribute>
		</xsl:if>
		<xsl:if test="$dotype=1">
			<xsl:choose>
				<xsl:when test="exsl:node-set($ptypemap)/type/@element_type">
					<!-- these get switched around in case of a GList or GSList -->
					<xsl:attribute name="type">
						<xsl:value-of select="exsl:node-set($ptypemap)/type/@element_type"/>
					</xsl:attribute>
					<xsl:attribute name="element_type">
						<xsl:value-of select="$ptype"/>
					</xsl:attribute>
				</xsl:when>
				<xsl:otherwise>
					<xsl:attribute name="type">
						<xsl:value-of select="$ptype"/>
					</xsl:attribute>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:if>
		<xsl:if test="exsl:node-set($ptypemap)/type/@elements_owned">
			<xsl:attribute name="elements_owned">
				<xsl:value-of select="exsl:node-set($ptypemap)/type/@elements_owned"/>
			</xsl:attribute>
		</xsl:if>
		<xsl:if test="exsl:node-set($ptypemap)/type/@owned">
			<xsl:attribute name="elements_owned">
				<xsl:value-of select="exsl:node-set($ptypemap)/type/@owned"/>
			</xsl:attribute>
		</xsl:if>
		<xsl:if test="@transfer-ownership='full'">
			<xsl:attribute name="owned">true</xsl:attribute>
		</xsl:if>

		<xsl:if test="@direction">
			<xsl:choose>
				<!-- it seems that we can just convert inout to ref -->
				<xsl:when test="@direction='inout'">
					<xsl:attribute name="pass_as">ref</xsl:attribute>
				</xsl:when>
				<xsl:otherwise>
					<xsl:attribute name="pass_as">
						<xsl:value-of select="@direction"/>
					</xsl:attribute>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:if>
		
		<xsl:variable name="pos" select="position()"></xsl:variable>
		
		<xsl:variable name="arrayIndex">
			<xsl:value-of select="count(..//gir:parameter[$pos = (gir:array/@length+1)]/preceding-sibling::gir:parameter)"/>
		</xsl:variable>
		
		<xsl:if test="$arrayIndex != 0">
			<xsl:attribute name="array_index"><xsl:value-of select="$arrayIndex"/></xsl:attribute>
		</xsl:if>

		<xsl:if test="gir:array">
			<xsl:attribute name="array">true</xsl:attribute>
			<xsl:choose>
				<xsl:when test="gir:array/@length">
					<xsl:attribute name="array_length_param_index">
						<xsl:value-of select="gir:array/@length"/>
					</xsl:attribute>
				</xsl:when>
				<xsl:when test="gir:array/@fixed-size">
					<xsl:attribute name="array_len">
						<xsl:value-of select="gir:array/@fixed-size"/>
					</xsl:attribute>
				</xsl:when>
				<xsl:otherwise>
					<xsl:if test="not(gir:array/@zero-terminated) or gir:array/@zero-terminated='1'">
						<xsl:attribute name="null_term_array">true</xsl:attribute>
					</xsl:if>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:if>


		<xsl:if test="exsl:node-set($ptypemap)/type/@warning">
			<warning>
				<xsl:value-of select="exsl:node-set($ptypemap)/type/@warning"/>
			</warning>
		</xsl:if>
		<!-- debug s="1" name="{$name}" pname="{$pname}" ptypemap="{$ptypemap}" typename="{$typename}" typea="{$type}" transfer-ownership="{$transfer-ownership}" mapping="{exsl:node-set($typemappings)/mappings/mapping[@from=$typename]/@to}" dotype="{$dotype}" / -->
		<!--
		<gtypes>
			<name><xsl:value-of select="$typename" /></name>
			<aname><xsl:value-of select="@name" /></aname>
			<thetype><xsl:value-of select="$thetype" /></thetype>
			<typemap>
				<xsl:copy-of select="$ptypemap" />
			</typemap>
			<type>
				<xsl:call-template name="map-gtype">
					<xsl:with-param name="type" select="$typename"/>
					<xsl:with-param name="external" select="$external"/>
				</xsl:call-template>
			</type>
		</gtypes>
-->

	</xsl:template>

	<xsl:template name="map-name">
		<xsl:param name="name"/>
		<xsl:choose>
			<xsl:when test="not($name) and gir:varargs">var_args</xsl:when>
			<xsl:when test="not(exsl:node-set($mappings)/mappings/mapping[@from=$name])">
				<xsl:value-of select="$name"/>
			</xsl:when>
			<xsl:otherwise>
				<xsl:value-of select="exsl:node-set($mappings)/mappings/mapping[@from=$name]/@to"/>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template name="sanitize">
		<xsl:param name="name"/>

		<xsl:choose>
			<xsl:when
				test="starts-with($name, '0') or starts-with($name, '1') or starts-with($name, '2') or starts-with($name, '3') or starts-with($name, '4') or starts-with($name, '5') or starts-with($name, '6') or starts-with($name, '7') or starts-with($name, '8') or starts-with($name, '9')">
				<xsl:choose>
					<xsl:when test="@c:type">
						<xsl:value-of select="@c:type"/>
					</xsl:when>
					<xsl:otherwise>_<xsl:value-of select="$name"/></xsl:otherwise>
				</xsl:choose>
			</xsl:when>
			<xsl:otherwise>
				<xsl:value-of select="$name"/>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template name="capitalize">
		<xsl:param name="string"/>
		<xsl:param name="sep">_</xsl:param>
		<xsl:value-of
			select="translate(substring($string,1,1),'abcdefghijklmnopqrstuvwxyz','ABCDEFGHIJKLMNOPQRSTUVWXYZ')"/>
		<xsl:choose>
			<xsl:when test="contains($string,$sep)">
				<xsl:value-of select="substring-before(substring($string,2),$sep)"/>
			</xsl:when>
			<xsl:otherwise>
				<xsl:value-of select="substring($string,2)"/>
			</xsl:otherwise>
		</xsl:choose>
		<xsl:if test="contains($string,$sep)">
			<xsl:call-template name="capitalize">
				<xsl:with-param name="string" select="substring-after($string,$sep)"/>
				<xsl:with-param name="sep" select="$sep"/>
			</xsl:call-template>
		</xsl:if>
	</xsl:template>

	<xsl:template name="map-type">
		<xsl:param name="type"/>
		<xsl:param name="name"/>
		<xsl:param name="transfer-ownership">none</xsl:param>
		<xsl:param name="nopointer"/>
		<xsl:param name="external"/>

		<type>

			<xsl:choose>
				<xsl:when test="gir:varargs">
					<xsl:attribute name="ctype">va_list</xsl:attribute>
					<xsl:attribute name="gtype">va_list</xsl:attribute>
				</xsl:when>
				<xsl:when test="gir:array">
					<xsl:call-template name="map-gtype">
						<xsl:with-param name="type" select="gir:array/@c:type"/>
						<xsl:with-param name="transfer-ownership" select="$transfer-ownership"/>
						<xsl:with-param name="nopointer" select="$nopointer"/>
						<xsl:with-param name="external" select="$external"/>
					</xsl:call-template>
				</xsl:when>
				<xsl:when test="gir:type/gir:type">
					<xsl:call-template name="map-gtype">
						<xsl:with-param name="type" select="gir:type/gir:type/@name"/>
						<xsl:with-param name="external" select="$external"/>
					</xsl:call-template>
					<xsl:attribute name="element_type">
						<xsl:value-of select="gir:type/@c:type"/>
					</xsl:attribute>
					<xsl:if test="$transfer-ownership = 'container'">
						<xsl:attribute name="owned">true</xsl:attribute>
					</xsl:if>
					<xsl:if test="$transfer-ownership = 'full'">
						<xsl:attribute name="elements_owned">true</xsl:attribute>
						<xsl:attribute name="owned">true</xsl:attribute>
					</xsl:if>
				</xsl:when>
				<xsl:when test="not($name)">
					<xsl:call-template name="map-gtype">
						<xsl:with-param name="type" select="$type"/>
						<xsl:with-param name="transfer-ownership" select="$transfer-ownership"/>
						<xsl:with-param name="nopointer" select="$nopointer"/>
						<xsl:with-param name="external" select="$external"/>
					</xsl:call-template>
				</xsl:when>

				<xsl:otherwise>
					<xsl:call-template name="map-gtype">
						<xsl:with-param name="type" select="$name"/>
						<xsl:with-param name="transfer-ownership" select="$transfer-ownership"/>
						<xsl:with-param name="nopointer" select="$nopointer"/>
						<xsl:with-param name="external" select="$external"/>
					</xsl:call-template>
				</xsl:otherwise>
			</xsl:choose>

		</type>
	</xsl:template>

	<xsl:template name="map-gtype">
		<xsl:param name="type"/>
		<xsl:param name="transfer-ownership">none</xsl:param>
		<xsl:param name="nopointer"/>
		<xsl:param name="external"/>


		<xsl:choose>
			<xsl:when test="contains($type, '.')">
				<xsl:variable name="ns" select="substring-before($type, '.')"/>
				<xsl:variable name="t" select="substring-after($type, '.')"/>
				<xsl:variable name="l"
					select="exsl:node-set($external)/include[@name=$ns]/gir:repository/gir:namespace[@name=$ns]"/>

				<xsl:call-template name="map-gtype-inner">
					<xsl:with-param name="type" select="$type"/>
					<xsl:with-param name="transfer-ownership" select="$transfer-ownership"/>
					<xsl:with-param name="l" select="$l"/>
					<xsl:with-param name="t" select="$t"/>
					<xsl:with-param name="nopointer" select="$nopointer"/>
				</xsl:call-template>

			</xsl:when>
			<xsl:otherwise>
				<xsl:variable name="t" select="$type"/>
				<xsl:variable name="l" select="ancestor::gir:namespace"/>

				<xsl:call-template name="map-gtype-inner">
					<xsl:with-param name="type" select="$type"/>
					<xsl:with-param name="transfer-ownership" select="$transfer-ownership"/>
					<xsl:with-param name="l" select="$l"/>
					<xsl:with-param name="t" select="$t"/>
					<xsl:with-param name="nopointer" select="$nopointer"/>
				</xsl:call-template>

			</xsl:otherwise>
		</xsl:choose>

	</xsl:template>

	<xsl:template name="map-gtype-inner">
		<xsl:param name="type"/>
		<xsl:param name="transfer-ownership"/>
		<xsl:param name="l"/>
		<xsl:param name="t"/>
		<xsl:param name="nopointer"/>

		<xsl:choose>

			<xsl:when test="exsl:node-set($typemappings)/mappings/mapping[@from=$t]">
				<xsl:attribute name="ctype">
					<xsl:value-of
						select="exsl:node-set($typemappings)/mappings/mapping[@from=$t]/@to"/>
				</xsl:attribute>
				<xsl:attribute name="gtype">
					<xsl:value-of
						select="exsl:node-set($typemappings)/mappings/mapping[@from=$t]/@to"/>
				</xsl:attribute>
			</xsl:when>

			<xsl:when test="exsl:node-set($typemappings)/mappings/mapping[@from=$type]">
				<xsl:attribute name="ctype">
					<xsl:value-of
						select="exsl:node-set($typemappings)/mappings/mapping[@from=$type]/@to"/>
				</xsl:attribute>
				<xsl:attribute name="gtype">
					<xsl:value-of
						select="exsl:node-set($typemappings)/mappings/mapping[@from=$type]/@to"/>
				</xsl:attribute>
			</xsl:when>


			<xsl:when test="$l/gir:class[@name=$t]">
				<xsl:attribute name="ctype"><xsl:value-of select="$l/gir:class[@name=$t]/@c:type"
					/>*</xsl:attribute>
				<xsl:attribute name="gtype">
					<xsl:value-of select="$l/gir:class[@name=$t]/@glib:type-name"/>
				</xsl:attribute>
			</xsl:when>
			<xsl:when test="$l/gir:record[@name=$t]">
				<xsl:choose>
					<xsl:when
						test="gir:type/@c:type and concat($l/gir:record[@name=$t]/@c:type,'*') != string(gir:type/@c:type)">
						<xsl:attribute name="warning">type does not match c:type -
								record:<xsl:value-of select="$l/@name"/>/<xsl:value-of select="$t"
							/>. '<xsl:value-of select="$l/gir:record[@name=$t]/@c:type"/>*' ==
								'<xsl:value-of select="gir:type/@c:type"/>'(ctype)</xsl:attribute>
						<xsl:attribute name="ctype">
							<xsl:call-template name="map-ctype">
								<xsl:with-param name="ctype" select="gir:type/@c:type"/>
							</xsl:call-template>
						</xsl:attribute>
					</xsl:when>
					<xsl:otherwise>
						<xsl:attribute name="ctype"><xsl:call-template name="map-ctype"
									><xsl:with-param name="ctype"
									select="$l/gir:record[@name=$t]/@c:type"
							/></xsl:call-template>*</xsl:attribute>
					</xsl:otherwise>
				</xsl:choose>
				<xsl:choose>
					<xsl:when test="$l/gir:record[@name=$t]/@glib:type-name">
						<xsl:attribute name="gtype">
							<xsl:value-of select="$l/gir:record[@name=$t]/@glib:type-name"/>
						</xsl:attribute>
					</xsl:when>
					<xsl:otherwise>
						<xsl:attribute name="gtype">
							<xsl:call-template name="map-ctype">
								<xsl:with-param name="ctype"
									select="$l/gir:record[@name=$t]/@c:type"/>
							</xsl:call-template>
						</xsl:attribute>
						<xsl:attribute name="warning">missing glib:type-name</xsl:attribute>
					</xsl:otherwise>
				</xsl:choose>
				<xsl:attribute name="warning">missing glib:type-name</xsl:attribute>
			</xsl:when>
			<xsl:when test="$l/gir:union[@name=$t]">
				<xsl:attribute name="ctype"><xsl:value-of select="$l/gir:union[@name=$t]/@c:type"
					/>*</xsl:attribute>
				<xsl:attribute name="gtype">
					<xsl:value-of select="$l/gir:union[@name=$t]/@glib:type-name"/>
				</xsl:attribute>
			</xsl:when>
			<xsl:when test="$l/gir:boxed[@name=$t]">
				<xsl:attribute name="ctype"><xsl:value-of select="$l/gir:boxed[@name=$t]/@c:type"
					/>*</xsl:attribute>
				<xsl:attribute name="gtype">
					<xsl:value-of select="$l/gir:boxed[@name=$t]/@glib:type-name"/>
				</xsl:attribute>
			</xsl:when>
			<xsl:when test="$l/gir:interface[@name=$t]">
				<xsl:attribute name="ctype">
					<xsl:choose>
						<xsl:when test="$l/gir:interface[@name=$t]/@c:type">
							<xsl:value-of select="$l/gir:interface[@name=$t]/@c:type"/>
						</xsl:when>
						<xsl:otherwise>
							<xsl:value-of select="$l/gir:interface[@name=$t]/@glib:type-name"/>
						</xsl:otherwise>
					</xsl:choose>
					<xsl:if test="$nopointer!=1">*</xsl:if>
				</xsl:attribute>
				<xsl:attribute name="gtype">
					<xsl:value-of select="$l/gir:interface[@name=$t]/@glib:type-name"/>
				</xsl:attribute>
			</xsl:when>

			<xsl:when test="$l/gir:bitfield[@name=$t]">
				<xsl:attribute name="ctype">
					<xsl:value-of select="$l/gir:bitfield[@name=$t]/@c:type"/>
				</xsl:attribute>
				<xsl:attribute name="gtype">
					<xsl:value-of select="$l/gir:bitfield[@name=$t]/@glib:type-name"/>
				</xsl:attribute>
			</xsl:when>
			<xsl:when test="$l/gir:enumeration[@name=$t]">
				<xsl:attribute name="ctype">
					<xsl:value-of select="$l/gir:enumeration[@name=$t]/@c:type"/>
				</xsl:attribute>
				<xsl:attribute name="gtype">
					<xsl:value-of select="$l/gir:enumeration[@name=$t]/@glib:type-name"/>
				</xsl:attribute>
			</xsl:when>
			<xsl:when test="$l/gir:callback[@name=$t]">
				<!-- It appears callbacks need to have the name as the type (and not the c:type) -->
				<xsl:choose>
					<xsl:when test="$l/gir:callback[@name=$t]/@c:type">
						<xsl:attribute name="ctype">
							<xsl:value-of select="$l/gir:callback[@name=$t]/@c:type"/>
						</xsl:attribute>
					</xsl:when>
					<xsl:otherwise>
						<xsl:attribute name="ctype">
							<xsl:value-of select="$t"/>
						</xsl:attribute>
					</xsl:otherwise>
				</xsl:choose>
				<xsl:attribute name="gtype">
					<xsl:value-of select="$l/gir:callback[@name=$t]/@glib:type-name"/>
				</xsl:attribute>
			</xsl:when>
			<xsl:when test="$l/gir:alias[@name=$t]">
				<xsl:attribute name="ctype">
					<xsl:value-of select="$l/gir:alias[@name=$t]/gir:type/@c:type"/>
				</xsl:attribute>
				<xsl:attribute name="gtype">
					<xsl:value-of select="$l/gir:alias[@name=$t]/gir:type/@glib:type-name"/>
				</xsl:attribute>
			</xsl:when>


			<xsl:when test="gir:callback[@name=current()/@name]">
				<xsl:attribute name="ctype"><xsl:value-of select="$l/@name"/><xsl:value-of
						select="$type"/>Func</xsl:attribute>
				<xsl:attribute name="gtype"><xsl:value-of select="$l/@name"/><xsl:value-of
						select="$type"/>Func</xsl:attribute>
			</xsl:when>

			<xsl:otherwise>
				<xsl:choose>
					<xsl:when test="$transfer-ownership='full'">
						<xsl:attribute name="ctype"><xsl:value-of select="$type"/>*</xsl:attribute>
						<xsl:attribute name="gtype">
							<xsl:value-of select="$type"/>
						</xsl:attribute>
					</xsl:when>
					<xsl:otherwise>
						<xsl:attribute name="ctype">
							<xsl:value-of select="$type"/>
						</xsl:attribute>
						<xsl:attribute name="gtype">
							<xsl:value-of select="$type"/>
						</xsl:attribute>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:otherwise>
		</xsl:choose>

	</xsl:template>

	<xsl:template name="get-real-type-name">
		<xsl:param name="node"/>
		<xsl:param name="external"/>

		<xsl:choose>
			<xsl:when test="$node/@glib:is-gtype-struct-for">
				<xsl:variable name="name">
					<xsl:value-of select="$node/@glib:is-gtype-struct-for"/>
				</xsl:variable>
				<xsl:value-of select="$name"/>
			</xsl:when>
			<xsl:otherwise>
				<xsl:variable name="name">
					<xsl:value-of select="$node/@name"/>
				</xsl:variable>
				<xsl:value-of select="$name"/>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>


	<xsl:template name="map-ctype">
		<xsl:param name="ctype"/>
		<xsl:choose>
			<xsl:when test="contains($ctype, 'const ')">
				<xsl:value-of select="substring($ctype, 7)"/>
			</xsl:when>
			<xsl:otherwise>
				<xsl:value-of select="$ctype"/>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>


	<xsl:template name="set-version-and-deprecated">
		<xsl:param name="version"/>
		<xsl:param name="deprecated"/>
		<xsl:param name="deprecated-version"/>
		<xsl:if test="$version">
			<xsl:attribute name="version">
				<xsl:value-of select="$version" />
			</xsl:attribute>
		</xsl:if>
		<xsl:if test="$deprecated">
			<xsl:attribute name="deprecated">true</xsl:attribute>
		</xsl:if>
		<xsl:if test="$deprecated-version">
			<xsl:attribute name="deprecated-version">
				<xsl:value-of select="$deprecated-version" />
			</xsl:attribute>
		</xsl:if>
	</xsl:template>
</xsl:stylesheet>

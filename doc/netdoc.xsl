<?xml version="1.0" encoding="iso-8859-1"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
<xsl:output method="html" doctype-public="-//W3C//DTD XHTML 1.0 Transitional//EN" doctype-system="http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd"/>

<xsl:template match="doc">
<html xmlns="http://www.w3.org/1999/xhtml">
<head><title><xsl:value-of select="@assembly"/>.dll documentation</title></head>
<body>
<xsl:apply-templates/>
</body>
</html>
</xsl:template>

<xsl:template match="namespace">
<b><xsl:value-of select="@name"/></b><br />
<xsl:apply-templates/>
</xsl:template>

<xsl:template match="class">
<a><xsl:attribute name="name"><xsl:value-of select="@name"/></xsl:attribute></a>
<ul><xsl:value-of select="@name"/>

<xsl:apply-templates select="summary"/>
<xsl:if test="boolean(remarks)">
	<xsl:apply-templates select="remarks"/>
	<br />
</xsl:if>

<xsl:if test="@base != ''">
	<ul><u>Inherits from:</u><xsl:text> </xsl:text>
	<xsl:call-template name="maybeLink">
		<xsl:with-param name="klass">
			<xsl:value-of select="@base"/>
		</xsl:with-param>
	</xsl:call-template>
	</ul>
	<br />
</xsl:if>
<xsl:if test="boolean(implements)">
	<ul><u>Implements</u></ul>
	<xsl:apply-templates select="./implements"/>
	<br />
</xsl:if>

<xsl:if test="boolean(constructor)">
	<ul><u>Constructors</u></ul>
	<xsl:apply-templates select="./constructor"/>
	<br />
</xsl:if>
<xsl:if test="boolean(method)">
	<ul><u>Methods</u></ul>
	<xsl:apply-templates select="./method"/>
	<br />
</xsl:if>
<xsl:if test="boolean(property)">
	<ul><u>Properties</u></ul>
	<xsl:apply-templates select="./property"/>
	<br />
</xsl:if>
<xsl:if test="boolean(event)">
	<ul><u>Events</u></ul>
	<xsl:apply-templates select="./event"/>
	<br />
</xsl:if>
<xsl:if test="boolean(field)">
	<ul><u>Fields</u></ul>
	<xsl:apply-templates select="./field"/>
	<br />
</xsl:if>


</ul>
</xsl:template>

<xsl:template match="constructor">
<ul><xsl:value-of select="@name"/> <xsl:apply-templates select="arguments"/><xsl:apply-templates select="summary"/><xsl:apply-templates select="remarks"/></ul>
</xsl:template>

<xsl:template match="method">
<ul>
	<xsl:call-template name="maybeLink">
		<xsl:with-param name="klass">
			<xsl:value-of select="@type"/>
		</xsl:with-param>
	</xsl:call-template>
<xsl:text> </xsl:text><xsl:value-of select="@name"/><xsl:apply-templates select="arguments"/><xsl:apply-templates select="summary"/><xsl:apply-templates select="remarks"/></ul>
</xsl:template>

<xsl:template match="property">
<ul>
	<xsl:call-template name="maybeLink">
		<xsl:with-param name="klass">
			<xsl:value-of select="@type"/>
		</xsl:with-param>
	</xsl:call-template>
<xsl:text> </xsl:text><xsl:value-of select="@name"/><xsl:apply-templates/></ul>
</xsl:template>

<xsl:template match="event|field">
<ul><xsl:value-of select="@name"/><xsl:apply-templates/></ul>
</xsl:template>

<xsl:template match="summary">
 - <font size="-1"><i><xsl:apply-templates/></i></font>
</xsl:template>

<xsl:template match="remarks">
<ul><xsl:apply-templates/></ul>
</xsl:template>

<xsl:template match="implements">
<ul><xsl:apply-templates/></ul>
</xsl:template>

<xsl:template match="interface">
	<xsl:call-template name="maybeLink">
		<xsl:with-param name="klass">
			<xsl:apply-templates/>
		</xsl:with-param>
	</xsl:call-template>
<xsl:text> </xsl:text>
</xsl:template>

<xsl:template name="maybeLink">
<xsl:param name="klass"/>
<!-- FIXME: handle arrays better -->
<xsl:variable name="is_primitive" select="boolean($klass = 'bool' or $klass = 'byte' or $klass = 'char' or $klass = 'decimal' or $klass = 'double' or $klass = 'int16' or $klass = 'int' or $klass = 'int64' or $klass = 'object' or $klass = 'sbyte' or $klass = 'single' or $klass = 'string' or $klass = 'uint16' or $klass = 'uint' or $klass = 'uint64' or $klass = 'void' or contains ($klass, '[]') or $klass = '[unknown]')"/>
<xsl:choose>
<xsl:when test="contains($klass, '.')">
	<xsl:variable name="ns" select="substring-before ($klass, '.')"/>
	<xsl:variable name="lcletters">abcdefghijklmnopqrstuvwxyz</xsl:variable>
	<xsl:variable name="ucletters">ABCDEFGHIJKLMNOPQRSTUVWXYZ</xsl:variable>
	<xsl:choose>
	<xsl:when test="$ns = 'GLib' or $ns = 'Atk' or $ns = 'Pango' or $ns = 'Gdk' or $ns = 'Gtk'">
		<a><xsl:attribute name="href"><xsl:value-of select="translate($ns, $ucletters, $lcletters)"/>-sharp-docs.html#<xsl:value-of select="substring-after ($klass, '.')"/></xsl:attribute><xsl:value-of select="$klass"/></a>
	</xsl:when>
	<xsl:otherwise><xsl:value-of select="$klass"/>
	</xsl:otherwise>
	</xsl:choose>
</xsl:when>
<xsl:when test="boolean($is_primitive)">
	<xsl:value-of select="$klass"/>
</xsl:when>
<xsl:otherwise>
	<a><xsl:attribute name="href">#<xsl:value-of select="$klass"/></xsl:attribute><xsl:value-of select="$klass"/></a>
</xsl:otherwise>
</xsl:choose>
</xsl:template>

<xsl:template match="arguments">
(<xsl:if test="count(*) != 0">
<xsl:call-template name="argument-iteration">
	<xsl:with-param name="needs_comma" select="boolean(0)"/>
	<xsl:with-param name="i" select="1"/>
	<xsl:with-param name="args" select="*"/>
</xsl:call-template>
</xsl:if>)
</xsl:template>

<xsl:template name="argument-iteration">
<xsl:param name="needs_comma"/>
<xsl:param name="i"/>
<xsl:param name="args"/>
<xsl:if test="boolean($needs_comma)"><xsl:text>, </xsl:text></xsl:if>
<xsl:apply-templates select="$args[$i]"/>

<xsl:if test="$i != count($args)">  
<xsl:call-template name="argument-iteration">
	<xsl:with-param name="needs_comma" select="boolean(1)"/>
	<xsl:with-param name="i" select="$i + 1"/>
	<xsl:with-param name="args" select="$args"/>
</xsl:call-template>
</xsl:if>

</xsl:template>

<xsl:template match="argument">
<xsl:value-of select="@modifiers"/>
	<xsl:call-template name="maybeLink">
		<xsl:with-param name="klass">
			<xsl:value-of select="@type"/>
		</xsl:with-param>
	</xsl:call-template>
<xsl:text> </xsl:text><xsl:value-of select="@name"/>
</xsl:template>

</xsl:stylesheet>

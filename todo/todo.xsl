<?xml version="1.0" encoding="iso-8859-1"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
<xsl:output method="html" doctype-public="-//W3C//DTD XHTML 1.0 Transitional//EN" doctype-system="http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd"/>

<xsl:template match="todo">
<html xmlns="http://www.w3.org/1999/xhtml">
<head><title>Gtk# TODO</title></head>
<body>
<xsl:apply-templates/>
</body>
</html>
</xsl:template>

<xsl:template match="item">
<table border="0">
<tr><td colspan="2"><b><xsl:apply-templates select="./brief"/></b></td></tr>
<tr><td><i>Description</i></td><td><xsl:apply-templates select="./description"/></td></tr>
<tr><td><i>Status</i></td><td><xsl:apply-templates select="./status"/>%</td></tr>
<tr><td><i>Notes</i></td><td><xsl:apply-templates select="./notes"/></td></tr>
<tr><td><i>Wrangler</i></td><td><xsl:apply-templates select="./wrangler"/></td></tr>
</table>
<br />
</xsl:template>

<xsl:template match="description|status|notes|wrangler">
<xsl:apply-templates/>
</xsl:template>

<!-- Handles copying unrecognized stuff: stolen from clahey.net -->
<xsl:template match="*">
<xsl:copy><xsl:for-each select="attribute::*"><xsl:copy/></xsl:for-each><xsl:apply-templates/></xsl:copy>
</xsl:template>

</xsl:stylesheet>

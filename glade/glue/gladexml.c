/* gladexml.c : Glue to access GladeXML fields
 *
 * Author: Ricardo Fernández Pascual <ric@users.sourceforge.net>
 *
 * <c> 2002 Ricardo Fernández Pascual
 */

#include <glade/glade.h>

const gchar *	gtksharp_glade_xml_get_filename		(GladeXML *gxml);


const gchar *
gtksharp_glade_xml_get_filename (GladeXML *gxml)
{
	return gxml->filename;
}


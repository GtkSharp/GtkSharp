/* attribute.c : Glue to access fields in PangoAttribute and
 * subclasses.
 *
 * Copyright (c) 2005 Novell, Inc.
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of version 2 of the Lesser GNU General 
 * Public License as published by the Free Software Foundation.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with this program; if not, write to the
 * Free Software Foundation, Inc., 59 Temple Place - Suite 330,
 * Boston, MA 02111-1307, USA.
 */

#include <pango/pango-attributes.h>

/* Forward declarations */
PangoAttrType pangosharp_attribute_get_attr_type (PangoAttribute *attr);
guint pangosharp_attribute_get_start_index (PangoAttribute *attr);
void pangosharp_attribute_set_start_index (PangoAttribute *attr, guint ind);
guint pangosharp_attribute_get_end_index (PangoAttribute *attr);
void pangosharp_attribute_set_end_index (PangoAttribute *attr, guint ind);
const char *pangosharp_attr_string_get_value (PangoAttrString *attr);
PangoLanguage *pangosharp_attr_language_get_value (PangoAttrLanguage *attr);
PangoColor pangosharp_attr_color_get_color (PangoAttrColor *attr);
int pangosharp_attr_int_get_value (PangoAttrInt *attr);
double pangosharp_attr_float_get_value (PangoAttrFloat *attr);
PangoFontDescription *pangosharp_attr_font_desc_get_desc (PangoAttrFontDesc *attr);
PangoRectangle pangosharp_attr_shape_get_ink_rect (PangoAttrShape *attr);
PangoRectangle pangosharp_attr_shape_get_logical_rect (PangoAttrShape *attr);
#ifdef GTK_SHARP_2_6
int pangosharp_attr_size_get_size (PangoAttrSize *attr);
gboolean pangosharp_attr_size_get_absolute (PangoAttrSize *attr);
#endif
/* */

PangoAttrType
pangosharp_attribute_get_attr_type (PangoAttribute *attr)
{
	return attr->klass->type;
}

guint 
pangosharp_attribute_get_start_index (PangoAttribute *attr)
{
	return attr->start_index;
}

void
pangosharp_attribute_set_start_index (PangoAttribute *attr, guint ind)
{
	attr->start_index = ind;
}

guint 
pangosharp_attribute_get_end_index (PangoAttribute *attr)
{
	return attr->end_index;
}

void
pangosharp_attribute_set_end_index (PangoAttribute *attr, guint ind)
{
	attr->end_index = ind;
}

const char *
pangosharp_attr_string_get_value (PangoAttrString *attr)
{
	return attr->value;
}

PangoLanguage *
pangosharp_attr_language_get_value (PangoAttrLanguage *attr)
{
	return attr->value;
}

PangoColor
pangosharp_attr_color_get_color (PangoAttrColor *attr)
{
	return attr->color;
}

int 
pangosharp_attr_int_get_value (PangoAttrInt *attr)
{
	return attr->value;
}

double 
pangosharp_attr_float_get_value (PangoAttrFloat *attr)
{
	return attr->value;
}

PangoFontDescription *
pangosharp_attr_font_desc_get_desc (PangoAttrFontDesc *attr)
{
	return attr->desc;
}

PangoRectangle 
pangosharp_attr_shape_get_ink_rect (PangoAttrShape *attr)
{
	return attr->ink_rect;
}

PangoRectangle 
pangosharp_attr_shape_get_logical_rect (PangoAttrShape *attr)
{
	return attr->logical_rect;
}

#ifdef GTK_SHARP_2_6
int 
pangosharp_attr_size_get_size (PangoAttrSize *attr)
{
	return attr->size;
}

gboolean 
pangosharp_attr_size_get_absolute (PangoAttrSize *attr)
{
	return attr->absolute;
}
#endif


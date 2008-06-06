/* object.c : Glue to clean up GtkObject references.
 *
 * Author: Mike Kestner <mkestner@speakeasy.net>
 *
 * Copyright (c) 2002 Mike Kestner
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

#include <glib-object.h>

/* Forward declarations */
int      gtksharp_object_get_ref_count (GObject *obj);
GObject *gtksharp_object_newv (GType type, gint cnt, gchar **names, GValue *vals);
void gtksharp_override_property_handlers(GType type, gpointer get_property_cb, gpointer set_property_cb);
GParamSpec *gtksharp_register_property(GType declaring_type, const gchar *name, const gchar *nick, const gchar *blurb, guint id, GType return_type, gboolean can_read, gboolean can_write);
/* */

int
gtksharp_object_get_ref_count (GObject *obj)
{
	return obj->ref_count;
}

GObject *
gtksharp_object_newv (GType type, gint cnt, gchar **names, GValue *vals)
{
	int i;
	GParameter *parms = NULL;
	GObject *result;

	if (cnt > 0)
		parms = g_new0 (GParameter, cnt);

	for (i = 0; i < cnt; i++) {
		parms[i].name = names[i];
		parms[i].value = vals[i];
	}

	result = g_object_newv (type, cnt, parms);

	g_free (parms);
	return result;
}

void
gtksharp_override_property_handlers(GType type, gpointer get_property_cb, gpointer set_property_cb)
{
	GObjectClass *type_class = g_type_class_peek (type);
	if(!type_class)
		type_class = g_type_class_ref (type);
	
	type_class->get_property = get_property_cb;
	type_class->set_property = set_property_cb;
}

GParamSpec *
gtksharp_register_property(GType declaring_type, const gchar *name, const gchar *nick, const gchar *blurb, guint id, GType return_type, gboolean can_read, gboolean can_write)
{
	GParamSpec *param_spec;
	GParamFlags flags = 0;
	GObjectClass *declaring_class = g_type_class_peek (declaring_type);
	if (!declaring_class)
		declaring_class = g_type_class_ref (declaring_type);
	if (can_read)
		flags |= G_PARAM_READABLE;
	if (can_write)
		flags |= G_PARAM_WRITABLE;

	/* Create the ParamSpec for the property
	*  These are used to hold default values and to validate values
	*  Both is not needed since the check for invalid values takes place in the managed set accessor of the property and properties do not
	*  contain default values. Therefore the ParamSpecs must allow every value that can be assigned to the property type.
	*  Furthermore the default value that is specified in the constructors will never be used and assigned to the property;
	*  they are not relevant, but have to be passed
	*/

	switch (return_type) {
	case G_TYPE_CHAR:
		param_spec = g_param_spec_char (name, nick, blurb, G_MININT8, G_MAXINT8, 0, flags);
		break;
	case G_TYPE_UCHAR:
		param_spec = g_param_spec_uchar (name, nick, blurb, 0, G_MAXUINT8, 0, flags);
		break;
	case G_TYPE_BOOLEAN:
		param_spec = g_param_spec_boolean (name, nick, blurb, FALSE, flags);
		break;
	case G_TYPE_INT:
		param_spec = g_param_spec_int (name, nick, blurb, G_MININT, G_MAXINT, 0, flags);
		break;
	case G_TYPE_UINT:
		param_spec = g_param_spec_uint (name, nick, blurb, 0, G_MAXUINT, 0, flags);
		break;
	case G_TYPE_LONG:
		param_spec = g_param_spec_long (name, nick, blurb, G_MINLONG, G_MAXLONG, 0, flags);
		break;
	case G_TYPE_ULONG:
		param_spec = g_param_spec_ulong (name, nick, blurb, 0, G_MAXULONG, 0, flags);
		break;
	case G_TYPE_INT64:
		param_spec = g_param_spec_int64 (name, nick, blurb, G_MININT64, G_MAXINT64, 0, flags);
		break;
	case G_TYPE_UINT64:
		param_spec = g_param_spec_uint64 (name, nick, blurb, 0, G_MAXUINT64, 0, flags);
		break;
	/* case G_TYPE_ENUM:  
	* case G_TYPE_FLAGS:
	* TODO: Implement both G_TYPE_ENUM and G_TYPE_FLAGS
	* My problem: Both g_param_spec_enum and g_param_spec_flags expect default property values and the members of the enum seemingly cannot be enumerated
	*/
	case G_TYPE_FLOAT:
		param_spec = g_param_spec_float (name, nick, blurb, G_MINFLOAT, G_MAXFLOAT, 0, flags);
		break;
	case G_TYPE_DOUBLE:
		param_spec = g_param_spec_double (name, nick, blurb, G_MINDOUBLE, G_MAXDOUBLE, 0, flags);
		break;
	case G_TYPE_STRING:
		param_spec = g_param_spec_string (name, nick, blurb, NULL, flags);
		break;
	case G_TYPE_POINTER:
		param_spec = g_param_spec_pointer (name, nick, blurb, flags);
		break;
	case G_TYPE_BOXED:
		param_spec = g_param_spec_boxed (name, nick, blurb, return_type, flags);
		break;
	case G_TYPE_OBJECT:
		param_spec = g_param_spec_object (name, nick, blurb, return_type, flags);
		break;
	default:
		// The property's return type is not supported
		return NULL;
	}

	g_object_class_install_property (declaring_class, id, param_spec);
	return param_spec;
}

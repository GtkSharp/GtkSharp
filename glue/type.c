/* type.c : GType utilities
 *
 * Author: Mike Kestner <mkestner@speakeasy.net>
 *
 * <c> 2002 Mike Kestner
 */

#include <glib-object.h>
#include <stdio.h>

/* Forward declarations */
G_CONST_RETURN gchar   *gtksharp_get_type_name (GObject *obj);

gboolean gtksharp_is_object (gpointer obj);

GType    gtksharp_get_type_id (GObject *obj);

GType    gtksharp_get_parent_type (GType typ);

G_CONST_RETURN gchar   *gtksharp_get_type_name_for_id (GType typ);

GType    gtksharp_register_type (gchar *name, GType parent);
/* */

G_CONST_RETURN gchar *
gtksharp_get_type_name (GObject *obj)
{
	return G_OBJECT_TYPE_NAME (obj);
}

gboolean
gtksharp_is_object (gpointer obj)
{
	return G_IS_OBJECT (obj);
}

GType
gtksharp_get_type_id (GObject *obj)
{
	return G_TYPE_FROM_INSTANCE (obj);
}

GType
gtksharp_get_parent_type (GType typ)
{
	return g_type_parent (typ);
}

G_CONST_RETURN gchar *
gtksharp_get_type_name_for_id (GType typ)
{
	return g_type_name (typ);
}

GType
gtksharp_register_type (gchar *name, GType parent)
{
	GTypeQuery query;
	GTypeInfo info = {0, NULL, NULL, NULL, NULL, NULL, 0, 0, NULL, NULL };

	g_type_query (parent, &query);

	info.class_size = query.class_size;
	info.instance_size = query.instance_size;

	return g_type_register_static (parent, name, &info, 0);
}

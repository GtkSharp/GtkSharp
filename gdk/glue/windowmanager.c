/* windowmanager.c : Glue to access the extended window 
 * manager hints via the root window properties using
 * gdk_property_get ()
 *
 * This work is based on the specification found here:
 *	http://www.freedesktop.org/standards/wm-spec/
 *
 * Author: Boyd Timothy <btimothy@novell.com>
 *
 * Copyright (c) 2004 Novell, Inc.
 */

#include <gdk/gdkscreen.h>
#include <gdk/gdkwindow.h>
#include <gdk/gdkproperty.h>

GList * gtksharp_get_gdk_net_supported (void);
guint * gtksharp_get_gdk_net_client_list (int *count);
gint gtksharp_get_gdk_net_number_of_desktops (void);
gint gtksharp_get_gdk_net_current_desktop (void);
guint gtksharp_get_gdk_net_active_window (void);
GList * gtksharp_get_gdk_net_workarea (void);

GList * 
gtksharp_get_gdk_net_supported (void)
{
	GdkAtom actual_property_type;
	int actual_format;
	int actual_length;
	long *data = NULL;
	GList *list = NULL;
	int i;

	if (!gdk_property_get (
			gdk_screen_get_root_window (gdk_screen_get_default ()),
			gdk_atom_intern ("_NET_SUPPORTED", FALSE),
			gdk_atom_intern ("ATOM", FALSE),
			0,
			G_MAXLONG,
			FALSE,
			&actual_property_type,
			&actual_format,
			&actual_length,
			(guchar **) &data)) {
		gchar *actual_property_type_name;
		g_critical ("Unable to get _NET_SUPPORTED");
		actual_property_type_name = gdk_atom_name (actual_property_type);
		if (actual_property_type_name) {
			g_message ("actual_property_type: %s", actual_property_type_name);
			g_free (actual_property_type_name);
		}
		return NULL;
	}

	/* Put all of the GdkAtoms into a GList to return */
	for (i = 0; i < actual_length / sizeof (long); i ++) {
		list = g_list_append (list, (GdkAtom) data [i]);
	}

	g_free (data);
	return list;
}

guint * 
gtksharp_get_gdk_net_client_list (int *count)
{
	GdkAtom actual_property_type;
	int actual_format;
	int actual_length;
	long *data = NULL;
	guint * list = NULL;
	int i;

	if (!gdk_property_get (
			gdk_screen_get_root_window (gdk_screen_get_default ()),
			gdk_atom_intern ("_NET_CLIENT_LIST", FALSE),
			gdk_atom_intern ("WINDOW", FALSE),
			0,
			G_MAXLONG,
			FALSE,
			&actual_property_type,
			&actual_format,
			&actual_length,
			(guchar **) &data)) {
		gchar *actual_property_type_name;
		g_critical ("Unable to get _NET_CLIENT_LIST");
		actual_property_type_name = gdk_atom_name (actual_property_type);
		if (actual_property_type_name) {
			g_message ("actual_property_type: %s", actual_property_type_name);
			g_free (actual_property_type_name);
		}
		return NULL;
	}

	*count = actual_length / sizeof (long);
	list = g_malloc (*count * sizeof (guint));
	/* Put all of the windows into a GList to return */
	for (i = 0; i < *count; i ++) {
		list [i] = data [i];
		g_message ("WinID: %d", list [i]);
	}

	g_free (data);
	return list;
}

gint 
gtksharp_get_gdk_net_number_of_desktops (void)
{
	GdkAtom actual_property_type;
	int actual_format;
	int actual_length;
	long *data = NULL;
	gint num_of_desktops;

	if (!gdk_property_get (
			gdk_screen_get_root_window (gdk_screen_get_default ()),
			gdk_atom_intern ("_NET_NUMBER_OF_DESKTOPS", FALSE),
			gdk_atom_intern ("CARDINAL", FALSE),
			0,
			G_MAXLONG,
			FALSE,
			&actual_property_type,
			&actual_format,
			&actual_length,
			(guchar **) &data)) {
		gchar *actual_property_type_name;
		g_critical ("Unable to get _NET_NUMBER_OF_DESKTOPS");
		actual_property_type_name = gdk_atom_name (actual_property_type);
		if (actual_property_type_name) {
			g_message ("actual_property_type: %s", actual_property_type_name);
			g_free (actual_property_type_name);
		}

		return -1;
	}

	num_of_desktops = (gint) data[0];
	g_free (data);

	return num_of_desktops;
}

gint 
gtksharp_get_gdk_net_current_desktop (void)
{
	GdkAtom actual_property_type;
	int actual_format;
	int actual_length;
	long *data = NULL;
	gint current_desktop;

	if (!gdk_property_get (
			gdk_screen_get_root_window (gdk_screen_get_default ()),
			gdk_atom_intern ("_NET_CURRENT_DESKTOP", FALSE),
			gdk_atom_intern ("CARDINAL", FALSE),
			0,
			G_MAXLONG,
			FALSE,
			&actual_property_type,
			&actual_format,
			&actual_length,
			(guchar **) &data)) {
		gchar *actual_property_type_name;
		g_critical ("Unable to get _NET_CURRENT_DESKTOP");
		actual_property_type_name = gdk_atom_name (actual_property_type);
		if (actual_property_type_name) {
			g_message ("actual_property_type: %s", actual_property_type_name);
			g_free (actual_property_type_name);
		}
		return -1;
	}

	current_desktop = (gint) data[0];
	g_free (data);

	return current_desktop;
}

guint 
gtksharp_get_gdk_net_active_window (void)
{
	GdkAtom actual_property_type;
	int actual_format;
	int actual_length;
	long *data = NULL;
	guint windowID = 0;

	if (!gdk_property_get (
			gdk_screen_get_root_window (gdk_screen_get_default ()),
			gdk_atom_intern ("_NET_ACTIVE_WINDOW", FALSE),		
			gdk_atom_intern ("WINDOW", FALSE),			
			0,							
			G_MAXLONG,						
			FALSE,							
			&actual_property_type,					
			&actual_format,						
			&actual_length,						
			(guchar **) &data)) {					
		gchar *actualPropertyTypeName;
		g_critical ("Unable to get _NET_ACTIVE_WINDOW");
		actualPropertyTypeName = gdk_atom_name (actual_property_type);
		if (actualPropertyTypeName) {
			g_message ("actual_property_type: %s", actualPropertyTypeName);
			g_free(actualPropertyTypeName);
		}
		return -1;
	}

	windowID = (gint) data [0];
	g_free (data);

	return windowID;
}

GList * 
gtksharp_get_gdk_net_workarea (void)
{
	GdkAtom actual_property_type;
	int actual_format;
	int actual_length;
	long *data = NULL;	
	int i = 0;
	GList *list = NULL;

	if (!gdk_property_get (
			gdk_screen_get_root_window (gdk_screen_get_default ()),	
			gdk_atom_intern ("_NET_WORKAREA", FALSE),		
			gdk_atom_intern ("CARDINAL", FALSE),			
			0,							
			G_MAXLONG,						
			FALSE,							
			&actual_property_type,					
			&actual_format,						
			&actual_length,						
			(guchar **) &data)) {					
		gchar *actualPropertyTypeName;
		g_critical ("Unable to get _NET_WORKAREA");
		actualPropertyTypeName = gdk_atom_name (actual_property_type);
		if (actualPropertyTypeName) {
			g_message ("actual_property_type: %s", actualPropertyTypeName);
			g_free(actualPropertyTypeName);
		}
		return FALSE;
	}

	for (i = 0; i < actual_length / sizeof (long); i += 4) {
		GdkRectangle *rectangle = g_malloc(sizeof (GdkRectangle));
		rectangle->x		= (int) data [i];
		rectangle->y		= (int) data [i + 1];
		rectangle->width	= (int) data [i + 2];
		rectangle->height	= (int) data [i + 3];
		list = g_list_append (list, rectangle);
	}


	if (data != NULL)
		g_free(data);

	return list;
}


/* device.c : Glue to access fields in GdkDevice.
 *
 * Author: Manuel V. Santos  <mvsl@telefonica.net>
 *
 * Copyright (c) Manuel V. Santos
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


#include <gdk/gdk.h>

/* Forward declarations */
gchar* gtksharp_gdk_device_get_name (GdkDevice *device);
GdkInputSource gtksharp_gdk_device_get_source (GdkDevice *device);
GdkInputMode gtksharp_gdk_device_get_mode (GdkDevice *device);
gboolean gtksharp_gdk_device_has_cursor (GdkDevice *device);
gint gtksharp_gdk_device_get_num_axes (GdkDevice *device);
gint gtksharp_gdk_device_get_num_keys (GdkDevice *device);
GdkDeviceAxis* gtksharp_gdk_device_get_axes (GdkDevice *device);
GdkDeviceKey* gtksharp_gdk_device_get_keys (GdkDevice *device);
/* */


gchar*
gtksharp_gdk_device_get_name (GdkDevice *device) 
{
	return device->name;
}

GdkInputSource
gtksharp_gdk_device_get_source (GdkDevice *device) 
{
	return device->source;
}

GdkInputMode
gtksharp_gdk_device_get_mode (GdkDevice *device)
{
	return device->mode;
}

gboolean
gtksharp_gdk_device_has_cursor (GdkDevice *device)
{
	return device->has_cursor;
}

gint 
gtksharp_gdk_device_get_num_axes (GdkDevice *device) 
{
	return device->num_axes;
}

gint
gtksharp_gdk_device_get_num_keys (GdkDevice *device) 
{
	return device->num_keys;
}

GdkDeviceAxis
gtksharp_gdk_device_get_device_axis (GdkDevice *device, guint i)
{
	return device->axes[i];
}

GdkDeviceKey
gtksharp_gdk_device_get_device_key (GdkDevice *device, guint i)
{
	return device->keys[i];
}


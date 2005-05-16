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
GdkDeviceAxis gtksharp_gdk_device_get_device_axis (GdkDevice *device, guint i);
GdkDeviceKey gtksharp_gdk_device_get_device_key (GdkDevice *device, guint i);

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


/* selection.c : Glue to access GdkAtoms defined in gdkselection.h
 *
 * Author: Mike Kestner  <mkestner@speakeasy.net>
 *
 * Copyright (c) 2003 Mike Kestner
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

#include <gdk/gdkselection.h>

GdkAtom gtksharp_get_gdk_selection_primary (void);
GdkAtom gtksharp_get_gdk_selection_secondary (void);
GdkAtom gtksharp_get_gdk_selection_clipboard (void);

/* FIXME: These are still left to do 
#define GDK_TARGET_BITMAP
#define GDK_TARGET_COLORMAP
#define GDK_TARGET_DRAWABLE
#define GDK_TARGET_PIXMAP
#define GDK_TARGET_STRING
#define GDK_SELECTION_TYPE_ATOM
#define GDK_SELECTION_TYPE_BITMAP
#define GDK_SELECTION_TYPE_COLORMAP
#define GDK_SELECTION_TYPE_DRAWABLE
#define GDK_SELECTION_TYPE_INTEGER
#define GDK_SELECTION_TYPE_PIXMAP
#define GDK_SELECTION_TYPE_WINDOW
#define GDK_SELECTION_TYPE_STRING
*/

GdkAtom
gtksharp_get_gdk_selection_primary ()
{
	return GDK_SELECTION_PRIMARY;
}

GdkAtom
gtksharp_get_gdk_selection_secondary ()
{
	return GDK_SELECTION_SECONDARY;
}

GdkAtom
gtksharp_get_gdk_selection_clipboard ()
{
	return GDK_SELECTION_CLIPBOARD;
}



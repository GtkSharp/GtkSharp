/* selection.c : Glue to access GdkAtoms defined in gdkselection.h
 *
 * Author: Mike Kestner  <mkestner@speakeasy.net>
 *
 * <c> 2003 Mike Kestner
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



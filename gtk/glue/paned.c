/* paned.c : Glue for accessing fields in the GtkPaned widget.
 *
 * Author: Duncan Mak  (duncan@ximian.com)
 *
 * Copyright (c) 2003 Ximian, Inc.
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

#include <gtk/gtkpaned.h>

/* Forward declarations */
GtkWidget *gtksharp_paned_get_child1 (GtkPaned *paned);
GtkWidget *gtksharp_paned_get_child2 (GtkPaned *paned);
/* */

GtkWidget*
gtksharp_paned_get_child1 (GtkPaned *paned)
{
	return paned->child1;
}

GtkWidget*
gtksharp_paned_get_child2 (GtkPaned *paned)
{
	return paned->child2;
}

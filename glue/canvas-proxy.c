/*
 * canvasproxy.c - glue functions for creating C# CanvasItems
 *
 * Author: Duncan Mak (duncan@ximian.com)
 *
 * Copyright (C), 2002. Ximian, Inc.
 *
 */

#include <gtk/gtkobject.h>

#include "canvas-proxy.h"
#include "canvas-proxy-marshal.h"

enum {
	UPDATE,
	REALIZE,
	UNREALIZE,
	MAP,
	UNMAP,
	COVERAGE,
	DRAW,
	RENDER,
	POINT,
	BOUNDS,
	LAST_SIGNAL,
};

static GnomeCanvasItemClass *parent_class;

static guint proxy_signals [LAST_SIGNAL];

/* Class initialization */
static void
gtksharp_canvas_proxy_class_init (CanvasProxyClass *class)
{
	GObjectClass *gobject_class;
	GtkObjectClass *object_class;
	GnomeCanvasItemClass *item_class;

	gobject_class = (GObjectClass *) class;
	object_class = (GtkObjectClass *) class;
	item_class = (GnomeCanvasItemClass *) class;

	parent_class = g_type_class_peek_parent (class);

	/* void (* update) (GnomeCanvasItem *item, double *affine, ArtSVP *clip_path, int flags); */
	proxy_signals [UPDATE] = g_signal_new ("update",
					       G_TYPE_FROM_CLASS (object_class),
					       G_SIGNAL_RUN_LAST,
					       G_STRUCT_OFFSET (GnomeCanvasItemClass, update),
					       NULL, NULL,
					       g_cclosure_user_marshal_VOID__OBJECT_DOUBLE_POINTER_INT,
					       G_TYPE_NONE, 3, G_TYPE_DOUBLE,
					       G_TYPE_POINTER, G_TYPE_INT);

	/* void (* realize) (GnomeCanvasItem *item); */
	proxy_signals [REALIZE] = g_signal_new ("realize",
						G_TYPE_FROM_CLASS (object_class),
						G_SIGNAL_RUN_LAST,
						G_STRUCT_OFFSET (GnomeCanvasItemClass, realize),
						NULL, NULL,
						g_cclosure_marshal_VOID__OBJECT,
						G_TYPE_NONE, 0);

	/* void (* unrealize) (GnomeCanvasItem *item); */
	proxy_signals [UNREALIZE] = g_signal_new ("unrealize",
						G_TYPE_FROM_CLASS (object_class),
						G_SIGNAL_RUN_LAST,
						G_STRUCT_OFFSET (GnomeCanvasItemClass, unrealize),
						NULL, NULL,
						g_cclosure_marshal_VOID__OBJECT,
						G_TYPE_NONE, 0);

	/* void (* map) (GnomeCanvasItem *item); */
	proxy_signals [MAP] = g_signal_new ("map",
					    G_TYPE_FROM_CLASS (object_class),
					    G_SIGNAL_RUN_LAST,
					    G_STRUCT_OFFSET (GnomeCanvasItemClass, map),
					    NULL, NULL,
					    g_cclosure_marshal_VOID__OBJECT,
					    G_TYPE_NONE, 0);

	/* void (* unmap) (GnomeCanvasItem *item); */
	proxy_signals [UNMAP] = g_signal_new ("unmap",
					    G_TYPE_FROM_CLASS (object_class),
					    G_SIGNAL_RUN_LAST,
					    G_STRUCT_OFFSET (GnomeCanvasItemClass, unmap),
					    NULL, NULL,
					    g_cclosure_marshal_VOID__OBJECT,
					    G_TYPE_NONE, 0);

	/* void (* draw) (GnomeCanvasItem *item, GdkDrawable *drawable, */
 	/*	       int x, int y, int width, int height); */
	proxy_signals [DRAW] = g_signal_new ("draw",
					     G_TYPE_FROM_CLASS (object_class),
					     G_SIGNAL_RUN_LAST,
					     G_STRUCT_OFFSET (GnomeCanvasItemClass, draw),
					     NULL, NULL,
					     g_cclosure_user_marshal_VOID__OBJECT_INT_INT_INT_INT,
					     G_TYPE_NONE, 5, GDK_TYPE_DRAWABLE,
					     G_TYPE_INT, G_TYPE_INT, G_TYPE_INT, G_TYPE_INT);

	/* void (* render) (GnomeCanvasItem *item, GnomeCanvasBuf *buf); */
	proxy_signals [RENDER] = g_signal_new ("render",
					       G_TYPE_FROM_CLASS (object_class),
					       G_SIGNAL_RUN_LAST,
					       G_STRUCT_OFFSET (GnomeCanvasItemClass, render),
					       NULL, NULL,
					       g_cclosure_marshal_VOID__POINTER,
					       G_TYPE_NONE, 1, G_TYPE_POINTER);

	/* double (* point) (GnomeCanvasItem *item, double x, double y, int cx, int cy, */
 	/*		  GnomeCanvasItem **actual_item); */
	proxy_signals [POINT] = g_signal_new ("point",
					       G_TYPE_FROM_CLASS (object_class),
					       G_SIGNAL_RUN_LAST,
					       G_STRUCT_OFFSET (GnomeCanvasItemClass, point),
					       NULL, NULL,
					       g_cclosure_user_marshal_DOUBLE__OBJECT_DOUBLE_DOUBLE_INT_INT_POINTER,
					       G_TYPE_DOUBLE, 5, G_TYPE_DOUBLE, G_TYPE_DOUBLE,
					       G_TYPE_INT, G_TYPE_INT, G_TYPE_POINTER);

	/* void (* bounds) (GnomeCanvasItem *item, double *x1, double *y1, double *x2, double *y2); */
	proxy_signals [BOUNDS] = g_signal_new ("bounds",
					       G_TYPE_FROM_CLASS (object_class),
					       G_SIGNAL_RUN_LAST,
					       G_STRUCT_OFFSET (GnomeCanvasItemClass, bounds),
					       NULL, NULL,
					       g_cclosure_user_marshal_VOID__OBJECT_POINTER_POINTER_POINTER_POINTER,
					       G_TYPE_NONE, 4, G_TYPE_POINTER, G_TYPE_POINTER,
					       G_TYPE_POINTER, G_TYPE_POINTER);
}

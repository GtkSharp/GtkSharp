/*
 * canvasproxy.h
 *
 * Author: Duncan Mak (duncan@ximian.com)
 *
 * Copyright (C), 2002. Ximian, Inc.
 *
 */

#ifndef CANVAS_PROXY_H
#define CANVAS_PROXY_H

#include <libgnomecanvas/gnome-canvas.h>

G_BEGIN_DECLS

typedef struct _CanvasProxy CanvasProxy;
typedef struct _CanvasProxyClass CanvasProxyClass;

struct _CanvasProxy {
	GnomeCanvasItem item;
};

struct _CanvasProxyClass {
	GnomeCanvasItemClass parent;
};

G_END_DECLS

#endif

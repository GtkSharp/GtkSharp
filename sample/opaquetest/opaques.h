/* -*- Mode: C; tab-width: 8; indent-tabs-mode: t; c-basic-offset: 8 -*- */
/*
 * Copyright (C) 2000-2003, Ximian, Inc.
 */

#ifndef GTKSHARP_OPAQUES_H
#define GTKSHARP_OPAQUES_H 1

#include <glib-object.h>

typedef void (*GtksharpGCFunc) (void);

typedef struct GtksharpOpaque GtksharpOpaque;
struct GtksharpOpaque {
	int serial;
	gboolean valid;

	GtksharpOpaque *friend;
};

typedef GtksharpOpaque *(*GtksharpOpaqueReturnFunc) (void);

GtksharpOpaque *gtksharp_opaque_new        (void);
int             gtksharp_opaque_get_serial (GtksharpOpaque *op);
void            gtksharp_opaque_set_friend (GtksharpOpaque *op,
					    GtksharpOpaque *friend);
GtksharpOpaque *gtksharp_opaque_get_friend (GtksharpOpaque *op);
GtksharpOpaque *gtksharp_opaque_copy       (GtksharpOpaque *op);
void            gtksharp_opaque_free       (GtksharpOpaque *op);

GtksharpOpaque *gtksharp_opaque_check      (GtksharpOpaqueReturnFunc func,
					    GtksharpGCFunc gc);
GtksharpOpaque *gtksharp_opaque_check_free (GtksharpOpaqueReturnFunc func,
					    GtksharpGCFunc gc);

int             gtksharp_opaque_get_last_serial (void);


typedef struct GtksharpRefcounted GtksharpRefcounted;
struct GtksharpRefcounted {
	int serial, refcount;
	gboolean valid;

	GtksharpRefcounted *friend;
};

typedef GtksharpRefcounted *(*GtksharpRefcountedReturnFunc) (void);

GtksharpRefcounted *gtksharp_refcounted_new          (void);
int                 gtksharp_refcounted_get_serial   (GtksharpRefcounted *ref);
void                gtksharp_refcounted_ref          (GtksharpRefcounted *ref);
void                gtksharp_refcounted_unref        (GtksharpRefcounted *ref);
int                 gtksharp_refcounted_get_refcount (GtksharpRefcounted *ref);
void                gtksharp_refcounted_set_friend   (GtksharpRefcounted *ref,
						      GtksharpRefcounted *friend);
GtksharpRefcounted *gtksharp_refcounted_get_friend   (GtksharpRefcounted *ref);

GtksharpRefcounted *gtksharp_refcounted_check        (GtksharpRefcountedReturnFunc func,
						      GtksharpGCFunc gc);
GtksharpRefcounted *gtksharp_refcounted_check_unref  (GtksharpRefcountedReturnFunc func,
						      GtksharpGCFunc gc);

int                 gtksharp_refcounted_get_last_serial (void);


gboolean gtksharp_opaquetest_get_error        (void);
void     gtksharp_opaquetest_set_error        (gboolean err);
void     gtksharp_opaquetest_set_expect_error (gboolean err);

#endif /* GTKSHARP_OPAQUES_H */

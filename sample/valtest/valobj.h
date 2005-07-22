/* -*- Mode: C; tab-width: 8; indent-tabs-mode: t; c-basic-offset: 8 -*- */
/*
 * Copyright (C) 2000-2003, Ximian, Inc.
 */

#ifndef GTKSHARP_VALOBJ_H
#define GTKSHARP_VALOBJ_H 1

#include <gtk/gtkwidget.h>
#include <gtk/gtkenums.h>

#define GTKSHARP_TYPE_VALOBJ            (gtksharp_valobj_get_type ())
#define GTKSHARP_VALOBJ(obj)            (G_TYPE_CHECK_INSTANCE_CAST ((obj), GTKSHARP_TYPE_VALOBJ, GtksharpValobj))
#define GTKSHARP_VALOBJ_CLASS(klass)    (G_TYPE_CHECK_CLASS_CAST ((klass), GTKSHARP_TYPE_VALOBJ, GtksharpValobjClass))
#define GTKSHARP_IS_VALOBJ(obj)         (G_TYPE_CHECK_INSTANCE_TYPE ((obj), GTKSHARP_TYPE_VALOBJ))
#define GTKSHARP_IS_VALOBJ_CLASS(klass) (G_TYPE_CHECK_CLASS_TYPE ((obj), GTKSHARP_TYPE_VALOBJ))
#define GTKSHARP_VALOBJ_GET_CLASS(obj)  (G_TYPE_INSTANCE_GET_CLASS ((obj), GTKSHARP_TYPE_VALOBJ, GtksharpValobjClass))

typedef struct {
	GObject parent;

	/*< private >*/
	gboolean the_boolean;
	int the_int;
	guint the_uint;
	gint64 the_int64;
	guint64 the_uint64;
	gunichar the_unichar;
	GtkArrowType the_enum;
	GtkAttachOptions the_flags;
	float the_float;
	double the_double;
	char *the_string;
	GdkRectangle the_rect;
	gpointer the_pointer;
	GtkWidget *the_object;
} GtksharpValobj;

typedef struct {
	GObjectClass parent_class;

} GtksharpValobjClass;

GType gtksharp_valobj_get_type (void);

GtksharpValobj   *gtksharp_valobj_new         (void);

gboolean          gtksharp_valobj_get_boolean (GtksharpValobj   *valobj);
void              gtksharp_valobj_set_boolean (GtksharpValobj   *valobj,
					       gboolean          val);
int               gtksharp_valobj_get_int     (GtksharpValobj   *valobj);
void              gtksharp_valobj_set_int     (GtksharpValobj   *valobj,
					       int               val);
guint             gtksharp_valobj_get_uint    (GtksharpValobj   *valobj);
void              gtksharp_valobj_set_uint    (GtksharpValobj   *valobj,
					       guint             val);
gint64            gtksharp_valobj_get_int64   (GtksharpValobj   *valobj);
void              gtksharp_valobj_set_int64   (GtksharpValobj   *valobj,
					       gint64            val);
guint64           gtksharp_valobj_get_uint64  (GtksharpValobj   *valobj);
void              gtksharp_valobj_set_uint64  (GtksharpValobj   *valobj,
					       guint64           val);
gunichar          gtksharp_valobj_get_unichar (GtksharpValobj   *valobj);
void              gtksharp_valobj_set_unichar (GtksharpValobj   *valobj,
					       gunichar          val);
GtkArrowType      gtksharp_valobj_get_enum    (GtksharpValobj   *valobj);
void              gtksharp_valobj_set_enum    (GtksharpValobj   *valobj,
					       GtkArrowType      val);
GtkAttachOptions  gtksharp_valobj_get_flags   (GtksharpValobj   *valobj);
void              gtksharp_valobj_set_flags   (GtksharpValobj   *valobj,
					       GtkAttachOptions  val);
float             gtksharp_valobj_get_float   (GtksharpValobj   *valobj);
void              gtksharp_valobj_set_float   (GtksharpValobj   *valobj,
					       float             val);
double            gtksharp_valobj_get_double  (GtksharpValobj   *valobj);
void              gtksharp_valobj_set_double  (GtksharpValobj   *valobj,
					       double            val);
char             *gtksharp_valobj_get_string  (GtksharpValobj   *valobj);
void              gtksharp_valobj_set_string  (GtksharpValobj   *valobj,
					       const char       *val);
GdkRectangle     *gtksharp_valobj_get_boxed   (GtksharpValobj   *valobj);
void              gtksharp_valobj_set_boxed   (GtksharpValobj   *valobj,
					       GdkRectangle     *val);
gpointer          gtksharp_valobj_get_pointer (GtksharpValobj   *valobj);
void              gtksharp_valobj_set_pointer (GtksharpValobj   *valobj,
					       gpointer          val);
GtkWidget        *gtksharp_valobj_get_object  (GtksharpValobj   *valobj);
void              gtksharp_valobj_set_object  (GtksharpValobj   *valobj,
					       GtkWidget        *val);

#endif /* GTKSHARP_VALOBJ_H */

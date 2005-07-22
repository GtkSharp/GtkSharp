/* valobj.c: An object with properties of each possible type
 *
 * Copyright (c) 2005 Novell, Inc.
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

#include "valobj.h"

G_DEFINE_TYPE (GtksharpValobj, gtksharp_valobj, G_TYPE_OBJECT)

/* We actually don't do properties of type PARAM, VALUE_ARRAY, or OVERRIDE */

enum {
	PROP_0,

	PROP_BOOLEAN,
	PROP_INT,
	PROP_UINT,
	PROP_INT64,
	PROP_UINT64,
	PROP_UNICHAR,
	PROP_ENUM,
	PROP_FLAGS,
	PROP_FLOAT,
	PROP_DOUBLE,
	PROP_STRING,
	PROP_BOXED,
	PROP_POINTER,
	PROP_OBJECT,

	LAST_PROP
};

static void set_property (GObject *object, guint prop_id,
			  const GValue *value, GParamSpec *pspec);
static void get_property (GObject *object, guint prop_id,
			  GValue *value, GParamSpec *pspec);

static void
gtksharp_valobj_init (GtksharpValobj *sock)
{
}

static void
gtksharp_valobj_class_init (GtksharpValobjClass *valobj_class)
{
	GObjectClass *object_class = G_OBJECT_CLASS (valobj_class);

	/* virtual method override */
	object_class->set_property = set_property;
	object_class->get_property = get_property;

	/* properties */
	g_object_class_install_property (
		object_class, PROP_BOOLEAN,
		g_param_spec_boolean ("boolean_prop", "Boolean", "boolean property",
				      FALSE,
				      G_PARAM_READWRITE | G_PARAM_CONSTRUCT));

	g_object_class_install_property (
		object_class, PROP_INT,
		g_param_spec_int ("int_prop", "Int", "int property",
				  G_MININT, G_MAXINT, 0,
				  G_PARAM_READWRITE | G_PARAM_CONSTRUCT));
	g_object_class_install_property (
		object_class, PROP_UINT,
		g_param_spec_uint ("uint_prop", "Unsigned Int", "uint property",
				   0, G_MAXUINT, 0,
				   G_PARAM_READWRITE | G_PARAM_CONSTRUCT));

	g_object_class_install_property (
		object_class, PROP_INT64,
		g_param_spec_int64 ("int64_prop", "Int64", "int64 property",
				    G_MININT64, G_MAXINT64, 0,
				    G_PARAM_READWRITE | G_PARAM_CONSTRUCT));
	g_object_class_install_property (
		object_class, PROP_UINT64,
		g_param_spec_uint64 ("uint64_prop", "Unsigned Int64", "uint64 property",
				     0, G_MAXUINT64, 0,
				     G_PARAM_READWRITE | G_PARAM_CONSTRUCT));

	g_object_class_install_property (
		object_class, PROP_UNICHAR,
		g_param_spec_unichar ("unichar_prop", "Unichar", "unichar property",
				      (gunichar)' ',
				      G_PARAM_READWRITE | G_PARAM_CONSTRUCT));

	g_object_class_install_property (
		object_class, PROP_ENUM,
		g_param_spec_enum ("enum_prop", "Enum", "enum property",
				   GTK_TYPE_ARROW_TYPE, GTK_ARROW_UP,
				   G_PARAM_READWRITE | G_PARAM_CONSTRUCT));
	g_object_class_install_property (
		object_class, PROP_FLAGS,
		g_param_spec_flags ("flags_prop", "Flags", "flags property",
				   GTK_TYPE_ATTACH_OPTIONS, 0,
				   G_PARAM_READWRITE | G_PARAM_CONSTRUCT));

	g_object_class_install_property (
		object_class, PROP_FLOAT,
		g_param_spec_float ("float_prop", "Float", "float property",
				    -G_MAXFLOAT, G_MAXFLOAT, 0.0f,
				    G_PARAM_READWRITE | G_PARAM_CONSTRUCT));
	g_object_class_install_property (
		object_class, PROP_DOUBLE,
		g_param_spec_double ("double_prop", "Double", "double property",
				     -G_MAXDOUBLE, G_MAXDOUBLE, 0.0f,
				     G_PARAM_READWRITE | G_PARAM_CONSTRUCT));

	g_object_class_install_property (
		object_class, PROP_STRING,
		g_param_spec_string ("string_prop", "String", "string property",
				     "foo",
				     G_PARAM_READWRITE | G_PARAM_CONSTRUCT));

	g_object_class_install_property (
		object_class, PROP_BOXED,
		g_param_spec_boxed ("boxed_prop", "Boxed", "boxed property",
				    GDK_TYPE_RECTANGLE,
				    G_PARAM_READWRITE));

	g_object_class_install_property (
		object_class, PROP_POINTER,
		g_param_spec_pointer ("pointer_prop", "Pointer", "pointer property",
				      G_PARAM_READWRITE));

	g_object_class_install_property (
		object_class, PROP_OBJECT,
		g_param_spec_object ("object_prop", "Object", "object property",
				     GTK_TYPE_WIDGET,
				     G_PARAM_READWRITE));
}

static void
set_property (GObject *object, guint prop_id,
	      const GValue *value, GParamSpec *pspec)
{
	GtksharpValobj *valobj = GTKSHARP_VALOBJ (object);

	switch (prop_id) {
	case PROP_BOOLEAN:
		valobj->the_boolean = g_value_get_boolean (value);
		break;
	case PROP_INT:
		valobj->the_int = g_value_get_int (value);
		break;
	case PROP_UINT:
		valobj->the_uint = g_value_get_uint (value);
		break;
	case PROP_INT64:
		valobj->the_int64 = g_value_get_int64 (value);
		break;
	case PROP_UINT64:
		valobj->the_uint64 = g_value_get_uint64 (value);
		break;
	case PROP_UNICHAR:
		valobj->the_unichar = (gunichar)g_value_get_uint (value);
		break;
	case PROP_ENUM:
		valobj->the_enum = g_value_get_enum (value);
		break;
	case PROP_FLAGS:
		valobj->the_flags = g_value_get_flags (value);
		break;
	case PROP_FLOAT:
		valobj->the_float = g_value_get_float (value);
		break;
	case PROP_DOUBLE:
		valobj->the_double = g_value_get_double (value);
		break;
	case PROP_STRING:
		if (valobj->the_string)
			g_free (valobj->the_string);
		valobj->the_string = g_value_dup_string (value);
		break;
	case PROP_BOXED:
		valobj->the_rect = *(GdkRectangle *)g_value_get_boxed (value);
		break;
	case PROP_POINTER:
		valobj->the_pointer = g_value_get_pointer (value);
		break;
	case PROP_OBJECT:
		if (valobj->the_object)
			g_object_unref (valobj->the_object);
		valobj->the_object = (GtkWidget *)g_value_dup_object (value);
		break;
	default:
		break;
	}
}

static void
get_property (GObject *object, guint prop_id,
	      GValue *value, GParamSpec *pspec)
{
	GtksharpValobj *valobj = GTKSHARP_VALOBJ (object);

	switch (prop_id) {
	case PROP_BOOLEAN:
		g_value_set_boolean (value, valobj->the_boolean);
		break;
	case PROP_INT:
		g_value_set_int (value, valobj->the_int);
		break;
	case PROP_UINT:
		g_value_set_uint (value, valobj->the_uint);
		break;
	case PROP_INT64:
		g_value_set_int64 (value, valobj->the_int64);
		break;
	case PROP_UINT64:
		g_value_set_uint64 (value, valobj->the_uint64);
		break;
	case PROP_UNICHAR:
		g_value_set_uint (value, (guint)valobj->the_unichar);
		break;
	case PROP_ENUM:
		g_value_set_enum (value, valobj->the_enum);
		break;
	case PROP_FLAGS:
		g_value_set_flags (value, valobj->the_flags);
		break;
	case PROP_FLOAT:
		g_value_set_float (value, valobj->the_float);
		break;
	case PROP_DOUBLE:
		g_value_set_double (value, valobj->the_double);
		break;
	case PROP_STRING:
		g_value_set_string (value, valobj->the_string);
		break;
	case PROP_BOXED:
		g_value_set_boxed (value, &valobj->the_rect);
		break;
	case PROP_POINTER:
		g_value_set_pointer (value, valobj->the_pointer);
		break;
	case PROP_OBJECT:
		g_value_set_object (value, valobj->the_object);
		break;
	default:
		break;
	}
}

GtksharpValobj *
gtksharp_valobj_new (void)
{
	return g_object_new (GTKSHARP_TYPE_VALOBJ, NULL);
}


gboolean
gtksharp_valobj_get_boolean (GtksharpValobj *valobj)
{
	return valobj->the_boolean;
}

void
gtksharp_valobj_set_boolean (GtksharpValobj *valobj, gboolean val)
{
	valobj->the_boolean = val;
}

int
gtksharp_valobj_get_int (GtksharpValobj *valobj)
{
	return valobj->the_int;
}

void
gtksharp_valobj_set_int (GtksharpValobj *valobj, int val)
{
	valobj->the_int = val;
}

guint
gtksharp_valobj_get_uint (GtksharpValobj *valobj)
{
	return valobj->the_uint;
}

void
gtksharp_valobj_set_uint (GtksharpValobj *valobj, guint val)
{
	valobj->the_uint = val;
}

gint64
gtksharp_valobj_get_int64 (GtksharpValobj *valobj)
{
	return valobj->the_int64;
}

void
gtksharp_valobj_set_int64 (GtksharpValobj *valobj, gint64 val)
{
	valobj->the_int64 = val;
}

guint64
gtksharp_valobj_get_uint64 (GtksharpValobj *valobj)
{
	return valobj->the_uint64;
}

void
gtksharp_valobj_set_uint64 (GtksharpValobj *valobj, guint64 val)
{
	valobj->the_uint64 = val;
}

gunichar
gtksharp_valobj_get_unichar (GtksharpValobj *valobj)
{
	return valobj->the_unichar;
}

void
gtksharp_valobj_set_unichar (GtksharpValobj *valobj, gunichar val)
{
	valobj->the_unichar = val;
}

GtkArrowType
gtksharp_valobj_get_enum (GtksharpValobj *valobj)
{
	return valobj->the_enum;
}

void
gtksharp_valobj_set_enum (GtksharpValobj *valobj, GtkArrowType val)
{
	valobj->the_enum = val;
}

GtkAttachOptions
gtksharp_valobj_get_flags (GtksharpValobj *valobj)
{
	return valobj->the_flags;
}

void
gtksharp_valobj_set_flags (GtksharpValobj *valobj, GtkAttachOptions val)
{
	valobj->the_flags = val;
}

float
gtksharp_valobj_get_float (GtksharpValobj *valobj)
{
	return valobj->the_float;
}

void
gtksharp_valobj_set_float (GtksharpValobj *valobj, float val)
{
	valobj->the_float = val;
}

double
gtksharp_valobj_get_double (GtksharpValobj *valobj)
{
	return valobj->the_double;
}

void
gtksharp_valobj_set_double (GtksharpValobj *valobj, double val)
{
	valobj->the_double = val;
}

char *
gtksharp_valobj_get_string (GtksharpValobj *valobj)
{
	return valobj->the_string;
}

void
gtksharp_valobj_set_string (GtksharpValobj *valobj, const char *val)
{
	if (valobj->the_string)
		g_free (valobj->the_string);
	valobj->the_string = g_strdup (val);
}

GdkRectangle *
gtksharp_valobj_get_boxed (GtksharpValobj *valobj)
{
	return &valobj->the_rect;
}

void
gtksharp_valobj_set_boxed (GtksharpValobj *valobj, GdkRectangle *val)
{
	valobj->the_rect = *val;
}

gpointer
gtksharp_valobj_get_pointer (GtksharpValobj *valobj)
{
	return valobj->the_pointer;
}

void
gtksharp_valobj_set_pointer (GtksharpValobj *valobj, gpointer val)
{
	valobj->the_pointer = val;
}

GtkWidget *
gtksharp_valobj_get_object (GtksharpValobj *valobj)
{
	return valobj->the_object;
}

void
gtksharp_valobj_set_object (GtksharpValobj *valobj, GtkWidget *val)
{
	if (valobj->the_object)
		g_object_unref (valobj->the_object);
	valobj->the_object = g_object_ref (val);
}

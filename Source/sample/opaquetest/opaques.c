/* opaques.c: Opaque memory management test objects
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

#include "opaques.h"
#include <stdio.h>

static int opserial, refserial;
static gboolean error = FALSE, expect_error = FALSE;

static gboolean check_error (gboolean valid, const char *msg, ...);

GtksharpOpaque *
gtksharp_opaque_new (void)
{
	GtksharpOpaque *op = g_new0 (GtksharpOpaque, 1);
	op->valid = TRUE;
	op->serial = opserial++;
	return op;
}

int
gtksharp_opaque_get_serial (GtksharpOpaque *op)
{
	check_error (op->valid, "get_serial on freed GtksharpOpaque serial %d\n", op->serial);
	return op->serial;
}

void gtksharp_opaque_set_friend (GtksharpOpaque *op, GtksharpOpaque *friend)
{
	check_error (op->valid, "set_friend on freed GtksharpOpaque serial %d\n", op->serial);
	op->friend = friend;
}

GtksharpOpaque *
gtksharp_opaque_get_friend (GtksharpOpaque *op)
{
	check_error (op->valid, "get_friend on freed GtksharpOpaque serial %d\n", op->serial);
	return op->friend;
}

GtksharpOpaque *
gtksharp_opaque_copy (GtksharpOpaque *op)
{
	check_error (op->valid, "copying freed GtksharpOpaque serial %d\n", op->serial);
	return gtksharp_opaque_new ();
}

void
gtksharp_opaque_free (GtksharpOpaque *op)
{
	check_error (op->valid, "Double free of GtksharpOpaque serial %d\n", op->serial);
	op->valid = FALSE;
	/* We don't actually free it */
}

GtksharpOpaque *
gtksharp_opaque_check (GtksharpOpaqueReturnFunc func, GtksharpGCFunc gc)
{
	GtksharpOpaque *op = func ();
	gc ();
	return op;
}

GtksharpOpaque *
gtksharp_opaque_check_free (GtksharpOpaqueReturnFunc func, GtksharpGCFunc gc)
{
	GtksharpOpaque *op = func ();
	gc ();
	gtksharp_opaque_free (op);
	gc ();
	return op;
}

int
gtksharp_opaque_get_last_serial (void)
{
	return opserial - 1;
}


GtksharpRefcounted *
gtksharp_refcounted_new (void)
{
	GtksharpRefcounted *ref = g_new0 (GtksharpRefcounted, 1);
	ref->valid = TRUE;
	ref->refcount = 1;
	ref->serial = refserial++;
	return ref;
}

int
gtksharp_refcounted_get_serial (GtksharpRefcounted *ref)
{
	check_error (ref->valid, "get_serial on freed GtksharpRefcounted serial %d\n", ref->serial);
	return ref->serial;
}
 
void
gtksharp_refcounted_ref (GtksharpRefcounted *ref)
{
	if (check_error (ref->valid, "ref on freed GtksharpRefcounted serial %d\n", ref->serial))
		return;
	ref->refcount++;
}

void
gtksharp_refcounted_unref (GtksharpRefcounted *ref)
{
	if (check_error (ref->valid, "unref on freed GtksharpRefcounted serial %d\n", ref->serial))
		return;
	if (--ref->refcount == 0) {
		ref->valid = FALSE;
		/* We don't actually free it */
	}
}

int
gtksharp_refcounted_get_refcount (GtksharpRefcounted *ref)
{
	check_error (ref->valid, "get_refcount on freed GtksharpRefcounted serial %d\n", ref->serial);
	return ref->refcount;
}

void
gtksharp_refcounted_set_friend (GtksharpRefcounted *ref, GtksharpRefcounted *friend)
{
	check_error (ref->valid, "set_friend on freed GtksharpRefcounted serial %d\n", ref->serial);
	if (ref->friend)
		gtksharp_refcounted_unref (ref->friend);
	ref->friend = friend;
	if (ref->friend)
		gtksharp_refcounted_ref (ref->friend);
}

GtksharpRefcounted *
gtksharp_refcounted_get_friend (GtksharpRefcounted *ref)
{
	check_error (ref->valid, "get_friend on freed GtksharpRefcounted serial %d\n", ref->serial);
	return ref->friend;
}

GtksharpRefcounted *
gtksharp_refcounted_check (GtksharpRefcountedReturnFunc func, GtksharpGCFunc gc)
{
	GtksharpRefcounted *ref = func ();
	gc ();
	return ref;
}

GtksharpRefcounted *
gtksharp_refcounted_check_unref (GtksharpRefcountedReturnFunc func, GtksharpGCFunc gc)
{
	GtksharpRefcounted *ref = func ();
	gc ();
	gtksharp_refcounted_unref (ref);
	gc ();
	return ref;
}

int
gtksharp_refcounted_get_last_serial (void)
{
	return refserial - 1;
}


static gboolean
check_error (gboolean valid, const char *msg, ...)
{
	va_list ap;

	if (valid)
		return FALSE;

	error = TRUE;
	if (expect_error) {
		expect_error = FALSE;
		return FALSE;
	}

	expect_error = FALSE;

	fprintf (stderr, "  UNMANAGED ERROR: ");
	va_start (ap, msg);
	vfprintf (stderr, msg, ap);
	va_end (ap);

	return TRUE;
}


gboolean
gtksharp_opaquetest_get_error (void)
{
	gboolean old_error = error;
	error = expect_error = FALSE;
	return old_error;
}

void
gtksharp_opaquetest_set_error (gboolean err)
{
	error = err;
}

void
gtksharp_opaquetest_set_expect_error (gboolean err)
{
	expect_error = err;
}

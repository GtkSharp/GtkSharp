/* 
 * nodestore.c
 *
 * Copyright (c) 2003  Novell, Inc.
 *
 * Authors: Mike Kestner  <mkestner@ximian.com>
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

#include <gtk/gtktreemodel.h>

typedef  GtkTreeModelFlags (* GtkSharpNodeStoreGetFlagsFunc) (void);
typedef  gint (* GtkSharpNodeStoreGetNColumnsFunc) (void);
typedef  GType (* GtkSharpNodeStoreGetColumnTypeFunc) (gint col);
typedef  gboolean (* GtkSharpNodeStoreGetNodeFunc) (gint *node_idx, GtkTreePath *path);
typedef  GtkTreePath* (* GtkSharpNodeStoreGetPathFunc) (gint node_idx);
typedef  void (* GtkSharpNodeStoreGetValueFunc) (gint node_idx, gint col, GValue *value);
typedef  gboolean (* GtkSharpNodeStoreIterNextFunc) (gint *node_idx);
typedef  gboolean (* GtkSharpNodeStoreIterChildrenFunc) (gint *first_child, gint parent);
typedef  gboolean (* GtkSharpNodeStoreIterHasChildFunc) (gint node_idx);
typedef  gint (* GtkSharpNodeStoreIterNChildrenFunc) (gint node_idx);
typedef  gboolean (* GtkSharpNodeStoreIterNthChildFunc) (gint *child, gint parent, gint n);
typedef  gboolean (* GtkSharpNodeStoreIterParentFunc) (gint *parent, gint child);

typedef struct _GtkSharpNodeStoreTreeModelIface {
	GtkSharpNodeStoreGetFlagsFunc get_flags;
	GtkSharpNodeStoreGetNColumnsFunc get_n_columns;
	GtkSharpNodeStoreGetColumnTypeFunc get_column_type;
	GtkSharpNodeStoreGetNodeFunc get_node;
	GtkSharpNodeStoreGetPathFunc get_path;
	GtkSharpNodeStoreGetValueFunc get_value;
	GtkSharpNodeStoreIterNextFunc iter_next;
	GtkSharpNodeStoreIterChildrenFunc iter_children;
	GtkSharpNodeStoreIterHasChildFunc iter_has_child;
	GtkSharpNodeStoreIterNChildrenFunc iter_n_children;
	GtkSharpNodeStoreIterNthChildFunc iter_nth_child;
	GtkSharpNodeStoreIterParentFunc iter_parent;
} GtkSharpNodeStoreTreeModelIface;

typedef struct _GtkSharpNodeStore {
	GObject parent;

	gint stamp;
	GtkSharpNodeStoreTreeModelIface tree_model;
} GtkSharpNodeStore;

typedef struct _GtkSharpNodeStoreClass {
	GObjectClass parent;
} GtkSharpNodeStoreClass;

GType gtksharp_node_store_get_type (void);
GObject * gtksharp_node_store_new (void);
void gtksharp_node_store_set_tree_model_callbacks (GtkSharpNodeStore *store, GtkSharpNodeStoreTreeModelIface *iface);
void gtksharp_node_store_emit_row_changed (GtkSharpNodeStore *store, GtkTreePath *path, gint node_idx);
void gtksharp_node_store_emit_row_inserted (GtkSharpNodeStore *store, GtkTreePath *path, gint node_idx);
void gtksharp_node_store_emit_row_deleted (GtkSharpNodeStore *store, GtkTreePath *path);
void gtksharp_node_store_emit_row_has_child_toggled (GtkSharpNodeStore *store, GtkTreePath *path, gint node_idx);

static GtkTreeModelFlags
gns_get_flags (GtkTreeModel *model)
{
	GtkSharpNodeStore *store = (GtkSharpNodeStore *) model;
	return store->tree_model.get_flags ();
}

static int
gns_get_n_columns (GtkTreeModel *model)
{
	GtkSharpNodeStore *store = (GtkSharpNodeStore *) model;
	return store->tree_model.get_n_columns ();
}

static GType
gns_get_column_type (GtkTreeModel *model, int col)
{
	GtkSharpNodeStore *store = (GtkSharpNodeStore *) model;
	return store->tree_model.get_column_type (col);
}

static gboolean
gns_get_iter (GtkTreeModel *model, GtkTreeIter *iter, GtkTreePath *path)
{
	GtkSharpNodeStore *store = (GtkSharpNodeStore *) model;
	gint node_idx;

	if (!store->tree_model.get_node (&node_idx, path))
		return FALSE;

	iter->stamp = store->stamp;
	iter->user_data = GINT_TO_POINTER (node_idx);
	return TRUE;
}

static GtkTreePath *
gns_get_path (GtkTreeModel *model, GtkTreeIter *iter)
{
	GtkSharpNodeStore *store = (GtkSharpNodeStore *) model;
	return store->tree_model.get_path (GPOINTER_TO_INT (iter->user_data));
}
	
static void
gns_get_value (GtkTreeModel *model, GtkTreeIter *iter, int col, GValue *value)
{
	GtkSharpNodeStore *store = (GtkSharpNodeStore *) model;
	store->tree_model.get_value (GPOINTER_TO_INT (iter->user_data), col, value);
}

static gboolean
gns_iter_next (GtkTreeModel *model, GtkTreeIter *iter)
{
	GtkSharpNodeStore *store = (GtkSharpNodeStore *) model;
	gint node_idx;

	if (store->stamp != iter->stamp)
		return FALSE;

	node_idx = GPOINTER_TO_INT (iter->user_data);
	if (!store->tree_model.iter_next (&node_idx)) {
		iter->stamp = -1;
		return FALSE;
	}

	iter->user_data = GINT_TO_POINTER (node_idx);
	return TRUE;
}

static gboolean
gns_iter_children (GtkTreeModel *model, GtkTreeIter *iter, GtkTreeIter *parent)
{
	GtkSharpNodeStore *store = (GtkSharpNodeStore *) model;
	gint child_idx, parent_idx;

	if (!parent) 
		parent_idx = -1;
	else {
		if (store->stamp != parent->stamp)
			return FALSE;
		parent_idx = GPOINTER_TO_INT (parent->user_data);
	}

	if (!store->tree_model.iter_children (&child_idx, parent_idx))
		return FALSE;

	iter->stamp = store->stamp;
	iter->user_data = GINT_TO_POINTER (child_idx);
	return TRUE;
}

static gboolean
gns_iter_has_child (GtkTreeModel *model, GtkTreeIter *iter)
{
	GtkSharpNodeStore *store = (GtkSharpNodeStore *) model;
	return store->tree_model.iter_has_child (GPOINTER_TO_INT (iter->user_data));
}

static int
gns_iter_n_children (GtkTreeModel *model, GtkTreeIter *iter)
{
	GtkSharpNodeStore *store = (GtkSharpNodeStore *) model;
	gint node_idx;

	if (!iter) 
		node_idx = -1;
	else {
		if (store->stamp != iter->stamp)
			return 0;
		node_idx = GPOINTER_TO_INT (iter->user_data);
	}

	return store->tree_model.iter_n_children (node_idx);
}

static gboolean
gns_iter_nth_child (GtkTreeModel *model, GtkTreeIter *iter, GtkTreeIter *parent, int n)
{
	GtkSharpNodeStore *store = (GtkSharpNodeStore *) model;
	gint child_idx, parent_idx;

	if (!parent) 
		parent_idx = -1;
	else {
		if (store->stamp != parent->stamp)
			return FALSE;
		parent_idx = GPOINTER_TO_INT (parent->user_data);
	}

	if (!store->tree_model.iter_nth_child (&child_idx, parent_idx, n))
		return FALSE;

	iter->stamp = store->stamp;
	iter->user_data = GINT_TO_POINTER (child_idx);
	return TRUE;
}

static gboolean
gns_iter_parent (GtkTreeModel *model, GtkTreeIter *iter, GtkTreeIter *child)
{
	GtkSharpNodeStore *store = (GtkSharpNodeStore *) model;
	gint parent;

	if (store->stamp != child->stamp)
		return FALSE;

	if (!store->tree_model.iter_parent (&parent, GPOINTER_TO_INT (child->user_data)))
		return FALSE;
	
	iter->stamp = store->stamp;
	iter->user_data = GINT_TO_POINTER (parent);
	return TRUE;
}

static void
gns_tree_model_init (GtkTreeModelIface *iface)
{
	iface->get_flags = gns_get_flags;
	iface->get_n_columns = gns_get_n_columns;
	iface->get_column_type = gns_get_column_type;
	iface->get_iter = gns_get_iter;
	iface->get_path = gns_get_path;
	iface->get_value = gns_get_value;
	iface->iter_next = gns_iter_next;
	iface->iter_children = gns_iter_children;
	iface->iter_has_child = gns_iter_has_child;
	iface->iter_n_children = gns_iter_n_children;
	iface->iter_nth_child = gns_iter_nth_child;
	iface->iter_parent = gns_iter_parent;
}

static void
gns_class_init (GObjectClass *klass)
{
}


static void
gns_init (GtkSharpNodeStore *store)
{
	store->stamp = 0;
	store->tree_model.get_flags = NULL;
	store->tree_model.get_n_columns = NULL;
	store->tree_model.get_column_type = NULL;
	store->tree_model.get_node = NULL;
	store->tree_model.get_path = NULL;
	store->tree_model.get_value = NULL;
	store->tree_model.iter_next = NULL;
	store->tree_model.iter_children = NULL;
	store->tree_model.iter_has_child = NULL;
	store->tree_model.iter_n_children = NULL;
	store->tree_model.iter_nth_child = NULL;
	store->tree_model.iter_parent = NULL;
}

GType
gtksharp_node_store_get_type (void)
{
	static GType gns_type = 0;

	if (!gns_type) {
		static const GTypeInfo gns_info = {
				sizeof (GtkSharpNodeStoreClass),
				NULL,           /* base_init */
				NULL,           /* base_finalize */
				(GClassInitFunc) gns_class_init,
				NULL,           /* class_finalize */
				NULL,           /* class_data */
				sizeof (GtkSharpNodeStore),
				0,
				(GInstanceInitFunc) gns_init };
				                                                                                                
		static const GInterfaceInfo tree_model_info = {
				(GInterfaceInitFunc) gns_tree_model_init,
				NULL,
				NULL };
					                                                                                              
		gns_type = g_type_register_static (G_TYPE_OBJECT, "GtkSharpNodeStore", &gns_info, 0);
 
		g_type_add_interface_static (gns_type, GTK_TYPE_TREE_MODEL, &tree_model_info);
	}

	return gns_type;
}

GObject *
gtksharp_node_store_new (void)
{
	return g_object_new (gtksharp_node_store_get_type (), NULL);
}

void
gtksharp_node_store_set_tree_model_callbacks (GtkSharpNodeStore *store, GtkSharpNodeStoreTreeModelIface *iface)
{
	store->tree_model = *iface;
}

void 
gtksharp_node_store_emit_row_changed (GtkSharpNodeStore *store, GtkTreePath *path, gint node_idx)
{
	GtkTreeIter iter;

	iter.stamp = store->stamp;
	iter.user_data = GINT_TO_POINTER (node_idx);

	gtk_tree_model_row_changed (GTK_TREE_MODEL (store), path, &iter);
}

void 
gtksharp_node_store_emit_row_inserted (GtkSharpNodeStore *store, GtkTreePath *path, gint node_idx)
{
	GtkTreeIter iter;

	iter.stamp = store->stamp;
	iter.user_data = GINT_TO_POINTER (node_idx);

	gtk_tree_model_row_inserted (GTK_TREE_MODEL (store), path, &iter);
}

void 
gtksharp_node_store_emit_row_deleted (GtkSharpNodeStore *store, GtkTreePath *path)
{
	gtk_tree_model_row_deleted (GTK_TREE_MODEL (store), path);
}

void 
gtksharp_node_store_emit_row_has_child_toggled (GtkSharpNodeStore *store, GtkTreePath *path, gint node_idx)
{
	GtkTreeIter iter;

	iter.stamp = store->stamp;
	iter.user_data = GINT_TO_POINTER (node_idx);

	gtk_tree_model_row_has_child_toggled (GTK_TREE_MODEL (store), path, &iter);
}


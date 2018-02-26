// NodeCellDataFunc.cs - a TreeCellDataFunc marshaler for ITreeNodes
//
// Author: Mike Kestner (mkestner@novell.com)
//
// Copyright (c) 2004 Novell, Inc.
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of version 2 of the Lesser GNU General 
// Public License as published by the Free Software Foundation.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.	 See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public
// License along with this program; if not, write to the
// Free Software Foundation, Inc., 59 Temple Place - Suite 330,
// Boston, MA 02111-1307, USA.


namespace Gtk {

	using System;

	public delegate void NodeCellDataFunc (TreeViewColumn tree_column, CellRenderer cell, ITreeNode node);


	internal class NodeCellDataFuncWrapper {

		public void NativeCallback (IntPtr tree_column, IntPtr cell, IntPtr tree_model, IntPtr iter_ptr, IntPtr data)
		{
			TreeViewColumn col = (Gtk.TreeViewColumn) GLib.Object.GetObject(tree_column);
			CellRenderer renderer = (Gtk.CellRenderer) GLib.Object.GetObject(cell);
			NodeStore.NodeStoreImplementor store = (NodeStore.NodeStoreImplementor) GLib.Object.GetObject(tree_model);
			TreeIter iter = TreeIter.New (iter_ptr);
			managed (col, renderer, store.GetNode (iter));
		}

		internal GtkSharp.CellLayoutDataFuncNative NativeDelegate;
		protected NodeCellDataFunc managed;

		public NodeCellDataFuncWrapper (NodeCellDataFunc managed)
		{
			NativeDelegate = new GtkSharp.CellLayoutDataFuncNative (NativeCallback);
			this.managed = managed;
		}
	}
}

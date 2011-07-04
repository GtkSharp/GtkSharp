// NodeView.cs - a TreeView implementation that exposes ITreeNodes
//
// Author: Duncan Mak (duncan@ximian.com)
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
	using System.Collections;
	using System.Reflection;
	using System.Runtime.InteropServices;

	public class NodeView : TreeView {

		NodeStore store;
		NodeSelection selection;

		public NodeView (NodeStore store) : base (store == null ? null : store.Adapter)
		{
			this.store = store;
		}

		public NodeView () : base () {}

		public NodeStore NodeStore {
			get {
				return store;
			}
			set {
				store = value;
				this.Model = store == null ? null : store.Adapter;
			}
		}

		public NodeSelection NodeSelection { 
			get {
				if (selection == null)
					selection = new NodeSelection (Selection);
				return selection;
			}
		}

		public Gtk.TreeViewColumn AppendColumn (string title, Gtk.CellRenderer cell, Gtk.NodeCellDataFunc cell_data) 
		{
			Gtk.TreeViewColumn col = new Gtk.TreeViewColumn ();
			col.Title = title;
			col.PackStart (cell, true);
			col.SetCellDataFunc (cell, cell_data);
			
			AppendColumn (col);
			return col;
		}
		
		public void ActivateRow (ITreeNode node, Gtk.TreeViewColumn column) {
			ActivateRow (store.GetPath (node), column);
		}
		
		public bool CollapseRow (ITreeNode node) {
			return CollapseRow (store.GetPath (node));
		}
		
		public Cairo.Surface CreateRowDragIcon (ITreeNode node) {
			return CreateRowDragIcon (store.GetPath (node));
		}
		
		public Gdk.Rectangle GetBackgroundArea (ITreeNode node, Gtk.TreeViewColumn column) {
			return GetBackgroundArea (store.GetPath (node), column);
		}
		
		public Gdk.Rectangle GetCellArea (ITreeNode node, Gtk.TreeViewColumn column) {
			return GetBackgroundArea (store.GetPath (node), column);
		}
		
		public ITreeNode GetNodeAtPos (int x, int y) {
			Gtk.TreePath nodePath;
			ITreeNode node = null;
			
			if (this.GetPathAtPos (x, y, out nodePath))
				node = store.GetNode (nodePath);
			
			return node;
		}
		
		public bool GetRowExpanded (ITreeNode node) {
			return GetRowExpanded (store.GetPath (node));
		}

		public bool GetVisibleRange (out ITreeNode startNode, out ITreeNode endNode) {
			Gtk.TreePath start_path, end_path;
			bool retVal = GetVisibleRange (out start_path, out end_path);
			if (retVal) {
				startNode = store.GetNode (start_path);
				endNode = store.GetNode (end_path);
			}
			else {
				startNode = null;
				endNode = null;
			}
			
			return retVal;
		}
		
		public void ScrollToCell (ITreeNode node, TreeViewColumn column, bool use_align, float row_align, float col_align) {
			ScrollToCell (store.GetPath (node),  column,  use_align,  row_align,  col_align);
		}
		
		public void SetTooltipCell (Tooltip tooltip, ITreeNode node, TreeViewColumn column, CellRenderer renderer) {
			SetTooltipCell (tooltip,  store.GetPath (node),  column,  renderer);
		}

		public void ExpandRow (ITreeNode node, bool open_all) {
			ExpandRow (store.GetPath (node), open_all);
		}

		public void ExpandToNode (ITreeNode node) {
			ExpandToPath (store.GetPath (node));
		}
	}
}


// NodeSelection.cs - a TreeSelection implementation that exposes ITreeNodes
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

	public class NodeSelection {

		private TreeSelection selection;
		
		public event EventHandler Changed;

		internal NodeSelection (TreeSelection selection)
		{
			this.selection = selection;

			selection.Changed += new EventHandler (ChangedHandler); 
		}

		private void ChangedHandler (object o, EventArgs args)
		{
			if (Changed != null)
				Changed (this, args); 
		}
		
		public bool NodeIsSelected (ITreeNode node)
		{
			return selection.IterIsSelected (NodeView.NodeStore.GetIter (node));
		}

		public bool PathIsSelected (TreePath path)
		{
			return selection.PathIsSelected (path);
		}

		public void SelectAll ()
		{
			selection.SelectAll ();
		}

		public void SelectNode (ITreeNode node)
		{
			selection.SelectIter (NodeView.NodeStore.GetIter (node));
		}

		public void SelectPath (TreePath path)
		{
			selection.SelectPath (path);
		}

		public void SelectRange (ITreeNode begin_node, ITreeNode end_node)
		{
			TreePath begin = NodeView.NodeStore.GetPath (begin_node);
			TreePath end = NodeView.NodeStore.GetPath (end_node);

			selection.SelectRange (begin, end);
		}
		
		public void UnselectAll ()
		{
			selection.UnselectAll ();
		}

		public void UnselectNode (ITreeNode node)
		{
			selection.UnselectIter (NodeView.NodeStore.GetIter (node));
		}

		public void UnselectPath (TreePath path) 
		{
			selection.UnselectPath (path);
		}

		public void UnselectRange (TreePath begin, TreePath end)
		{
			selection.UnselectRange (begin, end);
		}

		public void UnselectRange (ITreeNode begin_node, ITreeNode end_node)
		{
			TreePath begin = NodeView.NodeStore.GetPath (begin_node);
			TreePath end = NodeView.NodeStore.GetPath (end_node);

			selection.UnselectRange (begin, end);
		}

		public SelectionMode Mode {
			get { 
				return selection.Mode; 
			}
			set { 
				selection.Mode = value; 
			}
		}
		
		public NodeView NodeView {
			get { 
				return selection.TreeView as NodeView; 
			}
		}

		public ITreeNode[] SelectedNodes {
			get {
				TreePath [] paths = selection.GetSelectedRows ();
				int length = paths.Length;

				ITreeNode [] results = new ITreeNode [length];

				for (int i = 0; i < length; i++) 
					results [i] = NodeView.NodeStore.GetNode (paths [i]);

				return results;
			}
		}

		public ITreeNode SelectedNode {
			get {
				if (Mode == SelectionMode.Multiple)
					throw new InvalidOperationException ("SelectedNode is not valid with multi-selection mode");
				
				ITreeNode [] sn = SelectedNodes;
				if (sn.Length == 0)
					return null;
				return sn [0];
			}
			set {
				// with multiple mode, the behavior
				// here would be unclear. Does it just
				// select the `value' node or does it
				// make the `value' node the only
				// selected node.
				if (Mode == SelectionMode.Multiple)
					throw new InvalidOperationException ("SelectedNode is not valid with multi-selection mode");
				
				SelectNode (value);
			}
		}
	}
}

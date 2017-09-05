// TreeNode.cs - Abstract base class to subclass for TreeNode types
//
// Author: Mike Kestner  <mkestner@ximian.com>
//
// Copyright (c) 2003 Novell, Inc.
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of version 2 of the Lesser GNU General 
// Public License as published by the Free Software Foundation.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public
// License along with this program; if not, write to the
// Free Software Foundation, Inc., 59 Temple Place - Suite 330,
// Boston, MA 02111-1307, USA.


namespace Gtk {

	using System;
	using System.Collections;
	using System.Threading;

	public abstract class TreeNode : ITreeNode {

		// Only use interlocked operations
		static int next_idx = 0;

		int id;
		ITreeNode parent;
		ArrayList children = new ArrayList ();

		public TreeNode ()
		{
			id = Interlocked.Increment (ref next_idx);
		}

		public int ID {
			get {
				return id;
			}
		}

		public ITreeNode Parent {
			get {
				return parent;
			}
		}

		public int ChildCount {
			get {
				return children.Count;
			}
		}

		public int IndexOf (object o) 
		{
			return children.IndexOf (o);
		}

		internal void SetParent (ITreeNode parent)
		{
			this.parent = parent;
		}

		public ITreeNode this [int index] {
			get {
				if (index >= ChildCount)
					return null;

				return children [index] as ITreeNode;
			}
		}

		public event EventHandler Changed;

		protected void OnChanged ()
		{
			if (Changed == null)
				return;

			Changed (this, new EventArgs ());
		}

		public event TreeNodeAddedHandler ChildAdded;

		private void OnChildAdded (ITreeNode child)
		{
			if (ChildAdded == null)
				return;

			ChildAdded (this, child);
		}

		public event TreeNodeRemovedHandler ChildRemoved;

		private void OnChildRemoved (TreeNode child, int old_position)
		{
			if (ChildRemoved == null)
				return;

			ChildRemoved (this, child, old_position);
		}

		public void AddChild (TreeNode child)
		{
			children.Add (child);
			child.SetParent (this);
			OnChildAdded (child);
		}

		public void AddChild (TreeNode child, int position)
		{
			children.Insert (position, child);
			child.SetParent (this);
			OnChildAdded (child);
		}

		public void RemoveChild (TreeNode child)
		{
			int idx = children.IndexOf (child);
			if (idx < 0)
				return;

			children.Remove (child);
			child.SetParent (null);
			OnChildRemoved (child, idx);
		}
	}
}

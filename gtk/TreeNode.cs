// TreeNode.cs - Abstract base class to subclass for TreeNode types
//
// Author: Mike Kestner  <mkestner@ximian.com>
//
// <c> 2003 Novell, Inc.

namespace Gtk {

	using System;
	using System.Collections;

	public abstract class TreeNode : ITreeNode {

		static int next_idx = 0;

		int id;
		ITreeNode parent;
		ArrayList children = new ArrayList ();

		public TreeNode ()
		{
			id = next_idx++;
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
			set {
				parent = value;
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

		private void OnChildRemoved (int old_position)
		{
			if (ChildRemoved == null)
				return;

			ChildRemoved (this, old_position);
		}

		public void AddChild (ITreeNode child)
		{
			children.Add (child);
			child.Parent = this;
			OnChildAdded (child);
		}

		public void RemoveChild (ITreeNode child)
		{
			int idx = children.IndexOf (child);
			if (idx < 0)
				return;

			children.Remove (child);
			OnChildRemoved (idx);
		}
	}
}

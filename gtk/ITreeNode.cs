// ITreeNode.cs - Interface and delegates for tree node navigation and updating.
//
// Author: Mike Kestner  <mkestner@ximian.com>
//
// <c> 2003 Novell, Inc.

namespace Gtk {

	using System;

	public delegate void TreeNodeAddedHandler (object o, ITreeNode child);

	public delegate void TreeNodeRemovedHandler (object o, int old_position);

	public interface ITreeNode {

		int ID { get; }

		ITreeNode Parent { get; set; }

		int ChildCount { get; }

		ITreeNode this [int index] { get; }

		int IndexOf (object o);

		event EventHandler Changed;

		event TreeNodeAddedHandler ChildAdded;

		event TreeNodeRemovedHandler ChildRemoved;
	}
}

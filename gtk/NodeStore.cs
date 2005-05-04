// NodeStore.cs - Tree store implementation for TreeView.
//
// Author: Mike Kestner  <mkestner@novell.com>
//
// Copyright (c) 2003-2005 Novell, Inc.
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
	using System.Reflection;
	using System.Runtime.InteropServices;

	public class NodeStore : GLib.Object, IEnumerable {

        	class IDHashtable : Hashtable {
                	class IDComparer : IComparer {
                        	public int Compare (object x, object y)
                        	{
                                	if ((int) x == (int) y)
                                        	return 0;
                                	else
                                        	return 1;
                        	}
                	}

                	class IDHashCodeProvider : IHashCodeProvider {
				public int GetHashCode (object o)
				{
					return (int) o;
				}
			}

                	public IDHashtable () : base (new IDHashCodeProvider (), new IDComparer ()) {}
        	}

		[GLib.CDeclCallback]
		delegate int GetFlagsDelegate ();
		[GLib.CDeclCallback]
		delegate int GetNColumnsDelegate ();
		[GLib.CDeclCallback]
		delegate IntPtr GetColumnTypeDelegate (int col);
		[GLib.CDeclCallback]
		delegate bool GetNodeDelegate (out int node_idx, IntPtr path);
		[GLib.CDeclCallback]
		delegate IntPtr GetPathDelegate (int node_idx);
		[GLib.CDeclCallback]
		delegate void GetValueDelegate (int node_idx, int col, ref GLib.Value val);
		[GLib.CDeclCallback]
		delegate bool NextDelegate (ref int node_idx);
		[GLib.CDeclCallback]
		delegate bool ChildrenDelegate (out int child, int parent);
		[GLib.CDeclCallback]
		delegate bool HasChildDelegate (int node_idx);
		[GLib.CDeclCallback]
		delegate int NChildrenDelegate (int node_idx);
		[GLib.CDeclCallback]
		delegate bool NthChildDelegate (out int child, int parent, int n);
		[GLib.CDeclCallback]
		delegate bool ParentDelegate (out int parent, int child);

		[StructLayout(LayoutKind.Sequential)]
		struct TreeModelIfaceDelegates  {
			public GetFlagsDelegate get_flags;
			public GetNColumnsDelegate get_n_columns;
			public GetColumnTypeDelegate get_column_type;
			public GetNodeDelegate get_node;
			public GetPathDelegate get_path;
			public GetValueDelegate get_value;
			public NextDelegate next;
			public ChildrenDelegate children;
			public HasChildDelegate has_child;
			public NChildrenDelegate n_children;
			public NthChildDelegate nth_child;
			public ParentDelegate parent;
		}

		Hashtable node_hash = new IDHashtable ();
 		GLib.GType[] ctypes; 
		PropertyInfo[] getters;
		int n_cols = 1;
		bool list_only = false;
		ArrayList nodes = new ArrayList ();
		TreeModelIfaceDelegates tree_model_iface;

		int get_flags_cb ()
		{
			TreeModelFlags result = TreeModelFlags.ItersPersist;
			if (list_only)
				result |= TreeModelFlags.ListOnly;
			return (int) result;
		}

		int get_n_columns_cb ()
		{
			return n_cols;
		}

		IntPtr get_column_type_cb (int col)
		{
			return ctypes [col].Val;
		}

		bool get_node_cb (out int node_idx, IntPtr path)
		{
			if (path == IntPtr.Zero)
				throw new ArgumentNullException ("path");

			TreePath treepath = new TreePath (path);
			node_idx = -1;

			ITreeNode node = GetNodeAtPath (treepath);
			if (node == null)
				return false;

			node_idx = node.ID;
			node_hash [node.ID] = node;
			return true;
		}

		IntPtr get_path_cb (int node_idx)
		{
			ITreeNode node = node_hash [node_idx] as ITreeNode;
			if (node == null) throw new Exception ("Invalid Node ID");

			return GetPath (node).Handle;
		}

		[DllImport("libgobject-2.0-0.dll")]
		static extern void g_value_init (ref GLib.Value val, IntPtr type);

		void get_value_cb (int node_idx, int col, ref GLib.Value val)
		{
			ITreeNode node = node_hash [node_idx] as ITreeNode;
			if (node == null)
				return;
			g_value_init (ref val, ctypes [col].Val);
			object col_val = getters[col].GetValue (node, null);
			val.Val = col_val;
		}

		bool next_cb (ref int node_idx)
		{
			ITreeNode node = node_hash [node_idx] as ITreeNode;
			if (node == null)
				return false;

			int idx;
			if (node.Parent == null)
				idx = Nodes.IndexOf (node);
			else
				idx = node.Parent.IndexOf (node);
			
			if (idx < 0) throw new Exception ("Node not found in Nodes list");

			if (node.Parent == null) {
				if (++idx >= Nodes.Count)
					return false;
				node = Nodes [idx] as ITreeNode;
			} else {
				if (++idx >= node.Parent.ChildCount)
					return false;
				node = node.Parent [idx];
			}
			node_hash [node.ID] = node;
			node_idx = node.ID;
			return true;
		}

		bool children_cb (out int child_idx, int parent)
		{
			child_idx = -1;
			ITreeNode node;

			if (parent == -1) {
				if (Nodes.Count <= 0)
					return false;
				node = Nodes [0] as ITreeNode;
				child_idx = node.ID;
				node_hash [node.ID] = node;
				return true;
			}
				
			node = node_hash [parent] as ITreeNode;
			if (node == null || node.ChildCount <= 0)
				return false;

			ITreeNode child = node [0];
			node_hash [child.ID] = child;
			child_idx = child.ID;
			return true;
		}

		bool has_child_cb (int node_idx)
		{
			ITreeNode node = node_hash [node_idx] as ITreeNode;
			if (node == null || node.ChildCount <= 0)
				return false;

			return true;
		}

		int n_children_cb (int node_idx)
		{
			if (node_idx == -1)
				return Nodes.Count;
				
			ITreeNode node = node_hash [node_idx] as ITreeNode;
			if (node == null || node.ChildCount <= 0)
				return 0;

			return node.ChildCount;
		}

		bool nth_child_cb (out int child_idx, int parent, int n)
		{
			child_idx = -1;
			ITreeNode node;

			if (parent == -1) {
				if (Nodes.Count <= n)
					return false;
				node = Nodes [n] as ITreeNode;
				child_idx = node.ID;
				node_hash [node.ID] = node;
				return true;
			}
				
			node = node_hash [parent] as ITreeNode;
			if (node == null || node.ChildCount <= n)
				return false;

			ITreeNode child = node [n];
			node_hash [child.ID] = child;
			child_idx = child.ID;
			return true;
		}

		bool parent_cb (out int parent_idx, int child)
		{
			parent_idx = -1;
			ITreeNode node = node_hash [child] as ITreeNode;
			if (node == null || node.Parent == null)
				return false;

			node_hash [node.Parent.ID] = node.Parent;
			parent_idx = node.Parent.ID;
			return true;
		}

		[DllImport("gtksharpglue-2")]
		static extern void gtksharp_node_store_set_tree_model_callbacks (IntPtr raw, ref TreeModelIfaceDelegates cbs);

		private void BuildTreeModelIface ()
		{
			tree_model_iface.get_flags = new GetFlagsDelegate (get_flags_cb);
			tree_model_iface.get_n_columns = new GetNColumnsDelegate (get_n_columns_cb);
			tree_model_iface.get_column_type = new GetColumnTypeDelegate (get_column_type_cb);
			tree_model_iface.get_node = new GetNodeDelegate (get_node_cb);
			tree_model_iface.get_path = new GetPathDelegate (get_path_cb);
			tree_model_iface.get_value = new GetValueDelegate (get_value_cb);
			tree_model_iface.next = new NextDelegate (next_cb);
			tree_model_iface.children = new ChildrenDelegate (children_cb);
			tree_model_iface.has_child = new HasChildDelegate (has_child_cb);
			tree_model_iface.n_children = new NChildrenDelegate (n_children_cb);
			tree_model_iface.nth_child = new NthChildDelegate (nth_child_cb);
			tree_model_iface.parent = new ParentDelegate (parent_cb);

			gtksharp_node_store_set_tree_model_callbacks (Handle, ref tree_model_iface);
		}

		public NodeStore (Type node_type) : base (IntPtr.Zero)
		{
			CreateNativeObject (new string [0], new GLib.Value [0]);
			ScanType (node_type);
			BuildTreeModelIface ();
		}

		void ScanType (Type type)
		{
			foreach (TreeNodeAttribute attr in type.GetCustomAttributes (typeof (TreeNodeAttribute), false)) {
				n_cols = attr.ColumnCount;
				list_only = attr.ListOnly;
			}

 			ctypes = new GLib.GType [n_cols];
 			getters = new PropertyInfo [n_cols];

			foreach (PropertyInfo pi in type.GetProperties ()) {
				foreach (TreeNodeValueAttribute attr in pi.GetCustomAttributes (typeof (TreeNodeValueAttribute), false)) {
					int col = attr.Column;
					getters [col] = pi;
                                        ctypes[col] = (GLib.GType) pi.PropertyType;
				}
			}
		}

		private IList Nodes {
			get {
				return nodes as IList;
			}
		}							

		[DllImport("gtksharpglue-2")]
		static extern void gtksharp_node_store_emit_row_changed (IntPtr handle, IntPtr path, int node_idx);

		private void changed_cb (object o, EventArgs args)
		{
			ITreeNode node = o as ITreeNode;
			gtksharp_node_store_emit_row_changed (Handle, get_path_cb (node.ID), node.ID);
		}

		[DllImport("gtksharpglue-2")]
		static extern void gtksharp_node_store_emit_row_inserted (IntPtr handle, IntPtr path, int node_idx);

		private void child_added_cb (object o, ITreeNode child)
		{
			AddNodeInternal (child);
			gtksharp_node_store_emit_row_inserted (Handle, get_path_cb (child.ID), child.ID);
		}

		[DllImport("gtksharpglue-2")]
		static extern void gtksharp_node_store_emit_row_deleted (IntPtr handle, IntPtr path);

		[DllImport("gtksharpglue-2")]
		static extern void gtksharp_node_store_emit_row_has_child_toggled (IntPtr handle, IntPtr path, int node_idx);

		private void RemoveNodeInternal (ITreeNode node)
		{
			node_hash.Remove (node.ID);
			for (int i = 0; i < node.ChildCount; i++)
				RemoveNodeInternal (node [i]);
		}

		private void child_deleted_cb (object o, ITreeNode child, int idx)
		{
			ITreeNode node = o as ITreeNode;
			
			TreePath path = new TreePath (get_path_cb (node.ID));
			TreePath child_path = path.Copy ();
			child_path.AppendIndex (idx);

			RemoveNodeInternal (child);

			gtksharp_node_store_emit_row_deleted (Handle, child_path.Handle);

			if (node.ChildCount <= 0)
				gtksharp_node_store_emit_row_has_child_toggled (Handle, get_path_cb (node.ID), node.ID);
		}

		private void AddNodeInternal (ITreeNode node)
		{
			node_hash [node.ID] = node;

			node.Changed += new EventHandler (changed_cb);
			node.ChildAdded += new TreeNodeAddedHandler (child_added_cb);
			node.ChildRemoved += new TreeNodeRemovedHandler (child_deleted_cb);

			for (int i = 0; i < node.ChildCount; i++)
				AddNodeInternal (node [i]);
		}

		public void AddNode (ITreeNode node)
		{
			nodes.Add (node);
			AddNodeInternal (node);

			gtksharp_node_store_emit_row_inserted (Handle, get_path_cb (node.ID), node.ID);
		}

		public void AddNode (ITreeNode node, int position)
		{
			nodes.Insert (position, node);
			AddNodeInternal (node);

			gtksharp_node_store_emit_row_inserted (Handle, get_path_cb (node.ID), node.ID);
		}

		public void RemoveNode (ITreeNode node)
		{
			int idx = nodes.IndexOf (node);
			if (idx < 0)
				return;
			nodes.Remove (node);
			RemoveNodeInternal (node);

			TreePath path = new TreePath ();
			path.AppendIndex (idx);

			gtksharp_node_store_emit_row_deleted (Handle, path.Handle);
		}

		private ITreeNode GetNodeAtPath (TreePath path)
		{
			int[] indices = path.Indices;

			if (indices[0] >= Nodes.Count)
				return null;

			ITreeNode node = Nodes [indices [0]] as ITreeNode;
			int i;
			for (i = 1; i < path.Depth; i++) {
				if (indices [i] >= node.ChildCount)
					return null;

				node = node [indices [i]];
			}

			return node;
		}

		public ITreeNode GetNode (TreePath path)
		{
			if (path == null)
				throw new ArgumentNullException ();

			return GetNodeAtPath (path);
		}

		internal ITreeNode GetNode (TreeIter iter)
		{
			return node_hash [(int) iter.UserData] as ITreeNode;
		}

		internal TreePath GetPath (ITreeNode node)
		{
			TreePath path = new TreePath ();
			int idx;

			while (node.Parent != null) {
				idx = node.Parent.IndexOf (node);
				if (idx < 0) throw new Exception ("Badly formed tree");
				path.PrependIndex (idx);
				node = node.Parent;
			}
			idx = Nodes.IndexOf (node);
			if (idx < 0) throw new Exception ("Node not found in Nodes list");
			path.PrependIndex (idx);
			return path;
		}

		internal TreeIter GetIter (ITreeNode node)
		{
			TreeIter iter = new TreeIter ();
			iter.UserData = new IntPtr (node.ID);
			return iter;
		}

		[DllImport("gtksharpglue-2")]
		static extern IntPtr gtksharp_node_store_get_type ();
		
		public static new GLib.GType GType {
			get {
				return new GLib.GType (gtksharp_node_store_get_type ());
			}
		}

		public IEnumerator GetEnumerator ()
		{
			return nodes.GetEnumerator ();
		}
	}
}

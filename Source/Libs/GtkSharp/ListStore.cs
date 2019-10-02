// Gtk.ListStore.cs - Gtk ListStore class customizations
//
// Author: Kristian Rietveld <kris@gtk.org>
//
// (c) 2002 Kristian Rietveld
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
	using System.Runtime.InteropServices;

	public partial class ListStore : IEnumerable {
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate bool d_gtk_tree_model_iter_children2(IntPtr raw, out Gtk.TreeIter iter, IntPtr parent);
		static d_gtk_tree_model_iter_children2 gtk_tree_model_iter_children2 = FuncLoader.LoadFunction<d_gtk_tree_model_iter_children2>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_tree_model_iter_children"));
		public bool IterChildren (out Gtk.TreeIter iter) 
		{
			bool raw_ret = gtk_tree_model_iter_children2 (Handle, out iter, IntPtr.Zero);
			bool ret = raw_ret;
			return ret;
		}

		public int IterNChildren () 
		{
			int raw_ret = gtk_tree_model_iter_n_children (Handle, IntPtr.Zero);
			int ret = raw_ret;
			return ret;
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate bool d_gtk_tree_model_iter_nth_child2(IntPtr raw, out Gtk.TreeIter iter, IntPtr parent, int n);
		static d_gtk_tree_model_iter_nth_child2 gtk_tree_model_iter_nth_child2 = FuncLoader.LoadFunction<d_gtk_tree_model_iter_nth_child2>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_tree_model_iter_nth_child"));
		public bool IterNthChild (out Gtk.TreeIter iter, int n) 
		{
			bool raw_ret = gtk_tree_model_iter_nth_child2 (Handle, out iter, IntPtr.Zero, n);
			bool ret = raw_ret;
			return ret;
		}

		public void SetValue (Gtk.TreeIter iter, int column, bool value) 
		{
			GLib.Value val = new GLib.Value (value);
			SetValue (iter, column, val);
			val.Dispose ();
		}

		public void SetValue (Gtk.TreeIter iter, int column, double value) 
		{
			GLib.Value val = new GLib.Value (value);
			SetValue (iter, column, val);
			val.Dispose ();
		}

		public void SetValue (Gtk.TreeIter iter, int column, int value) 
		{
			GLib.Value val = new GLib.Value (value);
			SetValue (iter, column, val);
			val.Dispose ();
		}

		public void SetValue (Gtk.TreeIter iter, int column, long value)
		{
			GLib.Value val = new GLib.Value (value);
			SetValue (iter, column, val);
			val.Dispose ();
		}

		public void SetValue (Gtk.TreeIter iter, int column, string value) 
		{
			GLib.Value val = new GLib.Value (value);
			SetValue (iter, column, val);
			val.Dispose ();
		}

		public void SetValue (Gtk.TreeIter iter, int column, float value) 
		{
			GLib.Value val = new GLib.Value (value);
			SetValue (iter, column, val);
			val.Dispose ();
		}

		public void SetValue (Gtk.TreeIter iter, int column, uint value) 
		{
			GLib.Value val = new GLib.Value (value);
			SetValue (iter, column, val);
			val.Dispose ();
		}
		
		public void SetValue (Gtk.TreeIter iter, int column, object value) 
		{
			GLib.Value val = new GLib.Value (value);
			SetValue (iter, column, val);
			val.Dispose ();
		}

		public Gtk.TreeIter AppendValues (Array values) 
		{
			Gtk.TreeIter iter = Append();
			SetValues (iter, values.Explode ());
			return iter;
		}
		
		public Gtk.TreeIter AppendValues (params object[] values) 
		{
			Gtk.TreeIter iter = Append();
			SetValues (iter, values);
			return iter;
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_gtk_list_store_insert_with_valuesv2(IntPtr raw, out TreeIter iter, int position, int[] columns, GLib.Value[] values, int n_values);
		static d_gtk_list_store_insert_with_valuesv2 gtk_list_store_insert_with_valuesv2 = FuncLoader.LoadFunction<d_gtk_list_store_insert_with_valuesv2>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_list_store_insert_with_valuesv"));

		public TreeIter InsertWithValues (int position, params object[] values)
		{
			int[] columns = new int[values.Length];
			GLib.Value[] vals = new GLib.Value[values.Length];
			int n_values = 0;

			for (int i = 0; i < values.Length; i++) {
				if (values[i] != null) {
					columns[n_values] = i;
					vals[n_values] = new GLib.Value (values[i]);
					n_values++;
				}
			}

			TreeIter iter;
			gtk_list_store_insert_with_valuesv2 (Handle, out iter, position, columns, vals, n_values);

			for (int i = 0; i < n_values; i++)
				vals[i].Dispose ();

			return iter;
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_gtk_list_store_set_valuesv(IntPtr raw, ref TreeIter iter, int[] columns, GLib.Value[] values, int n_values);
		static d_gtk_list_store_set_valuesv gtk_list_store_set_valuesv = FuncLoader.LoadFunction<d_gtk_list_store_set_valuesv>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_list_store_set_valuesv"));

		public void SetValues (TreeIter iter, params object[] values)
		{
			int[] columns = new int[values.Length];
			GLib.Value[] vals = new GLib.Value[values.Length];
			int n_values = 0;

			for (int i = 0; i < values.Length; i++) {
				if (values[i] != null) {
					columns[n_values] = i;
					vals[n_values] = new GLib.Value (values[i]);
					n_values++;
				}
			}

			gtk_list_store_set_valuesv (Handle, ref iter, columns, vals, n_values);

			for (int i = 0; i < n_values; i++)
				vals[i].Dispose ();
		}

		public ListStore (params GLib.GType[] types) : base (IntPtr.Zero)
		{
			CreateNativeObject (new string [0], new GLib.Value [0]);
			ColumnTypes = types;
		}

		public ListStore (params Type[] types) : base (IntPtr.Zero)
		{
			GLib.GType[] gtypes = new GLib.GType[types.Length];
			int i = 0;
			foreach (Type type in types) {
				gtypes[i] = (GLib.GType) type;
				i++;
			}
			
			CreateNativeObject (new string [0], new GLib.Value [0]);
			ColumnTypes = gtypes;
		}

		public object GetValue(Gtk.TreeIter iter, int column) 
		{
			GLib.Value val = GLib.Value.Empty;
			GetValue (iter, column, ref val);
			object ret = val.Val;
			val.Dispose ();
			return ret;
		}

		public IEnumerator GetEnumerator()
		{
			return new TreeEnumerator(this);
		}

		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		delegate void RowsReorderedSignalDelegate (IntPtr arg0, IntPtr arg1, IntPtr arg2, IntPtr arg3, IntPtr gch);

		static void RowsReorderedSignalCallback (IntPtr arg0, IntPtr arg1, IntPtr arg2, IntPtr arg3, IntPtr gch)
		{
			Gtk.RowsReorderedArgs args = new Gtk.RowsReorderedArgs ();
			try {
				GLib.Signal sig = ((GCHandle) gch).Target as GLib.Signal;
				if (sig == null)
					throw new Exception("Unknown signal GC handle received " + gch);

				ListStore sender = GLib.Object.GetObject (arg0) as ListStore;
				args.Args = new object[3];
				args.Args[0] = arg1 == IntPtr.Zero ? null : (Gtk.TreePath) GLib.Opaque.GetOpaque (arg1, typeof (Gtk.TreePath), false);
				args.Args[1] = Gtk.TreeIter.New (arg2);
				int child_cnt = sender.IterNChildren ();
				int[] new_order = new int [child_cnt];
				Marshal.Copy (arg3, new_order, 0, child_cnt);
				args.Args[2] = new_order;
				Gtk.RowsReorderedHandler handler = (Gtk.RowsReorderedHandler) sig.Handler;
				handler (sender, args);
			} catch (Exception e) {
				GLib.ExceptionManager.RaiseUnhandledException (e, false);
			}
		}

		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		delegate void RowsReorderedVMDelegate (IntPtr tree_model, IntPtr path, IntPtr iter, IntPtr new_order);

		static RowsReorderedVMDelegate RowsReorderedVMCallback;

		static void rowsreordered_cb (IntPtr tree_model, IntPtr path_ptr, IntPtr iter_ptr, IntPtr new_order)
		{
			try {
				ListStore store = GLib.Object.GetObject (tree_model, false) as ListStore;
				TreePath path = GLib.Opaque.GetOpaque (path_ptr, typeof (TreePath), false) as TreePath;
				TreeIter iter = TreeIter.New (iter_ptr);
				int child_cnt = store.IterNChildren ();
				int[] child_order = new int [child_cnt];
				Marshal.Copy (new_order, child_order, 0, child_cnt);
				store.OnRowsReordered (path, iter, child_order);
			} catch (Exception e) {
				GLib.ExceptionManager.RaiseUnhandledException (e, true);
				// NOTREACHED: above call doesn't return
				throw e;
			}
		}

		private static void OverrideRowsReordered (GLib.GType gtype)
		{
			if (RowsReorderedVMCallback == null)
				RowsReorderedVMCallback = new RowsReorderedVMDelegate (rowsreordered_cb);
			OverrideVirtualMethod (gtype, "rows_reordered", RowsReorderedVMCallback);
		}

		[GLib.DefaultSignalHandler(Type=typeof(Gtk.ListStore), ConnectionMethod="OverrideRowsReordered")]
		protected virtual void OnRowsReordered (Gtk.TreePath path, Gtk.TreeIter iter, int[] new_order)
		{
			GLib.Value ret = GLib.Value.Empty;
			GLib.ValueArray inst_and_params = new GLib.ValueArray (4);
			GLib.Value[] vals = new GLib.Value [4];
			vals [0] = new GLib.Value (this);
			inst_and_params.Append (vals [0]);
			vals [1] = new GLib.Value (path);
			inst_and_params.Append (vals [1]);
			vals [2] = new GLib.Value (iter);
			inst_and_params.Append (vals [2]);
			int cnt = IterNChildren ();
			IntPtr new_order_ptr = Marshal.AllocHGlobal (Marshal.SizeOf (typeof (int)) * cnt);
			Marshal.Copy (new_order, 0, new_order_ptr, cnt);
			vals [3] = new GLib.Value (new_order_ptr);
			inst_and_params.Append (vals [3]);
			g_signal_chain_from_overridden (inst_and_params.ArrayPtr, ref ret);
			Marshal.FreeHGlobal (new_order_ptr);

			foreach (GLib.Value v in vals)
				v.Dispose ();
		}

		[GLib.Signal("rows_reordered")]
		public event Gtk.RowsReorderedHandler RowsReordered {
			add {
				AddSignalHandler ("rows_reordered", value, new RowsReorderedSignalDelegate(RowsReorderedSignalCallback));
			}
			remove {
				RemoveSignalHandler ("rows_reordered", value);
			}
		}
	}
}


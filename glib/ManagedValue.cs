// GLib.ManagedValue.cs : Managed types boxer
//
// Author: Rachel Hestilow <hestilow@ximian.com>
//
// (c) 2002 Rachel Hestilow

namespace GLib {
	using System;
	using System.Collections;
	using System.Runtime.InteropServices;
	using GLib;
	
	/// <summary>
	///  Managed types boxer	
	/// </summary>
	///
	/// <remarks>
	///  Utility class for creating GBoxed wrappers around managed types 
	/// </remarks>

	// FIXME:
	// This used to use GCHandles, but I rewrote it to debug
	// some odd interactions. Since the boxed code in GLib is designed
	// to not interact directly with the pointers, just using our own
	// arbitrary pointer values is fine. Still, it might be useful
	// to use GCHandle later on.
	
	internal class ManagedValue {
		private class ValueHolder {
			public object val;
			public int ref_count;
		}
		
		private delegate IntPtr CopyFunc (IntPtr ptr);
		private delegate void FreeFunc (IntPtr ptr);
		
		private static Hashtable pointers = new Hashtable ();
		private static IntPtr cur_ptr = IntPtr.Zero;
		private static CopyFunc copy;
		private static FreeFunc free;
		private static GType boxed_type = GType.Invalid;

		[DllImport("libgobject-2.0-0.dll")]
		private static extern IntPtr g_boxed_type_register_static (string typename, CopyFunc copy_func, FreeFunc free_func);
		
		public static GType GType {
			get {
				if (boxed_type == GType.Invalid) {
					copy = new CopyFunc (Copy);
					free = new FreeFunc (Free);
				
					boxed_type = new GLib.GType (g_boxed_type_register_static ("GtkSharpValue", copy, free));
				}

				return boxed_type;
			}
		}
		
		public static IntPtr Copy (IntPtr ptr)
		{
			ValueHolder holder = (ValueHolder) pointers[ptr];
			holder.ref_count++;
			return ptr;
		}

		public static void Free (IntPtr ptr)
		{
			ValueHolder holder = (ValueHolder) pointers[ptr];
			if (holder == null)
				return;
			holder.ref_count--;
			if (holder.ref_count < 1)
				pointers.Remove (ptr);
		}

		public static IntPtr WrapObject (object obj)
		{
			ValueHolder holder = new ValueHolder ();
			holder.val = obj;
			holder.ref_count = 1;
			cur_ptr = new IntPtr (((int) cur_ptr) + 1);
			pointers[cur_ptr] = holder;
			return cur_ptr;
		}

		public static object ObjectForWrapper (IntPtr ptr)
		{
			if (!pointers.Contains (ptr))
				return null;

			ValueHolder holder = (ValueHolder) pointers[ptr];
			return holder.val;
		}

	}
}


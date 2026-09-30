// DropTarget.cs - Gtk DropTarget class customizations
//
// This code is inserted after the automatically generated code.
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of version 2 of the Lesser GNU General
// Public License as published by the Free Software Foundation.

namespace Gtk {

	using System;
	using System.Runtime.InteropServices;

	public partial class DropTarget {

		// gtk_drop_target_set_gtypes takes "const GType *types, gsize n_types"
		// and gtk_drop_target_get_gtypes returns the same pair. Codegen has a
		// rule for a NULL-terminated array and none for "pointer plus count",
		// so both came out over a single GLib.GType:
		//
		//   public void SetGtypes (GLib.GType types, ulong n_types)
		//       => gtk_drop_target_set_gtypes (Handle, types.Val, n_types);
		//
		// which hands GTK the GType's own numeric value as the address of an
		// array of n_types GTypes -- G_TYPE_STRING is 64, so GTK dereferences
		// address 64 -- while the getter wrapped the array's address in a
		// GLib.GType and produced a "type" that is a pointer.
		//
		// This is the only way to make one drop target accept more than the
		// single type its constructor takes, so the multi-type case could not
		// be expressed at all. Same family as gdk_content_formats_get_gtypes.
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_gtk_drop_target_get_gtypes(IntPtr raw, out UIntPtr n_types);
		static d_gtk_drop_target_get_gtypes gtk_drop_target_get_gtypes = FuncLoader.LoadFunction<d_gtk_drop_target_get_gtypes>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_drop_target_get_gtypes"));

		public GLib.GType[] GetGtypes ()
		{
			UIntPtr count;
			IntPtr array = gtk_drop_target_get_gtypes (Handle, out count);
			if (array == IntPtr.Zero)
				return new GLib.GType [0];

			GLib.GType[] result = new GLib.GType [(int) count];
			for (int i = 0; i < result.Length; i++)
				result [i] = new GLib.GType (Marshal.ReadIntPtr (array, i * IntPtr.Size));
			return result;
		}

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_gtk_drop_target_set_gtypes(IntPtr raw, IntPtr[] types, UIntPtr n_types);
		static d_gtk_drop_target_set_gtypes gtk_drop_target_set_gtypes = FuncLoader.LoadFunction<d_gtk_drop_target_set_gtypes>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_drop_target_set_gtypes"));

		public void SetGtypes (GLib.GType[] types)
		{
			int count = types == null ? 0 : types.Length;
			IntPtr[] native = new IntPtr [count];
			for (int i = 0; i < count; i++)
				native [i] = types [i].Val;

			gtk_drop_target_set_gtypes (Handle, count == 0 ? null : native, new UIntPtr ((ulong) count));
		}

		public GLib.GType[] Gtypes {
			get { return GetGtypes (); }
			set { SetGtypes (value); }
		}
	}
}

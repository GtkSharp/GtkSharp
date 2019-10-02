// Gdk.Window.cs - Gdk Window class customizations
//
// Author: Moritz Balz <ich@mbalz.de>
//         Mike Kestner <mkestner@ximian.com>
//
// Copyright (c) 2003 Moritz Balz
// Copyright (c) 2004 - 2008 Novell, Inc.
//
// This code is inserted after the automatically generated code.
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

namespace Gdk {

	using System;
	using System.Collections.Generic;
	using System.Runtime.InteropServices;

	public partial class Window {

		public Window (Gdk.Window parent, Gdk.WindowAttr attributes, Gdk.WindowAttributesType attributes_mask) : this (parent, attributes, (int)attributes_mask) {}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_gdk_window_get_background_pattern(IntPtr raw);
		static d_gdk_window_get_background_pattern gdk_window_get_background_pattern = FuncLoader.LoadFunction<d_gdk_window_get_background_pattern>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gdk), "gdk_window_get_background_pattern"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_gdk_window_set_background_pattern(IntPtr raw, IntPtr pattern);
		static d_gdk_window_set_background_pattern gdk_window_set_background_pattern = FuncLoader.LoadFunction<d_gdk_window_set_background_pattern>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gdk), "gdk_window_set_background_pattern"));

		public Cairo.Pattern BackgroundPattern { 
			get {
				IntPtr raw_ret = gdk_window_get_background_pattern(Handle);
				Cairo.Pattern ret = Cairo.Pattern.Lookup (raw_ret, true);
				return ret;
			}
			set {
				gdk_window_set_background_pattern(Handle, (value == null) ? IntPtr.Zero : value.Handle);
			}
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_gdk_window_get_children(IntPtr raw);
		static d_gdk_window_get_children gdk_window_get_children = FuncLoader.LoadFunction<d_gdk_window_get_children>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gdk), "gdk_window_get_children"));

		public Window[] Children {
			get {
				IntPtr raw_ret = gdk_window_get_children(Handle);
				if (raw_ret == IntPtr.Zero)
					return new Window [0];
				GLib.List list = new GLib.List(raw_ret);
				Window[] result = new Window [list.Count];
				for (int i = 0; i < list.Count; i++)
					result [i] = list [i] as Window;
				return result;
			}
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_gdk_window_set_icon_list(IntPtr raw, IntPtr pixbufs);
		static d_gdk_window_set_icon_list gdk_window_set_icon_list = FuncLoader.LoadFunction<d_gdk_window_set_icon_list>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gdk), "gdk_window_set_icon_list"));

		public Pixbuf[] IconList {
			set {
				GLib.List list = new GLib.List(IntPtr.Zero);
				foreach (Pixbuf val in value)
					list.Append (val.Handle);
				gdk_window_set_icon_list(Handle, list.Handle);
			}
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_object_ref(IntPtr raw);
		static d_g_object_ref g_object_ref = FuncLoader.LoadFunction<d_g_object_ref>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_object_ref"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_gdk_window_destroy(IntPtr raw);
		static d_gdk_window_destroy gdk_window_destroy = FuncLoader.LoadFunction<d_gdk_window_destroy>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gdk), "gdk_window_destroy"));

		public void Destroy () 
		{
			// native method assumes an outstanding normal ref, but we hold a
			// toggle ref.  take out a normal ref for it to release,  and let 
			// Dispose release our toggle ref.
			g_object_ref (Handle);
			gdk_window_destroy(Handle);
			Dispose ();
		}

		public void MoveResize (Gdk.Rectangle rect) {
			gdk_window_move_resize (Handle, rect.X, rect.Y, rect.Width, rect.Height);
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_gdk_window_get_user_data(IntPtr raw, out IntPtr data);
		static d_gdk_window_get_user_data gdk_window_get_user_data = FuncLoader.LoadFunction<d_gdk_window_get_user_data>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gdk), "gdk_window_get_user_data"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_gdk_window_set_user_data(IntPtr raw, IntPtr user_data);
		static d_gdk_window_set_user_data gdk_window_set_user_data = FuncLoader.LoadFunction<d_gdk_window_set_user_data>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gdk), "gdk_window_set_user_data"));
		public IntPtr UserData {
			get {
				IntPtr data;
				gdk_window_get_user_data (Handle, out data);
				return data;
			}
			set {
				gdk_window_set_user_data(Handle, value);
			}
		} 
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_gdk_window_add_filter(IntPtr handle, GdkSharp.FilterFuncNative wrapper, IntPtr data);
		static d_gdk_window_add_filter gdk_window_add_filter = FuncLoader.LoadFunction<d_gdk_window_add_filter>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gdk), "gdk_window_add_filter"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_gdk_window_remove_filter(IntPtr handle, GdkSharp.FilterFuncNative wrapper, IntPtr data);
		static d_gdk_window_remove_filter gdk_window_remove_filter = FuncLoader.LoadFunction<d_gdk_window_remove_filter>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gdk), "gdk_window_remove_filter"));

		static IDictionary<FilterFunc, GdkSharp.FilterFuncWrapper> filter_all_hash;
		static IDictionary<FilterFunc, GdkSharp.FilterFuncWrapper> FilterAllHash {
			get {
				if (filter_all_hash == null) {
					filter_all_hash = new Dictionary<FilterFunc, GdkSharp.FilterFuncWrapper> ();
				}
				return filter_all_hash;
			}
		}

		public static void AddFilterForAll (FilterFunc func)
		{
			GdkSharp.FilterFuncWrapper wrapper = new GdkSharp.FilterFuncWrapper (func);
			FilterAllHash [func] = wrapper;
			gdk_window_add_filter (IntPtr.Zero, wrapper.NativeDelegate, IntPtr.Zero);
		}

		public static void RemoveFilterForAll (FilterFunc func)
		{
			GdkSharp.FilterFuncWrapper wrapper = null;
			if (FilterAllHash.TryGetValue (func, out wrapper)) {
				FilterAllHash.Remove (func);
				gdk_window_remove_filter (IntPtr.Zero, wrapper.NativeDelegate, IntPtr.Zero);
			}
		}

		public void AddFilter (FilterFunc function)
		{
			if (!Data.ContainsKey ("filter_func_hash")) {
				Data ["filter_func_hash"] = new Dictionary<FilterFunc, GdkSharp.FilterFuncWrapper> ();
			}
			var hash = Data ["filter_func_hash"] as Dictionary<FilterFunc, GdkSharp.FilterFuncWrapper>;
			GdkSharp.FilterFuncWrapper wrapper = new GdkSharp.FilterFuncWrapper (function);
			hash [function] = wrapper;
			gdk_window_add_filter (Handle, wrapper.NativeDelegate, IntPtr.Zero);
		}

		public void RemoveFilter (FilterFunc function)
		{
			var hash = Data ["filter_func_hash"] as Dictionary<FilterFunc, GdkSharp.FilterFuncWrapper>;
			GdkSharp.FilterFuncWrapper wrapper = null;
			if (hash.TryGetValue (function, out wrapper)) {
				hash.Remove (function);
				gdk_window_remove_filter (Handle, wrapper.NativeDelegate, IntPtr.Zero);
			}
		}

#if MANLY_ENOUGH_TO_INCLUDE
		public Cairo.Graphics CairoGraphics (out int offset_x, out int offset_y)
		{
			IntPtr real_drawable;
			Cairo.Graphics o = new Cairo.Graphics ();

			gdk_window_get_internal_paint_info (Handle, out real_drawable, out offset_x, out offset_y);
			IntPtr x11 = gdk_x11_drawable_get_xid (real_drawable);
			IntPtr display = gdk_x11_drawable_get_xdisplay (real_drawable);
			o.SetTargetDrawable (display, x11);

			return o;
		}


		public override Cairo.Graphics CairoGraphics ()
		{
			int x, y;
			return CairoGraphics (out x, out y);
		}
#endif
	}
}


// Screen.cs - customizations to Gdk.Screen
//
// Authors: Mike Kestner  <mkestner@ximian.com>
//
// Copyright (c) 2004 Novell, Inc.
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
	using System.Runtime.InteropServices;

	public partial class Screen {
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_gdk_screen_get_toplevel_windows(IntPtr raw);
		static d_gdk_screen_get_toplevel_windows gdk_screen_get_toplevel_windows = FuncLoader.LoadFunction<d_gdk_screen_get_toplevel_windows>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gdk), "gdk_screen_get_toplevel_windows"));

		public Window[] ToplevelWindows
		{
			get {
				IntPtr raw_ret = gdk_screen_get_toplevel_windows (Handle);
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
		delegate IntPtr d_gdk_screen_list_visuals(IntPtr raw);
		static d_gdk_screen_list_visuals gdk_screen_list_visuals = FuncLoader.LoadFunction<d_gdk_screen_list_visuals>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gdk), "gdk_screen_list_visuals"));

		public Visual[] ListVisuals ()
		{
			IntPtr raw_ret = gdk_screen_list_visuals (Handle);
			if (raw_ret == IntPtr.Zero)
				return new Visual [0];
			GLib.List list = new GLib.List(raw_ret);
			Visual[] result = new Visual [list.Count];
			for (int i = 0; i < list.Count; i++)
				result [i] = list [i] as Visual;
			return result;
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_gdk_screen_get_font_options(IntPtr raw);
		static d_gdk_screen_get_font_options gdk_screen_get_font_options = FuncLoader.LoadFunction<d_gdk_screen_get_font_options>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gdk), "gdk_screen_get_font_options"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_gdk_screen_set_font_options(IntPtr raw, IntPtr options);
		static d_gdk_screen_set_font_options gdk_screen_set_font_options = FuncLoader.LoadFunction<d_gdk_screen_set_font_options>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gdk), "gdk_screen_set_font_options"));

		[GLib.Property ("font-options")]
		public Cairo.FontOptions FontOptions {
			get  {
				IntPtr raw_ret = gdk_screen_get_font_options(Handle);
				if (raw_ret == IntPtr.Zero)
					return null;
				System.Reflection.BindingFlags flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.CreateInstance;
				return Activator.CreateInstance (typeof (Cairo.FontOptions), flags, null, new object [] {raw_ret}, null) as Cairo.FontOptions;
			}
			set  {
				gdk_screen_set_font_options(Handle, value == null ? IntPtr.Zero : value.Handle);
			}
		}
	}
}



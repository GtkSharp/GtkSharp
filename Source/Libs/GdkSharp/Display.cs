// Display.cs - customizations to Gdk.Display
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
	using System.Collections;
	using System.Runtime.InteropServices;

	public partial class Display {
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_gdk_display_get_pointer(IntPtr raw, IntPtr screen, out int x, out int y, out int mask);
		static d_gdk_display_get_pointer gdk_display_get_pointer = FuncLoader.LoadFunction<d_gdk_display_get_pointer>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gdk), "gdk_display_get_pointer"));

		[Obsolete]
		public void GetPointer(Gdk.Screen screen, out int x, out int y, out Gdk.ModifierType mask) {
			int mask_as_int;
			gdk_display_get_pointer(Handle, screen.Handle, out x, out y, out mask_as_int);
			mask = (Gdk.ModifierType) mask_as_int;
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_gdk_display_get_pointer2(IntPtr raw, out IntPtr screen, out int x, out int y, out int mask);
		static d_gdk_display_get_pointer2 gdk_display_get_pointer2 = FuncLoader.LoadFunction<d_gdk_display_get_pointer2>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gdk), "gdk_display_get_pointer"));

		public void GetPointer(out Gdk.Screen screen, out int x, out int y, out Gdk.ModifierType mask) {
			IntPtr screen_handle;
			int mask_as_int;
			gdk_display_get_pointer2(Handle, out screen_handle, out x, out y, out mask_as_int);
			screen = (Gdk.Screen) GLib.Object.GetObject(screen_handle);
			mask = (Gdk.ModifierType) mask_as_int;
		}

		public void GetPointer (out int x, out int y)
		{
			Gdk.ModifierType mod;
			Gdk.Screen screen;
			GetPointer (out screen, out x, out y, out mod);
		}

		public void GetPointer (out int x, out int y, out Gdk.ModifierType mod)
		{
			Gdk.Screen screen;
			GetPointer (out screen, out x, out y, out mod);
		}

		public void GetPointer (out Gdk.Screen screen, out int x, out int y)
		{
			Gdk.ModifierType mod;
			GetPointer (out screen, out x, out y, out mod);
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_gdk_display_list_devices(IntPtr raw);
		static d_gdk_display_list_devices gdk_display_list_devices = FuncLoader.LoadFunction<d_gdk_display_list_devices>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gdk), "gdk_display_list_devices"));

		public Device[] ListDevices ()
		{
			IntPtr raw_ret = gdk_display_list_devices (Handle);
			if (raw_ret == IntPtr.Zero)
				return new Device [0];
			GLib.List list = new GLib.List(raw_ret);
			Device[] result = new Device [list.Count];
			for (int i = 0; i < list.Count; i++)
				result [i] = list [i] as Device;
			return result;
		}
	}
}



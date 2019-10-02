// DisplayManager.cs - customizations to Gdk.DisplayManager
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

	public partial class DisplayManager {
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_gdk_display_manager_list_displays(IntPtr raw);
		static d_gdk_display_manager_list_displays gdk_display_manager_list_displays = FuncLoader.LoadFunction<d_gdk_display_manager_list_displays>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gdk), "gdk_display_manager_list_displays"));

		public Display[] ListDisplays ()
		{
			IntPtr raw_ret = gdk_display_manager_list_displays (Handle);
			if (raw_ret == IntPtr.Zero)
				return new Display [0];
			GLib.SList list = new GLib.SList(raw_ret);
			Display[] result = new Display [list.Count];
			for (int i = 0; i < list.Count; i++)
				result [i] = list [i] as Display;
			return result;
		}
	}
}



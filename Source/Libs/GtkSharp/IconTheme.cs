// IconTheme.cs - customizations to Gtk.IconTheme
//
// Authors: Mike Kestner  <mkestner@ximian.com>
//	    Jeroen Zwartepoorte  <jeroen@xs4all.nl>
//
// Copyright (c) 2004-2005 Novell, Inc.
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
	using System.Collections.Generic;
	using System.Runtime.InteropServices;

	public partial class IconTheme {
		// ListIcons: gtk_icon_theme_list_icons is gone in Gtk 4; the generated IconNames property replaces it.
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_gtk_icon_theme_get_search_path(IntPtr raw, out IntPtr path, out int n_elements);
		static d_gtk_icon_theme_get_search_path gtk_icon_theme_get_search_path = FuncLoader.LoadFunction<d_gtk_icon_theme_get_search_path>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_icon_theme_get_search_path"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_gtk_icon_theme_set_search_path(IntPtr raw, IntPtr[] path, int n_elements);
		static d_gtk_icon_theme_set_search_path gtk_icon_theme_set_search_path = FuncLoader.LoadFunction<d_gtk_icon_theme_set_search_path>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_icon_theme_set_search_path"));

		bool IsWindowsPlatform {
			get {
				switch (Environment.OSVersion.Platform) {
				case PlatformID.Win32NT:
				case PlatformID.Win32S:
				case PlatformID.Win32Windows:
				case PlatformID.WinCE:
					return true;
				default:
					return false;
				}
			}
		}

		// SetSearchPath: superseded by the generated SearchPath property.
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_gtk_icon_theme_get_icon_sizes(IntPtr raw, IntPtr icon_name);
		static d_gtk_icon_theme_get_icon_sizes gtk_icon_theme_get_icon_sizes = FuncLoader.LoadFunction<d_gtk_icon_theme_get_icon_sizes>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_icon_theme_get_icon_sizes"));

		public int[] GetIconSizes (string icon_name) 
		{
			IntPtr icon_name_as_native = GLib.Marshaller.StringToPtrGStrdup (icon_name);
			IntPtr raw_ret = gtk_icon_theme_get_icon_sizes(Handle, icon_name_as_native);
			var result = new List<int> ();
			int offset = 0;
			int size = Marshal.ReadInt32 (raw_ret, offset);
			while (size != 0) {
				result.Add (size);
				offset += 4;
				size = Marshal.ReadInt32 (raw_ret, offset);
			}
			GLib.Marshaller.Free (icon_name_as_native);
			GLib.Marshaller.Free (raw_ret);
			return result.ToArray ();
		}
	}
}


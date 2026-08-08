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
		//
		// The search path itself is generated: Gtk 4 gave
		// gtk_icon_theme_{get,set}_search_path the plain strv shape, so the
		// method pair becomes the SearchPath property. The two delegates that
		// used to stand here still described Gtk 3's (char ***, int *), and were
		// dead code holding the metadata rules that hid the real pair in place.

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


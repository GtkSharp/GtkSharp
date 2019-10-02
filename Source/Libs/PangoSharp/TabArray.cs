// Pango.TabArray.cs - Pango TabArray class customizations
//
// Author: Mike Kestner <mkestner@ximian.com>
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

namespace Pango {

	using System;
	using System.Runtime.InteropServices;

	public partial class TabArray {
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_pango_tab_array_get_tabs(IntPtr raw, out IntPtr alignments, out IntPtr locations);
		static d_pango_tab_array_get_tabs pango_tab_array_get_tabs = FuncLoader.LoadFunction<d_pango_tab_array_get_tabs>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Pango), "pango_tab_array_get_tabs"));

		public void GetTabs (out TabAlign[] alignments, out int[] locations) 
		{
			int sz = Size;
			IntPtr align_ptr, loc_ptr;
			alignments = new TabAlign [sz];
			locations = new int [sz];
			int[] tmp = new int [sz];
			if (sz == 0)
				return;
			pango_tab_array_get_tabs (Handle, out align_ptr, out loc_ptr);
			Marshal.Copy (loc_ptr, locations, 0, sz);
			Marshal.Copy (align_ptr, tmp, 0, sz);
			for (int i = 0; i < sz; i++)
				alignments [i] = (TabAlign) tmp [i];
			GLib.Marshaller.Free (align_ptr);
			GLib.Marshaller.Free (loc_ptr);
		}
	}
}


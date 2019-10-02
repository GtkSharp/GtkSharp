// Pango.FontMap.cs - Pango FontMap class customizations
//
// Authors:  Mike Kestner  <mkestner@ximian.com>
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

	public partial class FontMap {
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_pango_font_map_list_families2(IntPtr raw, out IntPtr families, out int n_families);
		static d_pango_font_map_list_families2 pango_font_map_list_families2 = FuncLoader.LoadFunction<d_pango_font_map_list_families2>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Pango), "pango_font_map_list_families"));

		public FontFamily [] Families {
			get {
				int count;
				IntPtr array_ptr;
				pango_font_map_list_families2 (Handle, out array_ptr, out count);
				if (array_ptr == IntPtr.Zero)
					return new FontFamily [0];
				FontFamily [] result = new FontFamily [count];
				for (int i = 0; i < count; i++) {
					IntPtr fam_ptr = Marshal.ReadIntPtr (array_ptr, i * IntPtr.Size);
					result [i] = GLib.Object.GetObject (fam_ptr) as FontFamily;
				}

				GLib.Marshaller.Free (array_ptr);
				return result;
			}
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_pango_font_map_list_families(IntPtr raw, IntPtr families, out int n_families);
		static d_pango_font_map_list_families pango_font_map_list_families = FuncLoader.LoadFunction<d_pango_font_map_list_families>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Pango), "pango_font_map_list_families"));

		[Obsolete]
		public int ListFamilies(Pango.FontFamily families) {
			int n_families;
			pango_font_map_list_families(Handle, families.Handle, out n_families);
			return n_families;
		}
	}
}


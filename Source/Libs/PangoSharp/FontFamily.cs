// Pango.FontFamily.cs - Pango FontFamily class customizations
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

	public partial class FontFamily {
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_pango_font_family_list_faces2(IntPtr raw, out IntPtr faces, out int n_faces);
		static d_pango_font_family_list_faces2 pango_font_family_list_faces2 = FuncLoader.LoadFunction<d_pango_font_family_list_faces2>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Pango), "pango_font_family_list_faces"));

		public FontFace [] Faces {
			get {
				int count;
				IntPtr array_ptr;
				pango_font_family_list_faces2 (Handle, out array_ptr, out count);
				if (array_ptr == IntPtr.Zero)
					return new FontFace [0];
				FontFace [] result = new FontFace [count];
				for (int i = 0; i < count; i++) {
					IntPtr fam_ptr = Marshal.ReadIntPtr (array_ptr, i * IntPtr.Size);
					result [i] = GLib.Object.GetObject (fam_ptr) as FontFace;
				}

				GLib.Marshaller.Free (array_ptr);
				return result;
			}
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_pango_font_family_list_faces(IntPtr raw, IntPtr faces, out int n_faces);
		static d_pango_font_family_list_faces pango_font_family_list_faces = FuncLoader.LoadFunction<d_pango_font_family_list_faces>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Pango), "pango_font_family_list_faces"));

		[Obsolete]
		public int ListFaces(Pango.FontFace faces) {
			int n_faces;
			pango_font_family_list_faces(Handle, faces.Handle, out n_faces);
			return n_faces;
		}
	}
}


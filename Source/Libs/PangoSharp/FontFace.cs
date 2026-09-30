// Pango.FontFace.cs - Pango FontFace class customizations
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

	public partial class FontFace {

		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		delegate void d_pango_font_face_list_sizes (IntPtr raw, out IntPtr sizes, out int n_sizes);
		static d_pango_font_face_list_sizes pango_font_face_list_sizes = FuncLoader.LoadFunction<d_pango_font_face_list_sizes> (FuncLoader.GetProcAddress (GLibrary.Load (Library.Pango), "pango_font_face_list_sizes"));

		// "int **sizes" is a pointer to a pointer the callee allocates. The
		// api.xml records it as out, which for an int* means "out int" -- a
		// four-byte slot -- and Pango wrote an eight-byte address through it.
		// Four bytes of stack past the end, on every call, before the count was
		// even read. Same family as the caller-allocates defects in Gdk.
		//
		// A scalable font answers with NULL and zero, which is why nothing had
		// noticed: the overrun writes four zero bytes rather than an address.
		public int[] ListSizes ()
		{
			int count;
			IntPtr array_ptr;
			pango_font_face_list_sizes (Handle, out array_ptr, out count);
			if (array_ptr == IntPtr.Zero || count <= 0)
				return new int [0];

			int[] result = new int [count];
			Marshal.Copy (array_ptr, result, 0, count);
			GLib.Marshaller.Free (array_ptr);
			return result;
		}
	}
}

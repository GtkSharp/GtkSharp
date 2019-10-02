// Pango.LayoutLine.cs - Pango LayoutLine class customizations
//
// Authors: Jeroen Zwartepoorte <jeroen@xs4all.nl
//	    Mike Kestner <mkestner@ximian.com>
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

	public partial class LayoutLine {

#if NOT_BROKEN
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_pango_layout_line_get_x_ranges(IntPtr raw, int start_index, int end_index, out IntPtr ranges_handle, out int n_ranges);
		static d_pango_layout_line_get_x_ranges pango_layout_line_get_x_ranges = FuncLoader.LoadFunction<d_pango_layout_line_get_x_ranges>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Pango), "pango_layout_line_get_x_ranges"));
#endif

		public void GetXRanges(int start_index, int end_index, out int[][] ranges)
		{
			// FIXME: this is broken
			throw new NotImplementedException ();
#if NOT_BROKEN
			int count;
			IntPtr array_ptr;
			pango_layout_line_get_x_ranges(Handle, start_index, end_index, out array_ptr, out count);
			ranges = new int[count] [];
			for (int i = 0; i < count; i++) {
				IntPtr tmp = new IntPtr (array_ptr + 2 * i * IntPtr.Size);
				IntPtr rng_ptr = Marshal.ReadIntPtr (tmp);
				IntPtr end_ptr = Marshal.ReadIntPtr (tmp, IntPtr.Size);

			}
			Marshal.Copy (array_ptr, ranges, 0, count);
			GLib.Marshaller.Free (array_ptr);
#endif
		}
	}
}


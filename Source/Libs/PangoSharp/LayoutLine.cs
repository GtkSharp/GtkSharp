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
	using System.Runtime.InteropServices;

	public partial class LayoutLine {

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_pango_layout_line_get_x_ranges(IntPtr raw, int start_index, int end_index, out IntPtr ranges_handle, out int n_ranges);
		static readonly d_pango_layout_line_get_x_ranges pango_layout_line_get_x_ranges = FuncLoader.LoadFunction<d_pango_layout_line_get_x_ranges>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Pango), "pango_layout_line_get_x_ranges"));

		public int[] GetXRanges(int start_index, int end_index)
		{
			int count;
			IntPtr array_ptr;
			pango_layout_line_get_x_ranges(Handle, start_index, end_index, out array_ptr, out count);
			int[] array = new int[count * 2];
			Marshal.Copy(array_ptr, array, 0, count * 2);
			GLib.Marshaller.Free (array_ptr);
			return array;
		}

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_pango_layout_line_get_height(IntPtr raw, out int height);
		static readonly d_pango_layout_line_get_height pango_layout_line_get_height = FuncLoader.LoadFunction<d_pango_layout_line_get_height>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Pango), "pango_layout_line_get_height"));

		public int Height
		{
			get
			{
				int height;
				pango_layout_line_get_height(Handle, out height);
				return height;
			}
		}
	}
}

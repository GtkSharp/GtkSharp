// Pango.Coverage.cs - Pango Coverage class customizations
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

	public partial class Coverage {
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_pango_coverage_to_bytes(IntPtr raw, out IntPtr bytes, out int n_bytes);
		static d_pango_coverage_to_bytes pango_coverage_to_bytes = FuncLoader.LoadFunction<d_pango_coverage_to_bytes>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Pango), "pango_coverage_to_bytes"));

		public void ToBytes(out byte[] bytes) 
		{
			int count;
			IntPtr array_ptr;
			pango_coverage_to_bytes (Handle, out array_ptr, out count);
			bytes = new byte [count];
			Marshal.Copy (array_ptr, bytes, 0, count);
			GLib.Marshaller.Free (array_ptr);
		}
	}
}


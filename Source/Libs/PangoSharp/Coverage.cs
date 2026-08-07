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

		// Deprecated since Pango 1.44, when coverage was reimplemented on top of
		// hb_set: pango_coverage_to_bytes now writes NULL and 0 rather than
		// serialising anything. Marshal.Copy rejects a null source whatever the
		// length, so the unguarded version threw ArgumentNullException on every
		// modern Pango instead of handing back the empty array the call means.
		public void ToBytes(out byte[] bytes)
		{
			int count;
			IntPtr array_ptr;
			pango_coverage_to_bytes (Handle, out array_ptr, out count);
			if (array_ptr == IntPtr.Zero || count <= 0) {
				bytes = new byte [0];
				return;
			}
			bytes = new byte [count];
			Marshal.Copy (array_ptr, bytes, 0, count);
			GLib.Marshaller.Free (array_ptr);
		}

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_pango_coverage_from_bytes([In] byte[] bytes, int n_bytes);
		static d_pango_coverage_from_bytes pango_coverage_from_bytes = FuncLoader.LoadFunction<d_pango_coverage_from_bytes>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Pango), "pango_coverage_from_bytes"));

		// The other half of the same deprecated pair. Its "guchar *bytes" is an
		// input array, and codegen bound it "out byte" -- so the one thing a
		// caller had to supply was the one thing the signature would not let it
		// supply. Like ToBytes above, Pango 1.44 made it inert; it returns NULL.
		public static Coverage FromBytes (byte[] bytes)
		{
			if (bytes == null)
				throw new ArgumentNullException ("bytes");

			IntPtr raw_ret = pango_coverage_from_bytes (bytes, bytes.Length);
			return raw_ret == IntPtr.Zero ? null : GLib.Object.GetObject (raw_ret, true) as Coverage;
		}
	}
}


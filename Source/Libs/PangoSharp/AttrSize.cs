// Pango.AttrSize - Pango.Attribute for font size
//
// Copyright (c) 2005 Novell, Inc.
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

	public class AttrSize : Attribute {
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_pango_attr_size_new(int size);
		static d_pango_attr_size_new pango_attr_size_new = FuncLoader.LoadFunction<d_pango_attr_size_new>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Pango), "pango_attr_size_new"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_pango_attr_size_new_absolute(int size);
		static d_pango_attr_size_new_absolute pango_attr_size_new_absolute = FuncLoader.LoadFunction<d_pango_attr_size_new_absolute>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Pango), "pango_attr_size_new_absolute"));

		public AttrSize (int size) : this (pango_attr_size_new (size)) {}

		public AttrSize (int size, bool absolute) : this (absolute ? pango_attr_size_new (size) : pango_attr_size_new_absolute (size)) {}

		internal AttrSize (IntPtr raw) : base (raw) {}

		new struct NativeStruct {
			Attribute.NativeStruct attr;
			public int sz;
			public uint absolute;
		}

		public int Size {
			get {
				NativeStruct native = (NativeStruct) Marshal.PtrToStructure (Handle, typeof (NativeStruct));
				return native.sz;
			}
		}

		public bool Absolute {
			get {
				NativeStruct native = (NativeStruct) Marshal.PtrToStructure (Handle, typeof (NativeStruct));
				return native.absolute != 0;
			}
		}
	}
}


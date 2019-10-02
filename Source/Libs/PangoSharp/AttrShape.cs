// Pango.AttrShape - Pango.Attribute for shape restrictions
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

	public class AttrShape : Attribute {
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_pango_attr_shape_new(ref Pango.Rectangle ink_rect, ref Pango.Rectangle logical_rect);
		static d_pango_attr_shape_new pango_attr_shape_new = FuncLoader.LoadFunction<d_pango_attr_shape_new>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Pango), "pango_attr_shape_new"));

		public AttrShape (Pango.Rectangle ink_rect, Pango.Rectangle logical_rect) : this (pango_attr_shape_new (ref ink_rect, ref logical_rect)) {}

		internal AttrShape (IntPtr raw) : base (raw) {}

		new struct NativeStruct {
			Attribute.NativeStruct attr;
			public Rectangle ink_rect;
			public Rectangle logical_rect;
			IntPtr data;
			IntPtr copy_func;
			IntPtr destroy_func;
		}

		public Pango.Rectangle InkRect {
			get {
				NativeStruct native = (NativeStruct) Marshal.PtrToStructure (Handle, typeof (NativeStruct));
				return native.ink_rect;
			}
		}

		public Pango.Rectangle LogicalRect {
			get {
				NativeStruct native = (NativeStruct) Marshal.PtrToStructure (Handle, typeof (NativeStruct));
				return native.logical_rect;
			}
		}
	}
}


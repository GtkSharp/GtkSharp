// Pango.AttrFontDesc - Pango.Attribute for Pango.FontDescription
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

	public class AttrFontDesc : Attribute {
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_pango_attr_font_desc_new(IntPtr font_desc);
		static d_pango_attr_font_desc_new pango_attr_font_desc_new = FuncLoader.LoadFunction<d_pango_attr_font_desc_new>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Pango), "pango_attr_font_desc_new"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_pango_font_description_copy(IntPtr raw);
		static d_pango_font_description_copy pango_font_description_copy = FuncLoader.LoadFunction<d_pango_font_description_copy>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Pango), "pango_font_description_copy"));

		public AttrFontDesc (Pango.FontDescription font_desc) : this (pango_attr_font_desc_new (pango_font_description_copy (font_desc.Handle))) {}

		internal AttrFontDesc (IntPtr raw) : base (raw) {}

		new struct NativeStruct {
			Attribute.NativeStruct attr;
			public IntPtr desc;
		}

		public Pango.FontDescription Desc {
			get {
				NativeStruct native = (NativeStruct) Marshal.PtrToStructure (Handle, typeof (NativeStruct));
				return new Pango.FontDescription (native.desc);
			}
		}
	}
}


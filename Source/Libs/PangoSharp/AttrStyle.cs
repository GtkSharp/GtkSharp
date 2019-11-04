// Pango.AttrStyle - Pango.Attribute for Pango.Style
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

	public class AttrStyle : Attribute {
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_pango_attr_style_new(Pango.Style style);
		static d_pango_attr_style_new pango_attr_style_new = FuncLoader.LoadFunction<d_pango_attr_style_new>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Pango), "pango_attr_style_new"));

		public AttrStyle (Pango.Style style) : this (pango_attr_style_new (style)) {}

		internal AttrStyle (IntPtr raw) : base (raw) {}

		public Pango.Style Style {
			get {
				return (Pango.Style) (AttrInt.New (Handle).Value);
			}
		}
	}
}


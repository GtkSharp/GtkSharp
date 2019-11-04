// Pango.AttrStrikethroughColor - Pango.Attribute for strikethrough color
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

	public class AttrStrikethroughColor : Attribute {
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_pango_attr_strikethrough_color_new(ushort red, ushort green, ushort blue);
		static d_pango_attr_strikethrough_color_new pango_attr_strikethrough_color_new = FuncLoader.LoadFunction<d_pango_attr_strikethrough_color_new>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Pango), "pango_attr_strikethrough_color_new"));

		public AttrStrikethroughColor (ushort red, ushort green, ushort blue) : this (pango_attr_strikethrough_color_new (red, green, blue)) {}

		public AttrStrikethroughColor (Pango.Color color) : this (pango_attr_strikethrough_color_new (color.Red, color.Green, color.Blue)) {}

		internal AttrStrikethroughColor (IntPtr raw) : base (raw) {}

		public Pango.Color Color {
			get {
				return AttrColor.New (Handle).Color;
			}
		}
	}
}


// Pango.AttrUnderlineColor - Pango.Attribute for underline color
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

	public class AttrUnderlineColor : Attribute {

		[DllImport("libpango-1.0-0.dll")]
		static extern IntPtr pango_attr_underline_color_new (ushort red, ushort green, ushort blue);

		public AttrUnderlineColor (ushort red, ushort green, ushort blue) : this (pango_attr_underline_color_new (red, green, blue)) {}

		public AttrUnderlineColor (Pango.Color color) : this (pango_attr_underline_color_new (color.Red, color.Green, color.Blue)) {}

		internal AttrUnderlineColor (IntPtr raw) : base (raw) {}

		[DllImport("pangosharpglue-2")]
		static extern Pango.Color pangosharp_attr_color_get_color (IntPtr raw);

		public Pango.Color Color {
			get {
				return pangosharp_attr_color_get_color (Handle);
			}
		}
	}
}

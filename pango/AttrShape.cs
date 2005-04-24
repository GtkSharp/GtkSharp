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

		[DllImport("libpango-1.0-0.dll")]
		static extern IntPtr pango_attr_shape_new (ref Pango.Rectangle ink_rect, ref Pango.Rectangle logical_rect);

		public AttrShape (Pango.Rectangle ink_rect, Pango.Rectangle logical_rect) : this (pango_attr_shape_new (ref ink_rect, ref logical_rect)) {}

		internal AttrShape (IntPtr raw) : base (raw) {}

		[DllImport("pangosharpglue-2")]
		static extern Pango.Rectangle pangosharp_attr_shape_get_ink_rect (IntPtr raw);

		public Pango.Rectangle InkRect {
			get {
				return pangosharp_attr_shape_get_ink_rect (Handle);
			}
		}

		[DllImport("pangosharpglue-2")]
		static extern Pango.Rectangle pangosharp_attr_shape_get_logical_rect (IntPtr raw);

		public Pango.Rectangle LogicalRect {
			get {
				return pangosharp_attr_shape_get_logical_rect (Handle);
			}
		}
	}
}

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

		[DllImport("libpango-1.0-0.dll")]
		static extern IntPtr pango_attr_size_new (int size);

		[DllImport("libpango-1.0-0.dll")]
		static extern IntPtr pango_attr_size_new_absolute (int size);

		public AttrSize (int size) : this (pango_attr_size_new (size)) {}

		public AttrSize (int size, bool absolute) : this (absolute ? pango_attr_size_new (size) : pango_attr_size_new_absolute (size)) {}

		internal AttrSize (IntPtr raw) : base (raw) {}

		[DllImport("pangosharpglue-2")]
		static extern int pangosharp_attr_size_get_size (IntPtr raw);

		public int Size {
			get {
				return pangosharp_attr_size_get_size (Handle);
			}
		}

		[DllImport("pangosharpglue-2")]
		static extern bool pangosharp_attr_size_get_absolute (IntPtr raw);

		public bool Absolute {
			get {
				return pangosharp_attr_size_get_absolute (Handle);
			}
		}
	}
}

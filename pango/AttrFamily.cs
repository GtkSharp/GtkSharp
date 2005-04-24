// Pango.AttrFamily - Pango.Attribute for font families
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

	public class AttrFamily : Attribute {

		[DllImport("libpango-1.0-0.dll")]
		static extern IntPtr pango_attr_family_new (IntPtr family);

		public AttrFamily (string family) : base (NewAttrFamily (family)) {}

		static IntPtr NewAttrFamily (string family)
		{
			IntPtr family_raw = GLib.Marshaller.StringToPtrGStrdup (family);
			IntPtr attr_raw = pango_attr_family_new (family_raw);
			GLib.Marshaller.Free (family_raw);
			return attr_raw;
		}

		internal AttrFamily (IntPtr raw) : base (raw) {}

		[DllImport("pangosharpglue-2")]
		static extern IntPtr pangosharp_attr_string_get_value (IntPtr raw);

		public string Family {
			get {
				IntPtr raw_family = pangosharp_attr_string_get_value (Handle);
				return GLib.Marshaller.Utf8PtrToString (raw_family);
			}
		}
	}
}

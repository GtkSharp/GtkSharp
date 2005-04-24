// Pango.AttrLanguage - Pango.Attribute for Pango.Language
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

	public class AttrLanguage : Attribute {

		[DllImport("libpango-1.0-0.dll")]
		static extern IntPtr pango_attr_language_new (IntPtr language);

		public AttrLanguage (Pango.Language language) : this (pango_attr_language_new (language.Handle)) {}

		internal AttrLanguage (IntPtr raw) : base (raw) {}

		[DllImport("pangosharpglue-2")]
		static extern IntPtr pangosharp_attr_language_get_value (IntPtr raw);

		public Pango.Language Language {
			get {
				IntPtr raw_ret = pangosharp_attr_language_get_value (Handle);
				return new Pango.Language (raw_ret);
			}
		}
	}
}

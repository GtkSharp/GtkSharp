// Pango.AttrUnderline - Pango.Attribute for Pango.Underline
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

	public class AttrUnderline : Attribute {
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_pango_attr_underline_new(Pango.Underline underline);
		static d_pango_attr_underline_new pango_attr_underline_new = FuncLoader.LoadFunction<d_pango_attr_underline_new>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Pango), "pango_attr_underline_new"));

		public AttrUnderline (Pango.Underline underline) : this (pango_attr_underline_new (underline)) {}

		internal AttrUnderline (IntPtr raw) : base (raw) {}

		public Pango.Underline Underline {
			get {
				return (Pango.Underline) (AttrInt.New (Handle).Value);
			}
		}
	}
}


// Pango.AttrScale - Pango.Attribute for font size scale
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

	public class AttrScale : Attribute {
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_pango_attr_scale_new(double scale);
		static d_pango_attr_scale_new pango_attr_scale_new = FuncLoader.LoadFunction<d_pango_attr_scale_new>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Pango), "pango_attr_scale_new"));

		public AttrScale (double scale) : this (pango_attr_scale_new (scale)) {}

		internal AttrScale (IntPtr raw) : base (raw) {}

		public double Scale {
			get {
				return AttrFloat.New (Handle).Value;
			}
		}
	}
}


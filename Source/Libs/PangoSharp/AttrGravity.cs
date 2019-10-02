// Pango.AttrGravity - Pango.Attribute for Gravity
//
// Copyright (c) 2007 Novell, Inc.
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

	public class AttrGravity : Attribute {
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_pango_attr_gravity_new(int gravity);
		static d_pango_attr_gravity_new pango_attr_gravity_new = FuncLoader.LoadFunction<d_pango_attr_gravity_new>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Pango), "pango_attr_gravity_new"));

		public AttrGravity (Gravity gravity) : this (pango_attr_gravity_new ((int) gravity)) {}

		internal AttrGravity (IntPtr raw) : base (raw) {}

		public Gravity Gravity {
			get {
				return (Gravity) (AttrInt.New (Handle).Value);
			}
		}
	}
}


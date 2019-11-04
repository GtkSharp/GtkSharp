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
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_pango_attr_family_new(IntPtr family);
		static d_pango_attr_family_new pango_attr_family_new = FuncLoader.LoadFunction<d_pango_attr_family_new>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Pango), "pango_attr_family_new"));

		public AttrFamily (string family) : base (NewAttrFamily (family)) {}

		static IntPtr NewAttrFamily (string family)
		{
			IntPtr family_raw = GLib.Marshaller.StringToPtrGStrdup (family);
			IntPtr attr_raw = pango_attr_family_new (family_raw);
			GLib.Marshaller.Free (family_raw);
			return attr_raw;
		}

		internal AttrFamily (IntPtr raw) : base (raw) {}

		new struct NativeStruct {
			Attribute.NativeStruct attr;
			public IntPtr value;
		}

		public string Family {
			get {
				NativeStruct native = (NativeStruct) Marshal.PtrToStructure (Handle, typeof (NativeStruct));
				return GLib.Marshaller.Utf8PtrToString (native.value);
			}
		}
	}
}


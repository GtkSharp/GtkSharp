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
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_pango_attr_language_new(IntPtr language);
		static d_pango_attr_language_new pango_attr_language_new = FuncLoader.LoadFunction<d_pango_attr_language_new>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Pango), "pango_attr_language_new"));

		public AttrLanguage (Pango.Language language) : this (pango_attr_language_new (language.Handle)) {}

		internal AttrLanguage (IntPtr raw) : base (raw) {}

		new struct NativeStruct {
			Attribute.NativeStruct attr;
			public IntPtr value;
		}

		public Pango.Language Language {
			get {
				NativeStruct native = (NativeStruct) Marshal.PtrToStructure (Handle, typeof (NativeStruct));
				return new Pango.Language (native.value);
			}
		}
	}
}


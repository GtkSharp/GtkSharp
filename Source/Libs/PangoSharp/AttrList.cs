// Pango.AttrList.cs - Pango AttrList customizations
//
// Authors:  Mike Kestner  <mkestner@novell.com>
//
// Copyright (c) 2008 Novell, Inc.
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

	public partial class AttrList {
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_pango_attribute_copy(IntPtr raw);
		static d_pango_attribute_copy pango_attribute_copy = FuncLoader.LoadFunction<d_pango_attribute_copy>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Pango), "pango_attribute_copy"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_pango_attr_list_insert(IntPtr raw, IntPtr attr);
		static d_pango_attr_list_insert pango_attr_list_insert = FuncLoader.LoadFunction<d_pango_attr_list_insert>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Pango), "pango_attr_list_insert"));

		public void Insert (Pango.Attribute attr) 
		{
			pango_attr_list_insert (Handle, pango_attribute_copy (attr.Handle));
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_pango_attr_list_insert_before(IntPtr raw, IntPtr attr);
		static d_pango_attr_list_insert_before pango_attr_list_insert_before = FuncLoader.LoadFunction<d_pango_attr_list_insert_before>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Pango), "pango_attr_list_insert_before"));

		public void InsertBefore (Pango.Attribute attr)
		{
			pango_attr_list_insert_before (Handle, pango_attribute_copy (attr.Handle));
		}
	}
}


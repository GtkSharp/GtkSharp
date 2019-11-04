// Pango.AttrIterator.cs - Pango AttrIterator class customizations
//
// Author: Mike Kestner  <mkestner@ximian.com>
//
// Copyright (c) 2004 Novell, Inc.
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

	public partial class AttrIterator {
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_pango_attr_iterator_get_font(IntPtr raw, IntPtr desc, out IntPtr language, out IntPtr extra_attrs);
		static d_pango_attr_iterator_get_font pango_attr_iterator_get_font = FuncLoader.LoadFunction<d_pango_attr_iterator_get_font>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Pango), "pango_attr_iterator_get_font"));

		public void GetFont (out Pango.FontDescription desc, out Pango.Language language, out Pango.Attribute[] extra_attrs)
		{
			desc = new FontDescription ();
			IntPtr language_handle, list_handle;
			pango_attr_iterator_get_font (Handle, desc.Handle, out language_handle, out list_handle);
			desc.Family = desc.Family; // change static string to allocated one
			language = language_handle == IntPtr.Zero ? null : new Language (language_handle);
			if (list_handle == IntPtr.Zero) {
				extra_attrs = new Pango.Attribute [0];
				return;
			}
			GLib.SList list = new GLib.SList (list_handle);
			extra_attrs = new Pango.Attribute [list.Count];
			int i = 0;
			foreach (IntPtr raw_attr in list)
				extra_attrs [i++] = Pango.Attribute.GetAttribute (raw_attr);
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_pango_attr_iterator_get_attrs(IntPtr raw);
		static d_pango_attr_iterator_get_attrs pango_attr_iterator_get_attrs = FuncLoader.LoadFunction<d_pango_attr_iterator_get_attrs>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Pango), "pango_attr_iterator_get_attrs"));

		public Pango.Attribute[] Attrs {
			get {
				IntPtr list_handle = pango_attr_iterator_get_attrs (Handle);
				if (list_handle == IntPtr.Zero)
					return new Pango.Attribute [0];
				GLib.SList list = new GLib.SList (list_handle);
				Pango.Attribute[] attrs = new Pango.Attribute [list.Count];
				int i = 0;
				foreach (IntPtr raw_attr in list)
					attrs [i++] = Pango.Attribute.GetAttribute (raw_attr);
				return attrs;
			}
		}
	}
}


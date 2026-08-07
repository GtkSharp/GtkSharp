// Pango.GlyphItem.cs - Pango GlyphItem class customizations
//
// Author: Mike Kestner  <mkestner@ximian.com>
//
// Copyright (c) 2004-2005 Novell, Inc.
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

	public partial struct GlyphItem {
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_pango_glyph_item_apply_attrs(ref Pango.GlyphItem raw, IntPtr text, IntPtr list);
		static d_pango_glyph_item_apply_attrs pango_glyph_item_apply_attrs = FuncLoader.LoadFunction<d_pango_glyph_item_apply_attrs>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Pango), "pango_glyph_item_apply_attrs"));

		public GlyphItem[] ApplyAttrs (string text, Pango.AttrList list)
		{
			IntPtr native_text = GLib.Marshaller.StringToPtrGStrdup (text);
			IntPtr list_handle = pango_glyph_item_apply_attrs (ref this, native_text, list.Handle);
			GLib.Marshaller.Free (native_text);
			if (list_handle == IntPtr.Zero)
				return new GlyphItem [0];
			GLib.SList item_list = new GLib.SList (list_handle, typeof (GlyphItem));
			GlyphItem[] result = new GlyphItem [item_list.Count];
			int i = 0;
			foreach (GlyphItem item in item_list)
				result [i++] = item;
			return result;
		}

		[Obsolete ("Replaced by Glyphs property")]
		public Pango.GlyphString glyphs {
			get { return Glyphs; }
		}

		[Obsolete ("Replaced by Item property")]
		public Pango.Item item {
			get { return Item; }
		}
	}
}


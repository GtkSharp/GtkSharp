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

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_pango_glyph_item_get_logical_widths(ref Pango.GlyphItem raw, IntPtr text, [In, Out] int[] logical_widths);
		static d_pango_glyph_item_get_logical_widths pango_glyph_item_get_logical_widths = FuncLoader.LoadFunction<d_pango_glyph_item_get_logical_widths>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Pango), "pango_glyph_item_get_logical_widths"));

		// One width per character of the item, not one width. Codegen bound the
		// array "out int", so pango wrote item.NumChars ints through a four-byte
		// slot and the caller was handed the first of them.
		public int[] GetLogicalWidths (string text)
		{
			int[] widths = new int [Item == null ? 0 : Item.NumChars];
			IntPtr native_text = GLib.Marshaller.StringToPtrGStrdup (text);
			pango_glyph_item_get_logical_widths (ref this, native_text, widths);
			GLib.Marshaller.Free (native_text);
			return widths;
		}

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_pango_glyph_item_letter_space(ref Pango.GlyphItem raw, IntPtr text, [In] Pango.LogAttr[] log_attrs, int letter_spacing);
		static d_pango_glyph_item_letter_space pango_glyph_item_letter_space = FuncLoader.LoadFunction<d_pango_glyph_item_letter_space>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Pango), "pango_glyph_item_letter_space"));

		// The two array arguments are indexed differently, which is the trap:
		// text is the whole paragraph (Item.Offset indexes into it) while
		// log_attrs starts at *this item's* first character, so a caller with a
		// paragraph-wide array has to slice it from Item.CharOffset. Spacing is
		// added once per cluster that the attributes call a cursor position, so
		// the width grows by exactly cluster count times letter_spacing.
		public void LetterSpace (string text, Pango.LogAttr[] log_attrs, int letter_spacing)
		{
			IntPtr native_text = GLib.Marshaller.StringToPtrGStrdup (text);
			pango_glyph_item_letter_space (ref this, native_text, log_attrs, letter_spacing);
			GLib.Marshaller.Free (native_text);
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


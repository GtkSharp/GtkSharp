// Pango.GlyphString.cs - Pango GlyphString class customizations
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

	public partial class GlyphString {

		// PangoGlyphString is { int num_glyphs; PangoGlyphInfo *glyphs;
		// int *log_clusters; int space; }. Both arrays are pointers whose length
		// is num_glyphs, so the generated field accessors would have handed back
		// a bare IntPtr and are hidden in the metadata -- which left the glyphs
		// a shaping run produced, and the character each one came from,
		// unreachable from managed code altogether.

		public Pango.GlyphInfo[] Glyphs {
			get {
				int count = NumGlyphs;
				IntPtr array_ptr = Marshal.ReadIntPtr (Handle, (int) abi_info.GetFieldOffset ("glyphs"));
				if (array_ptr == IntPtr.Zero || count <= 0)
					return new Pango.GlyphInfo [0];

				int size = Marshal.SizeOf<Pango.GlyphInfo> ();
				Pango.GlyphInfo[] result = new Pango.GlyphInfo [count];
				for (int i = 0; i < count; i++)
					result [i] = Pango.GlyphInfo.New (new IntPtr (array_ptr.ToInt64 () + i * size));
				return result;
			}
		}

		// One entry per glyph: the byte index, within the text that was shaped,
		// of the character this glyph belongs to. Two glyphs sharing a value are
		// one cluster.
		public int[] LogClusters {
			get {
				int count = NumGlyphs;
				IntPtr array_ptr = Marshal.ReadIntPtr (Handle, (int) abi_info.GetFieldOffset ("log_clusters"));
				if (array_ptr == IntPtr.Zero || count <= 0)
					return new int [0];

				int[] result = new int [count];
				Marshal.Copy (array_ptr, result, 0, count);
				return result;
			}
		}

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_pango_glyph_string_get_logical_widths2(IntPtr raw, IntPtr text, int length, int embedding_level, [In, Out] int[] logical_widths);
		static d_pango_glyph_string_get_logical_widths2 pango_glyph_string_get_logical_widths2 = FuncLoader.LoadFunction<d_pango_glyph_string_get_logical_widths2>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Pango), "pango_glyph_string_get_logical_widths"));

		// logical_widths holds one width per character of text. Codegen bound it
		// "out int" and returned that one int, so pango wrote the rest past the
		// end of a four-byte stack slot.
		public int[] GetLogicalWidths (string text, int embedding_level)
		{
			int[] widths = new int [Pango.Global.CharCount (text)];
			IntPtr native_text = GLib.Marshaller.StringToPtrGStrdup (text);
			pango_glyph_string_get_logical_widths2 (Handle, native_text, System.Text.Encoding.UTF8.GetByteCount (text), embedding_level, widths);
			GLib.Marshaller.Free (native_text);
			return widths;
		}

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_pango_glyph_string_index_to_x_full2(IntPtr raw, IntPtr text, int length, IntPtr analysis, [In] Pango.LogAttr[] attrs, int index_, bool trailing, out int x_pos);
		static d_pango_glyph_string_index_to_x_full2 pango_glyph_string_index_to_x_full2 = FuncLoader.LoadFunction<d_pango_glyph_string_index_to_x_full2>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Pango), "pango_glyph_string_index_to_x_full"));

		// attrs is an array of one log attr per character, read up to the index
		// asked about; a single struct left pango reading past the end of it.
		public int IndexToXFull (string text, Pango.Analysis analysis, Pango.LogAttr[] attrs, int index_, bool trailing)
		{
			int x_pos;
			IntPtr native_text = GLib.Marshaller.StringToPtrGStrdup (text);
			IntPtr native_analysis = GLib.Marshaller.StructureToPtrAlloc (analysis);
			pango_glyph_string_index_to_x_full2 (Handle, native_text, System.Text.Encoding.UTF8.GetByteCount (text), native_analysis, attrs, index_, trailing, out x_pos);
			GLib.Marshaller.Free (native_text);
			Marshal.FreeHGlobal (native_analysis);
			return x_pos;
		}

		[Obsolete("Pango.GlyphString is a reference type now, use null")]
		public static GlyphString Zero = null;

		[Obsolete("Replaced by GlyphString(IntPtr) constructor")]
		public static GlyphString New (IntPtr raw)
		{
			return new GlyphString (raw);
		}

		[Obsolete("Replaced by GlyphString() constructor")]
		public static GlyphString New ()
		{
			return new GlyphString ();
		}
	}
}

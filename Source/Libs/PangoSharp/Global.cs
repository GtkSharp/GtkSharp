// Pango.Global.cs - Pango Global class customizations
//
// Authors:  Mike Kestner  <mkestner@ximian.com>
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

	public partial class Global {

		[Obsolete]
		public static bool ScanInt(string pos, out int out_param) {
			IntPtr native = GLib.Marshaller.StringToPtrGStrdup (pos);
			bool raw_ret = pango_scan_int(ref native, out out_param);
			GLib.Marshaller.Free (native);
			bool ret = raw_ret;
			return ret;
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate bool d_pango_parse_markup(IntPtr markup, int length, uint accel_marker, out IntPtr attr_list_handle, out IntPtr text, out uint accel_char, IntPtr err);
		static d_pango_parse_markup pango_parse_markup = FuncLoader.LoadFunction<d_pango_parse_markup>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Pango), "pango_parse_markup"));

		public static bool ParseMarkup (string markup, char accel_marker, out Pango.AttrList attrs, out string text, out char accel_char)
		{
			uint ucs4_accel_char;
			IntPtr text_as_native;
			IntPtr attrs_handle;
			IntPtr native_markup = GLib.Marshaller.StringToPtrGStrdup (markup);
			bool result = pango_parse_markup (native_markup, -1, GLib.Marshaller.CharToGUnichar (accel_marker), out attrs_handle, out text_as_native, out ucs4_accel_char, IntPtr.Zero);
			GLib.Marshaller.Free (native_markup);
			accel_char = GLib.Marshaller.GUnicharToChar (ucs4_accel_char);
			text = GLib.Marshaller.Utf8PtrToString (text_as_native);
			attrs = new Pango.AttrList (attrs_handle);
			return result;
		}
		
		public static bool IsSupported => GLibrary.IsSupported(Library.Pango);

		// Pango's break functions all fill an array with one PangoLogAttr per
		// character plus one for the position after the last, and take its length
		// as a separate argument. The api.xml cannot describe that, so codegen
		// emitted "PangoLogAttr attrs, int attrs_len" and marshalled a *single*
		// four-byte struct -- pango then wrote attrs_len of them through it. A
		// heap overrun on every call, silent because pango_default_break ignores
		// attrs_len entirely (it is G_GNUC_UNUSED). All four are hidden in the
		// metadata and rebound here over a real array, sized by this binding so
		// that the caller cannot get the length wrong.

		// The number of Unicode code points, which is what g_utf8_strlen counts
		// and what pango sizes a log-attr array by. Not text.Length: a character
		// outside the BMP is two UTF-16 units and one code point.
		internal static int CharCount (string text)
		{
			if (text == null)
				return 0;
			int chars = 0;
			for (int i = 0; i < text.Length; i++)
				if (!Char.IsLowSurrogate (text [i]))
					chars++;
			return chars;
		}

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_pango_get_log_attrs2(IntPtr text, int length, int level, IntPtr language, [In, Out] Pango.LogAttr[] attrs, int attrs_len);
		static d_pango_get_log_attrs2 pango_get_log_attrs2 = FuncLoader.LoadFunction<d_pango_get_log_attrs2>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Pango), "pango_get_log_attrs"));

		public static Pango.LogAttr[] GetLogAttrs (string text, int level, Pango.Language language)
		{
			Pango.LogAttr[] attrs = new Pango.LogAttr [CharCount (text) + 1];
			IntPtr native_text = GLib.Marshaller.StringToPtrGStrdup (text);
			pango_get_log_attrs2 (native_text, System.Text.Encoding.UTF8.GetByteCount (text), level,
					      language == null ? IntPtr.Zero : language.Handle, attrs, attrs.Length);
			GLib.Marshaller.Free (native_text);
			return attrs;
		}

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_pango_default_break2(IntPtr text, int length, IntPtr analysis, [In, Out] Pango.LogAttr[] attrs, int attrs_len);
		static d_pango_default_break2 pango_default_break2 = FuncLoader.LoadFunction<d_pango_default_break2>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Pango), "pango_default_break"));

		public static Pango.LogAttr[] DefaultBreak (string text, Pango.Analysis analysis)
		{
			Pango.LogAttr[] attrs = new Pango.LogAttr [CharCount (text) + 1];
			IntPtr native_text = GLib.Marshaller.StringToPtrGStrdup (text);
			IntPtr native_analysis = GLib.Marshaller.StructureToPtrAlloc (analysis);
			pango_default_break2 (native_text, System.Text.Encoding.UTF8.GetByteCount (text), native_analysis, attrs, attrs.Length);
			GLib.Marshaller.Free (native_text);
			Marshal.FreeHGlobal (native_analysis);
			return attrs;
		}

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_pango_break2(IntPtr text, int length, IntPtr analysis, [In, Out] Pango.LogAttr[] attrs, int attrs_len);
		static d_pango_break2 pango_break2 = FuncLoader.LoadFunction<d_pango_break2>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Pango), "pango_break"));

		[Obsolete ("Use DefaultBreak followed by TailorBreak")]
		public static Pango.LogAttr[] Break (string text, Pango.Analysis analysis)
		{
			Pango.LogAttr[] attrs = new Pango.LogAttr [CharCount (text) + 1];
			IntPtr native_text = GLib.Marshaller.StringToPtrGStrdup (text);
			IntPtr native_analysis = GLib.Marshaller.StructureToPtrAlloc (analysis);
			pango_break2 (native_text, System.Text.Encoding.UTF8.GetByteCount (text), native_analysis, attrs, attrs.Length);
			GLib.Marshaller.Free (native_text);
			Marshal.FreeHGlobal (native_analysis);
			return attrs;
		}

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_pango_tailor_break2(IntPtr text, int length, IntPtr analysis, int offset, [In, Out] Pango.LogAttr[] attrs, int attrs_len);
		static d_pango_tailor_break2 pango_tailor_break2 = FuncLoader.LoadFunction<d_pango_tailor_break2>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Pango), "pango_tailor_break"));

		// Unlike the others this one adjusts attributes that are already there,
		// so the array is the caller's and is passed in as well as out.
		public static void TailorBreak (string text, Pango.Analysis analysis, int offset, Pango.LogAttr[] attrs)
		{
			if (attrs == null)
				throw new ArgumentNullException ("attrs");

			IntPtr native_text = GLib.Marshaller.StringToPtrGStrdup (text);
			IntPtr native_analysis = GLib.Marshaller.StructureToPtrAlloc (analysis);
			pango_tailor_break2 (native_text, System.Text.Encoding.UTF8.GetByteCount (text), native_analysis, offset, attrs, attrs.Length);
			GLib.Marshaller.Free (native_text);
			Marshal.FreeHGlobal (native_analysis);
		}

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_pango_log2vis_get_embedding_levels2(IntPtr text, int length, ref int pbase_dir);
		static d_pango_log2vis_get_embedding_levels2 pango_log2vis_get_embedding_levels2 = FuncLoader.LoadFunction<d_pango_log2vis_get_embedding_levels2>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Pango), "pango_log2vis_get_embedding_levels"));

		// Returns a g_malloc'd guint8 per character, which the api.xml records as
		// "guint8* owned", and codegen bound as a plain byte: the low eight bits
		// of the array's address were handed back as the answer and the array was
		// leaked. Bidi levels are not readable from managed code at all that way.
		public static byte[] Log2visGetEmbeddingLevels (string text, ref Pango.Direction pbase_dir)
		{
			int native_pbase_dir = (int) pbase_dir;
			IntPtr native_text = GLib.Marshaller.StringToPtrGStrdup (text);
			IntPtr array_ptr = pango_log2vis_get_embedding_levels2 (native_text, System.Text.Encoding.UTF8.GetByteCount (text), ref native_pbase_dir);
			GLib.Marshaller.Free (native_text);
			pbase_dir = (Pango.Direction) native_pbase_dir;

			if (array_ptr == IntPtr.Zero)
				return new byte [0];

			byte[] result = new byte [CharCount (text)];
			Marshal.Copy (array_ptr, result, 0, result.Length);
			GLib.Marshaller.Free (array_ptr);
			return result;
		}
	}
}




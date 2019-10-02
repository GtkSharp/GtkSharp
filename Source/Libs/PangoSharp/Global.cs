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
	}
}




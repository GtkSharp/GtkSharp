// Pango.Layout.cs - Pango Layout class customizations
//
// Authors: Pedro Abelleira Seco <pedroabelleira@yahoo.es>
//          Mike Kestner  <mkestner@ximian.com>
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

	public partial class Layout {
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_pango_layout_get_lines(IntPtr raw);
		static d_pango_layout_get_lines pango_layout_get_lines = FuncLoader.LoadFunction<d_pango_layout_get_lines>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Pango), "pango_layout_get_lines"));

		public LayoutLine[] Lines {
			get {
				IntPtr list_ptr = pango_layout_get_lines(Handle);
				if (list_ptr == IntPtr.Zero)
					return new LayoutLine [0];
				GLib.SList list = new GLib.SList(list_ptr, typeof (IntPtr));
				LayoutLine[] result = new LayoutLine [list.Count];
				for (int i = 0; i < list.Count; i++)
					result[i] = new LayoutLine ((IntPtr)list[i]);

				return result;
			}
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_pango_layout_set_markup_with_accel(IntPtr raw, IntPtr markup, int length, uint accel_marker, out uint accel_char);
		static d_pango_layout_set_markup_with_accel pango_layout_set_markup_with_accel = FuncLoader.LoadFunction<d_pango_layout_set_markup_with_accel>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Pango), "pango_layout_set_markup_with_accel"));

		public void SetMarkupWithAccel (string markup, char accel_marker, out char accel_char)
		{
			uint ucs4_accel_char;
			IntPtr native_markup = GLib.Marshaller.StringToPtrGStrdup (markup);
			pango_layout_set_markup_with_accel (Handle, native_markup, -1, GLib.Marshaller.CharToGUnichar (accel_marker), out ucs4_accel_char);
			GLib.Marshaller.Free (native_markup);
			accel_char = GLib.Marshaller.GUnicharToChar (ucs4_accel_char);
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_pango_layout_get_log_attrs(IntPtr raw, out IntPtr attrs, out int n_attrs);
		static d_pango_layout_get_log_attrs pango_layout_get_log_attrs = FuncLoader.LoadFunction<d_pango_layout_get_log_attrs>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Pango), "pango_layout_get_log_attrs"));

		public LogAttr [] LogAttrs {
			get {
				int count;
				IntPtr array_ptr;
				pango_layout_get_log_attrs (Handle, out array_ptr, out count);
				if (array_ptr == IntPtr.Zero)
					return new LogAttr [0];
				LogAttr [] result = new LogAttr [count];
				for (int i = 0; i < count; i++) {
					IntPtr fam_ptr = Marshal.ReadIntPtr (array_ptr, i * IntPtr.Size);
					result [i] = LogAttr.New (fam_ptr);
				}

				GLib.Marshaller.Free (array_ptr);
				return result;
			}
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_pango_layout_set_text(IntPtr raw, IntPtr text, int length);
		static d_pango_layout_set_text pango_layout_set_text = FuncLoader.LoadFunction<d_pango_layout_set_text>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Pango), "pango_layout_set_text"));

		public void SetText (string text) 
		{
			IntPtr native_text = GLib.Marshaller.StringToPtrGStrdup (text);
			pango_layout_set_text (Handle, native_text, -1);
			GLib.Marshaller.Free (native_text);
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_pango_layout_set_markup(IntPtr raw, IntPtr markup, int length);
		static d_pango_layout_set_markup pango_layout_set_markup = FuncLoader.LoadFunction<d_pango_layout_set_markup>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Pango), "pango_layout_set_markup"));

		public void SetMarkup (string markup) 
		{
			IntPtr native_markup = GLib.Marshaller.StringToPtrGStrdup (markup);
			pango_layout_set_markup (Handle, native_markup, -1);
			GLib.Marshaller.Free (native_markup);
		}
	}
}


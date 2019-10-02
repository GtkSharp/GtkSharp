// Gdk.Text.cs - Custom implementation for Text class
//
// Authors: Mike Kestner <mkestner@ximian.com>
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


namespace Gdk {

	using System;
	using System.Runtime.InteropServices;

	public class TextProperty {
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate int d_gdk_text_property_to_utf8_list_for_display(IntPtr display, IntPtr encoding, int format, byte[] text, int length, out IntPtr list);
		static d_gdk_text_property_to_utf8_list_for_display gdk_text_property_to_utf8_list_for_display = FuncLoader.LoadFunction<d_gdk_text_property_to_utf8_list_for_display>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gdk), "gdk_text_property_to_utf8_list_for_display"));

		public static string[] ToStringListForDisplay (Gdk.Display display, Gdk.Atom encoding, int format, byte[] text, int length) 
		{
			IntPtr list_ptr;
			int count = gdk_text_property_to_utf8_list_for_display (display.Handle, encoding.Handle, format, text, length, out list_ptr);

			if (count == 0)
				return new string [0];

			string[] result = new string [count];
			for (int i = 0; i < count; i++) {
				IntPtr ptr = Marshal.ReadIntPtr (list_ptr, i * IntPtr.Size);
				result [i] = GLib.Marshaller.Utf8PtrToString (ptr);
			}
			GLib.Marshaller.StrFreeV (list_ptr);
			return result;
		}
	}
}


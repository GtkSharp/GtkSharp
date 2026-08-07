// GLib.GString.cs : Marshaler for GStrings 
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


namespace GLib {
	using System;
	using System.Runtime.InteropServices;
	
	public class GString : GLib.IWrapper {

		IntPtr handle;
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_string_free(IntPtr mem, bool free_segments);
		static d_g_string_free g_string_free = FuncLoader.LoadFunction<d_g_string_free>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_string_free"));

		~GString ()
		{
			g_string_free (handle, true);
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_string_new(IntPtr text);
		static d_g_string_new g_string_new = FuncLoader.LoadFunction<d_g_string_new>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_string_new"));

		public GString (string text) 
		{
			IntPtr native_text = Marshaller.StringToPtrGStrdup (text);
			handle = g_string_new (native_text);
			Marshaller.Free (native_text);
		}

		public IntPtr Handle {
			get {
				return handle;
			}
		}
		
		public static string PtrToString (IntPtr ptr) 
		{
			return Marshaller.Utf8PtrToString (ptr);
		}
	}
}



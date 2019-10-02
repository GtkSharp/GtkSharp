// GLib.FileUtils.cs - GFileUtils class implementation
//
// Author: Martin Baulig <martin@gnome.org>
//
// Copyright (c) 2002 Ximian, Inc
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

	public class FileUtils
	{
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate bool d_g_file_get_contents(IntPtr filename, out IntPtr contents, out int length, out IntPtr error);
		static d_g_file_get_contents g_file_get_contents = FuncLoader.LoadFunction<d_g_file_get_contents>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_file_get_contents"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate bool d_g_file_get_contents_utf8(IntPtr filename, out IntPtr contents, out int length, out IntPtr error);
		static d_g_file_get_contents_utf8 g_file_get_contents_utf8 = FuncLoader.LoadFunction<d_g_file_get_contents_utf8>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_file_get_contents_utf8"));

		public static string GetFileContents (string filename)
		{
			int length;
			IntPtr contents, error;
			IntPtr native_filename = Marshaller.StringToPtrGStrdup (filename);

			if (Global.IsWindowsPlatform) {
				if (!g_file_get_contents_utf8 (native_filename, out contents, out length, out error))
					throw new GException (error);
			} else {
				if (!g_file_get_contents (native_filename, out contents, out length, out error))
					throw new GException (error);
			}

			Marshaller.Free (native_filename);
			return Marshaller.Utf8PtrToString (contents);
		}

		private FileUtils () {}
	}
}


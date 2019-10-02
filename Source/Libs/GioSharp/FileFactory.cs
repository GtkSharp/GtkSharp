//
// FileFactory.cs
//
// Author(s):
//	Stephane Delcroix  <stephane@delcroix.org>
//
// Copyright (c) 2008 Stephane Delcroix
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

using System;
using System.Runtime.InteropServices;

namespace GLib
{
	public class FileFactory
	{
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_file_new_for_uri(string uri);
		static d_g_file_new_for_uri g_file_new_for_uri = FuncLoader.LoadFunction<d_g_file_new_for_uri>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gio), "g_file_new_for_uri"));

		public static IFile NewForUri (string uri)
		{
			return GLib.FileAdapter.GetObject (g_file_new_for_uri (uri), false) as IFile;
		}

		public static IFile NewForUri (Uri uri)
		{
			return GLib.FileAdapter.GetObject (g_file_new_for_uri (uri.ToString ()), false) as IFile;
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_file_new_for_path(string path);
		static d_g_file_new_for_path g_file_new_for_path = FuncLoader.LoadFunction<d_g_file_new_for_path>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gio), "g_file_new_for_path"));
		
		public static IFile NewForPath (string path)
		{
			return GLib.FileAdapter.GetObject (g_file_new_for_path (path), false) as IFile;
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_file_new_for_commandline_arg(string arg);
		static d_g_file_new_for_commandline_arg g_file_new_for_commandline_arg = FuncLoader.LoadFunction<d_g_file_new_for_commandline_arg>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gio), "g_file_new_for_commandline_arg"));

		public static IFile NewFromCommandlineArg (string arg)
		{
			return GLib.FileAdapter.GetObject (g_file_new_for_commandline_arg (arg), false) as IFile;
		}
	}
}


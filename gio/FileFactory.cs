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
		[DllImport ("libgio-2.0-0.dll")]
		private static extern IntPtr g_file_new_for_uri (string uri);

		public static File NewForUri (string uri)
		{
			return GLib.FileAdapter.GetObject (g_file_new_for_uri (uri), false) as File;
		}

		public static File NewForUri (Uri uri)
		{
			return GLib.FileAdapter.GetObject (g_file_new_for_uri (uri.ToString ()), false) as File;
		}

		[DllImport ("libgio-2.0-0.dll")]
		private static extern IntPtr g_file_new_for_path (string path);
		
		public static File NewForPath (string path)
		{
			return GLib.FileAdapter.GetObject (g_file_new_for_path (path), false) as File;	
		}

		[DllImport ("libgio-2.0-0.dll")]
		private static extern IntPtr g_file_new_for_commandline_arg (string arg);

		public static File NewFromCommandlineArg (string arg)
		{
			return GLib.FileAdapter.GetObject (g_file_new_for_commandline_arg (arg), false) as File;
		}
	}
}

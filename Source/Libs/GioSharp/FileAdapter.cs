// FileAdapter.cs - customizations to GLib.FileAdapter
//
// Authors: Stephane Delcroix  <stephane@delcroix.org>
//
// Copyright (c) 2008 Novell, Inc.
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
	
	public partial class FileAdapter {
		public override string ToString ()
		{
			return Uri.ToString ();
		}
		
		public bool Exists {
			get { return QueryExists (null); }
		}
		
		public bool Delete ()
		{
			return Delete (null);
		}
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_file_get_uri(IntPtr raw);
		static d_g_file_get_uri g_file_get_uri = FuncLoader.LoadFunction<d_g_file_get_uri>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gio), "g_file_get_uri"));
		
		public System.Uri Uri {
			get {
				IntPtr raw_ret = g_file_get_uri(Handle);
				string ret = GLib.Marshaller.PtrToStringGFree(raw_ret);
				return new System.Uri (ret);
			}
		}
	}
}


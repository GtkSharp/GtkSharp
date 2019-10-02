// AppInfoAdapter.cs - customizations to GLib.AppInfoAdapter
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
	
	public partial class AppInfoAdapter {
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_app_info_get_all();
		static d_g_app_info_get_all g_app_info_get_all = FuncLoader.LoadFunction<d_g_app_info_get_all>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gio), "g_app_info_get_all"));

		public static GLib.IAppInfo[] GetAll () {
			IntPtr raw_ret = g_app_info_get_all();
			GLib.IAppInfo[] ret = (GLib.IAppInfo[]) GLib.Marshaller.ListPtrToArray (raw_ret, typeof (GLib.List), true, false, typeof (GLib.IAppInfo));
			return ret;
		}
	}
}


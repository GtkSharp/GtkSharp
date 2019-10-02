// Copyright (c) 2005 Novell, Inc.
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

	public delegate void NotifyHandler (object o, NotifyArgs args);

	public class NotifyArgs : GLib.SignalArgs {
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_param_spec_get_name(IntPtr pspec);
		static d_g_param_spec_get_name g_param_spec_get_name = FuncLoader.LoadFunction<d_g_param_spec_get_name>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_param_spec_get_name"));

		public string Property {
			get {
				IntPtr raw_ret = g_param_spec_get_name ((IntPtr) Args[0]);
				return Marshaller.Utf8PtrToString (raw_ret);
			}
		}
	}
}


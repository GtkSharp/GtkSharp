// Pixdata.cs 
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

	public partial struct Pixdata {
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_gdk_pixdata_serialize(ref Gdk.Pixdata raw, out uint len);
		static d_gdk_pixdata_serialize gdk_pixdata_serialize = FuncLoader.LoadFunction<d_gdk_pixdata_serialize>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GdkPixbuf), "gdk_pixdata_serialize"));
	
		public byte [] Serialize () {
			uint len;
			IntPtr raw_ret = gdk_pixdata_serialize (ref this, out len);

			byte [] data = new byte [len];
			Marshal.Copy (raw_ret, data, 0, (int)len);
			GLib.Marshaller.Free (raw_ret);
			return data;
		}
	}
}



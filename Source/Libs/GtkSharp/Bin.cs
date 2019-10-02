// Gtk.Bin.cs - Gtk Bin class customizations
//
// Author: Mike Kestner <mkestner@ximian.com> 
//
// Copyright (C) 2004 Novell, Inc.
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

namespace Gtk {

	using System;
	using System.Runtime.InteropServices;

	public partial class Bin {
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_gtk_bin_get_child(IntPtr raw);
		static d_gtk_bin_get_child gtk_bin_get_child = FuncLoader.LoadFunction<d_gtk_bin_get_child>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_bin_get_child"));

		public new Gtk.Widget Child { 
			get {
				IntPtr raw_ret = gtk_bin_get_child(Handle);
				Gtk.Widget ret;
				if (raw_ret == IntPtr.Zero)
					ret = null;
				else
					ret = (Gtk.Widget) GLib.Object.GetObject(raw_ret);
				return ret;
			}
			set {
				GLib.Value val = new GLib.Value(value);
				SetProperty("child", val);
				val.Dispose ();
			}
		}
	}
}


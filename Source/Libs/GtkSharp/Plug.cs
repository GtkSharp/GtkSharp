// Gtk.Plug.cs - Gtk Plug class customizations
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

	public partial class Plug {
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_gtk_plug_new(UIntPtr socket_id);
		static d_gtk_plug_new gtk_plug_new = FuncLoader.LoadFunction<d_gtk_plug_new>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_plug_new"));

		public Plug (ulong socket_id) : base (IntPtr.Zero)
		{
			if (GetType () != typeof (Plug)) {
				CreateNativeObject (new string [0], new GLib.Value [0]);
				Construct (Convert.ToUInt32(socket_id));
				return;
			}
			Raw = gtk_plug_new (new UIntPtr (socket_id));
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_gtk_plug_new_for_display(IntPtr display, UIntPtr socket_id);
		static d_gtk_plug_new_for_display gtk_plug_new_for_display = FuncLoader.LoadFunction<d_gtk_plug_new_for_display>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_plug_new_for_display"));

		public Plug (Gdk.Display display, ulong socket_id) : base (IntPtr.Zero)
		{
			if (GetType () != typeof (Plug)) {
				CreateNativeObject (new string [0], new GLib.Value [0]);
				ConstructForDisplay (display, Convert.ToUInt32(socket_id));
				return;
			}
			Raw = gtk_plug_new_for_display (display.Handle, new UIntPtr (socket_id));
		}
	}
}


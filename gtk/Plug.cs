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

		[DllImport ("libgtk-win32-3.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr gtk_plug_new(UIntPtr socket_id);

		public Plug (ulong socket_id) : base (IntPtr.Zero)
		{
			if (GetType () != typeof (Plug)) {
				CreateNativeObject (new string [0], new GLib.Value [0]);
				Construct (socket_id);
				return;
			}
			Raw = gtk_plug_new (new UIntPtr (socket_id));
		}

		[DllImport ("libgtk-win32-3.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr gtk_plug_new_for_display(IntPtr display, UIntPtr socket_id);

		public Plug (Gdk.Display display, ulong socket_id) : base (IntPtr.Zero)
		{
			if (GetType () != typeof (Plug)) {
				CreateNativeObject (new string [0], new GLib.Value [0]);
				ConstructForDisplay (display, socket_id);
				return;
			}
			Raw = gtk_plug_new_for_display (display.Handle, new UIntPtr (socket_id));
		}
	}
}

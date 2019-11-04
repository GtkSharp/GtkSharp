// Gtk.Button.cs - Gtk Button class customizations
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

	public partial class Button {
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_gtk_button_new_from_stock(IntPtr stock_id);
		static d_gtk_button_new_from_stock gtk_button_new_from_stock = FuncLoader.LoadFunction<d_gtk_button_new_from_stock>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_button_new_from_stock"));

		public Button (string stock_id) : base (IntPtr.Zero)
		{
			if (GetType () != typeof (Button)) {
				GLib.Value[] vals = new GLib.Value [2];
				string[] names = new string [2];
				names [0] = "label";
				vals [0] = new GLib.Value (stock_id);
				names [1] = "use_stock";
				vals [1] = new GLib.Value (true);
				CreateNativeObject (names, vals);
				return;
			}
			IntPtr native = GLib.Marshaller.StringToPtrGStrdup (stock_id);
			Raw = gtk_button_new_from_stock (native);
			GLib.Marshaller.Free (native);
		}

		public Button (Widget widget) : this ()
		{
			Add (widget);
		}
	}
}


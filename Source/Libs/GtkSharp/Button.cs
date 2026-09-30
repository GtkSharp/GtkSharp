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
		delegate IntPtr d_button_new_with_label_ctor(IntPtr label);
		static d_button_new_with_label_ctor button_new_with_label_ctor = FuncLoader.LoadFunction<d_button_new_with_label_ctor>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_button_new_with_label"));

		// Gtk 3 read this string as a stock id and fell back to using it as a
		// label. Gtk 4 removed the stock registry along with
		// gtk_button_new_from_stock and the use_stock property, so the string is
		// simply the label -- which is what nearly every caller already meant.
		public Button (string label) : base (IntPtr.Zero)
		{
			if (GetType () != typeof (Button)) {
				GLib.Value[] vals = new GLib.Value [1];
				string[] names = new string [1];
				names [0] = "label";
				vals [0] = new GLib.Value (label);
				CreateNativeObject (names, vals);
				return;
			}
			IntPtr native = GLib.Marshaller.StringToPtrGStrdup (label);
			Raw = button_new_with_label_ctor (native);
			GLib.Marshaller.Free (native);
		}

		// Button(Widget): GtkContainer is gone; set Child instead.
	}
}


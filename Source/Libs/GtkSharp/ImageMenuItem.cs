// Gtk.ImageMenuItem.cs - Gtk ImageMenuItem class customizations
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

	public partial class ImageMenuItem {
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_gtk_image_menu_item_new_with_mnemonic(IntPtr label);
		static d_gtk_image_menu_item_new_with_mnemonic gtk_image_menu_item_new_with_mnemonic = FuncLoader.LoadFunction<d_gtk_image_menu_item_new_with_mnemonic>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_image_menu_item_new_with_mnemonic"));

		public ImageMenuItem (string label) : base (IntPtr.Zero)
		{
			if (GetType() != typeof (ImageMenuItem)) {
				CreateNativeObject (new string [0], new GLib.Value [0]);
				AccelLabel al = new AccelLabel ("");
				al.TextWithMnemonic = label;
				al.SetAlignment (0.0f, 0.5f);
				Add (al);
				al.AccelWidget = this;
				return;
			}

			IntPtr native = GLib.Marshaller.StringToPtrGStrdup (label);
			Raw = gtk_image_menu_item_new_with_mnemonic (native);
			GLib.Marshaller.Free (native);
		}
	}
}


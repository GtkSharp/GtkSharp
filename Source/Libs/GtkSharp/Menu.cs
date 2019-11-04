// Gtk.Menu.cs - Gtk Menu class customizations
//
// Author: John Luke <john.luke@gmail.com> 
//
// Copyright (C) 2004 John Luke
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

	public partial class Menu {

		[Obsolete("Replaced by overload without IntPtr argument")]
		public void Popup (Gtk.Widget parent_menu_shell, Gtk.Widget parent_menu_item, Gtk.MenuPositionFunc func, IntPtr data, uint button, uint activate_time) {
			Popup (parent_menu_shell, parent_menu_item, func, button, activate_time);
		}

		public void Popup ()
		{
			PopupAtPointer (null);
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_gtk_menu_set_screen(IntPtr raw, IntPtr screen);
		static d_gtk_menu_set_screen gtk_menu_set_screen = FuncLoader.LoadFunction<d_gtk_menu_set_screen>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_menu_set_screen"));

		public new Gdk.Screen Screen {
			get {
				return base.Screen;
			}
			set {
				gtk_menu_set_screen (Handle, value.Handle);
			}
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_gtk_menu_set_active(IntPtr raw, uint index_);
		static d_gtk_menu_set_active gtk_menu_set_active = FuncLoader.LoadFunction<d_gtk_menu_set_active>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_menu_set_active"));

		public void SetActive (uint index_)
		{
			gtk_menu_set_active (Handle, index_);
		}
	}
}


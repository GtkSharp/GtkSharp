// Gtk.Action.cs - Gtk Action class customizations
//
// Author: John Luke  <john.luke@gmail.com>
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

	public partial class Action {

		public Action (string name, string label) : this (name, label, null, null)
		{}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_gtk_action_get_proxies(IntPtr raw);
		static d_gtk_action_get_proxies gtk_action_get_proxies = FuncLoader.LoadFunction<d_gtk_action_get_proxies>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_action_get_proxies"));

		public Gtk.Widget[] Proxies {
			get {
				IntPtr raw_ret = gtk_action_get_proxies (Handle);
				GLib.SList list = new GLib.SList (raw_ret);
				Widget[] result = new Widget [list.Count];
				for (int i = 0; i < list.Count; i++)
					result [i] = list [i] as Widget;
				return result;
			}
		}
	}
}


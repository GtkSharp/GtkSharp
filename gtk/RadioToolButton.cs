// Gtk.RadioToolButton.cs - Gtk RadioToolButton class customizations
//
// Author: Mike Kestner <mkestner@novell.com> 
//
// Copyright (c) 2006 Novell, Inc. 
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

	public partial class RadioToolButton {

		[DllImport ("libgtk-win32-3.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr gtk_radio_tool_button_new (IntPtr group);

		public RadioToolButton (RadioToolButton[] group) : base (IntPtr.Zero)
		{
			if (GetType () != typeof (RadioToolButton)) {
				CreateNativeObject (new string [0], new GLib.Value [0]);
				Group = group;
				return;
			}
			IntPtr native_group = IntPtr.Zero;
			if (group != null) {
				GLib.List list = new GLib.List(IntPtr.Zero);
				foreach (RadioToolButton item in group) {
					list.Append (item.Handle);
				}
				native_group = list.Handle;
			}
			Raw = gtk_radio_tool_button_new(native_group);
		}

		[DllImport ("libgtk-win32-3.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr gtk_radio_tool_button_new_from_stock (IntPtr group, IntPtr stock_id);

		public RadioToolButton (RadioToolButton[] group, string stock_id) : base (IntPtr.Zero)
		{
			if (GetType () != typeof (RadioToolButton)) {
				GLib.Value[] vals = new GLib.Value [1];
				string[] names = { "stock_id" };
				vals [0] = new GLib.Value (stock_id);
				CreateNativeObject (names, vals);
				Group = group;
				return;
			}
			IntPtr stock_id_as_native = GLib.Marshaller.StringToPtrGStrdup (stock_id);
			IntPtr native_group = IntPtr.Zero;
			if (group != null) {
				GLib.List list = new GLib.List(IntPtr.Zero);
				foreach (RadioToolButton item in group) {
					list.Append (item.Handle);
				}
				native_group = list.Handle;
			}
			Raw = gtk_radio_tool_button_new_from_stock(native_group, stock_id_as_native);
			GLib.Marshaller.Free (stock_id_as_native);
		}

		[DllImport("libgtk-win32-3.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr gtk_radio_tool_button_get_group(IntPtr raw);

		[DllImport ("libgtk-win32-3.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern void gtk_radio_tool_button_set_group(IntPtr raw, IntPtr list);

		[GLib.Property ("group")]
		public RadioToolButton[] Group {
			get  {
				IntPtr raw_ret = gtk_radio_tool_button_get_group(Handle);
				RadioToolButton[] ret = (RadioToolButton[]) GLib.Marshaller.ListPtrToArray (raw_ret, typeof(GLib.SList), false, false, typeof(RadioToolButton));
				return ret;
			}
			set {
				IntPtr native_group = IntPtr.Zero;
				if (value != null) {
					GLib.List list = new GLib.List(IntPtr.Zero);
					foreach (RadioToolButton item in value) {
						list.Append (item.Handle);
					}
					native_group = list.Handle;
				}
				gtk_radio_tool_button_set_group(Handle, native_group);
			}
		}
	}
}

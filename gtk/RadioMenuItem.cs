//  Gtk.RadioMenuItem.cs - Gtk RadioMenuItem customizations
//
//  Authors:  John Luke  <jluke@cfl.rr.com>
//            Mike Kestner <mkestner@ximian.com>
//
//  Copyright (c) 2004 Novell, Inc.
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

	public partial class RadioMenuItem {

		public RadioMenuItem (string label) : base (IntPtr.Zero)
		{
			if (GetType() != typeof (RadioMenuItem)) {
				CreateNativeObject (new string [0], new GLib.Value [0]);
				AccelLabel al = new AccelLabel ("");
				al.TextWithMnemonic = label;
				al.SetAlignment (0.0f, 0.5f);
				Add (al);
				al.AccelWidget = this;
				return;
			}

			IntPtr label_as_native = GLib.Marshaller.StringToPtrGStrdup (label);
			Raw = gtk_radio_menu_item_new_with_mnemonic (IntPtr.Zero, label_as_native);
			GLib.Marshaller.Free (label_as_native);
		}

		[DllImport ("libgtk-win32-3.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr gtk_radio_menu_item_new_with_mnemonic(IntPtr group, IntPtr label);

		public RadioMenuItem (RadioMenuItem[] group, string label) : base (IntPtr.Zero)
		{
			if (GetType () != typeof (RadioMenuItem)) {
				CreateNativeObject (new string [0], new GLib.Value [0]);
				AccelLabel al = new AccelLabel ("");
				al.TextWithMnemonic = label;
				al.SetAlignment (0.0f, 0.5f);
				Add (al);
				al.AccelWidget = this;
				Group = group;
				return;
			}
			IntPtr native_label = GLib.Marshaller.StringToPtrGStrdup (label);
			IntPtr native_group = IntPtr.Zero;
			if (group != null) {
				GLib.List list = new GLib.List(IntPtr.Zero);
				foreach (RadioMenuItem item in group) {
					list.Append (item.Handle);
				}
				native_group = list.Handle;
			}
			Raw = gtk_radio_menu_item_new_with_mnemonic(native_group, native_label);
			GLib.Marshaller.Free (native_label);
		}

		[DllImport("libgtk-win32-3.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr gtk_radio_menu_item_get_group(IntPtr raw);

		[DllImport ("libgtk-win32-3.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern void gtk_radio_menu_item_set_group(IntPtr raw, IntPtr list);

		[GLib.Property ("group")]
		public RadioMenuItem[] Group {
			get  {
				IntPtr raw_ret = gtk_radio_menu_item_get_group(Handle);
				RadioMenuItem[] ret = (RadioMenuItem[]) GLib.Marshaller.ListPtrToArray (raw_ret, typeof(GLib.SList), false, false, typeof(RadioMenuItem));
				return ret;
			}
			set {
				IntPtr native_group = IntPtr.Zero;
				if (value != null) {
					GLib.List list = new GLib.List(IntPtr.Zero);
					foreach (RadioMenuItem item in value) {
						list.Append (item.Handle);
					}
					native_group = list.Handle;
				}
				gtk_radio_menu_item_set_group(Handle, native_group);
			}
		}
	}
}

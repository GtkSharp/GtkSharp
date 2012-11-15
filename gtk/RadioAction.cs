// Gtk.RadioAction.cs - Gtk RadioAction class customizations
//
// Author: Bertrand Lorentz <bertrand.lorentz@gmail.com>
//
// Copyright (c) 2006 Bertrand Lorentz
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

namespace Gtk
{
	using System;
	using System.Runtime.InteropServices;

	public partial class RadioAction
	{
		[DllImport("libgtk-win32-3.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr gtk_radio_action_get_group(IntPtr raw);

		[DllImport ("libgtk-win32-3.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern void gtk_radio_action_set_group(IntPtr raw, IntPtr list);

		[GLib.Property ("group")]
		public RadioAction[] Group {
			get  {
				IntPtr raw_ret = gtk_radio_action_get_group(Handle);
				RadioAction[] ret = (RadioAction[]) GLib.Marshaller.ListPtrToArray (raw_ret, typeof(GLib.SList), false, false, typeof(RadioAction));
				return ret;
			}
			set {
				IntPtr native_group = IntPtr.Zero;
				if (value != null) {
					GLib.List list = new GLib.List(IntPtr.Zero);
					foreach (RadioAction item in value) {
						list.Append (item.Handle);
					}
					native_group = list.Handle;
				}
				gtk_radio_action_set_group(Handle, native_group);
			}
		}
	}
}

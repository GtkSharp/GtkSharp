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

	public partial class MessageDialog {

		[DllImport ("libgtk-win32-3.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr gtk_message_dialog_new (IntPtr parent_window, DialogFlags flags, MessageType type, ButtonsType bt, IntPtr msg, IntPtr args);

		[DllImport ("libgtk-win32-3.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr gtk_message_dialog_new_with_markup (IntPtr parent_window, DialogFlags flags, MessageType type, ButtonsType bt, IntPtr msg, IntPtr args);

		public MessageDialog (Gtk.Window parent_window, DialogFlags flags, MessageType type, ButtonsType bt, bool use_markup, string format, params object[] args)
		{
			IntPtr p = (parent_window != null) ? parent_window.Handle : IntPtr.Zero;

			if (format == null) {
				Raw = gtk_message_dialog_new (p, flags, type, bt, IntPtr.Zero, IntPtr.Zero);
				return;
			}

			IntPtr nmsg = GLib.Marshaller.StringToPtrGStrdup (GLib.Marshaller.StringFormat (format, args));
			if (use_markup)
				Raw = gtk_message_dialog_new_with_markup (p, flags, type, bt, nmsg, IntPtr.Zero);
			else
				Raw = gtk_message_dialog_new (p, flags, type, bt, nmsg, IntPtr.Zero);
			GLib.Marshaller.Free (nmsg);
		}

		public MessageDialog (Gtk.Window parent_window, DialogFlags flags, MessageType type, ButtonsType bt, string format, params object[] args) : this (parent_window, flags, type, bt, true, format, args) {}

	}
}

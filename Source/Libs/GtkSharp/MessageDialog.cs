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
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_gtk_message_dialog_new(IntPtr parent_window, DialogFlags flags, MessageType type, ButtonsType bt, IntPtr msg, IntPtr args);
		static d_gtk_message_dialog_new gtk_message_dialog_new = FuncLoader.LoadFunction<d_gtk_message_dialog_new>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_message_dialog_new"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_gtk_message_dialog_new_with_markup(IntPtr parent_window, DialogFlags flags, MessageType type, ButtonsType bt, IntPtr msg, IntPtr args);
		static d_gtk_message_dialog_new_with_markup gtk_message_dialog_new_with_markup = FuncLoader.LoadFunction<d_gtk_message_dialog_new_with_markup>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_message_dialog_new_with_markup"));

		public MessageDialog (Gtk.Window parent_window, DialogFlags flags, MessageType type, ButtonsType bt, bool use_markup, string format, params object[] args) : base (IntPtr.Zero)
		{
			IntPtr p = (parent_window != null) ? parent_window.Handle : IntPtr.Zero;

			if (format == null) {
				Raw = gtk_message_dialog_new (p, flags, type, bt, IntPtr.Zero, IntPtr.Zero);
				return;
			}

			// String.Format, not Marshaller.StringFormat. The latter doubles every
			// per cent sign, which was the right thing to do while the composed
			// message was being handed to Gtk as message_format -- printf would
			// turn "%%" back into "%". Now that the format is a literal "%s" and
			// the message is the argument behind it, nothing will un-double them,
			// and the dialog would read "100%% complete".
			//
			// Passing text as data rather than as a format is the more robust of
			// the two arrangements: it cannot be got wrong by a message that
			// happens to contain a conversion this escaping did not anticipate.
			IntPtr nmsg = GLib.Marshaller.StringToPtrGStrdup (String.Format (format, args));
			IntPtr nformat = GLib.Marshaller.StringToPtrGStrdup ("%s");

			if (use_markup)
				Raw = gtk_message_dialog_new_with_markup (p, flags, type, bt, nformat, nmsg);
			else
				Raw = gtk_message_dialog_new (p, flags, type, bt, nformat, nmsg);

			GLib.Marshaller.Free (nmsg);
			GLib.Marshaller.Free (nformat);
		}

		public MessageDialog (Gtk.Window parent_window, DialogFlags flags, MessageType type, ButtonsType bt, string format, params object[] args) : this (parent_window, flags, type, bt, true, format, args) {}

	}
}


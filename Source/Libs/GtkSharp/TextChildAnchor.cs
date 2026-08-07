// TextChildAnchor.cs - customizations to Gtk.TextChildAnchor
//
// Authors: Mike Kestner  <mkestner@ximian.com>
//
// Copyright (c) 2004 Novell, Inc.
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

	public partial class TextChildAnchor {
		// Gtk 4 changed both halves of this call: it takes an out-parameter for
		// the count, and it returns a GtkWidget** array rather than a GList*.
		// The Gtk 3 shape - one argument, wrapped in a GLib.List - therefore
		// left the callee writing the count through whatever happened to be in
		// the second argument register, and then read an array of pointers as
		// though it were a linked list.
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_gtk_text_child_anchor_get_widgets(IntPtr raw, out uint out_len);
		static d_gtk_text_child_anchor_get_widgets gtk_text_child_anchor_get_widgets = FuncLoader.LoadFunction<d_gtk_text_child_anchor_get_widgets>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_text_child_anchor_get_widgets"));

		public Widget[] Widgets {
			get {
				uint len;
				IntPtr raw_ret = gtk_text_child_anchor_get_widgets (Handle, out len);
				if (raw_ret == IntPtr.Zero)
					return new Widget [0];

				Widget[] result = new Widget [len];
				for (int i = 0; i < result.Length; i++)
					result [i] = GLib.Object.GetObject (Marshal.ReadIntPtr (raw_ret, i * IntPtr.Size)) as Widget;

				// (transfer container): the array is ours to free, the widgets are not.
				GLib.Marshaller.Free (raw_ret);
				return result;
			}
		}
	}
}


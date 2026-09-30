// Gtk.PrintSettings.cs - the page-range array the api.xml cannot describe
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

	public partial class PrintSettings {

		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		delegate IntPtr d_gtk_print_settings_get_page_ranges (IntPtr raw, out int num_ranges);
		static d_gtk_print_settings_get_page_ranges gtk_print_settings_get_page_ranges = FuncLoader.LoadFunction<d_gtk_print_settings_get_page_ranges> (FuncLoader.GetProcAddress (GLibrary.Load (Library.Gtk), "gtk_print_settings_get_page_ranges"));

		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		delegate void d_gtk_print_settings_set_page_ranges (IntPtr raw, Gtk.PageRange[] page_ranges, int num_ranges);
		static d_gtk_print_settings_set_page_ranges gtk_print_settings_set_page_ranges = FuncLoader.LoadFunction<d_gtk_print_settings_set_page_ranges> (FuncLoader.GetProcAddress (GLibrary.Load (Library.Gtk), "gtk_print_settings_set_page_ranges"));

		// gtk_print_settings_get_page_ranges returns "GtkPageRange *" plus a
		// count, transfer full. The api.xml can say neither, so codegen bound the
		// return value as one GtkPageRange: every range after the first was
		// unreachable, and the g_malloc'd block was leaked on every call.
		public Gtk.PageRange[] GetPageRanges ()
		{
			int count;
			IntPtr array_ptr = gtk_print_settings_get_page_ranges (Handle, out count);
			if (array_ptr == IntPtr.Zero)
				return new Gtk.PageRange [0];

			try {
				if (count <= 0)
					return new Gtk.PageRange [0];

				Gtk.PageRange[] result = new Gtk.PageRange [count];
				int size = Marshal.SizeOf (typeof (Gtk.PageRange));
				for (int i = 0; i < count; i++)
					result [i] = Gtk.PageRange.New (new IntPtr (array_ptr.ToInt64 () + i * size));
				return result;
			} finally {
				GLib.Marshaller.Free (array_ptr);
			}
		}

		// The mirror image: Gtk reads num_ranges elements out of the pointer it
		// is handed, so passing one marshalled struct with a count above one read
		// past the end of it.
		public void SetPageRanges (Gtk.PageRange[] page_ranges)
		{
			// A zero-length managed array does not necessarily marshal to a
			// readable address, so an empty set of ranges is sent as one unread
			// element rather than as a pointer Gtk might dereference.
			if (page_ranges == null || page_ranges.Length == 0) {
				gtk_print_settings_set_page_ranges (Handle, new Gtk.PageRange [1], 0);
				return;
			}

			gtk_print_settings_set_page_ranges (Handle, page_ranges, page_ranges.Length);
		}
	}
}

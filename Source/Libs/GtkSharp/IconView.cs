// IconView.cs - customizations to Gtk.IconView
//
// Authors: Mike Kestner  <mkestner@ximian.com>
//
// Copyright (c) 2005 Novell, Inc.
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

	public partial class IconView {

		public void SetAttributes (CellRenderer cell, params object[] attrs)
		{
			if (attrs.Length % 2 != 0)
				throw new ArgumentException ("attrs should contain pairs of attribute/col");

			ClearAttributes (cell);
			for (int i = 0; i < attrs.Length - 1; i += 2) {
				AddAttribute (cell, (string) attrs [i], (int) attrs [i + 1]);
			}
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_gtk_icon_view_scroll_to_path(IntPtr raw, IntPtr path, bool use_align, float row_align, float col_align);
		static d_gtk_icon_view_scroll_to_path gtk_icon_view_scroll_to_path = FuncLoader.LoadFunction<d_gtk_icon_view_scroll_to_path>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_icon_view_scroll_to_path"));

		public void ScrollToPath (Gtk.TreePath path) 
		{
			gtk_icon_view_scroll_to_path(Handle, path == null ? IntPtr.Zero : path.Handle, false, 0.0f, 0.0f);
		}

		public void ScrollToPath (Gtk.TreePath path, float row_align, float col_align) 
		{
			gtk_icon_view_scroll_to_path(Handle, path == null ? IntPtr.Zero : path.Handle, true, row_align, col_align);
		}
	}
}


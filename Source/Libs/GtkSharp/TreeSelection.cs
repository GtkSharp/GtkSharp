// TreeSelection.cs - customizations to Gtk.TreeSelection
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

	public partial class TreeSelection {
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_gtk_tree_selection_get_selected_rows2(IntPtr raw, IntPtr model);
		static d_gtk_tree_selection_get_selected_rows2 gtk_tree_selection_get_selected_rows2 = FuncLoader.LoadFunction<d_gtk_tree_selection_get_selected_rows2>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_tree_selection_get_selected_rows"));

		public TreePath[] GetSelectedRows ()
		{
			IntPtr list_ptr = gtk_tree_selection_get_selected_rows2 (Handle, IntPtr.Zero);
			if (list_ptr == IntPtr.Zero)
				return new TreePath [0];

			GLib.List list = new GLib.List (list_ptr, typeof (Gtk.TreePath));
			return (TreePath[]) GLib.Marshaller.ListToArray (list, typeof (Gtk.TreePath));
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate bool d_gtk_tree_selection_get_selected2(IntPtr raw, IntPtr model, out Gtk.TreeIter iter);
		static d_gtk_tree_selection_get_selected2 gtk_tree_selection_get_selected2 = FuncLoader.LoadFunction<d_gtk_tree_selection_get_selected2>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_tree_selection_get_selected"));
		
		public bool GetSelected (out Gtk.TreeIter iter)
		{
			return gtk_tree_selection_get_selected2 (Handle, IntPtr.Zero, out iter);
		}
	}
}


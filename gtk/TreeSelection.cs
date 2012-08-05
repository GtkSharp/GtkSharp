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

		[DllImport ("libgtk-win32-3.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr gtk_tree_selection_get_selected_rows (IntPtr raw, IntPtr model);

		public TreePath[] GetSelectedRows ()
		{
			IntPtr list_ptr = gtk_tree_selection_get_selected_rows (Handle, IntPtr.Zero);
			if (list_ptr == IntPtr.Zero)
				return new TreePath [0];

			GLib.List list = new GLib.List (list_ptr, typeof (Gtk.TreePath));
			return (TreePath[]) GLib.Marshaller.ListToArray (list, typeof (Gtk.TreePath));
		}

		[DllImport ("libgtk-win32-3.0-0.dll", EntryPoint="gtk_tree_selection_get_selected", CallingConvention = CallingConvention.Cdecl)]
		static extern bool gtk_tree_selection_get_selected_without_model (IntPtr raw, IntPtr model, out Gtk.TreeIter iter);
		
		public bool GetSelected (out Gtk.TreeIter iter)
		{
			return gtk_tree_selection_get_selected_without_model (Handle, IntPtr.Zero, out iter);
		}
	}
}

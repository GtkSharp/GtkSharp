// Gtk.TreeViewColumn.cs - Gtk TreeViewColumn class customizations
//
// Author: Rachel Hestilow <hestilow@ximian.com> 
//
// Copyright (c) 2003 Rachel Hestilow 
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

	public partial class TreeViewColumn {

		public void SetAttributes (CellRenderer cell, params object[] attrs)
		{
			if (attrs.Length % 2 != 0)
				throw new ArgumentException ("attrs should contain pairs of attribute/col");

			ClearAttributes (cell);
			for (int i = 0; i < attrs.Length - 1; i += 2) {
				AddAttribute (cell, (string) attrs [i], (int) attrs [i + 1]);
			}
		}

		private void _NewWithAttributes (string title, Gtk.CellRenderer cell, Array attrs) {
			Title = title;
			PackStart (cell, true);
			for (int i = 0; (i + 1) < attrs.Length; i += 2) {
				AddAttribute (cell, (string) ((object[])attrs)[i], (int)((object[])attrs)[i + 1]);
			}
		}

		public TreeViewColumn (string title, Gtk.CellRenderer cell, Array attrs) : this ()
		{
			_NewWithAttributes (title, cell, attrs);
		}
		
		public TreeViewColumn (string title, Gtk.CellRenderer cell, params object[] attrs) : this ()
		{
			_NewWithAttributes (title, cell, attrs);
		}

		public void SetCellDataFunc (CellRenderer cell_renderer, NodeCellDataFunc func)
		{
			if (func == null) {
				gtk_tree_view_column_set_cell_data_func (Handle, cell_renderer == null ? IntPtr.Zero : cell_renderer.Handle, (GtkSharp.TreeCellDataFuncNative) null, IntPtr.Zero, null);
				return;
			}

			NodeCellDataFuncWrapper func_wrapper = new NodeCellDataFuncWrapper (func);
			GCHandle gch = GCHandle.Alloc (func_wrapper);
			gtk_cell_layout_set_cell_data_func (Handle, cell_renderer == null ? IntPtr.Zero : cell_renderer.Handle, func_wrapper.NativeDelegate, (IntPtr) gch, GLib.DestroyHelper.NotifyHandler);
		}
	}
}

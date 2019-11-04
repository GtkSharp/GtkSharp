// Gtk.TreeView.cs - Gtk TreeView class customizations
//
// Authors:
//	Kristian Rietveld <kris@gtk.org>
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// Copyright (c) 2002 Kristian Rietveld
// Copyright (c) 2003 Ximian, Inc. (http://www.ximian.com)
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

	public partial class TreeView {

		/*
		public Gdk.Color OddRowColor {
			get {
				GLib.Value value = StyleGetPropertyValue ("odd-row-color");
				Gdk.Color ret = (Gdk.Color)value;
				value.Dispose ();
				return ret;
			}
		}

		public Gdk.Color EvenRowColor {
			get {
				GLib.Value value = StyleGetPropertyValue ("even-row-color");
				Gdk.Color ret = (Gdk.Color)value;
				value.Dispose ();
				return ret;
			}
		}
		*/
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate bool d_gtk_tree_view_get_path_at_pos(IntPtr raw, int x, int y, out IntPtr path, out IntPtr column, out int cell_x, out int cell_y);
		static d_gtk_tree_view_get_path_at_pos gtk_tree_view_get_path_at_pos = FuncLoader.LoadFunction<d_gtk_tree_view_get_path_at_pos>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_tree_view_get_path_at_pos"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate bool d_gtk_tree_view_get_path_at_pos2(IntPtr raw, int x, int y, out IntPtr path, out IntPtr column, IntPtr cell_x, IntPtr cell_y);
		static d_gtk_tree_view_get_path_at_pos2 gtk_tree_view_get_path_at_pos2 = FuncLoader.LoadFunction<d_gtk_tree_view_get_path_at_pos2>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_tree_view_get_path_at_pos"));

		public bool GetPathAtPos (int x, int y, out Gtk.TreePath path, out Gtk.TreeViewColumn column, out int cell_x, out int cell_y)
		{
			IntPtr pathHandle;
			IntPtr columnHandle;
			bool raw_ret = gtk_tree_view_get_path_at_pos (Handle, x, y, out pathHandle, out columnHandle, out cell_x, out cell_y);
			if (raw_ret) {
				column = (Gtk.TreeViewColumn) GLib.Object.GetObject (columnHandle, false);
				path = (Gtk.TreePath) GLib.Opaque.GetOpaque (pathHandle, typeof (Gtk.TreePath), true);
			} else {
				path = null;
				column = null;
			}

			return raw_ret;
		}


		public bool GetPathAtPos (int x, int y, out Gtk.TreePath path)
		{
			IntPtr pathHandle;
			IntPtr columnHandle;
			bool raw_ret = gtk_tree_view_get_path_at_pos2 (Handle, x, y, out pathHandle, out columnHandle, IntPtr.Zero, IntPtr.Zero);
			if (raw_ret)
				path = (Gtk.TreePath) GLib.Opaque.GetOpaque (pathHandle, typeof (Gtk.TreePath), true);
			else
				path = null;

			return raw_ret;
		}

		public bool GetPathAtPos (int x, int y, out Gtk.TreePath path, out Gtk.TreeViewColumn column)
		{
			IntPtr pathHandle;
			IntPtr columnHandle;
			bool raw_ret = gtk_tree_view_get_path_at_pos2 (Handle, x, y, out pathHandle, out columnHandle, IntPtr.Zero, IntPtr.Zero);
			if (raw_ret) {
				path = (Gtk.TreePath) GLib.Opaque.GetOpaque (pathHandle, typeof (Gtk.TreePath), true);
				column = (Gtk.TreeViewColumn) GLib.Object.GetObject (columnHandle, false);
			} else {
				path = null;
				column = null;
			}

			return raw_ret;
		}

		public TreeViewColumn AppendColumn (string title, CellRenderer cell, TreeCellDataFunc cell_data) 
		{
			Gtk.TreeViewColumn col = new Gtk.TreeViewColumn ();
			col.Title = title;
			col.PackStart (cell, true);
			col.SetCellDataFunc (cell, cell_data);
			
			AppendColumn (col);
			return col;
		}
		
		public TreeViewColumn AppendColumn (string title, CellRenderer cell, CellLayoutDataFunc cell_data) {
			Gtk.TreeViewColumn col = new Gtk.TreeViewColumn ();
			col.Title = title;
			col.PackStart (cell, true);
			col.SetCellDataFunc (cell, cell_data);
			
			AppendColumn (col);
			return col;
		}
		
		public Gtk.TreeViewColumn AppendColumn (string title, Gtk.CellRenderer cell, params object[] attrs) {
			Gtk.TreeViewColumn col = new Gtk.TreeViewColumn (title, cell, attrs);
			AppendColumn (col);
			return col;
		}

		public int InsertColumn (int pos, string title, CellRenderer cell, CellLayoutDataFunc cell_data) 
		{
			TreeViewColumn col = new TreeViewColumn ();
			col.Title = title;
			col.PackStart (cell, true);
			col.SetCellDataFunc (cell, cell_data);
			return InsertColumn (col, pos);
		}
		
		public int InsertColumn (int pos, string title, CellRenderer cell, params object[] attrs) 
		{
			TreeViewColumn col = new TreeViewColumn (title, cell, attrs);
			return InsertColumn (col, pos);
		}
	}
}


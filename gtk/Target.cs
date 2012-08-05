// Gtk.Target.cs - Gtk.Target class customizations
//
// Author: Christian Hoff  <christian_hoff@gmx.net>
//
// Copyright (c) 2009 Christian Hoff
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

	public partial class Target {

		[DllImport ("libgtk-win32-3.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr gtk_target_table_new_from_list(IntPtr list, out int n_targets);

		public static Gtk.TargetEntry[] TableNewFromList(Gtk.TargetList list) {
			int n_targets;
			IntPtr array_ptr = gtk_target_table_new_from_list (list.Handle, out n_targets);

			Gtk.TargetEntry[] ret = new Gtk.TargetEntry [n_targets];
			int unmanaged_struct_size = Marshal.SizeOf (typeof (Gtk.TargetEntry));
			for (int i = 0; i < n_targets; i++) {
				ret [i] = Gtk.TargetEntry.New (new IntPtr (array_ptr.ToInt64 () + i * unmanaged_struct_size));
			}
			return ret;
		}
	}
}

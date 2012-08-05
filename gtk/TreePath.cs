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

	public partial class TreePath {

		// Patch submitted by malte on bug #49518
		[DllImport ("libgtk-win32-3.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr gtk_tree_path_get_indices(IntPtr raw);

		public int [] Indices { 
			get {
				IntPtr ptr = gtk_tree_path_get_indices(Handle);
				int [] arr = new int [Depth];
				Marshal.Copy (ptr, arr, 0, Depth);
				return arr;
			}
		}

		public TreePath (int[] indices) : this ()
		{
			foreach (int i in indices)
				AppendIndex (i);
		}

		public override bool Equals (object o)
		{
			if (!(o is TreePath))
				return false;

			return (Compare (o as TreePath) == 0);
		}

		public override int GetHashCode ()
		{
			return ToString ().GetHashCode ();
		}
	}
}

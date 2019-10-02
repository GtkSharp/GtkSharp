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

	public partial class IconSet {
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate void d_gtk_icon_set_get_sizes(IntPtr raw, out int *pointer_to_enum, out int n_sizes);
		static d_gtk_icon_set_get_sizes gtk_icon_set_get_sizes = FuncLoader.LoadFunction<d_gtk_icon_set_get_sizes>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_icon_set_get_sizes"));

		/// <summary> Sizes Property </summary>
		/// <remarks> To be completed </remarks>
		public Gtk.IconSize [] Sizes {
			get {

				Gtk.IconSize [] retval;

				unsafe {
					int length;
					int *pointer_to_enum;
					gtk_icon_set_get_sizes (Handle, out pointer_to_enum, out length);
					retval = new Gtk.IconSize [length];
					for (int i = 0; i < length; i++)
						retval [i] = (Gtk.IconSize) pointer_to_enum [i];

					GLib.Marshaller.Free ((IntPtr)pointer_to_enum);
				}

				return retval;
			}
		}
	}
}


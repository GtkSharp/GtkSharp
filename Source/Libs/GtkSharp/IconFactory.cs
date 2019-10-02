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

	public partial class IconFactory {
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_gtk_icon_size_lookup(IconSize size, out int width, out int height);
		static d_gtk_icon_size_lookup gtk_icon_size_lookup = FuncLoader.LoadFunction<d_gtk_icon_size_lookup>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_icon_size_lookup"));

		/// <summary> Query icon dimensions </summary>
                /// <remarks> Queries dimensions for icons of the specified size. </remarks>
		public void LookupIconSize (IconSize size, out int width, out int height)
		{
			gtk_icon_size_lookup (size, out width, out height);
		}
	}
}


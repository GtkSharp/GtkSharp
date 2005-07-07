// Modules.cs
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


namespace Gnome
{
	using System;
	using System.Runtime.InteropServices;

	public class Modules
	{
		[DllImport("gnome-2")]
		static extern System.IntPtr libgnome_module_info_get ();
		[DllImport("gnomeui-2")]
		static extern System.IntPtr libgnomeui_module_info_get ();

		public static ModuleInfo LibGnome {
			get { 
				return new ModuleInfo (libgnome_module_info_get ());
			}
		}

		public static ModuleInfo UI {
			get { 
				return new ModuleInfo (libgnomeui_module_info_get ());
			}
		}
	}
}


// Pango.Language.cs - Pango Language class customizations
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

namespace Pango {

	using System;
	using System.Runtime.InteropServices;

	public partial class Language {

		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		delegate IntPtr d_pango_language_get_scripts (IntPtr raw, out int num_scripts);
		static d_pango_language_get_scripts pango_language_get_scripts = FuncLoader.LoadFunction<d_pango_language_get_scripts> (FuncLoader.GetProcAddress (GLibrary.Load (Library.Pango), "pango_language_get_scripts"));

		// pango_language_get_scripts returns "const PangoScript *" plus a count.
		// The gir says <array length="0">; the api.xml has nowhere to put that, so
		// codegen declared the return value as an int and cast it to a Script --
		// i.e. it handed back the low 32 bits of the array's address as an
		// enumeration value. Reading the return of this function was meaningless.
		//
		// The array belongs to a static table inside Pango (transfer none), so it
		// is copied out and never freed.
		public Pango.Script[] GetScripts ()
		{
			int count;
			IntPtr array_ptr = pango_language_get_scripts (Handle, out count);
			if (array_ptr == IntPtr.Zero || count <= 0)
				return new Pango.Script [0];

			int[] raw = new int [count];
			Marshal.Copy (array_ptr, raw, 0, count);
			Pango.Script[] result = new Pango.Script [count];
			for (int i = 0; i < count; i++)
				result [i] = (Pango.Script) raw [i];
			return result;
		}
	}
}

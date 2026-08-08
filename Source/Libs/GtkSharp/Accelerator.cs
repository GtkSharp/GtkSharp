// Accelerator.cs - customizations to Gtk.Accelerator
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
	using System.Collections.Generic;
	using System.Runtime.InteropServices;

	public partial class Accelerator {

		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		delegate bool d_gtk_accelerator_parse_with_keycode (IntPtr accelerator, IntPtr display, out uint accelerator_key, out IntPtr accelerator_codes, out int accelerator_mods);
		static d_gtk_accelerator_parse_with_keycode gtk_accelerator_parse_with_keycode = FuncLoader.LoadFunction<d_gtk_accelerator_parse_with_keycode> (FuncLoader.GetProcAddress (GLibrary.Load (Library.Gtk), "gtk_accelerator_parse_with_keycode"));

		// accelerator_codes is "guint **": Gtk allocates a zero-terminated array
		// of the hardware keycodes that produce this keyval and hands ownership
		// of it to the caller. The api.xml cannot say either of those things, so
		// codegen declared it "out uint" -- a four-byte slot for Gtk to write an
		// eight-byte pointer into, whose value was then reported as though it
		// were a keycode, and whose array was leaked.
		//
		// A keycode of 0 is not a keycode, which is what lets Gtk terminate the
		// array with one; the count is not returned anywhere else.
		public static bool ParseWithKeycode (string accelerator, Gdk.Display display, out uint accelerator_key, out uint[] accelerator_codes, out Gdk.ModifierType accelerator_mods)
		{
			IntPtr native_accelerator = GLib.Marshaller.StringToPtrGStrdup (accelerator);
			IntPtr native_codes;
			int native_mods;

			bool raw_ret = gtk_accelerator_parse_with_keycode (native_accelerator,
				display == null ? IntPtr.Zero : display.Handle,
				out accelerator_key, out native_codes, out native_mods);

			GLib.Marshaller.Free (native_accelerator);
			accelerator_mods = (Gdk.ModifierType) native_mods;

			if (native_codes == IntPtr.Zero) {
				accelerator_codes = new uint [0];
				return raw_ret;
			}

			var codes = new List<uint> ();
			for (int offset = 0; ; offset += 4) {
				uint code = unchecked ((uint) Marshal.ReadInt32 (native_codes, offset));
				if (code == 0)
					break;
				codes.Add (code);
			}

			GLib.Marshaller.Free (native_codes);
			accelerator_codes = codes.ToArray ();
			return raw_ret;
		}
	}
}

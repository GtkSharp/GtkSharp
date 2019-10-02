// Keymap.cs - customizations to Gdk.Keymap
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


namespace Gdk {

	using System;
	using System.Runtime.InteropServices;

	public partial class Keymap {
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate bool d_gdk_keymap_get_entries_for_keycode(IntPtr raw, uint hardware_keycode, out IntPtr keys, out IntPtr keyvals, out int n_entries);
		static d_gdk_keymap_get_entries_for_keycode gdk_keymap_get_entries_for_keycode = FuncLoader.LoadFunction<d_gdk_keymap_get_entries_for_keycode>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gdk), "gdk_keymap_get_entries_for_keycode"));

		public void GetEntriesForKeycode(uint hardware_keycode, out Gdk.KeymapKey[] keys, out uint[] keyvals) 
		{
			IntPtr key_ptr, keyval_ptr;
			int count;
			if (gdk_keymap_get_entries_for_keycode(Handle, hardware_keycode, out key_ptr, out keyval_ptr, out count)) {
				keys = new KeymapKey [count];
				keyvals = new uint [count];
				int[] tmp = new int [count];
				Marshal.Copy (keyval_ptr, tmp, 0, count);
				for (int i = 0; i < count; i++) {
					IntPtr ptr = new IntPtr ((long) key_ptr + i * Marshal.SizeOf (typeof (KeymapKey)));
					keyvals [i] = (uint) tmp [i];
					keys [i] = KeymapKey.New (ptr);
				}
				GLib.Marshaller.Free (key_ptr);
				GLib.Marshaller.Free (keyval_ptr);
			} else {
				keys = new KeymapKey [0];
				keyvals = new uint [0];
			}
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate bool d_gdk_keymap_get_entries_for_keyval(IntPtr raw, uint keyval, out IntPtr keys, out int n_keys);
		static d_gdk_keymap_get_entries_for_keyval gdk_keymap_get_entries_for_keyval = FuncLoader.LoadFunction<d_gdk_keymap_get_entries_for_keyval>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gdk), "gdk_keymap_get_entries_for_keyval"));

		public KeymapKey[] GetEntriesForKeyval (uint keyval) 
		{
			IntPtr key_ptr;
			int count;
			if (gdk_keymap_get_entries_for_keyval(Handle, keyval, out key_ptr, out count)) {
				KeymapKey[] result = new KeymapKey [count];
				for (int i = 0; i < count; i++) {
					IntPtr ptr = new IntPtr ((long) key_ptr + i * Marshal.SizeOf (typeof (KeymapKey)));
					result [i] = KeymapKey.New (ptr);
				}
				GLib.Marshaller.Free (key_ptr);
				return result;
			} else
				return new KeymapKey [0];
		}
	}
}



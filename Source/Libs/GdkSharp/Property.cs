// Gdk.Property.cs - Custom implementation for Property class
//
// Authors: Mike Kestner <mkestner@novell.com>
//
// Copyright (c) 2007 Novell, Inc.
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

namespace Gdk {

	using System;
	using System.Runtime.InteropServices;

	public partial class Property {
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_gdk_property_change2(IntPtr window, IntPtr property, IntPtr type, int format, int mode, out byte data, int nelements);
		static d_gdk_property_change2 gdk_property_change2 = FuncLoader.LoadFunction<d_gdk_property_change2>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gdk), "gdk_property_change"));

		[Obsolete ("Replaced by corrected overload with data parameter")]
		public static byte Change(Gdk.Window window, Gdk.Atom property, Gdk.Atom type, int format, Gdk.PropMode mode, int nelements) {
			byte data;
			gdk_property_change2(window == null ? IntPtr.Zero : window.Handle, property == null ? IntPtr.Zero : property.Handle, type == null ? IntPtr.Zero : type.Handle, format, (int) mode, out data, nelements);
			return data;
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate bool d_gdk_property_get(IntPtr window, IntPtr property, IntPtr type, UIntPtr offset, UIntPtr length, bool pdelete, out IntPtr actual_property_type, out int actual_format, out int actual_length, out IntPtr data);
		static d_gdk_property_get gdk_property_get = FuncLoader.LoadFunction<d_gdk_property_get>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gdk), "gdk_property_get"));

		public static bool Get(Gdk.Window window, Gdk.Atom property, Gdk.Atom type, ulong offset, ulong length, int pdelete, out Gdk.Atom actual_property_type, out int actual_format, out int actual_length, out byte[] data) {
			IntPtr actual_property_type_as_native;
			IntPtr actual_data;
			bool raw_ret = gdk_property_get(window == null ? IntPtr.Zero : window.Handle, property == null ? IntPtr.Zero : property.Handle, type == null ? IntPtr.Zero : type.Handle, new UIntPtr (offset), new UIntPtr (length), pdelete == 0 ? false : true, out actual_property_type_as_native, out actual_format, out actual_length, out actual_data);
			data = null;
			if (raw_ret) {
				data = new byte [actual_length];
				Marshal.Copy (actual_data, data, 0, actual_length);
				GLib.Marshaller.Free (actual_data);
			}

			bool ret = raw_ret;
			actual_property_type = actual_property_type_as_native == IntPtr.Zero ? null : (Gdk.Atom) GLib.Opaque.GetOpaque (actual_property_type_as_native, typeof (Gdk.Atom), false);
			return ret;
		}

		public static bool Get (Gdk.Window window, Gdk.Atom property, Gdk.Atom type, ulong offset, ulong length, bool pdelete, out int[] data) {
			IntPtr actual_property_type, raw_data;
			int actual_length, format;
			bool ret = gdk_property_get(window == null ? IntPtr.Zero : window.Handle, property == null ? IntPtr.Zero : property.Handle, type == null ? IntPtr.Zero : type.Handle, new UIntPtr (offset), new UIntPtr (length), pdelete, out actual_property_type, out format, out actual_length, out raw_data);
			if (ret) {
				try {
					int block_size;
					if (format == 32) { // data returned in blocks the size of a C long
#if WIN64LONGS
						block_size = sizeof(int);
#else
						block_size = IntPtr.Size;
#endif
					} else if (format == 8 || format == 16)
						block_size = format;
					else
						throw new NotSupportedException (String.Format ("Unable to read properties in {0}-bit format", format));

					int size = actual_length / block_size;
					data = new int [size];
					for (int idx = 0; idx < size; idx++) {
						IntPtr elem_ptr = new IntPtr (raw_data.ToInt64 () + idx * block_size);
						switch (format) {
						case 8:
							data [idx] = Marshal.ReadByte (elem_ptr);
							break;
						case 16:
							data [idx] = Marshal.ReadInt16 (elem_ptr);
							break;
						case 32:
							data [idx] = Marshal.ReadInt32 (elem_ptr);
							break;
						}
					}
				} finally {
					GLib.Marshaller.Free (raw_data);
				}
			} else
				data = null;
			
			return ret;
		}

		public static bool Get (Gdk.Window window, Gdk.Atom property, bool pdelete, out int[] data) {
			return Get (window, property, 0, uint.MaxValue - 3, pdelete, out data);
		}

		public static bool Get (Gdk.Window window, Gdk.Atom property, ulong offset, ulong length, bool pdelete, out int[] data) {
			return Get (window, property, Gdk.Atom.Intern ("CARDINAL", false), offset, length, pdelete, out data);
		}

		public static bool Get (Gdk.Window window, Gdk.Atom property, bool pdelete, out Gdk.Atom[] data) {
			return Get (window, property, 0, uint.MaxValue - 3, pdelete, out data);
		}

		public static bool Get (Gdk.Window window, Gdk.Atom property, ulong offset, ulong length, bool pdelete, out Gdk.Atom[] atoms) {
			int[] raw_atoms;
			if (!Get (window, property, Gdk.Atom.Intern ("ATOM", false), offset, length, pdelete, out raw_atoms)) {
				atoms = null;
				return false;
			}

			atoms = new Gdk.Atom [raw_atoms.GetLength (0)];
			for (int idx = 0; idx < raw_atoms.GetLength (0); idx++) {
				atoms [idx] = new Gdk.Atom (new IntPtr (raw_atoms [idx]));
			}
			return true;
		}

		public static bool Get (Gdk.Window window, Gdk.Atom property, bool pdelete, out Gdk.Rectangle[] rects) {
			return Get (window, property, 0, uint.MaxValue - 3, pdelete, out rects);
		}

		public static bool Get (Gdk.Window window, Gdk.Atom property, ulong offset, ulong length, bool pdelete, out Gdk.Rectangle[] rects) {
			int[] raw_rects;
			if (!Get (window, property, Gdk.Atom.Intern ("CARDINAL", false), offset, length, pdelete, out raw_rects)) {
				rects = null;
				return false;
			}

			rects = new Gdk.Rectangle [raw_rects.GetLength (0) / 4];
			for (int idx = 0; idx < rects.GetLength (0); idx ++) {
				rects [idx] = new Gdk.Rectangle (raw_rects [idx * 4], raw_rects [idx * 4 + 1], raw_rects [idx * 4 + 2], raw_rects [idx * 4 + 3]);
			}
			return true;
		}

#if FIXME30
		public static bool Get (Gdk.Window window, Gdk.Atom property, bool pdelete, out Gdk.Window[] windows) {
			return Get (window, property, 0, uint.MaxValue - 3, pdelete, out windows);
		}

		public static bool Get (Gdk.Window window, Gdk.Atom property, ulong offset, ulong length, bool pdelete, out Gdk.Window[] windows) {
			int[] raw_windows;
			if (!Get (window, property, Gdk.Atom.Intern ("WINDOW", false), offset, length, pdelete, out raw_windows)) {
				windows = null;
				return false;
			}

			windows = new Gdk.Window [raw_windows.GetLength (0)];
			for (int idx = 0; idx < raw_windows.GetLength (0); idx ++) {
				windows [idx] = Gdk.Window.ForeignNew ((uint) raw_windows [idx]);
			}
			return true;
		}
#endif
	}
}



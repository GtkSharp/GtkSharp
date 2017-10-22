// Bytes.cs - Bytes class implementation
//
// Author: Stephan Sundermann <stephansundermann@gmail.com>
//
// Copyright (c) 2014 Stephan Sundermann
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

namespace GLib {

	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Runtime.InteropServices;

	public partial class Bytes : GLib.Opaque, IComparable<Bytes>, IEquatable<Bytes>
	{
		[DllImport (Global.GLibNativeDll, CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr g_bytes_get_type ();

		public static GLib.GType GType { 
			get {
				IntPtr raw_ret = g_bytes_get_type ();
				GLib.GType ret = new GLib.GType (raw_ret);
				return ret;
			}
		}

		public Bytes (IntPtr raw) : base (raw) {}

		[DllImport (Global.GLibNativeDll, CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr g_bytes_new (byte [] data, UIntPtr size);

		public Bytes (byte [] data)
		{
			Raw = g_bytes_new (data, new UIntPtr ((ulong)data.Length));
		}

		[DllImport (Global.GLibNativeDll, CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr g_bytes_new_from_bytes (IntPtr raw, UIntPtr offset, UIntPtr length);

		public Bytes (Bytes bytes, ulong offset, ulong length)
		{
			Raw = g_bytes_new_from_bytes (bytes.Handle, new UIntPtr (offset), new UIntPtr (length));
		}

		[DllImport (Global.GLibNativeDll, CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr g_bytes_new_take (byte [] data, UIntPtr size);

		public static Bytes NewTake (byte [] data)
		{
			return new Bytes (g_bytes_new_take (data, new UIntPtr ((ulong)data.Length)));
		}

		[DllImport (Global.GLibNativeDll, CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr g_bytes_new_static (byte [] data, UIntPtr size);

		public static Bytes NewStatic (byte [] data)
		{
			return new Bytes (g_bytes_new_static (data, new UIntPtr ((ulong)data.Length)));
		}

		[DllImport (Global.GLibNativeDll, CallingConvention = CallingConvention.Cdecl)]
		static extern int g_bytes_compare (IntPtr raw, IntPtr bytes);

		public int CompareTo (Bytes bytes)
		{
			return g_bytes_compare (Handle, bytes.Handle);
		}

		[DllImport (Global.GLibNativeDll, CallingConvention = CallingConvention.Cdecl)]
		static extern bool g_bytes_equal (IntPtr raw, IntPtr bytes2);

		public bool Equals (Bytes other)
		{
			return g_bytes_equal (Handle, other.Handle);
		}

		[DllImport (Global.GLibNativeDll, CallingConvention = CallingConvention.Cdecl)]
		static extern UIntPtr g_bytes_get_size (IntPtr raw);

		public ulong Size { 
			get {
				return (ulong) g_bytes_get_size (Handle);
			}
		}

		[DllImport (Global.GLibNativeDll, CallingConvention = CallingConvention.Cdecl)]
		static extern uint g_bytes_hash (IntPtr raw);

		public uint GetHash ()
		{
			return g_bytes_hash (Handle);
		}

		[DllImport (Global.GLibNativeDll, CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr g_bytes_get_data (IntPtr raw, out UIntPtr size);

		public byte [] Data {
			get {
				UIntPtr size;
				IntPtr ptr = g_bytes_get_data (Handle, out size);

				if (ptr == IntPtr.Zero)
					return null;

				int sz = (int) size;
				byte [] bytes = new byte [sz];
				Marshal.Copy (ptr, bytes, 0, sz);

				return bytes;
			}
		}

		[DllImport (Global.GLibNativeDll, CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr g_bytes_ref (IntPtr raw);

		protected override void Ref (IntPtr raw)
		{
			if (!Owned) {
				g_bytes_ref (raw);
				Owned = true;
			}
		}

		[DllImport (Global.GLibNativeDll, CallingConvention = CallingConvention.Cdecl)]
		static extern void g_bytes_unref (IntPtr raw);

		protected override void Unref (IntPtr raw)
		{
			if (Owned) {
				g_bytes_unref (raw);
				Owned = false;
			}
		}

		class FinalizerInfo
		{
			IntPtr handle;

			public FinalizerInfo (IntPtr handle)
			{
				this.handle = handle;
			}

			public bool Handler ()
			{
				g_bytes_unref (handle);
				return false;
			}
		}

		~Bytes ()
		{
			if (!Owned)
				return;
			FinalizerInfo info = new FinalizerInfo (Handle);
			GLib.Timeout.Add (50, new GLib.TimeoutHandler (info.Handler));
		}
	}
}

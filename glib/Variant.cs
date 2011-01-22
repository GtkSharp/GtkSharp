// Copyright (c) 2011 Novell, Inc.
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

using System;
using System.Runtime.InteropServices;

namespace GLib {

	public class Variant : IDisposable {

		IntPtr handle;
		public IntPtr Handle {
			get { return handle; }
		}

		// Docs say that GVariant is threadsafe.
		~Variant ()
		{
			Dispose (false);
		}

		public void Dispose ()
		{
			Dispose (true);
		}

		[DllImport ("libglib-2.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern void g_variant_unref (IntPtr handle);

		void Dispose (bool disposing)
		{
			if (handle == IntPtr.Zero)
				return;

			g_variant_unref (handle);
			handle = IntPtr.Zero;
			if (disposing)
				GC.SuppressFinalize (this);
		}

		[DllImport ("libglib-2.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr g_variant_ref_sink (IntPtr handle);

		public Variant (IntPtr handle)
		{
			this.handle = g_variant_ref_sink (handle);
		}

		[DllImport ("libglib-2.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr g_variant_new_boolean (bool val);

		public Variant (bool val) : this (g_variant_new_boolean (val)) {}

		[DllImport ("libglib-2.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr g_variant_new_byte (byte val);

		public Variant (byte val) : this (g_variant_new_byte (val)) {}

		[DllImport ("libglib-2.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr g_variant_new_int16 (short val);

		public Variant (short val) : this (g_variant_new_int16 (val)) {}

		[DllImport ("libglib-2.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr g_variant_new_uint16 (ushort val);

		public Variant (ushort val) : this (g_variant_new_uint16 (val)) {}

		[DllImport ("libglib-2.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr g_variant_new_int32 (int val);

		public Variant (int val) : this (g_variant_new_int32 (val)) {}

		[DllImport ("libglib-2.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr g_variant_new_uint32 (uint val);

		public Variant (uint val) : this (g_variant_new_uint32 (val)) {}

		[DllImport ("libglib-2.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr g_variant_new_int64 (long val);

		public Variant (long val) : this (g_variant_new_int64 (val)) {}

		[DllImport ("libglib-2.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr g_variant_new_uint64 (ulong val);

		public Variant (ulong val) : this (g_variant_new_uint64 (val)) {}

		[DllImport ("libglib-2.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr g_variant_new_double (double val);

		public Variant (double val) : this (g_variant_new_double (val)) {}

		[DllImport ("libglib-2.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr g_variant_new_string (IntPtr val);

		public Variant (string val)
		{
			IntPtr native_val = Marshaller.StringToPtrGStrdup (val);
			handle = g_variant_ref_sink (g_variant_new_string (native_val));
			Marshaller.Free (native_val);
		}

		[DllImport ("libglib-2.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern bool g_variant_get_boolean (IntPtr handle);

		public static explicit operator bool (Variant val)
		{
			return g_variant_get_boolean (val.Handle);
		}

		[DllImport ("libglib-2.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern byte g_variant_get_byte (IntPtr handle);

		public static explicit operator byte (Variant val)
		{
			return g_variant_get_byte (val.Handle);
		}

		[DllImport ("libglib-2.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern short g_variant_get_int16 (IntPtr handle);

		public static explicit operator short (Variant val)
		{
			return g_variant_get_int16 (val.Handle);
		}

		[DllImport ("libglib-2.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern ushort g_variant_get_uint16 (IntPtr handle);

		public static explicit operator ushort (Variant val)
		{
			return g_variant_get_uint16 (val.Handle);
		}

		[DllImport ("libglib-2.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern int g_variant_get_int32 (IntPtr handle);

		public static explicit operator int (Variant val)
		{
			return g_variant_get_int32 (val.Handle);
		}

		[DllImport ("libglib-2.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern uint g_variant_get_uint32 (IntPtr handle);

		public static explicit operator uint (Variant val)
		{
			return g_variant_get_uint32 (val.Handle);
		}

		[DllImport ("libglib-2.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern long g_variant_get_int64 (IntPtr handle);

		public static explicit operator long (Variant val)
		{
			return g_variant_get_int64 (val.Handle);
		}

		[DllImport ("libglib-2.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern ulong g_variant_get_uint64 (IntPtr handle);

		public static explicit operator ulong (Variant val)
		{
			return g_variant_get_uint64 (val.Handle);
		}

		[DllImport ("libglib-2.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern double g_variant_get_double (IntPtr handle);

		public static explicit operator double (Variant val)
		{
			return g_variant_get_double (val.Handle);
		}

		[DllImport ("libglib-2.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr g_variant_get_string (IntPtr handle);

		public static explicit operator string (Variant val)
		{
			IntPtr str = g_variant_get_string (val.Handle);
			return str == IntPtr.Zero ? null : GLib.Marshaller.Utf8PtrToString (str);
		}
	}
}

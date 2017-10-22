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
using System.Collections.Generic;

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

		[DllImport (Global.GLibNativeDll, CallingConvention = CallingConvention.Cdecl)]
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

		[DllImport (Global.GLibNativeDll, CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr g_variant_ref_sink (IntPtr handle);

		public Variant (IntPtr handle)
		{
			this.handle = g_variant_ref_sink (handle);
		}

		[DllImport (Global.GLibNativeDll, CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr g_variant_get_type (IntPtr val);

		VariantType type;
		public VariantType Type {
			get {
				if (type == null)
					type = new VariantType (g_variant_get_type (Handle));
				return type;
			}
		}

		[DllImport (Global.GLibNativeDll, CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr g_variant_new_variant (IntPtr val);

		public static Variant NewVariant (Variant val) {
			return new Variant (g_variant_new_variant (val.Handle));
		}

		[DllImport (Global.GLibNativeDll, CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr g_variant_new_boolean (bool val);

		public Variant (bool val) : this (g_variant_new_boolean (val)) {}

		[DllImport (Global.GLibNativeDll, CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr g_variant_new_byte (byte val);

		public Variant (byte val) : this (g_variant_new_byte (val)) {}

		[DllImport (Global.GLibNativeDll, CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr g_variant_new_int16 (short val);

		public Variant (short val) : this (g_variant_new_int16 (val)) {}

		[DllImport (Global.GLibNativeDll, CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr g_variant_new_uint16 (ushort val);

		public Variant (ushort val) : this (g_variant_new_uint16 (val)) {}

		[DllImport (Global.GLibNativeDll, CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr g_variant_new_int32 (int val);

		public Variant (int val) : this (g_variant_new_int32 (val)) {}

		[DllImport (Global.GLibNativeDll, CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr g_variant_new_uint32 (uint val);

		public Variant (uint val) : this (g_variant_new_uint32 (val)) {}

		[DllImport (Global.GLibNativeDll, CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr g_variant_new_int64 (long val);

		public Variant (long val) : this (g_variant_new_int64 (val)) {}

		[DllImport (Global.GLibNativeDll, CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr g_variant_new_uint64 (ulong val);

		public Variant (ulong val) : this (g_variant_new_uint64 (val)) {}

		[DllImport (Global.GLibNativeDll, CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr g_variant_new_double (double val);

		public Variant (double val) : this (g_variant_new_double (val)) {}

		[DllImport (Global.GLibNativeDll, CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr g_variant_new_string (IntPtr val);

		public Variant (string val)
		{
			IntPtr native_val = Marshaller.StringToPtrGStrdup (val);
			handle = g_variant_ref_sink (g_variant_new_string (native_val));
			Marshaller.Free (native_val);
		}

		[DllImport (Global.GLibNativeDll, CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr g_variant_new_strv (IntPtr[] strv, IntPtr length);

		public Variant (string[] strv)
		{
			IntPtr[] native = Marshaller.StringArrayToNullTermPointer (strv);
			handle = g_variant_ref_sink (g_variant_new_strv (native, new IntPtr ((long) strv.Length)));
			Marshaller.Free (native);
		}

		[DllImport (Global.GLibNativeDll, CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr g_variant_new_tuple (IntPtr[] children, UIntPtr n_children);

		public static Variant NewTuple (Variant[] children)
		{
			if (children == null)
				return new Variant (g_variant_new_tuple (null, new UIntPtr (0ul)));

			IntPtr[] native = new IntPtr[children.Length];
			for (int i = 0; i < children.Length; i++)
				native[i] = children[i].Handle;

			return new Variant (g_variant_new_tuple (native, new UIntPtr ((ulong) children.Length)));
		}

		[DllImport (Global.GLibNativeDll, CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr g_variant_new_array (IntPtr child_type, IntPtr[] children, UIntPtr n_children);

		public static Variant NewArray (Variant[] children)
		{
			if (children == null) {
				throw new ArgumentNullException ("children", "To create an empty array use NewArray (VariantType.<Type>, null)");
			}

			return NewArray (null, children);
		}

		public static Variant NewArray (VariantType type, Variant[] children)
		{
			if (children == null) {
				if (type == null) {
					throw new ArgumentException ("The type and children parameters cannot be both null");
				} else {
					return new Variant (g_variant_new_array (type.Handle, null, new UIntPtr (0ul)));
				}
			}

			IntPtr[] native = new IntPtr[children.Length];
			for (int i = 0; i < children.Length; i++)
				native[i] = children[i].Handle;

			return new Variant (g_variant_new_array (type == null ? (IntPtr) null : type.Handle,
			                                         native, new UIntPtr ((ulong) children.Length)));
		}

		[DllImport (Global.GLibNativeDll, CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr g_variant_new_dict_entry (IntPtr k, IntPtr v);

		public static Variant NewDictEntry (Variant k, Variant v)
		{
			return new Variant (g_variant_new_dict_entry (k.Handle, v.Handle));
		}

		public Variant (IDictionary<string, Variant> dict)
		{
			VariantType type = VariantType.NewDictionaryEntry (
				VariantType.String,
				VariantType.Variant);

			var pairs = new List<Variant> ();
			foreach (var kvp in dict)
				pairs.Add (NewDictEntry (new Variant (kvp.Key), NewVariant (kvp.Value)));

			handle = g_variant_ref_sink (NewArray (type, pairs.ToArray ()).Handle);
		}

		[DllImport (Global.GLibNativeDll, CallingConvention = CallingConvention.Cdecl)]
		static extern bool g_variant_get_boolean (IntPtr handle);

		public static explicit operator bool (Variant val)
		{
			return g_variant_get_boolean (val.Handle);
		}

		[DllImport (Global.GLibNativeDll, CallingConvention = CallingConvention.Cdecl)]
		static extern byte g_variant_get_byte (IntPtr handle);

		public static explicit operator byte (Variant val)
		{
			return g_variant_get_byte (val.Handle);
		}

		[DllImport (Global.GLibNativeDll, CallingConvention = CallingConvention.Cdecl)]
		static extern short g_variant_get_int16 (IntPtr handle);

		public static explicit operator short (Variant val)
		{
			return g_variant_get_int16 (val.Handle);
		}

		[DllImport (Global.GLibNativeDll, CallingConvention = CallingConvention.Cdecl)]
		static extern ushort g_variant_get_uint16 (IntPtr handle);

		public static explicit operator ushort (Variant val)
		{
			return g_variant_get_uint16 (val.Handle);
		}

		[DllImport (Global.GLibNativeDll, CallingConvention = CallingConvention.Cdecl)]
		static extern int g_variant_get_int32 (IntPtr handle);

		public static explicit operator int (Variant val)
		{
			return g_variant_get_int32 (val.Handle);
		}

		[DllImport (Global.GLibNativeDll, CallingConvention = CallingConvention.Cdecl)]
		static extern uint g_variant_get_uint32 (IntPtr handle);

		public static explicit operator uint (Variant val)
		{
			return g_variant_get_uint32 (val.Handle);
		}

		[DllImport (Global.GLibNativeDll, CallingConvention = CallingConvention.Cdecl)]
		static extern long g_variant_get_int64 (IntPtr handle);

		public static explicit operator long (Variant val)
		{
			return g_variant_get_int64 (val.Handle);
		}

		[DllImport (Global.GLibNativeDll, CallingConvention = CallingConvention.Cdecl)]
		static extern ulong g_variant_get_uint64 (IntPtr handle);

		public static explicit operator ulong (Variant val)
		{
			return g_variant_get_uint64 (val.Handle);
		}

		[DllImport (Global.GLibNativeDll, CallingConvention = CallingConvention.Cdecl)]
		static extern double g_variant_get_double (IntPtr handle);

		public static explicit operator double (Variant val)
		{
			return g_variant_get_double (val.Handle);
		}

		[DllImport (Global.GLibNativeDll, CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr g_variant_get_string (IntPtr handle, IntPtr length);

		public static explicit operator string (Variant val)
		{
			IntPtr str = g_variant_get_string (val.Handle, IntPtr.Zero);
			return GLib.Marshaller.Utf8PtrToString (str);
		}

		[DllImport (Global.GLibNativeDll, CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr g_variant_print (IntPtr variant, bool type_annotate);

		public string Print (bool type_annotate)
		{
			IntPtr str = g_variant_print (handle, type_annotate);
			return Marshaller.PtrToStringGFree (str);
		}

		[DllImport (Global.GLibNativeDll, CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr g_variant_n_children (IntPtr handle);

		[DllImport (Global.GLibNativeDll, CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr g_variant_get_child_value (IntPtr handle, IntPtr index);

		public Variant[] ToArray ()
		{
			var n_children = (long) g_variant_n_children (Handle);
			var ret = new Variant[n_children];

			for (long i = 0; i < n_children; i++) {
				var h = g_variant_get_child_value (Handle, new IntPtr (i));
				ret[i] = new Variant (h);
				g_variant_unref (h);
			}

			return ret;
		}

		[DllImport (Global.GLibNativeDll, CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr g_variant_get_variant (IntPtr handle);

		public Dictionary<string, Variant> ToAsv ()
		{
			var ret = new Dictionary<string, Variant> ();

			foreach (var dictEntry in ToArray ()) {
				var pair = dictEntry.ToArray ();
				var key = (string) pair[0];
				var h = g_variant_get_variant (pair[1].Handle);
				var value = new Variant (h);
				g_variant_unref (h);

				ret.Add (key, value);
			}

			return ret;
		}
	}
}

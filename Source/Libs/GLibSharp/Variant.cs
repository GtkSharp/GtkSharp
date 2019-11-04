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
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_g_variant_unref(IntPtr handle);
		static d_g_variant_unref g_variant_unref = FuncLoader.LoadFunction<d_g_variant_unref>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_variant_unref"));

		void Dispose (bool disposing)
		{
			if (handle == IntPtr.Zero)
				return;

			g_variant_unref (handle);
			handle = IntPtr.Zero;
			if (disposing)
				GC.SuppressFinalize (this);
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_variant_ref_sink(IntPtr handle);
		static d_g_variant_ref_sink g_variant_ref_sink = FuncLoader.LoadFunction<d_g_variant_ref_sink>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_variant_ref_sink"));

		public Variant (IntPtr handle)
		{
			this.handle = g_variant_ref_sink (handle);
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_variant_get_type(IntPtr val);
		static d_g_variant_get_type g_variant_get_type = FuncLoader.LoadFunction<d_g_variant_get_type>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_variant_get_type"));

		VariantType type;
		public VariantType Type {
			get {
				if (type == null)
					type = new VariantType (g_variant_get_type (Handle));
				return type;
			}
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_variant_new_variant(IntPtr val);
		static d_g_variant_new_variant g_variant_new_variant = FuncLoader.LoadFunction<d_g_variant_new_variant>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_variant_new_variant"));

		public static Variant NewVariant (Variant val) {
			return new Variant (g_variant_new_variant (val.Handle));
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_variant_new_boolean(bool val);
		static d_g_variant_new_boolean g_variant_new_boolean = FuncLoader.LoadFunction<d_g_variant_new_boolean>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_variant_new_boolean"));

		public Variant (bool val) : this (g_variant_new_boolean (val)) {}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_variant_new_byte(byte val);
		static d_g_variant_new_byte g_variant_new_byte = FuncLoader.LoadFunction<d_g_variant_new_byte>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_variant_new_byte"));

		public Variant (byte val) : this (g_variant_new_byte (val)) {}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_variant_new_int16(short val);
		static d_g_variant_new_int16 g_variant_new_int16 = FuncLoader.LoadFunction<d_g_variant_new_int16>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_variant_new_int16"));

		public Variant (short val) : this (g_variant_new_int16 (val)) {}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_variant_new_uint16(ushort val);
		static d_g_variant_new_uint16 g_variant_new_uint16 = FuncLoader.LoadFunction<d_g_variant_new_uint16>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_variant_new_uint16"));

		public Variant (ushort val) : this (g_variant_new_uint16 (val)) {}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_variant_new_int32(int val);
		static d_g_variant_new_int32 g_variant_new_int32 = FuncLoader.LoadFunction<d_g_variant_new_int32>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_variant_new_int32"));

		public Variant (int val) : this (g_variant_new_int32 (val)) {}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_variant_new_uint32(uint val);
		static d_g_variant_new_uint32 g_variant_new_uint32 = FuncLoader.LoadFunction<d_g_variant_new_uint32>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_variant_new_uint32"));

		public Variant (uint val) : this (g_variant_new_uint32 (val)) {}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_variant_new_int64(long val);
		static d_g_variant_new_int64 g_variant_new_int64 = FuncLoader.LoadFunction<d_g_variant_new_int64>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_variant_new_int64"));

		public Variant (long val) : this (g_variant_new_int64 (val)) {}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_variant_new_uint64(ulong val);
		static d_g_variant_new_uint64 g_variant_new_uint64 = FuncLoader.LoadFunction<d_g_variant_new_uint64>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_variant_new_uint64"));

		public Variant (ulong val) : this (g_variant_new_uint64 (val)) {}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_variant_new_double(double val);
		static d_g_variant_new_double g_variant_new_double = FuncLoader.LoadFunction<d_g_variant_new_double>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_variant_new_double"));

		public Variant (double val) : this (g_variant_new_double (val)) {}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_variant_new_string(IntPtr val);
		static d_g_variant_new_string g_variant_new_string = FuncLoader.LoadFunction<d_g_variant_new_string>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_variant_new_string"));

		public Variant (string val)
		{
			IntPtr native_val = Marshaller.StringToPtrGStrdup (val);
			handle = g_variant_ref_sink (g_variant_new_string (native_val));
			Marshaller.Free (native_val);
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_variant_new_strv(IntPtr[] strv, IntPtr length);
		static d_g_variant_new_strv g_variant_new_strv = FuncLoader.LoadFunction<d_g_variant_new_strv>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_variant_new_strv"));

		public Variant (string[] strv)
		{
			IntPtr[] native = Marshaller.StringArrayToNullTermPointer (strv);
			handle = g_variant_ref_sink (g_variant_new_strv (native, new IntPtr ((long) strv.Length)));
			Marshaller.Free (native);
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_variant_new_tuple(IntPtr[] children, UIntPtr n_children);
		static d_g_variant_new_tuple g_variant_new_tuple = FuncLoader.LoadFunction<d_g_variant_new_tuple>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_variant_new_tuple"));

		public static Variant NewTuple (Variant[] children)
		{
			if (children == null)
				return new Variant (g_variant_new_tuple (null, new UIntPtr (0ul)));

			IntPtr[] native = new IntPtr[children.Length];
			for (int i = 0; i < children.Length; i++)
				native[i] = children[i].Handle;

			return new Variant (g_variant_new_tuple (native, new UIntPtr ((ulong) children.Length)));
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_variant_new_array(IntPtr child_type, IntPtr[] children, UIntPtr n_children);
		static d_g_variant_new_array g_variant_new_array = FuncLoader.LoadFunction<d_g_variant_new_array>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_variant_new_array"));

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
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_variant_new_dict_entry(IntPtr k, IntPtr v);
		static d_g_variant_new_dict_entry g_variant_new_dict_entry = FuncLoader.LoadFunction<d_g_variant_new_dict_entry>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_variant_new_dict_entry"));

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
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate bool d_g_variant_get_boolean(IntPtr handle);
		static d_g_variant_get_boolean g_variant_get_boolean = FuncLoader.LoadFunction<d_g_variant_get_boolean>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_variant_get_boolean"));

		public static explicit operator bool (Variant val)
		{
			return g_variant_get_boolean (val.Handle);
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate byte d_g_variant_get_byte(IntPtr handle);
		static d_g_variant_get_byte g_variant_get_byte = FuncLoader.LoadFunction<d_g_variant_get_byte>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_variant_get_byte"));

		public static explicit operator byte (Variant val)
		{
			return g_variant_get_byte (val.Handle);
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate short d_g_variant_get_int16(IntPtr handle);
		static d_g_variant_get_int16 g_variant_get_int16 = FuncLoader.LoadFunction<d_g_variant_get_int16>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_variant_get_int16"));

		public static explicit operator short (Variant val)
		{
			return g_variant_get_int16 (val.Handle);
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate ushort d_g_variant_get_uint16(IntPtr handle);
		static d_g_variant_get_uint16 g_variant_get_uint16 = FuncLoader.LoadFunction<d_g_variant_get_uint16>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_variant_get_uint16"));

		public static explicit operator ushort (Variant val)
		{
			return g_variant_get_uint16 (val.Handle);
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate int d_g_variant_get_int32(IntPtr handle);
		static d_g_variant_get_int32 g_variant_get_int32 = FuncLoader.LoadFunction<d_g_variant_get_int32>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_variant_get_int32"));

		public static explicit operator int (Variant val)
		{
			return g_variant_get_int32 (val.Handle);
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate uint d_g_variant_get_uint32(IntPtr handle);
		static d_g_variant_get_uint32 g_variant_get_uint32 = FuncLoader.LoadFunction<d_g_variant_get_uint32>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_variant_get_uint32"));

		public static explicit operator uint (Variant val)
		{
			return g_variant_get_uint32 (val.Handle);
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate long d_g_variant_get_int64(IntPtr handle);
		static d_g_variant_get_int64 g_variant_get_int64 = FuncLoader.LoadFunction<d_g_variant_get_int64>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_variant_get_int64"));

		public static explicit operator long (Variant val)
		{
			return g_variant_get_int64 (val.Handle);
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate ulong d_g_variant_get_uint64(IntPtr handle);
		static d_g_variant_get_uint64 g_variant_get_uint64 = FuncLoader.LoadFunction<d_g_variant_get_uint64>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_variant_get_uint64"));

		public static explicit operator ulong (Variant val)
		{
			return g_variant_get_uint64 (val.Handle);
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate double d_g_variant_get_double(IntPtr handle);
		static d_g_variant_get_double g_variant_get_double = FuncLoader.LoadFunction<d_g_variant_get_double>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_variant_get_double"));

		public static explicit operator double (Variant val)
		{
			return g_variant_get_double (val.Handle);
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_variant_get_string(IntPtr handle, IntPtr length);
		static d_g_variant_get_string g_variant_get_string = FuncLoader.LoadFunction<d_g_variant_get_string>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_variant_get_string"));

		public static explicit operator string (Variant val)
		{
			IntPtr str = g_variant_get_string (val.Handle, IntPtr.Zero);
			return GLib.Marshaller.Utf8PtrToString (str);
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_variant_print(IntPtr variant, bool type_annotate);
		static d_g_variant_print g_variant_print = FuncLoader.LoadFunction<d_g_variant_print>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_variant_print"));

		public string Print (bool type_annotate)
		{
			IntPtr str = g_variant_print (handle, type_annotate);
			return Marshaller.PtrToStringGFree (str);
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_variant_n_children(IntPtr handle);
		static d_g_variant_n_children g_variant_n_children = FuncLoader.LoadFunction<d_g_variant_n_children>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_variant_n_children"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_variant_get_child_value(IntPtr handle, IntPtr index);
		static d_g_variant_get_child_value g_variant_get_child_value = FuncLoader.LoadFunction<d_g_variant_get_child_value>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_variant_get_child_value"));

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
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_variant_get_variant(IntPtr handle);
		static d_g_variant_get_variant g_variant_get_variant = FuncLoader.LoadFunction<d_g_variant_get_variant>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_variant_get_variant"));

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


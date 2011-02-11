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

	public class VariantType : IDisposable {

		public static VariantType Boolean = new VariantType ("b");
		public static VariantType Byte = new VariantType ("y");
		public static VariantType Int16 = new VariantType ("n");
		public static VariantType UInt16 = new VariantType ("q");
		public static VariantType Int32 = new VariantType ("i");
		public static VariantType Uint32 = new VariantType ("u");
		public static VariantType Int64 = new VariantType ("x");
		public static VariantType UInt64 = new VariantType ("t");
		public static VariantType Double = new VariantType ("d");
		public static VariantType String = new VariantType ("s");
		public static VariantType Path = new VariantType ("o");
		public static VariantType Signature = new VariantType ("g");
		public static VariantType Variant = new VariantType ("v");
		public static VariantType HandleType = new VariantType ("h");
		public static VariantType Unit = new VariantType ("()");
		public static VariantType Any = new VariantType ("*");
		public static VariantType Basic = new VariantType ("?");
		public static VariantType Maybe = new VariantType ("m*");
		public static VariantType Array = new VariantType ("a*");
		public static VariantType Tuple = new VariantType ("r");
		public static VariantType DictEntry = new VariantType ("{?*}");
		public static VariantType Dictionary = new VariantType ("a{?*}");
		public static VariantType StringArray = new VariantType ("as");
		public static VariantType ByteString = new VariantType ("ay");
		public static VariantType ByteStringArray = new VariantType ("aay");

		[DllImport ("libglib-2.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern bool g_variant_type_string_is_valid (IntPtr type_string);

		public static bool StringIsValid (string type_string)
		{
			IntPtr native = Marshaller.StringToPtrGStrdup (type_string);
			bool ret = g_variant_type_string_is_valid (native);
			Marshaller.Free (native);
			return ret;
		}

		private VariantType () {}

		IntPtr handle;
		public IntPtr Handle {
			get { return handle; }
		}

		// Docs say that GVariant is threadsafe.
		~VariantType ()
		{
			Dispose (false);
		}

		public void Dispose ()
		{
			Dispose (true);
		}

		[DllImport ("libglib-2.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern void g_variant_type_free (IntPtr handle);

		void Dispose (bool disposing)
		{
			if (handle == IntPtr.Zero)
				return;

			g_variant_type_free (handle);
			handle = IntPtr.Zero;
			if (disposing)
				GC.SuppressFinalize (this);
		}

		[DllImport ("libglib-2.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr g_variant_type_copy (IntPtr handle);

		public VariantType (IntPtr handle)
		{
			this.handle = g_variant_type_copy (handle);
		}

		[DllImport ("libglib-2.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr g_variant_type_new (IntPtr type_name);

		public VariantType (string type_string)
		{
			IntPtr native = Marshaller.StringToPtrGStrdup (type_string);
			handle = g_variant_type_new (native);
			Marshaller.Free (native);
		}

		public VariantType Copy ()
		{
			return new VariantType (Handle);
		}

		[DllImport ("libglib-2.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern bool g_variant_type_equals (IntPtr a, IntPtr b);

		public override bool Equals (object o)
		{
			return (o is VariantType) && g_variant_type_equals (Handle, (o as VariantType).Handle);
		}

		[DllImport ("libglib-2.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern int g_variant_type_hash (IntPtr a);

		public override int GetHashCode ()
		{
			return g_variant_type_hash (Handle);
		}

		[DllImport ("libglib-2.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr g_variant_type_peek_string (IntPtr a);

		public override string ToString ()
		{
			return Marshaller.Utf8PtrToString (g_variant_type_peek_string (Handle));
		}

		[DllImport ("libglib-2.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern bool g_variant_type_is_array (IntPtr type);

		public bool IsArray {
			get { return g_variant_type_is_array (Handle); }
		}


		[DllImport ("libglib-2.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern bool g_variant_type_is_basic (IntPtr type);

		public bool IsBasic {
			get { return g_variant_type_is_basic (Handle); }
		}

		[DllImport ("libglib-2.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern bool g_variant_type_is_container (IntPtr type);

		public bool IsContainer {
			get { return g_variant_type_is_container (Handle); }
		}

		[DllImport ("libglib-2.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern bool g_variant_type_is_definite (IntPtr type);

		public bool IsDefinite {
			get { return g_variant_type_is_definite (Handle); }
		}

		[DllImport ("libglib-2.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern bool g_variant_type_is_dict_entry (IntPtr type);

		public bool IsDictionaryEntry {
			get { return g_variant_type_is_dict_entry (Handle); }
		}

		[DllImport ("libglib-2.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern bool g_variant_type_is_maybe (IntPtr type);

		public bool IsMaybe {
			get { return g_variant_type_is_maybe (Handle); }
		}

		[DllImport ("libglib-2.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern bool g_variant_type_is_tuple (IntPtr type);

		public bool IsTuple {
			get { return g_variant_type_is_tuple (Handle); }
		}

		[DllImport ("libglib-2.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern bool g_variant_type_is_variant (IntPtr type);

		public bool IsVariant {
			get { return g_variant_type_is_variant (Handle); }
		}

		[DllImport ("libglib-2.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern bool g_variant_type_is_subtype_of (IntPtr type, IntPtr supertype);

		public bool IsSubtypeOf (VariantType supertype)
		{
			return g_variant_type_is_subtype_of (Handle, supertype == null ? IntPtr.Zero : supertype.Handle);
		}

		[DllImport ("libglib-2.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr g_variant_type_element (IntPtr type);

		public VariantType Element ()
		{
			return new VariantType (g_variant_type_element (Handle));
		}

		[DllImport ("libglib-2.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr g_variant_type_first (IntPtr type);

		public VariantType First ()
		{
			return new VariantType (g_variant_type_first (Handle));
		}

		[DllImport ("libglib-2.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr g_variant_type_next (IntPtr type);

		public VariantType Next ()
		{
			return new VariantType (g_variant_type_next (Handle));
		}

		[DllImport ("libglib-2.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr g_variant_type_n_items (IntPtr type);

		public long NItems ()
		{
			return g_variant_type_n_items (Handle).ToInt64 ();
		}

		[DllImport ("libglib-2.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr g_variant_type_key (IntPtr type);

		public VariantType Key ()
		{
			return new VariantType (g_variant_type_key (Handle));
		}

		[DllImport ("libglib-2.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr g_variant_type_value (IntPtr type);

		public VariantType Value ()
		{
			return new VariantType (g_variant_type_value (Handle));
		}

		[DllImport ("libglib-2.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr g_variant_type_new_array (IntPtr element);

		public static VariantType NewArray (VariantType element)
		{
			VariantType result = new VariantType ();
			result.handle = g_variant_type_new_array (element == null ? IntPtr.Zero : element.Handle);
			return result;
		}

		[DllImport ("libglib-2.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr g_variant_type_new_dict_entry (IntPtr key, IntPtr value);

		public static VariantType NewDictionaryEntry (VariantType key, VariantType value)
		{
			VariantType result = new VariantType ();
			result.handle = g_variant_type_new_dict_entry (key == null ? IntPtr.Zero : key.Handle, value == null ? IntPtr.Zero : value.Handle);
			return result;
		}

		[DllImport ("libglib-2.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr g_variant_type_new_maybe (IntPtr element);

		public static VariantType NewMaybe (VariantType element)
		{
			VariantType result = new VariantType ();
			result.handle = g_variant_type_new_maybe (element == null ? IntPtr.Zero : element.Handle);
			return result;
		}

		[DllImport ("libglib-2.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr g_variant_type_new_tuple (IntPtr[] items, int n_items);

		public static VariantType NewTuple (VariantType[] items)
		{
			VariantType result = new VariantType ();
			IntPtr[] native = new IntPtr [items.Length];
			for (int i = 0; i < items.Length; i++)
				native [i] = items [i].Handle;
			result.handle = g_variant_type_new_tuple (native, native.Length);
			return result;
		}
	}
}

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
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate bool d_g_variant_type_string_is_valid(IntPtr type_string);
		static d_g_variant_type_string_is_valid g_variant_type_string_is_valid = FuncLoader.LoadFunction<d_g_variant_type_string_is_valid>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_variant_type_string_is_valid"));

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
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_g_variant_type_free(IntPtr handle);
		static d_g_variant_type_free g_variant_type_free = FuncLoader.LoadFunction<d_g_variant_type_free>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_variant_type_free"));

		void Dispose (bool disposing)
		{
			if (handle == IntPtr.Zero)
				return;

			g_variant_type_free (handle);
			handle = IntPtr.Zero;
			if (disposing)
				GC.SuppressFinalize (this);
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_variant_type_copy(IntPtr handle);
		static d_g_variant_type_copy g_variant_type_copy = FuncLoader.LoadFunction<d_g_variant_type_copy>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_variant_type_copy"));

		public VariantType (IntPtr handle)
		{
			this.handle = g_variant_type_copy (handle);
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_variant_type_new(IntPtr type_name);
		static d_g_variant_type_new g_variant_type_new = FuncLoader.LoadFunction<d_g_variant_type_new>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_variant_type_new"));

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
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate bool d_g_variant_type_equal(IntPtr a, IntPtr b);
		static d_g_variant_type_equal g_variant_type_equal = FuncLoader.LoadFunction<d_g_variant_type_equal>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_variant_type_equal"));

		public override bool Equals (object o)
		{
			return (o is VariantType) && g_variant_type_equal (Handle, (o as VariantType).Handle);
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate int d_g_variant_type_hash(IntPtr a);
		static d_g_variant_type_hash g_variant_type_hash = FuncLoader.LoadFunction<d_g_variant_type_hash>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_variant_type_hash"));

		public override int GetHashCode ()
		{
			return g_variant_type_hash (Handle);
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_variant_type_peek_string(IntPtr a);
		static d_g_variant_type_peek_string g_variant_type_peek_string = FuncLoader.LoadFunction<d_g_variant_type_peek_string>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_variant_type_peek_string"));

		public override string ToString ()
		{
			return Marshaller.Utf8PtrToString (g_variant_type_peek_string (Handle));
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate bool d_g_variant_type_is_array(IntPtr type);
		static d_g_variant_type_is_array g_variant_type_is_array = FuncLoader.LoadFunction<d_g_variant_type_is_array>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_variant_type_is_array"));

		public bool IsArray {
			get { return g_variant_type_is_array (Handle); }
		}

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate bool d_g_variant_type_is_basic(IntPtr type);
		static d_g_variant_type_is_basic g_variant_type_is_basic = FuncLoader.LoadFunction<d_g_variant_type_is_basic>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_variant_type_is_basic"));

		public bool IsBasic {
			get { return g_variant_type_is_basic (Handle); }
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate bool d_g_variant_type_is_container(IntPtr type);
		static d_g_variant_type_is_container g_variant_type_is_container = FuncLoader.LoadFunction<d_g_variant_type_is_container>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_variant_type_is_container"));

		public bool IsContainer {
			get { return g_variant_type_is_container (Handle); }
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate bool d_g_variant_type_is_definite(IntPtr type);
		static d_g_variant_type_is_definite g_variant_type_is_definite = FuncLoader.LoadFunction<d_g_variant_type_is_definite>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_variant_type_is_definite"));

		public bool IsDefinite {
			get { return g_variant_type_is_definite (Handle); }
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate bool d_g_variant_type_is_dict_entry(IntPtr type);
		static d_g_variant_type_is_dict_entry g_variant_type_is_dict_entry = FuncLoader.LoadFunction<d_g_variant_type_is_dict_entry>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_variant_type_is_dict_entry"));

		public bool IsDictionaryEntry {
			get { return g_variant_type_is_dict_entry (Handle); }
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate bool d_g_variant_type_is_maybe(IntPtr type);
		static d_g_variant_type_is_maybe g_variant_type_is_maybe = FuncLoader.LoadFunction<d_g_variant_type_is_maybe>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_variant_type_is_maybe"));

		public bool IsMaybe {
			get { return g_variant_type_is_maybe (Handle); }
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate bool d_g_variant_type_is_tuple(IntPtr type);
		static d_g_variant_type_is_tuple g_variant_type_is_tuple = FuncLoader.LoadFunction<d_g_variant_type_is_tuple>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_variant_type_is_tuple"));

		public bool IsTuple {
			get { return g_variant_type_is_tuple (Handle); }
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate bool d_g_variant_type_is_variant(IntPtr type);
		static d_g_variant_type_is_variant g_variant_type_is_variant = FuncLoader.LoadFunction<d_g_variant_type_is_variant>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_variant_type_is_variant"));

		public bool IsVariant {
			get { return g_variant_type_is_variant (Handle); }
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate bool d_g_variant_type_is_subtype_of(IntPtr type, IntPtr supertype);
		static d_g_variant_type_is_subtype_of g_variant_type_is_subtype_of = FuncLoader.LoadFunction<d_g_variant_type_is_subtype_of>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_variant_type_is_subtype_of"));

		public bool IsSubtypeOf (VariantType supertype)
		{
			return g_variant_type_is_subtype_of (Handle, supertype == null ? IntPtr.Zero : supertype.Handle);
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_variant_type_element(IntPtr type);
		static d_g_variant_type_element g_variant_type_element = FuncLoader.LoadFunction<d_g_variant_type_element>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_variant_type_element"));

		public VariantType Element ()
		{
			return new VariantType (g_variant_type_element (Handle));
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_variant_type_first(IntPtr type);
		static d_g_variant_type_first g_variant_type_first = FuncLoader.LoadFunction<d_g_variant_type_first>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_variant_type_first"));

		public VariantType First ()
		{
			return new VariantType (g_variant_type_first (Handle));
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_variant_type_next(IntPtr type);
		static d_g_variant_type_next g_variant_type_next = FuncLoader.LoadFunction<d_g_variant_type_next>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_variant_type_next"));

		public VariantType Next ()
		{
			return new VariantType (g_variant_type_next (Handle));
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_variant_type_n_items(IntPtr type);
		static d_g_variant_type_n_items g_variant_type_n_items = FuncLoader.LoadFunction<d_g_variant_type_n_items>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_variant_type_n_items"));

		public long NItems ()
		{
			return g_variant_type_n_items (Handle).ToInt64 ();
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_variant_type_key(IntPtr type);
		static d_g_variant_type_key g_variant_type_key = FuncLoader.LoadFunction<d_g_variant_type_key>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_variant_type_key"));

		public VariantType Key ()
		{
			return new VariantType (g_variant_type_key (Handle));
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_variant_type_value(IntPtr type);
		static d_g_variant_type_value g_variant_type_value = FuncLoader.LoadFunction<d_g_variant_type_value>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_variant_type_value"));

		public VariantType Value ()
		{
			return new VariantType (g_variant_type_value (Handle));
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_variant_type_new_array(IntPtr element);
		static d_g_variant_type_new_array g_variant_type_new_array = FuncLoader.LoadFunction<d_g_variant_type_new_array>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_variant_type_new_array"));

		public static VariantType NewArray (VariantType element)
		{
			VariantType result = new VariantType ();
			result.handle = g_variant_type_new_array (element == null ? IntPtr.Zero : element.Handle);
			return result;
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_variant_type_new_dict_entry(IntPtr key, IntPtr value);
		static d_g_variant_type_new_dict_entry g_variant_type_new_dict_entry = FuncLoader.LoadFunction<d_g_variant_type_new_dict_entry>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_variant_type_new_dict_entry"));

		public static VariantType NewDictionaryEntry (VariantType key, VariantType value)
		{
			VariantType result = new VariantType ();
			result.handle = g_variant_type_new_dict_entry (key == null ? IntPtr.Zero : key.Handle, value == null ? IntPtr.Zero : value.Handle);
			return result;
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_variant_type_new_maybe(IntPtr element);
		static d_g_variant_type_new_maybe g_variant_type_new_maybe = FuncLoader.LoadFunction<d_g_variant_type_new_maybe>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_variant_type_new_maybe"));

		public static VariantType NewMaybe (VariantType element)
		{
			VariantType result = new VariantType ();
			result.handle = g_variant_type_new_maybe (element == null ? IntPtr.Zero : element.Handle);
			return result;
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_variant_type_new_tuple(IntPtr[] items, int n_items);
		static d_g_variant_type_new_tuple g_variant_type_new_tuple = FuncLoader.LoadFunction<d_g_variant_type_new_tuple>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_variant_type_new_tuple"));

		public static VariantType NewTuple (VariantType[] items)
		{
			VariantType result = new VariantType ();
			IntPtr[] native = new IntPtr [items.Length];
			for (int i = 0; i < items.Length; i++)
				native [i] = items [i].Handle;
			result.handle = g_variant_type_new_tuple (native, native.Length);
			return result;
		}
		
		// These fields depend on function pointers and therefore must be placed below them.
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
	}
}


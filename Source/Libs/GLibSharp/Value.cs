// GLib.Value.cs - GLib Value class implementation
//
// Author: Mike Kestner <mkestner@speakeasy.net>
//
// Copyright (c) 2001 Mike Kestner
// Copyright (c) 2003-2004 Novell, Inc.
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
	using System.Reflection;
	using System.Runtime.InteropServices;

	[StructLayout (LayoutKind.Sequential)]
	public struct Value : IDisposable {

		IntPtr type;
#pragma warning disable 0414
		long pad1;
		long pad2;
#pragma warning restore 0414

		public static Value Empty;

		public Value (GLib.GType gtype)
		{
			type = IntPtr.Zero;
			pad1 = pad2 = 0;
			g_value_init (ref this, gtype.Val);
		}

		public Value (object obj)
		{
			type = IntPtr.Zero;
			pad1 = pad2 = 0;

			GType gtype = (GType) obj.GetType ();
			g_value_init (ref this, gtype.Val);
			Val = obj;
		}

		public Value (bool val) : this (GType.Boolean)
		{
			g_value_set_boolean (ref this, val);
		}

		public Value (byte val) : this (GType.UChar)
		{
			g_value_set_uchar (ref this, val);
		}

		public Value (sbyte val) : this (GType.Char)
		{
			g_value_set_char (ref this, val);
		}

		public Value (int val) : this (GType.Int)
		{
			g_value_set_int (ref this, val);
		}

		public Value (uint val) : this (GType.UInt)
		{
			g_value_set_uint (ref this, val); 
		}

		public Value (ushort val) : this (GType.UInt)
		{
			g_value_set_uint (ref this, val); 
		}

		public Value (long val) : this (GType.Int64)
		{
			g_value_set_int64 (ref this, val);
		}

		public Value (ulong val) : this (GType.UInt64)
		{
			g_value_set_uint64 (ref this, val);
		}

		public Value (float val) : this (GType.Float)
		{
			g_value_set_float (ref this, val);
		}

		public Value (double val) : this (GType.Double)
		{
			g_value_set_double (ref this, val);
		}

		public Value (string val) : this (GType.String)
		{
			IntPtr native_val = GLib.Marshaller.StringToPtrGStrdup (val);
			g_value_set_string (ref this, native_val); 
			GLib.Marshaller.Free (native_val);
		}

		public Value (ValueArray val) : this (ValueArray.GType)
		{
			g_value_set_boxed (ref this, val.Handle);
		}

		public Value (IntPtr val) : this (GType.Pointer)
		{
			g_value_set_pointer (ref this, val); 
		}

		public Value (Variant variant) : this (GType.Variant)
		{
			g_value_set_variant (ref this, variant == null ? IntPtr.Zero : variant.Handle);
		}

		public Value (Opaque val, string type_name)
		{
			type = IntPtr.Zero;
			pad1 = pad2 = 0;
			g_value_init (ref this, GType.FromName (type_name).Val);
			g_value_set_boxed (ref this, val.Handle);
		}

		public Value (GLib.Object val) : this (val == null ? GType.Object : val.NativeType)
		{
			g_value_set_object (ref this, val == null ? IntPtr.Zero : val.Handle);
		}

		public Value (GLib.GInterfaceAdapter val) : this (val == null ? GType.Object : val.GInterfaceGType)
		{
			g_value_set_object (ref this, val == null ? IntPtr.Zero : val.Handle);
		}

		public Value (GLib.Object obj, string prop_name)
		{
			type = IntPtr.Zero;
			pad1 = pad2 = 0;
			InitForProperty (obj, prop_name);
		}

		[Obsolete]
		public Value (IntPtr obj, string prop_name, Opaque val)
		{
			type = IntPtr.Zero;
			pad1 = pad2 = 0;
			InitForProperty (GLib.Object.GetObject (obj), prop_name);
			g_value_set_boxed (ref this, val.Handle);
		}

		public Value (string[] val) : this (new GLib.GType (g_strv_get_type ()))
		{
			if (val == null) {
				g_value_set_boxed (ref this, IntPtr.Zero);
				return;
			}

			IntPtr native_array = Marshal.AllocHGlobal ((val.Length + 1) * IntPtr.Size);
			for (int i = 0; i < val.Length; i++)
				Marshal.WriteIntPtr (native_array, i * IntPtr.Size, GLib.Marshaller.StringToPtrGStrdup (val[i]));
			Marshal.WriteIntPtr (native_array, val.Length * IntPtr.Size, IntPtr.Zero);

			g_value_set_boxed (ref this, native_array);

			for (int i = 0; i < val.Length; i++)
				GLib.Marshaller.Free (Marshal.ReadIntPtr (native_array, i * IntPtr.Size));
			Marshal.FreeHGlobal (native_array);
		}

		public void Dispose () 
		{
			g_value_unset (ref this);
		}

		public void Init (GLib.GType gtype)
		{
			g_value_init (ref this, gtype.Val);
		}


		public static explicit operator bool (Value val)
		{
			return g_value_get_boolean (ref val);
		}

		public static explicit operator byte (Value val)
		{
			return g_value_get_uchar (ref val);
		}

		public static explicit operator sbyte (Value val)
		{
			return g_value_get_char (ref val);
		}

		public static explicit operator int (Value val)
		{
			return g_value_get_int (ref val);
		}

		public static explicit operator uint (Value val)
		{
			return g_value_get_uint (ref val);
		}

		public static explicit operator ushort (Value val)
		{
			return (ushort) g_value_get_uint (ref val);
		}

		public static explicit operator long (Value val)
		{
			if (val.type == GType.Long.Val)
				return val.GetLongForPlatform ();
			else
				return g_value_get_int64 (ref val);
		}

		public static explicit operator ulong (Value val)
		{
			if (val.type == GType.ULong.Val)
				return val.GetULongForPlatform ();
			else
				return g_value_get_uint64 (ref val);
		}

		public static explicit operator Enum (Value val)
		{
			if (val.HoldsFlags)
				return (Enum)Enum.ToObject (GType.LookupType (val.type), g_value_get_flags (ref val));
			else
				return (Enum)Enum.ToObject (GType.LookupType (val.type), g_value_get_enum (ref val));
		}

		public static explicit operator float (Value val)
		{
			return g_value_get_float (ref val);
		}

		public static explicit operator double (Value val)
		{
			return g_value_get_double (ref val);
		}

		public static explicit operator string (Value val)
		{
			IntPtr str = g_value_get_string (ref val);
			return str == IntPtr.Zero ? null : GLib.Marshaller.Utf8PtrToString (str);
		}

		public static explicit operator ValueArray (Value val)
		{
			return new ValueArray (g_value_get_boxed (ref val));
		}

		public static explicit operator IntPtr (Value val)
		{
			return g_value_get_pointer (ref val);
		}

		public static explicit operator GLib.Opaque (Value val)
		{
			return GLib.Opaque.GetOpaque (g_value_get_boxed (ref val), (Type) new GType (val.type), false);
		}

		public static explicit operator GLib.Variant (Value val)
		{
			IntPtr native_variant = g_value_get_variant (ref val);
			return native_variant == IntPtr.Zero ? null : new Variant (native_variant);
		}

		public static explicit operator GLib.VariantType (Value val)
		{
			return new VariantType (g_value_get_boxed (ref val));
		}

		public static explicit operator GLib.Object (Value val)
		{
			return GLib.Object.GetObject (g_value_get_object (ref val), false);
		}

		public static explicit operator string[] (Value val)
		{
			IntPtr native_array = g_value_get_boxed (ref val);
			if (native_array == IntPtr.Zero)
				return null;

			int count = 0;
			while (Marshal.ReadIntPtr (native_array, count * IntPtr.Size) != IntPtr.Zero)
				count++;
			string[] strings = new string[count];
			for (int i = 0; i < count; i++)
				strings[i] = GLib.Marshaller.Utf8PtrToString (Marshal.ReadIntPtr (native_array, i * IntPtr.Size));
			return strings;
		}

		object ToRegisteredType () {
			Type t = GLib.GType.LookupType (type);
			ConstructorInfo ci = null;
			
			try {
				while (ci == null && t != null) {
					if (!t.IsAbstract)
						ci = t.GetConstructor (new Type[] { typeof (GLib.Value) });
					if (ci == null)
						t = t.BaseType;
				}
			} catch (Exception) {
				ci = null;
			}

			if (ci == null)
				throw new Exception ("Unknown type " + new GType (type).ToString ());
			
			return ci.Invoke (new object[] {this});
		}

		void FromRegisteredType (object val) {
			Type t = GLib.GType.LookupType (type);
			MethodInfo mi = null;
			
			try {
				while (mi == null && t != null) {
					mi = t.GetMethod ("SetGValue", new Type[] { Type.GetType ("GLib.Value&") });
					if (mi != null && (mi.IsAbstract || mi.ReturnType != typeof (void)))
						mi = null;
					if (mi == null)
						t = t.BaseType;
				}
			} catch (Exception) {
				mi = null;
			}
			
			if (mi == null)
				throw new Exception ("Unknown type " + new GType (type).ToString ());
			
			object[] parameters = new object[] { this };
			mi.Invoke (val, parameters);
			this = (GLib.Value) parameters[0];
		}

		long GetLongForPlatform ()
		{
			switch (Environment.OSVersion.Platform) {
			case PlatformID.Win32NT:
			case PlatformID.Win32S:
			case PlatformID.Win32Windows:
			case PlatformID.WinCE:
				return (long) g_value_get_long_as_int (ref this);
			default:
				return g_value_get_long (ref this).ToInt64 ();
			}
		}

		ulong GetULongForPlatform ()
		{
			switch (Environment.OSVersion.Platform) {
			case PlatformID.Win32NT:
			case PlatformID.Win32S:
			case PlatformID.Win32Windows:
			case PlatformID.WinCE:
				return (ulong) g_value_get_ulong_as_uint (ref this);
			default:
				return g_value_get_ulong (ref this).ToUInt64 ();
			}
		}

		void SetLongForPlatform (long val)
		{
			switch (Environment.OSVersion.Platform) {
			case PlatformID.Win32NT:
			case PlatformID.Win32S:
			case PlatformID.Win32Windows:
			case PlatformID.WinCE:
				g_value_set_long2 (ref this, (int) val);
				break;
			default:
				g_value_set_long (ref this, new IntPtr (val));
				break;
			}
		}

		void SetULongForPlatform (ulong val)
		{
			switch (Environment.OSVersion.Platform) {
			case PlatformID.Win32NT:
			case PlatformID.Win32S:
			case PlatformID.Win32Windows:
			case PlatformID.WinCE:
				g_value_set_ulong2 (ref this, (uint) val);
				break;
			default:
				g_value_set_ulong (ref this, new UIntPtr (val));
				break;
			}
		}

		object ToEnum ()
		{
			Type t = GType.LookupType (type);
			
			if (t == null) {
				if (HoldsFlags)
					return g_value_get_flags (ref this);
				else
					return g_value_get_enum (ref this);
			} else {
				return (Enum) this;
			}
		}

		object ToBoxed ()
		{
			IntPtr boxed_ptr = g_value_get_boxed (ref this);

			if (boxed_ptr == IntPtr.Zero)
				return null;

			Type t = GType.LookupType (type);
			if (t == null)
				throw new Exception ("Unknown type " + new GType (type).ToString ());
			else if (t.IsSubclassOf (typeof (GLib.Opaque)))
				return (GLib.Opaque) this;

			MethodInfo mi = t.GetMethod ("New", BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy, null, new Type[] { typeof(IntPtr) }, null);
			if (mi != null)
				return mi.Invoke (null, new object[] {boxed_ptr});

			ConstructorInfo ci = t.GetConstructor (new Type[] { typeof(IntPtr), typeof (bool) });
			if (ci != null)
				return ci.Invoke (new object[] { boxed_ptr, false });

			ci = t.GetConstructor (new Type[] { typeof(IntPtr) });
			if (ci != null)
				return ci.Invoke (new object[] { boxed_ptr });

			return Marshal.PtrToStructure (boxed_ptr, t);
		}

		public object Val
		{
			get {
				if (type == GType.Boolean.Val)
					return (bool) this;
				else if (type == GType.UChar.Val)
					return (byte) this;
				else if (type == GType.Char.Val)
					return (sbyte) this;
				else if (type == GType.Int.Val)
					return (int) this;
				else if (type == GType.UInt.Val)
					return (uint) this;
				else if (type == GType.Int64.Val)
					return (long) this;
				else if (type == GType.Long.Val)
					return GetLongForPlatform ();
				else if (type == GType.UInt64.Val)
					return (ulong) this;
				else if (type == GType.ULong.Val)
					return GetULongForPlatform ();
				else if (GType.Is (type, GType.Enum) ||
					 GType.Is (type, GType.Flags))
					return ToEnum ();
				else if (type == GType.Float.Val)
					return (float) this;
				else if (type == GType.Double.Val)
					return (double) this;
				else if (type == GType.String.Val)
					return (string) this;
				else if (type == GType.Pointer.Val)
					return (IntPtr) this;
				else if (type == GType.Variant.Val)
					return (GLib.Variant) this;
				else if (type == GType.Param.Val)
					return g_value_get_param (ref this);
				else if (type == ValueArray.GType.Val)
					return new ValueArray (g_value_get_boxed (ref this));
				else if (type == ManagedValue.GType.Val)
					return ManagedValue.ObjectForWrapper (g_value_get_boxed (ref this));
				else if (GType.Is (type, GType.Object))
					return (GLib.Object) this;
				else if (GType.Is (type, GType.Boxed))
					return ToBoxed ();
				else if (GType.LookupType (type) != null)
					return ToRegisteredType ();
				else if (type == IntPtr.Zero)
					return null;
				else
					throw new Exception ("Unknown type " + new GType (type).ToString ());
			}
			set {
				if (type == GType.Boolean.Val)
					g_value_set_boolean (ref this, (bool) value);
				else if (type == GType.UChar.Val)
					g_value_set_uchar (ref this, (byte) value);
				else if (type == GType.Char.Val)
					g_value_set_char (ref this, (sbyte) value);
				else if (type == GType.Int.Val)
					g_value_set_int (ref this, (int) value);
				else if (type == GType.UInt.Val)
					g_value_set_uint (ref this, (uint) value);
				else if (type == GType.Int64.Val)
					g_value_set_int64 (ref this, (long) value);
				else if (type == GType.Long.Val)
					SetLongForPlatform ((long) value);
				else if (type == GType.UInt64.Val)
					g_value_set_uint64 (ref this, (ulong) value);
				else if (type == GType.ULong.Val)
					SetULongForPlatform (Convert.ToUInt64 (value));
				else if (GType.Is (type, GType.Enum))
					g_value_set_enum (ref this, (int)value);
				else if (GType.Is (type, GType.Flags))
					g_value_set_flags (ref this, (uint)(int)value);
				else if (type == GType.Float.Val)
					g_value_set_float (ref this, (float) value);
				else if (type == GType.Double.Val)
					g_value_set_double (ref this, (double) value);
				else if (type == GType.Variant.Val)
					g_value_set_variant (ref this, ((GLib.Variant) value).Handle);
				else if (type == GType.String.Val) {
					IntPtr native = GLib.Marshaller.StringToPtrGStrdup ((string)value);
					g_value_set_string (ref this, native);
					GLib.Marshaller.Free (native);
				} else if (type == GType.Pointer.Val) {
					if (value.GetType () == typeof (IntPtr)) {
						g_value_set_pointer (ref this, (IntPtr) value);
						return;
					} else if (value is IWrapper) {
						g_value_set_pointer (ref this, ((IWrapper)value).Handle);
						return;
					}
					IntPtr buf = Marshal.AllocHGlobal (Marshal.SizeOf (value.GetType()));
					Marshal.StructureToPtr (value, buf, false);
					g_value_set_pointer (ref this, buf);
				} else if (type == GType.Param.Val) {
					g_value_set_param (ref this, (IntPtr) value);
				} else if (type == ValueArray.GType.Val) {
					g_value_set_boxed (ref this, ((ValueArray) value).Handle);
				} else if (type == ManagedValue.GType.Val) {
					IntPtr wrapper = ManagedValue.WrapObject (value);
					g_value_set_boxed (ref this, wrapper);
					ManagedValue.ReleaseWrapper (wrapper);
				} else if (GType.Is (type, GType.Object))
					if(value is GLib.Object)
						g_value_set_object (ref this, (value as GLib.Object).Handle);
					else
						g_value_set_object (ref this, ((GInterfaceAdapter)value).Handle);
				else if (GType.Is (type, GType.Boxed)) {
					if (value is IWrapper) {
						g_value_set_boxed (ref this, ((IWrapper)value).Handle);
						return;
					}
					IntPtr buf = Marshaller.StructureToPtrAlloc (value);
					g_value_set_boxed (ref this, buf);
					Marshal.FreeHGlobal (buf);
				} else if (GLib.GType.LookupType (type) != null) {
					FromRegisteredType (value);
				} else
					throw new Exception ("Unknown type " + new GType (type).ToString ());
			}
		}

		internal void Update (object val)
		{
			if (GType.Is (type, GType.Boxed) && val != null && !(val is IWrapper)) {
				MethodInfo mi = val.GetType ().GetMethod ("Update", BindingFlags.NonPublic | BindingFlags.Instance);
				IntPtr boxed_ptr = g_value_get_boxed (ref this);

				if (mi == null && !val.GetType ().IsDefined (typeof(StructLayoutAttribute), false))
					return;

				if (mi == null)
					Marshal.StructureToPtr (val, boxed_ptr, false);
				else
					mi.Invoke (val, null);
			}
		}

		bool HoldsFlags {
			get { return g_type_check_value_holds (ref this, GType.Flags.Val); }
		}

		void InitForProperty (Object obj, string name)
		{
			GType gtype = obj.NativeType;
			InitForProperty (gtype, name);
		}

		void InitForProperty (GType gtype, string name)
		{
			IntPtr p_name = Marshaller.StringToPtrGStrdup (name);
			IntPtr spec_ptr = g_object_class_find_property (gtype.GetClassPtr (), p_name);
			Marshaller.Free (p_name);

			if (spec_ptr == IntPtr.Zero)
				throw new Exception (String.Format ("No property with name '{0}' in type '{1}'", name, gtype.ToString()));
			
			ParamSpec spec = new ParamSpec (spec_ptr);
			g_value_init (ref this, spec.ValueType.Val);
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_object_class_find_property(IntPtr klass, IntPtr name);
		static d_g_object_class_find_property g_object_class_find_property = FuncLoader.LoadFunction<d_g_object_class_find_property>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_object_class_find_property"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate bool d_g_type_check_value_holds(ref Value val, IntPtr gtype);
		static d_g_type_check_value_holds g_type_check_value_holds = FuncLoader.LoadFunction<d_g_type_check_value_holds>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_type_check_value_holds"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_g_value_init(ref GLib.Value val, IntPtr gtype);
		static d_g_value_init g_value_init = FuncLoader.LoadFunction<d_g_value_init>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_value_init"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_g_value_unset(ref GLib.Value val);
		static d_g_value_unset g_value_unset = FuncLoader.LoadFunction<d_g_value_unset>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_value_unset"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_g_value_set_boolean(ref Value val, bool data);
		static d_g_value_set_boolean g_value_set_boolean = FuncLoader.LoadFunction<d_g_value_set_boolean>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_value_set_boolean"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_g_value_set_uchar(ref Value val, byte data);
		static d_g_value_set_uchar g_value_set_uchar = FuncLoader.LoadFunction<d_g_value_set_uchar>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_value_set_uchar"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_g_value_set_char(ref Value val, sbyte data);
		static d_g_value_set_char g_value_set_char = FuncLoader.LoadFunction<d_g_value_set_char>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_value_set_char"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_g_value_set_boxed(ref Value val, IntPtr data);
		static d_g_value_set_boxed g_value_set_boxed = FuncLoader.LoadFunction<d_g_value_set_boxed>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_value_set_boxed"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_g_value_set_double(ref Value val, double data);
		static d_g_value_set_double g_value_set_double = FuncLoader.LoadFunction<d_g_value_set_double>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_value_set_double"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_g_value_set_float(ref Value val, float data);
		static d_g_value_set_float g_value_set_float = FuncLoader.LoadFunction<d_g_value_set_float>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_value_set_float"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_g_value_set_int(ref Value val, int data);
		static d_g_value_set_int g_value_set_int = FuncLoader.LoadFunction<d_g_value_set_int>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_value_set_int"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_g_value_set_int64(ref Value val, long data);
		static d_g_value_set_int64 g_value_set_int64 = FuncLoader.LoadFunction<d_g_value_set_int64>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_value_set_int64"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_g_value_set_long(ref Value val, IntPtr data);
		static d_g_value_set_long g_value_set_long = FuncLoader.LoadFunction<d_g_value_set_long>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_value_set_long"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_g_value_set_long2(ref Value val, int data);
		static d_g_value_set_long2 g_value_set_long2 = FuncLoader.LoadFunction<d_g_value_set_long2>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_value_set_long"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_g_value_set_uint64(ref Value val, ulong data);
		static d_g_value_set_uint64 g_value_set_uint64 = FuncLoader.LoadFunction<d_g_value_set_uint64>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_value_set_uint64"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_g_value_set_object(ref Value val, IntPtr data);
		static d_g_value_set_object g_value_set_object = FuncLoader.LoadFunction<d_g_value_set_object>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_value_set_object"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_g_value_set_param(ref Value val, IntPtr data);
		static d_g_value_set_param g_value_set_param = FuncLoader.LoadFunction<d_g_value_set_param>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_value_set_param"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_g_value_set_pointer(ref Value val, IntPtr data);
		static d_g_value_set_pointer g_value_set_pointer = FuncLoader.LoadFunction<d_g_value_set_pointer>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_value_set_pointer"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_g_value_set_string(ref Value val, IntPtr data);
		static d_g_value_set_string g_value_set_string = FuncLoader.LoadFunction<d_g_value_set_string>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_value_set_string"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_g_value_set_uint(ref Value val, uint data);
		static d_g_value_set_uint g_value_set_uint = FuncLoader.LoadFunction<d_g_value_set_uint>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_value_set_uint"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_g_value_set_ulong(ref Value val, UIntPtr data);
		static d_g_value_set_ulong g_value_set_ulong = FuncLoader.LoadFunction<d_g_value_set_ulong>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_value_set_ulong"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_g_value_set_ulong2(ref Value val, uint data);
		static d_g_value_set_ulong2 g_value_set_ulong2 = FuncLoader.LoadFunction<d_g_value_set_ulong2>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_value_set_ulong"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_g_value_set_enum(ref Value val, int data);
		static d_g_value_set_enum g_value_set_enum = FuncLoader.LoadFunction<d_g_value_set_enum>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_value_set_enum"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_g_value_set_flags(ref Value val, uint data);
		static d_g_value_set_flags g_value_set_flags = FuncLoader.LoadFunction<d_g_value_set_flags>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_value_set_flags"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_g_value_set_variant(ref Value val, IntPtr data);
		static d_g_value_set_variant g_value_set_variant = FuncLoader.LoadFunction<d_g_value_set_variant>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_value_set_variant"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate bool d_g_value_get_boolean(ref Value val);
		static d_g_value_get_boolean g_value_get_boolean = FuncLoader.LoadFunction<d_g_value_get_boolean>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_value_get_boolean"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate byte d_g_value_get_uchar(ref Value val);
		static d_g_value_get_uchar g_value_get_uchar = FuncLoader.LoadFunction<d_g_value_get_uchar>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_value_get_uchar"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate sbyte d_g_value_get_char(ref Value val);
		static d_g_value_get_char g_value_get_char = FuncLoader.LoadFunction<d_g_value_get_char>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_value_get_char"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_value_get_boxed(ref Value val);
		static d_g_value_get_boxed g_value_get_boxed = FuncLoader.LoadFunction<d_g_value_get_boxed>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_value_get_boxed"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate double d_g_value_get_double(ref Value val);
		static d_g_value_get_double g_value_get_double = FuncLoader.LoadFunction<d_g_value_get_double>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_value_get_double"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate float d_g_value_get_float(ref Value val);
		static d_g_value_get_float g_value_get_float = FuncLoader.LoadFunction<d_g_value_get_float>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_value_get_float"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate int d_g_value_get_int(ref Value val);
		static d_g_value_get_int g_value_get_int = FuncLoader.LoadFunction<d_g_value_get_int>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_value_get_int"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate long d_g_value_get_int64(ref Value val);
		static d_g_value_get_int64 g_value_get_int64 = FuncLoader.LoadFunction<d_g_value_get_int64>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_value_get_int64"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_value_get_long(ref Value val);
		static d_g_value_get_long g_value_get_long = FuncLoader.LoadFunction<d_g_value_get_long>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_value_get_long"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate int d_g_value_get_long_as_int(ref Value val);
		static d_g_value_get_long_as_int g_value_get_long_as_int = FuncLoader.LoadFunction<d_g_value_get_long_as_int>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_value_get_long"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate ulong d_g_value_get_uint64(ref Value val);
		static d_g_value_get_uint64 g_value_get_uint64 = FuncLoader.LoadFunction<d_g_value_get_uint64>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_value_get_uint64"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate UIntPtr d_g_value_get_ulong(ref Value val);
		static d_g_value_get_ulong g_value_get_ulong = FuncLoader.LoadFunction<d_g_value_get_ulong>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_value_get_ulong"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate int d_g_value_get_ulong_as_uint(ref Value val);
		static d_g_value_get_ulong_as_uint g_value_get_ulong_as_uint = FuncLoader.LoadFunction<d_g_value_get_ulong_as_uint>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_value_get_ulong"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_value_get_object(ref Value val);
		static d_g_value_get_object g_value_get_object = FuncLoader.LoadFunction<d_g_value_get_object>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_value_get_object"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_value_get_param(ref Value val);
		static d_g_value_get_param g_value_get_param = FuncLoader.LoadFunction<d_g_value_get_param>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_value_get_param"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_value_get_pointer(ref Value val);
		static d_g_value_get_pointer g_value_get_pointer = FuncLoader.LoadFunction<d_g_value_get_pointer>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_value_get_pointer"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_value_get_string(ref Value val);
		static d_g_value_get_string g_value_get_string = FuncLoader.LoadFunction<d_g_value_get_string>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_value_get_string"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate uint d_g_value_get_uint(ref Value val);
		static d_g_value_get_uint g_value_get_uint = FuncLoader.LoadFunction<d_g_value_get_uint>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_value_get_uint"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate int d_g_value_get_enum(ref Value val);
		static d_g_value_get_enum g_value_get_enum = FuncLoader.LoadFunction<d_g_value_get_enum>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_value_get_enum"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate uint d_g_value_get_flags(ref Value val);
		static d_g_value_get_flags g_value_get_flags = FuncLoader.LoadFunction<d_g_value_get_flags>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_value_get_flags"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_strv_get_type();
		static d_g_strv_get_type g_strv_get_type = FuncLoader.LoadFunction<d_g_strv_get_type>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_strv_get_type"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_value_get_variant(ref Value val);
		static d_g_value_get_variant g_value_get_variant = FuncLoader.LoadFunction<d_g_value_get_variant>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_value_get_variant"));
	}
}


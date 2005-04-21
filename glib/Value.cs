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
	using System.Collections;
	using System.Runtime.InteropServices;

	[StructLayout (LayoutKind.Sequential)]
	public struct Value : IDisposable {

		IntPtr type;
		long pad_1;
		long pad_2;

		public static Value Empty;

		[DllImport("libgobject-2.0-0.dll")]
		static extern void g_value_init (ref GLib.Value val, IntPtr gtype);

		[DllImport("libgobject-2.0-0.dll")]
		static extern void g_value_unset (ref GLib.Value val);

		[DllImport("glibsharpglue-2")]
		static extern IntPtr gtksharp_value_create_from_property(ref GLib.Value val, IntPtr obj, IntPtr name);

		[DllImport("glibsharpglue-2")]
		static extern IntPtr gtksharp_value_create_from_type_and_property(ref GLib.Value val, IntPtr gtype, IntPtr name);

		[DllImport("glibsharpglue-2")]
		static extern IntPtr gtksharp_value_create_from_type_name(ref GLib.Value val, IntPtr type_name);

		public void Dispose () 
		{
			g_value_unset (ref this);
		}

		public void Init (GLib.GType gtype)
		{
			g_value_init (ref this, gtype.Val);
		}

		public Value (GLib.GType gtype)
		{
			type = IntPtr.Zero;
			pad_1 = pad_2 = 0;
			g_value_init (ref this, gtype.Val);
		}

		public Value (GLib.Object obj, string prop_name)
		{
			type = IntPtr.Zero;
			pad_1 = pad_2 = 0;
			IntPtr prop = GLib.Marshaller.StringToPtrGStrdup (prop_name);
			gtksharp_value_create_from_property (ref this, obj.Handle, prop);
			GLib.Marshaller.Free (prop);
		}

		[DllImport("libgobject-2.0-0.dll")]
		static extern void g_value_set_boolean (ref Value val, bool data);

		public Value (bool val) : this (GType.Boolean)
		{
			g_value_set_boolean (ref this, val);
		}

		[DllImport("libgobject-2.0-0.dll")]
		static extern void g_value_set_boxed (ref Value val, IntPtr data);

		public Value (Opaque val, string type_name)
		{
			type = IntPtr.Zero;
			pad_1 = pad_2 = 0;
			IntPtr native = GLib.Marshaller.StringToPtrGStrdup (type_name);
			gtksharp_value_create_from_type_name (ref this, native);
			GLib.Marshaller.Free (native);
			g_value_set_boxed (ref this, val.Handle);
		}

		[Obsolete]
		public Value (IntPtr obj, string prop_name, Opaque val)
		{
			type = IntPtr.Zero;
			pad_1 = pad_2 = 0;
			IntPtr native = GLib.Marshaller.StringToPtrGStrdup (prop_name);
			gtksharp_value_create_from_property (ref this, obj, native);
			GLib.Marshaller.Free (native);
			g_value_set_boxed (ref this, val.Handle);
		}

		[DllImport("libgobject-2.0-0.dll")]
		static extern void g_value_set_double (ref Value val, double data);

		public Value (double val) : this (GType.Double)
		{
			g_value_set_double (ref this, val);
		}

		[DllImport("libgobject-2.0-0.dll")]
		static extern void g_value_set_float (ref Value val, float data);

		public Value (float val) : this (GType.Float)
		{
			g_value_set_float (ref this, val);
		}

		[DllImport("libgobject-2.0-0.dll")]
		static extern void g_value_set_int (ref Value val, int data);

		public Value (int val) : this (GType.Int)
		{
			g_value_set_int (ref this, val);
		}

		[DllImport("libgobject-2.0-0.dll")]
		static extern void g_value_set_int64 (ref Value val, long data);

		public Value (long val) : this (GType.Int64)
		{
			g_value_set_int64 (ref this, val);
		}

		[DllImport("libgobject-2.0-0.dll")]
		static extern void g_value_set_uint64 (ref Value val, ulong data);

		public Value (ulong val) : this (GType.UInt64)
		{
			g_value_set_uint64 (ref this, val);
		}


		[DllImport("libgobject-2.0-0.dll")]
		static extern void g_value_set_object (ref Value val, IntPtr data);

		public Value (GLib.Object val) : this (val == null ? GType.Object : val.NativeType)
		{
			g_value_set_object (ref this, val == null ? IntPtr.Zero : val.Handle);
		}

		[DllImport("libgobject-2.0-0.dll")]
		static extern void g_value_set_pointer (ref Value val, IntPtr data);

		public Value (IntPtr val) : this (GType.Pointer)
		{
			g_value_set_pointer (ref this, val); 
		}

		[DllImport("libgobject-2.0-0.dll")]
		static extern void g_value_set_string (ref Value val, IntPtr data);

		public Value (string val) : this (GType.String)
		{
			IntPtr native_val = GLib.Marshaller.StringToPtrGStrdup (val);
			g_value_set_string (ref this, native_val); 
			GLib.Marshaller.Free (native_val);
		}

		[DllImport("libgobject-2.0-0.dll")]
		static extern void g_value_set_uint (ref Value val, uint data);

		public Value (uint val) : this (GType.UInt)
		{
			g_value_set_uint (ref this, val); 
		}

		public Value (ushort val) : this (GType.UInt)
		{
			g_value_set_uint (ref this, val); 
		}

		[DllImport("libgobject-2.0-0.dll")]
		static extern void g_value_set_enum (ref Value val, int data);
		[DllImport("libgobject-2.0-0.dll")]
		static extern void g_value_set_flags (ref Value val, uint data);
		[DllImport("libgobject-2.0-0.dll")]
		static extern void g_value_set_char (ref Value val, char data);
		
		public Value (EnumWrapper wrap, string type_name)
		{
			type = IntPtr.Zero;
			pad_1 = pad_2 = 0;
			IntPtr native = GLib.Marshaller.StringToPtrGStrdup (type_name);
			gtksharp_value_create_from_type_name (ref this, native);
			GLib.Marshaller.Free (native);
			if (wrap.flags)
				g_value_set_flags (ref this, (uint) (int) wrap); 
			else
				g_value_set_enum (ref this, (int) wrap); 
		}

		[Obsolete]
		public Value (GLib.Object obj, string prop_name, EnumWrapper wrap)
		{
			type = IntPtr.Zero;
			pad_1 = pad_2 = 0;
			IntPtr native = GLib.Marshaller.StringToPtrGStrdup (prop_name);
			gtksharp_value_create_from_type_and_property (ref this, obj.NativeType.Val, native);
			GLib.Marshaller.Free (native);
			if (wrap.flags)
				g_value_set_flags (ref this, (uint) (int) wrap); 
			else
				g_value_set_enum (ref this, (int) wrap); 
		}

		public Value (object obj)
		{
			type = IntPtr.Zero;
			pad_1 = pad_2 = 0;

			GType gtype = TypeConverter.LookupType (obj.GetType ());
			if (gtype == GType.Object) {
				g_value_init (ref this, ((GLib.Object) obj).NativeType.Val);
			} else {
				g_value_init (ref this, gtype.Val);
			}

			Val = obj;
		}

		[DllImport("libgobject-2.0-0.dll")]
		static extern bool g_value_get_boolean (ref Value val);

		public static explicit operator bool (Value val)
		{
			return g_value_get_boolean (ref val);
		}

		[DllImport("libgobject-2.0-0.dll")]
		static extern IntPtr g_value_get_boxed (ref Value val);

		public static explicit operator GLib.Opaque (Value val)
		{
			return GLib.Opaque.GetOpaque (g_value_get_boxed (ref val));
		}

		public static explicit operator GLib.Boxed (Value val)
		{
			return new GLib.Boxed (g_value_get_boxed (ref val));
		}

		[DllImport("libgobject-2.0-0.dll")]
		static extern double g_value_get_double (ref Value val);

		public static explicit operator double (Value val)
		{
			return g_value_get_double (ref val);
		}

		[DllImport("libgobject-2.0-0.dll")]
		static extern float g_value_get_float (ref Value val);

		public static explicit operator float (Value val)
		{
			return g_value_get_float (ref val);
		}

		[DllImport("libgobject-2.0-0.dll")]
		static extern int g_value_get_int (ref Value val);

		public static explicit operator int (Value val)
		{
			return g_value_get_int (ref val);
		}

		[DllImport("libgobject-2.0-0.dll")]
		static extern long g_value_get_int64 (ref Value val);

		public static explicit operator long (Value val)
		{
			return g_value_get_int64 (ref val);
		}

		[DllImport("libgobject-2.0-0.dll")]
		static extern ulong g_value_get_uint64 (ref Value val);

		public static explicit operator ulong (Value val)
		{
			return g_value_get_uint64 (ref val);
		}


		[DllImport("libgobject-2.0-0.dll")]
		static extern IntPtr g_value_get_object (ref Value val);

		public static explicit operator GLib.Object (Value val)
		{
			return GLib.Object.GetObject(g_value_get_object (ref val), false);
		}

		public static explicit operator GLib.UnwrappedObject (Value val)
		{
			return new UnwrappedObject(g_value_get_object (ref val));
		}

		[DllImport("libgobject-2.0-0.dll")]
		static extern IntPtr g_value_get_pointer (ref Value val);

		public static explicit operator IntPtr (Value val)
		{
			return g_value_get_pointer (ref val);
		}

		[DllImport("libgobject-2.0-0.dll")]
		static extern IntPtr g_value_get_string (ref Value val);

		public static explicit operator String (Value val)
		{
			IntPtr str = g_value_get_string (ref val);
			return str == IntPtr.Zero ? null : GLib.Marshaller.Utf8PtrToString (str);
		}

		[DllImport("libgobject-2.0-0.dll")]
		static extern uint g_value_get_uint (ref Value val);

		public static explicit operator uint (Value val)
		{
			return g_value_get_uint (ref val);
		}

		public static explicit operator ushort (Value val)
		{
			return (ushort) g_value_get_uint (ref val);
		}

		[DllImport("libgobject-2.0-0.dll")]
		static extern int g_value_get_enum (ref Value val);
		[DllImport("libgobject-2.0-0.dll")]
		static extern uint g_value_get_flags (ref Value val);
		[DllImport("glibsharpglue-2")]
		static extern bool glibsharp_value_holds_flags (ref Value val);

		public static explicit operator EnumWrapper (Value val)
		{
			if (glibsharp_value_holds_flags (ref val))
				return new EnumWrapper ((int)g_value_get_flags (ref val), true);
			else
				return new EnumWrapper (g_value_get_enum (ref val), false);
		}

		[DllImport("glibsharpglue-2")]
		static extern IntPtr gtksharp_value_get_value_type (ref Value val);

		[DllImport("libgobject-2.0-0.dll")]
		static extern bool g_type_is_a (IntPtr type, IntPtr is_a_type);
		
		public object Val
		{
			get {
				GLib.GType type = new GLib.GType (gtksharp_value_get_value_type (ref this));
				if (type == ManagedValue.GType) {
					return ManagedValue.ObjectForWrapper (g_value_get_boxed (ref this));
				}

				if (type == GType.String)
					return (string) this;
				else if (type == GType.Boolean)
					return (bool) this;
				else if (type == GType.Int)
					return (int) this;
				else if (type == GType.Int64)
					return (long) this;
				else if (type == GType.UInt64)
					return (ulong) this;
				else if (type == GType.Double)
					return (double) this;
				else if (type == GType.Float)
					return (float) this;
				else if (type == GType.Char)
					return (char) this;
				else if (type == GType.UInt)
					return (uint) this;
				else if (g_type_is_a (type.Val, GType.Object.Val))
					return (GLib.Object) this;
				else
					throw new Exception ("Unknown type");
			}
			set {
				IntPtr buf;
				GType type = TypeConverter.LookupType (value.GetType());
				if (type == ManagedValue.GType)
					g_value_set_boxed (ref this, ManagedValue.WrapObject (value));
				else if (type == GType.String) {
					IntPtr native = GLib.Marshaller.StringToPtrGStrdup ((string)value);
					g_value_set_string (ref this, native);
					GLib.Marshaller.Free (native);
				} else if (type == GType.Boolean)
					g_value_set_boolean (ref this, (bool) value);
				else if (type == GType.Int)
					g_value_set_int (ref this, (int) value);
				else if (type == GType.Int64)
					g_value_set_int64 (ref this, (long) value);
				else if (type == GType.UInt64)
					g_value_set_uint64 (ref this, (ulong) value);
				else if (type == GType.Double)
					g_value_set_double (ref this, (double) value);
				else if (type == GType.Float)
					g_value_set_float (ref this, (float) value);
				else if (type == GType.Char)
					g_value_set_char (ref this, (char) value);
				else if (type == GType.UInt)
					g_value_set_uint (ref this, (uint) value);
				else if (type == GType.Object)
					g_value_set_object (ref this, ((GLib.Object) value).Handle);
				else if (type == GType.Pointer) {
					if (value is IWrapper) {
						g_value_set_pointer (ref this, ((IWrapper)value).Handle);
						return;
					}
					buf = Marshal.AllocHGlobal (Marshal.SizeOf (value.GetType()));
					Marshal.StructureToPtr (value, buf, false);
					g_value_set_pointer (ref this, buf);
				} else if (g_type_is_a (type.Val, GLib.GType.Boxed.Val)) {
					if (value is IWrapper) {
						g_value_set_boxed (ref this, ((IWrapper)value).Handle);
						return;
					}
					buf = Marshal.AllocHGlobal (Marshal.SizeOf (value.GetType()));
					Marshal.StructureToPtr (value, buf, false);
					g_value_set_boxed (ref this, buf);
					Marshal.FreeHGlobal (buf);
				} else
					throw new Exception ("Unknown type");
			}
		}
	}
}

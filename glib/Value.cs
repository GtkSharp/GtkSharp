// GLib.Value.cs - GLib Value class implementation
//
// Author: Mike Kestner <mkestner@speakeasy.net>
//
// Copyright (c) 2001 Mike Kestner
// Copyright (c) 2003-2004 Novell, Inc.

namespace GLib {

	using System;
	using System.Collections;
	using System.Runtime.InteropServices;
	using GLibSharp;

	[StructLayout (LayoutKind.Sequential)]
	public struct Value : IDisposable {

		GType type;
		long pad_1;
		long pad_2;

		public static Value Empty;

		[DllImport("libgobject-2.0-0.dll")]
		static extern void g_value_init (ref GLib.Value val, IntPtr gtype);

		[DllImport("libgobject-2.0-0.dll")]
		static extern void g_value_unset (ref GLib.Value val);

		[DllImport("glibsharpglue")]
		static extern IntPtr gtksharp_value_create_from_property(ref GLib.Value val, IntPtr obj, string name);

		public void Dispose () 
		{
			g_value_unset (ref this);
		}

		public Value (GLib.GType gtype)
		{
			type = GType.Invalid;
			pad_1 = pad_2 = 0;
			g_value_init (ref this, gtype.Val);
		}

		public Value (GLib.Object obj, string prop_name)
		{
			type = GType.Invalid;
			pad_1 = pad_2 = 0;
			gtksharp_value_create_from_property (ref this, obj.Handle, prop_name);
		}

		[DllImport("libgobject-2.0-0.dll")]
		static extern void g_value_set_boolean (ref Value val, bool data);

		public Value (bool val) : this (GType.Boolean)
		{
			g_value_set_boolean (ref this, val);
		}

		[DllImport("libgobject-2.0-0.dll")]
		static extern void g_value_set_boxed (ref Value val, IntPtr data);

/*
		public Value (GLib.Boxed val)
		{
			_val = gtksharp_value_create(GType.Boxed);
			//g_value_set_boxed (_val, val.Handle);
		}

		public Value (IntPtr obj, string prop_name, Boxed val)
		{
			_val = gtksharp_value_create_from_property (obj, prop_name);
			//g_value_set_boxed (_val, val.Handle);
		}
*/

		public Value (IntPtr obj, string prop_name, Opaque val)
		{
			type = GType.Invalid;
			pad_1 = pad_2 = 0;
			gtksharp_value_create_from_property (ref this, obj, prop_name);
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
		static extern void g_value_set_object (ref Value val, IntPtr data);

		public Value (GLib.Object val) : this (val.NativeType)
		{
			g_value_set_object (ref this, val.Handle);
		}

		[DllImport("libgobject-2.0-0.dll")]
		static extern void g_value_set_pointer (ref Value val, IntPtr data);

		public Value (IntPtr val) : this (GType.Pointer)
		{
			g_value_set_pointer (ref this, val); 
		}

		[DllImport("libgobject-2.0-0.dll")]
		static extern void g_value_set_string (ref Value val, string data);

		public Value (string val) : this (GType.String)
		{
			g_value_set_string (ref this, val); 
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
		
		public Value (IntPtr obj, string prop_name, EnumWrapper wrap)
		{
			type = GType.Invalid;
			pad_1 = pad_2 = 0;
			gtksharp_value_create_from_property (ref this, obj, prop_name);
			if (wrap.flags)
				g_value_set_flags (ref this, (uint) (int) wrap); 
			else
				g_value_set_enum (ref this, (int) wrap); 
		}

		public Value (object obj)
		{
			type = GType.Invalid;
			pad_1 = pad_2 = 0;

			GType type = TypeConverter.LookupType (obj.GetType ());
			if (type == GType.None) {
				g_value_init (ref this, ManagedValue.GType.Val);
			} else if (type == GType.Object) {
				g_value_init (ref this, ((GLib.Object) obj).NativeType.Val);
			} else {
				g_value_init (ref this, type.Val);
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
		static extern IntPtr g_value_get_object (ref Value val);

		public static explicit operator GLib.Object (Value val)
		{
			return GLib.Object.GetObject(g_value_get_object (ref val), true);
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
			return str == IntPtr.Zero ? null : Marshal.PtrToStringAnsi (str);
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

		public static explicit operator EnumWrapper (Value val)
		{
			// FIXME: handle flags
			return new EnumWrapper (g_value_get_enum (ref val), false);
		}

		[DllImport("glibsharpglue")]
		static extern IntPtr gtksharp_value_get_value_type (ref Value val);

		[DllImport("libgobject-2.0-0.dll")]
		static extern bool g_type_is_a (IntPtr type, IntPtr is_a_type);
		
		[DllImport("libgobject-2.0-0.dll")]
		static extern void g_value_take_boxed (ref Value val, IntPtr data);

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
				else if (type == GType.Double)
					return (double) this;
				else if (type == GType.Float)
					return (float) this;
				else if (type == GType.Char)
					return (char) this;
				else if (type == GType.UInt)
					return (uint) this;
				else if (type == GType.Object)
					return (GLib.Object) this;
				else
					throw new Exception ("Unknown type");
			}
			set {
				IntPtr buf;
				GType type = GLibSharp.TypeConverter.LookupType (value.GetType());
				if (type == GType.None)
					g_value_set_boxed (ref this, ManagedValue.WrapObject (value));
				else if (type == GType.String)
					g_value_set_string (ref this, (string) value);
				else if (type == GType.Boolean)
					g_value_set_boolean (ref this, (bool) value);
				else if (type == GType.Int)
					g_value_set_int (ref this, (int) value);
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
					g_value_take_boxed (ref this, buf);
				} else
					throw new Exception ("Unknown type");
			}
		}
	}
}

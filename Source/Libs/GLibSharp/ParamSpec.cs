// ParamSpec.cs - GParamSpec class wrapper implementation
//
// Authors: Mike Kestner <mkestner@novell.com>
//
// Copyright (c) 2008 Novell, Inc.
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
	using System.Runtime.InteropServices;

	internal enum ParamFlags {
		None = 0,
		Readable = 1 << 0,
		Writable = 1 << 1,
		Construct = 1 << 2,
		ConstructOnly = 1 << 3,
	}

	public class ParamSpec {

		IntPtr handle;

		public ParamSpec (string name, string nick, string blurb, GType type, bool readable, bool writable) : this (name, nick, blurb, type, (readable ? ParamFlags.Readable : ParamFlags.None) | (writable ? ParamFlags.Writable : ParamFlags.None)) {}

		internal ParamSpec (string name, string nick, string blurb, GType type, ParamFlags pflags)
		{
			int flags = (int) pflags;

			IntPtr p_name = GLib.Marshaller.StringToPtrGStrdup (name);
			IntPtr p_nick = GLib.Marshaller.StringToPtrGStrdup (nick);
			IntPtr p_blurb = GLib.Marshaller.StringToPtrGStrdup (blurb);

			if (type == GType.Char)
				handle = g_param_spec_char (p_name, p_nick, p_blurb, SByte.MinValue, SByte.MaxValue, 0, flags);
			else if (type == GType.UChar)
				handle = g_param_spec_uchar (p_name, p_nick, p_blurb, Byte.MinValue, Byte.MaxValue, 0, flags);
			else if (type == GType.Boolean)
				handle = g_param_spec_boolean (p_name, p_nick, p_blurb, false, flags);
			else if (type == GType.Int)
				handle = g_param_spec_int (p_name, p_nick, p_blurb, Int32.MinValue, Int32.MaxValue, 0, flags);
			else if (type == GType.UInt)
				handle = g_param_spec_uint (p_name, p_nick, p_blurb, 0, UInt32.MaxValue, 0, flags);
			else if (type == GType.Long)
				handle = g_param_spec_long (p_name, p_nick, p_blurb, IntPtr.Zero, IntPtr.Size == 4 ? new IntPtr (Int32.MaxValue) : new IntPtr (Int64.MaxValue), IntPtr.Zero, flags);
			else if (type == GType.ULong)
				handle = g_param_spec_ulong (p_name, p_nick, p_blurb, UIntPtr.Zero, UIntPtr.Size == 4 ? new UIntPtr (UInt32.MaxValue) : new UIntPtr (UInt64.MaxValue), UIntPtr.Zero, flags);
			else if (type == GType.Int64)
				handle = g_param_spec_int64 (p_name, p_nick, p_blurb, Int64.MinValue, Int64.MaxValue, 0, flags);
			else if (type == GType.UInt64)
				handle = g_param_spec_uint64 (p_name, p_nick, p_blurb, 0, UInt64.MaxValue, 0, flags);
			else if (type.GetBaseType () == GType.Enum)
				handle = g_param_spec_enum (p_name, p_nick, p_blurb, type.Val, (int) (Enum.GetValues((Type)type).GetValue (0)), flags);
			/*else if (type == GType.Flags)
			*	g_param_spec_flags (p_name, p_nick, p_blurb, type.Val, Enum.GetValues((Type)type) [0], flags);
			* TODO:
			* Both g_param_spec_enum and g_param_spec_flags expect default property values and the members of the enum seemingly cannot be enumerated
			*/
			else if (type == GType.Float)
				handle = g_param_spec_float (p_name, p_nick, p_blurb, Single.MinValue, Single.MaxValue, 0.0f, flags);
			else if (type == GType.Double)
				handle = g_param_spec_double (p_name, p_nick, p_blurb, Double.MinValue, Double.MaxValue, 0.0, flags);
			else if (type == GType.String)
				handle = g_param_spec_string (p_name, p_nick, p_blurb, IntPtr.Zero, flags);
			else if (type == GType.Pointer)
				handle = g_param_spec_pointer (p_name, p_nick, p_blurb, flags);
			else if (type.Val == g_gtype_get_type ())
				handle = g_param_spec_gtype (p_name, p_nick, p_blurb, GType.None.Val, flags);
			else if (g_type_is_a (type.Val, GType.Boxed.Val))
				handle = g_param_spec_boxed (p_name, p_nick, p_blurb, type.Val, flags);
			else if (g_type_is_a (type.Val, GType.Object.Val))
				handle = g_param_spec_object (p_name, p_nick, p_blurb, type.Val, flags);
			else
				throw new ArgumentException ("type:" + type.ToString ());

			GLib.Marshaller.Free (p_name);
			GLib.Marshaller.Free (p_nick);
			GLib.Marshaller.Free (p_blurb);
		}

		public ParamSpec (IntPtr native)
		{
			handle = native;
		}

		public IntPtr Handle {
			get { return handle; }
		}

		public GType ValueType {
			get {
				GParamSpec spec = (GParamSpec) Marshal.PtrToStructure (Handle, typeof (GParamSpec));
				return new GType (spec.value_type);
			}
		}

		public string Name {
			get {
				GParamSpec spec = (GParamSpec) Marshal.PtrToStructure (Handle, typeof (GParamSpec));
				return Marshaller.Utf8PtrToString (spec.name);
			}
		}

		public override string ToString ()
		{
			GParamSpec spec = (GParamSpec) Marshal.PtrToStructure (Handle, typeof (GParamSpec));
			GType valtype= new GType (spec.value_type);
			GType ownertype= new GType (spec.owner_type);
			return "ParamSpec: name=" +  Marshaller.Utf8PtrToString (spec.name) + " value_type=" + valtype.ToString() + " owner_type=" + ownertype.ToString();
		}

		struct GTypeInstance {
			public IntPtr g_class;
		}

		struct GParamSpec {
			public GTypeInstance  g_type_instance;

			public IntPtr name;
			public ParamFlags flags;
			public IntPtr value_type;
			public IntPtr owner_type;

			public IntPtr _nick;
			public IntPtr _blurb;
			public IntPtr qdata;
			public uint ref_count;
			public uint param_id;
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_param_spec_char(IntPtr name, IntPtr nick, IntPtr blurb, sbyte min, sbyte max, sbyte dval, int flags);
		static d_g_param_spec_char g_param_spec_char = FuncLoader.LoadFunction<d_g_param_spec_char>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_param_spec_char"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_param_spec_uchar(IntPtr name, IntPtr nick, IntPtr blurb, byte min, byte max, byte dval, int flags);
		static d_g_param_spec_uchar g_param_spec_uchar = FuncLoader.LoadFunction<d_g_param_spec_uchar>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_param_spec_uchar"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_param_spec_boolean(IntPtr name, IntPtr nick, IntPtr blurb, bool dval, int flags);
		static d_g_param_spec_boolean g_param_spec_boolean = FuncLoader.LoadFunction<d_g_param_spec_boolean>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_param_spec_boolean"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_param_spec_enum(IntPtr name, IntPtr nick, IntPtr blurb, IntPtr enum_type, int dval, int flags);
		static d_g_param_spec_enum g_param_spec_enum = FuncLoader.LoadFunction<d_g_param_spec_enum>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_param_spec_enum"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_param_spec_int(IntPtr name, IntPtr nick, IntPtr blurb, int min, int max, int dval, int flags);
		static d_g_param_spec_int g_param_spec_int = FuncLoader.LoadFunction<d_g_param_spec_int>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_param_spec_int"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_param_spec_uint(IntPtr name, IntPtr nick, IntPtr blurb, uint min, uint max, uint dval, int flags);
		static d_g_param_spec_uint g_param_spec_uint = FuncLoader.LoadFunction<d_g_param_spec_uint>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_param_spec_uint"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_param_spec_long(IntPtr name, IntPtr nick, IntPtr blurb, IntPtr min, IntPtr max, IntPtr dval, int flags);
		static d_g_param_spec_long g_param_spec_long = FuncLoader.LoadFunction<d_g_param_spec_long>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_param_spec_long"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_param_spec_ulong(IntPtr name, IntPtr nick, IntPtr blurb, UIntPtr min, UIntPtr max, UIntPtr dval, int flags);
		static d_g_param_spec_ulong g_param_spec_ulong = FuncLoader.LoadFunction<d_g_param_spec_ulong>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_param_spec_ulong"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_param_spec_int64(IntPtr name, IntPtr nick, IntPtr blurb, long min, long max, long dval, int flags);
		static d_g_param_spec_int64 g_param_spec_int64 = FuncLoader.LoadFunction<d_g_param_spec_int64>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_param_spec_int64"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_param_spec_uint64(IntPtr name, IntPtr nick, IntPtr blurb, ulong min, ulong max, ulong dval, int flags);
		static d_g_param_spec_uint64 g_param_spec_uint64 = FuncLoader.LoadFunction<d_g_param_spec_uint64>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_param_spec_uint64"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_param_spec_float(IntPtr name, IntPtr nick, IntPtr blurb, float min, float max, float dval, int flags);
		static d_g_param_spec_float g_param_spec_float = FuncLoader.LoadFunction<d_g_param_spec_float>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_param_spec_float"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_param_spec_double(IntPtr name, IntPtr nick, IntPtr blurb, double min, double max, double dval, int flags);
		static d_g_param_spec_double g_param_spec_double = FuncLoader.LoadFunction<d_g_param_spec_double>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_param_spec_double"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_param_spec_string(IntPtr name, IntPtr nick, IntPtr blurb, IntPtr dval, int flags);
		static d_g_param_spec_string g_param_spec_string = FuncLoader.LoadFunction<d_g_param_spec_string>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_param_spec_string"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_param_spec_pointer(IntPtr name, IntPtr nick, IntPtr blurb, int flags);
		static d_g_param_spec_pointer g_param_spec_pointer = FuncLoader.LoadFunction<d_g_param_spec_pointer>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_param_spec_pointer"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_param_spec_gtype(IntPtr name, IntPtr nick, IntPtr blurb, IntPtr dval, int flags);
		static d_g_param_spec_gtype g_param_spec_gtype = FuncLoader.LoadFunction<d_g_param_spec_gtype>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_param_spec_gtype"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_param_spec_boxed(IntPtr name, IntPtr nick, IntPtr blurb, IntPtr return_type, int flags);
		static d_g_param_spec_boxed g_param_spec_boxed = FuncLoader.LoadFunction<d_g_param_spec_boxed>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_param_spec_boxed"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_param_spec_object(IntPtr name, IntPtr nick, IntPtr blurb, IntPtr return_type, int flags);
		static d_g_param_spec_object g_param_spec_object = FuncLoader.LoadFunction<d_g_param_spec_object>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_param_spec_object"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_gtype_get_type();
		static d_g_gtype_get_type g_gtype_get_type = FuncLoader.LoadFunction<d_g_gtype_get_type>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_gtype_get_type"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate bool d_g_type_is_a(IntPtr a, IntPtr b);
		static d_g_type_is_a g_type_is_a = FuncLoader.LoadFunction<d_g_type_is_a>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_type_is_a"));

	}
}



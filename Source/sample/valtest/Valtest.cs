// Valtest.cs: GLib.Value regression test
//
// Copyright (c) 2005 Novell, Inc.

using Gtksharp;
using System;

public class Valtest {

	static int errors = 0;

	const bool BOOL_VAL = true;
	const int INT_VAL = -73523;
	const uint UINT_VAL = 99999U;
	const long INT64_VAL = -5000000000;
	const ulong UINT64_VAL = 5000000000U;
	const char UNICHAR_VAL = '\x20AC'; // euro
	const Gtk.ArrowType ENUM_VAL = Gtk.ArrowType.Left;
	const Gtk.AttachOptions FLAGS_VAL = Gtk.AttachOptions.Expand | Gtk.AttachOptions.Fill;
	const float FLOAT_VAL = 1.5f;
	const double DOUBLE_VAL = Math.PI;
	const string STRING_VAL = "This is a test";
	static Gdk.Rectangle BOXED_VAL;
	static IntPtr POINTER_VAL;
	static Gtk.Widget OBJECT_VAL;

	public static int Main ()
	{
		Gtk.Application.Init ();

		BOXED_VAL = new Gdk.Rectangle (1, 2, 3, 4);
		POINTER_VAL = (IntPtr) System.Runtime.InteropServices.GCHandle.Alloc ("foo");
		OBJECT_VAL = new Gtk.DrawingArea ();

		// Part 1: Make sure values of all types round-trip correctly within Gtk#
		GLib.Value val;

		try {
			val = new GLib.Value (BOOL_VAL);
			if ((bool)val != BOOL_VAL)
				CVError ("boolean cast", BOOL_VAL, (bool)val, val.Val);
			if ((bool)val.Val != BOOL_VAL)
				CVError ("boolean Val", BOOL_VAL, (bool)val, val.Val);
		} catch (Exception e) {
			ExceptionError ("boolean", e);
		}

		try {
			val = new GLib.Value (INT_VAL);
			if ((int)val != INT_VAL)
				CVError ("int cast", INT_VAL, (int)val, val.Val);
			if ((int)val.Val != INT_VAL)
				CVError ("int Val", INT_VAL, (int)val, val.Val);
		} catch (Exception e) {
			ExceptionError ("int", e);
		}

		try {
			val = new GLib.Value (UINT_VAL);
			if ((uint)val != UINT_VAL)
				CVError ("uint cast", UINT_VAL, (uint)val, val.Val);
			if ((uint)val.Val != UINT_VAL)
				CVError ("uint Val", UINT_VAL, (uint)val, val.Val);
		} catch (Exception e) {
			ExceptionError ("uint", e);
		}

		try {
			val = new GLib.Value (INT64_VAL);
			if ((long)val != INT64_VAL)
				CVError ("int64 cast", INT64_VAL, (long)val, val.Val);
			if ((long)val.Val != INT64_VAL)
				CVError ("int64 Val", INT64_VAL, (long)val, val.Val);
		} catch (Exception e) {
			ExceptionError ("int64", e);
		}

		try {
			val = new GLib.Value (UINT64_VAL);
			if ((ulong)val != UINT64_VAL)
				CVError ("uint64 cast", UINT64_VAL, (ulong)val, val.Val);
			if ((ulong)val.Val != UINT64_VAL)
				CVError ("uint64 Val", UINT64_VAL, (ulong)val, val.Val);
		} catch (Exception e) {
			ExceptionError ("uint64", e);
		}

		// gunichar doesn't have its own GValue type, it shares with guint

		try {
			val = new GLib.Value (ENUM_VAL);
			if ((Gtk.ArrowType)(Enum)val != ENUM_VAL)
				CVError ("enum cast", ENUM_VAL, (Gtk.ArrowType)(Enum)val, val.Val);
			if ((Gtk.ArrowType)(Enum)val.Val != ENUM_VAL)
				CVError ("enum Val", ENUM_VAL, (Gtk.ArrowType)(Enum)val, val.Val);
		} catch (Exception e) {
			ExceptionError ("enum", e);
		}

		try {
			val = new GLib.Value (FLAGS_VAL);
			if ((Gtk.AttachOptions)(Enum)val != FLAGS_VAL)
				CVError ("flags cast", FLAGS_VAL, (Gtk.AttachOptions)(Enum)val, val.Val);
			if ((Gtk.AttachOptions)(Enum)val.Val != FLAGS_VAL)
				CVError ("flags Val", FLAGS_VAL, (Gtk.AttachOptions)(Enum)val, val.Val);
		} catch (Exception e) {
			ExceptionError ("flags", e);
		}

		try {
			val = new GLib.Value (FLOAT_VAL);
			if ((float)val != FLOAT_VAL)
				CVError ("float cast", FLOAT_VAL, (float)val, val.Val);
			if ((float)val.Val != FLOAT_VAL)
				CVError ("float Val", FLOAT_VAL, (float)val, val.Val);
		} catch (Exception e) {
			ExceptionError ("float", e);
		}

		try {
			val = new GLib.Value (DOUBLE_VAL);
			if ((double)val != DOUBLE_VAL)
				CVError ("double cast", DOUBLE_VAL, (double)val, val.Val);
			if ((double)val.Val != DOUBLE_VAL)
				CVError ("double Val", DOUBLE_VAL, (double)val, val.Val);
		} catch (Exception e) {
			ExceptionError ("double", e);
		}

		try {
			val = new GLib.Value (STRING_VAL);
			if ((string)val != STRING_VAL)
				CVError ("string cast", STRING_VAL, (string)val, val.Val);
			if ((string)val.Val != STRING_VAL)
				CVError ("string Val", STRING_VAL, (string)val, val.Val);
		} catch (Exception e) {
			ExceptionError ("string", e);
		}

		try {
			val = new GLib.Value (BOXED_VAL);
			if ((Gdk.Rectangle)val != BOXED_VAL)
				CVError ("boxed cast", BOXED_VAL, (Gdk.Rectangle)val, val.Val);
			// Can't currently use .Val on boxed types
		} catch (Exception e) {
			ExceptionError ("boxed", e);
		}

		try {
			val = new GLib.Value (POINTER_VAL);
			if ((IntPtr)val != POINTER_VAL)
				CVError ("pointer cast", POINTER_VAL, (IntPtr)val, val.Val);
			if ((IntPtr)val.Val != POINTER_VAL)
				CVError ("pointer Val", POINTER_VAL, (IntPtr)val, val.Val);
		} catch (Exception e) {
			ExceptionError ("pointer", e);
		}

		try {
			val = new GLib.Value (OBJECT_VAL);
			if ((Gtk.DrawingArea)val != OBJECT_VAL)
				CVError ("object cast", OBJECT_VAL, (Gtk.DrawingArea)val, val.Val);
			if ((Gtk.DrawingArea)val.Val != OBJECT_VAL)
				CVError ("object Val", OBJECT_VAL, (Gtk.DrawingArea)val, val.Val);
		} catch (Exception e) {
			ExceptionError ("object", e);
		}

		// Test ManagedValue

		Structtest st = new Structtest (5, "foo");
		try {
			val = new GLib.Value (st);
			// No direct GLib.Value -> ManagedValue cast
			Structtest st2 = (Structtest)val.Val;
			if (st.Int != st2.Int || st.String != st2.String)
				CVError ("ManagedValue Val", st, (Gtk.DrawingArea)val, val.Val);
		} catch (Exception e) {
			ExceptionError ("ManagedValue", e);
		}

		// Part 2: method->unmanaged->property round trip
		Valobj vo;
		vo = new Valobj ();

		vo.Boolean = BOOL_VAL;
		if (vo.BooleanProp != BOOL_VAL)
			MPError ("boolean method->prop", BOOL_VAL, vo.Boolean, vo.BooleanProp);

		vo.Int = INT_VAL;
		if (vo.IntProp != INT_VAL)
			MPError ("int method->prop", INT_VAL, vo.Int, vo.IntProp);

		vo.Uint = UINT_VAL;
		if (vo.UintProp != UINT_VAL)
			MPError ("uint method->prop", UINT_VAL, vo.Uint, vo.UintProp);

		vo.Int64 = INT64_VAL;
		if (vo.Int64Prop != INT64_VAL)
			MPError ("int64 method->prop", INT64_VAL, vo.Int64, vo.Int64Prop);

		vo.Uint64 = UINT64_VAL;
		if (vo.Uint64Prop != UINT64_VAL)
			MPError ("uint64 method->prop", UINT64_VAL, vo.Uint64, vo.Uint64Prop);

		vo.Unichar = UNICHAR_VAL;
		if (vo.UnicharProp != UNICHAR_VAL)
			MPError ("unichar method->prop", UNICHAR_VAL, vo.Unichar, vo.UnicharProp);

		vo.Enum = ENUM_VAL;
		if (vo.EnumProp != ENUM_VAL)
			MPError ("enum method->prop", ENUM_VAL, vo.Enum, vo.EnumProp);

		vo.Flags = FLAGS_VAL;
		if (vo.FlagsProp != (FLAGS_VAL))
			MPError ("flags method->prop", FLAGS_VAL, vo.Flags, vo.FlagsProp);

		vo.Float = FLOAT_VAL;
		if (vo.FloatProp != FLOAT_VAL)
			MPError ("float method->prop", FLOAT_VAL, vo.Float, vo.FloatProp);

		vo.Double = DOUBLE_VAL;
		if (vo.DoubleProp != DOUBLE_VAL)
			MPError ("double method->prop", DOUBLE_VAL, vo.Double, vo.DoubleProp);

		vo.String = STRING_VAL;
		if (vo.StringProp != STRING_VAL)
			MPError ("string method->prop", STRING_VAL, vo.String, vo.StringProp);

		vo.Boxed = BOXED_VAL;
		if (vo.BoxedProp != BOXED_VAL)
			MPError ("boxed method->prop", BOXED_VAL, vo.Boxed, vo.BoxedProp);

		vo.Pointer = POINTER_VAL;
		if (vo.PointerProp != POINTER_VAL)
			MPError ("pointer method->prop", POINTER_VAL, vo.Pointer, vo.PointerProp);

		vo.Object = OBJECT_VAL;
		if (vo.ObjectProp != OBJECT_VAL) {
			MPError ("object method->prop", OBJECT_VAL.GetType().Name + " " + OBJECT_VAL.GetHashCode (),
				 vo.Object == null ? "null" : vo.Object.GetType().Name + " " + vo.Object.GetHashCode (),
				 vo.ObjectProp == null ? "null" : vo.ObjectProp.GetType().Name + " " + vo.ObjectProp.GetHashCode ());
		}


		// Part 3: property->unmanaged->method round trip
		vo = new Valobj ();

		vo.BooleanProp = BOOL_VAL;
		if (vo.Boolean != BOOL_VAL)
			MPError ("boolean prop->method", BOOL_VAL, vo.Boolean, vo.BooleanProp);

		vo.IntProp = INT_VAL;
		if (vo.Int != INT_VAL)
			MPError ("int prop->method", INT_VAL, vo.Int, vo.IntProp);

		vo.UintProp = UINT_VAL;
		if (vo.Uint != UINT_VAL)
			MPError ("uint prop->method", UINT_VAL, vo.Uint, vo.UintProp);

		vo.Int64Prop = INT64_VAL;
		if (vo.Int64 != INT64_VAL)
			MPError ("int64 prop->method", INT64_VAL, vo.Int64, vo.Int64Prop);

		vo.Uint64Prop = UINT64_VAL;
		if (vo.Uint64 != UINT64_VAL)
			MPError ("uint64 prop->method", UINT64_VAL, vo.Uint64, vo.Uint64Prop);

		vo.UnicharProp = UNICHAR_VAL;
		if (vo.Unichar != UNICHAR_VAL)
			MPError ("unichar prop->method", UNICHAR_VAL, vo.Unichar, vo.UnicharProp);

		vo.EnumProp = ENUM_VAL;
		if (vo.Enum != ENUM_VAL)
			MPError ("enum prop->method", ENUM_VAL, vo.Enum, vo.EnumProp);

		vo.FlagsProp = FLAGS_VAL;
		if (vo.Flags != (FLAGS_VAL))
			MPError ("flags prop->method", FLAGS_VAL, vo.Flags, vo.FlagsProp);

		vo.FloatProp = FLOAT_VAL;
		if (vo.Float != FLOAT_VAL)
			MPError ("float prop->method", FLOAT_VAL, vo.Float, vo.FloatProp);

		vo.DoubleProp = DOUBLE_VAL;
		if (vo.Double != DOUBLE_VAL)
			MPError ("double prop->method", DOUBLE_VAL, vo.Double, vo.DoubleProp);

		vo.StringProp = STRING_VAL;
		if (vo.String != STRING_VAL)
			MPError ("string prop->method", STRING_VAL, vo.String, vo.StringProp);

		vo.BoxedProp = BOXED_VAL;
		if (vo.Boxed != BOXED_VAL)
			MPError ("boxed prop->method", BOXED_VAL, vo.Boxed, vo.BoxedProp);

		vo.PointerProp = POINTER_VAL;
		if (vo.Pointer != POINTER_VAL)
			MPError ("pointer prop->method", POINTER_VAL, vo.Pointer, vo.PointerProp);

		vo.ObjectProp = OBJECT_VAL;
		if (vo.Object != OBJECT_VAL) {
			MPError ("object prop->method", OBJECT_VAL.GetType().Name + " " + OBJECT_VAL.GetHashCode (),
				 vo.Object == null ? "null" : vo.Object.GetType().Name + " " + vo.Object.GetHashCode (),
				 vo.ObjectProp == null ? "null" : vo.ObjectProp.GetType().Name + " " + vo.ObjectProp.GetHashCode ());
		}

		Console.WriteLine ("{0} errors", errors);

		return errors;
	}

	static void CVError (string test, object expected, object cast, object value)
	{
		Console.Error.WriteLine ("Failed test {0}. Expected '{1}', got '{2}' from cast, '{3}' from Value",
					 test, expected, cast, value);
		errors++;
	}

	static void ExceptionError (string test, Exception e)
	{
		Console.Error.WriteLine ("Exception in test {0}: {1}",
					 test, e.Message);
		errors++;
	}

	static void MPError (string test, object expected, object method, object prop)
	{
		Console.Error.WriteLine ("Failed test {0}. Expected '{1}', got '{2}' from method, '{3}' from prop",
					 test, expected, method, prop);
		errors++;
	}
}

public struct Structtest {
	public int Int;
	public string String;

	public Structtest (int Int, string String)
	{
		this.Int = Int;
		this.String = String;
	}

	public override string ToString ()
	{
		return Int.ToString () + "/" + String.ToString ();
	}
}

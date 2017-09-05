// PropertyRegistration.cs - GObject property registration sample
//
// Author: Mike Kestner <mkestner@novell.com>
//
// Copyright (c) 2008 Novell, Inc.

namespace GtkSamples {

	using System;

	public class TestObject : GLib.Object {

		public static int Main (string[] args)
		{
			GLib.GType.Init ();
			TestObject obj = new TestObject ();
			obj.TestInt ();
			obj.TestUInt ();
			obj.TestLong ();
			obj.TestULong ();
			obj.TestByte ();
			obj.TestSByte ();
			obj.TestBool ();
			obj.TestFloat ();
			obj.TestDouble ();
			obj.TestString ();
			//obj.TestIntPtr ();
			//obj.TestBoxed ();
			obj.TestGObject ();
			Console.WriteLine ("All properties succeeded.");
			return 0;
		}

		int my_int;
		[GLib.Property ("my_int")]
		public int MyInt {
			get { return my_int; }
			set { my_int = value; }
		}

		public void TestInt ()
		{
			GLib.Value val = new GLib.Value (42);
			SetProperty ("my_int", val);
			val.Dispose ();
			if (MyInt != 42) {
				Console.Error.WriteLine ("int Property setter did not run.");
				Environment.Exit (1);
			}
			GLib.Value val2 = GetProperty ("my_int");
			if ((int)val2.Val != 42) {
				Console.Error.WriteLine ("int Property set/get roundtrip failed.");
				Environment.Exit (1);
			}
			Console.WriteLine ("int succeeded.");
		}

		uint my_uint;
		[GLib.Property ("my_uint")]
		public uint MyUInt {
			get { return my_uint; }
			set { my_uint = value; }
		}

		public void TestUInt ()
		{
			GLib.Value val = new GLib.Value ((uint)42);
			SetProperty ("my_uint", val);
			val.Dispose ();
			if (MyUInt != 42) {
				Console.Error.WriteLine ("uint Property setter did not run.");
				Environment.Exit (1);
			}
			GLib.Value val2 = GetProperty ("my_uint");
			if ((uint)val2.Val != 42) {
				Console.Error.WriteLine ("uint Property set/get roundtrip failed.");
				Environment.Exit (1);
			}
			Console.WriteLine ("uint succeeded.");
		}

		long my_long;
		[GLib.Property ("my_long")]
		public long MyLong {
			get { return my_long; }
			set { my_long = value; }
		}

		public void TestLong ()
		{
			GLib.Value val = new GLib.Value ((long)42);
			SetProperty ("my_long", val);
			val.Dispose ();
			if (MyLong != 42) {
				Console.Error.WriteLine ("long Property setter did not run.");
				Environment.Exit (1);
			}
			GLib.Value val2 = GetProperty ("my_long");
			if ((long)val2.Val != 42) {
				Console.Error.WriteLine ("long Property set/get roundtrip failed.");
				Environment.Exit (1);
			}
			Console.WriteLine ("long succeeded.");
		}

		ulong my_ulong;
		[GLib.Property ("my_ulong")]
		public ulong MyULong {
			get { return my_ulong; }
			set { my_ulong = value; }
		}

		public void TestULong ()
		{
			GLib.Value val = new GLib.Value ((ulong)42);
			SetProperty ("my_ulong", val);
			val.Dispose ();
			if (MyULong != 42) {
				Console.Error.WriteLine ("ulong Property setter did not run.");
				Environment.Exit (1);
			}
			GLib.Value val2 = GetProperty ("my_ulong");
			if ((ulong)val2.Val != 42) {
				Console.Error.WriteLine ("ulong Property set/get roundtrip failed.");
				Environment.Exit (1);
			}
			Console.WriteLine ("ulong succeeded.");
		}

		byte my_byte;
		[GLib.Property ("my_byte")]
		public byte MyByte {
			get { return my_byte; }
			set { my_byte = value; }
		}

		public void TestByte ()
		{
			GLib.Value val = new GLib.Value ((byte)42);
			SetProperty ("my_byte", val);
			val.Dispose ();
			if (MyByte != 42) {
				Console.Error.WriteLine ("byte Property setter did not run.");
				Environment.Exit (1);
			}
			GLib.Value val2 = GetProperty ("my_byte");
			if ((byte)val2.Val != 42) {
				Console.Error.WriteLine ("byte Property set/get roundtrip failed.");
				Environment.Exit (1);
			}
			Console.WriteLine ("byte succeeded.");
		}

		sbyte my_sbyte;
		[GLib.Property ("my_sbyte")]
		public sbyte MySByte {
			get { return my_sbyte; }
			set { my_sbyte = value; }
		}

		public void TestSByte ()
		{
			GLib.Value val = new GLib.Value ((sbyte)42);
			SetProperty ("my_sbyte", val);
			val.Dispose ();
			if (MySByte != 42) {
				Console.Error.WriteLine ("sbyte Property setter did not run.");
				Environment.Exit (1);
			}
			GLib.Value val2 = GetProperty ("my_sbyte");
			if ((sbyte)val2.Val != 42) {
				Console.Error.WriteLine ("sbyte Property set/get roundtrip failed.");
				Environment.Exit (1);
			}
			Console.WriteLine ("sbyte succeeded.");
		}

		bool my_bool;
		[GLib.Property ("my_bool")]
		public bool MyBool {
			get { return my_bool; }
			set { my_bool = value; }
		}

		public void TestBool ()
		{
			GLib.Value val = new GLib.Value (true);
			SetProperty ("my_bool", val);
			val.Dispose ();
			if (!MyBool) {
				Console.Error.WriteLine ("bool Property setter did not run.");
				Environment.Exit (1);
			}
			GLib.Value val2 = GetProperty ("my_bool");
			if (!((bool)val2.Val)) {
				Console.Error.WriteLine ("bool Property set/get roundtrip failed.");
				Environment.Exit (1);
			}
			Console.WriteLine ("bool succeeded.");
		}

		float my_float;
		[GLib.Property ("my_float")]
		public float MyFloat {
			get { return my_float; }
			set { my_float = value; }
		}

		public void TestFloat ()
		{
			GLib.Value val = new GLib.Value (42.0f);
			SetProperty ("my_float", val);
			val.Dispose ();
			if (MyFloat != 42.0f) {
				Console.Error.WriteLine ("float Property setter did not run.");
				Environment.Exit (1);
			}
			GLib.Value val2 = GetProperty ("my_float");
			if ((float)val2.Val != 42.0f) {
				Console.Error.WriteLine ("float Property set/get roundtrip failed.");
				Environment.Exit (1);
			}
			Console.WriteLine ("float succeeded.");
		}

		double my_double;
		[GLib.Property ("my_double")]
		public double MyDouble {
			get { return my_double; }
			set { my_double = value; }
		}

		public void TestDouble ()
		{
			GLib.Value val = new GLib.Value (42.0);
			SetProperty ("my_double", val);
			val.Dispose ();
			if (MyDouble != 42.0) {
				Console.Error.WriteLine ("double Property setter did not run.");
				Environment.Exit (1);
			}
			GLib.Value val2 = GetProperty ("my_double");
			if ((double)val2.Val != 42.0) {
				Console.Error.WriteLine ("double Property set/get roundtrip failed.");
				Environment.Exit (1);
			}
			Console.WriteLine ("double succeeded.");
		}

		string my_string;
		[GLib.Property ("my_string")]
		public string MyString {
			get { return my_string; }
			set { my_string = value; }
		}

		public void TestString ()
		{
			GLib.Value val = new GLib.Value ("42");
			SetProperty ("my_string", val);
			val.Dispose ();
			if (MyString != "42") {
				Console.Error.WriteLine ("string Property setter did not run.");
				Environment.Exit (1);
			}
			GLib.Value val2 = GetProperty ("my_string");
			if ((string)val2.Val != "42") {
				Console.Error.WriteLine ("string Property set/get roundtrip failed.");
				Environment.Exit (1);
			}
			Console.WriteLine ("string succeeded.");
		}

#if false
		IntPtr my_intptr;
		[GLib.Property ("my_intptr")]
		public IntPtr MyIntPtr {
			get { return my_intptr; }
			set { my_intptr = value; }
		}

		public void TestIntPtr ()
		{
			IntPtr ptr = System.Runtime.InteropServices.Marshal.AllocHGlobal (4);
			Console.WriteLine (ptr);
			GLib.Value val = new GLib.Value (ptr);
			SetProperty ("my_intptr", val);
			val.Dispose ();
			if (MyIntPtr != ptr) {
				Console.Error.WriteLine ("IntPtr Property setter did not run.");
				Environment.Exit (1);
			}
			GLib.Value val2 = GetProperty ("my_intptr");
			Console.WriteLine (val2.Val);
			if (!val2.Val.Equals (ptr)) {
				Console.Error.WriteLine ("IntPtr Property set/get roundtrip failed.");
				Environment.Exit (1);
			}
			Console.WriteLine ("IntPtr succeeded.");
		}

		Gdk.Color my_boxed;
		[GLib.Property ("my_boxed")]
		public Gdk.Color MyBoxed {
			get { return my_boxed; }
			set { my_boxed = value; }
		}

		public void TestBoxed ()
		{
			Gdk.Color color = new Gdk.Color (0, 0, 0);
			GLib.Value val = (GLib.Value) color;
			SetProperty ("my_boxed", val);
			val.Dispose ();
			if (!MyBoxed.Equals (color)) {
				Console.Error.WriteLine ("boxed Property setter did not run.");
				Environment.Exit (1);
			}
			GLib.Value val2 = GetProperty ("my_boxed");
			if (color.Equals ((Gdk.Color)val2.Val)) {
				Console.Error.WriteLine ("boxed Property set/get roundtrip failed.");
				Environment.Exit (1);
			}
			Console.WriteLine ("boxed succeeded.");
		}
#endif

		GLib.Object my_object;
		[GLib.Property ("my_object")]
		public GLib.Object MyObject {
			get { return my_object; }
			set { my_object = value; }
		}

		public void TestGObject ()
		{
			Gtk.Window win = new Gtk.Window ("test");
			GLib.Value val = new GLib.Value (win);
			SetProperty ("my_object", val);
			val.Dispose ();
			if (MyObject != win) {
				Console.Error.WriteLine ("GObject Property setter did not run.");
				Environment.Exit (1);
			}
			GLib.Value val2 = GetProperty ("my_object");
			if ((GLib.Object)val2.Val != win) {
				Console.Error.WriteLine ("GObject Property set/get roundtrip failed.");
				Environment.Exit (1);
			}
			Console.WriteLine ("GObject succeeded.");
		}

#if false
		int my_int;
		[GLib.Property ("my_int")]
		public int MyInt {
			get { return my_int; }
			set { my_int = value; }
		}

		public void TestInt ()
		{
			GLib.Value val = new GLib.Value (42);
			SetProperty ("my_int", val);
			val.Dispose ();
			if (MyInt != 42) {
				Console.Error.WriteLine ("Property setter did not run.");
				Environment.Exit (1);
			}
			GLib.Value val2 = GetProperty ("my_int");
			if ((int)val2.Val != 42) {
				Console.Error.WriteLine ("Property set/get roundtrip failed.");
				Environment.Exit (1);
			}
		}

		int my_int;
		[GLib.Property ("my_int")]
		public int MyInt {
			get { return my_int; }
			set { my_int = value; }
		}

		public void TestInt ()
		{
			GLib.Value val = new GLib.Value (42);
			SetProperty ("my_int", val);
			val.Dispose ();
			if (MyInt != 42) {
				Console.Error.WriteLine ("Property setter did not run.");
				Environment.Exit (1);
			}
			GLib.Value val2 = GetProperty ("my_int");
			if ((int)val2.Val != 42) {
				Console.Error.WriteLine ("Property set/get roundtrip failed.");
				Environment.Exit (1);
			}
		}

		int my_int;
		[GLib.Property ("my_int")]
		public int MyInt {
			get { return my_int; }
			set { my_int = value; }
		}

		public void TestInt ()
		{
			GLib.Value val = new GLib.Value (42);
			SetProperty ("my_int", val);
			val.Dispose ();
			if (MyInt != 42) {
				Console.Error.WriteLine ("Property setter did not run.");
				Environment.Exit (1);
			}
			GLib.Value val2 = GetProperty ("my_int");
			if ((int)val2.Val != 42) {
				Console.Error.WriteLine ("Property set/get roundtrip failed.");
				Environment.Exit (1);
			}
		}

		int my_int;
		[GLib.Property ("my_int")]
		public int MyInt {
			get { return my_int; }
			set { my_int = value; }
		}

		public void TestInt ()
		{
			GLib.Value val = new GLib.Value (42);
			SetProperty ("my_int", val);
			val.Dispose ();
			if (MyInt != 42) {
				Console.Error.WriteLine ("Property setter did not run.");
				Environment.Exit (1);
			}
			GLib.Value val2 = GetProperty ("my_int");
			if ((int)val2.Val != 42) {
				Console.Error.WriteLine ("Property set/get roundtrip failed.");
				Environment.Exit (1);
			}
		}
#endif

	}
}

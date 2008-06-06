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
			GLib.Value val = new GLib.Value (42);
			obj.SetProperty ("my_prop", val);
			val.Dispose ();
			if (obj.MyProp != 42) {
				Console.Error.WriteLine ("Property setter did not run.");
				return 1;
			}
			GLib.Value val2 = obj.GetProperty ("my_prop");
			if ((int)val2.Val != 42) {
				Console.Error.WriteLine ("Property set/get roundtrip failed.");
				return 1;
			}
			Console.WriteLine ("Round trip succeeded.");
			return 0;
		}

		int my_prop;

		[GLib.Property ("my_prop")]
		public int MyProp {
			get { return my_prop; }
			set { 
				my_prop = value;
				Console.WriteLine ("Property setter invoked.");
			 }
		}
	}
}

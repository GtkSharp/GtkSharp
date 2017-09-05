// Author: Mike Kestner <mkestner@novell.com>
//
// Copyright (c) 2009 Novell, Inc.

namespace GtkSharp {

	using Gtk;
	using System;
	using System.Runtime.InteropServices;

	public class InstantiationTest : Gtk.Window  {

		[DllImport ("libgobject-2.0.so.0")]
		static extern IntPtr g_object_new (IntPtr gtype, string prop, string val, IntPtr dummy);

		[DllImport ("libgtk-3.so.0")]
		static extern void gtk_widget_show (IntPtr handle);

		public static int Main (string[] args)
		{
			Application.Init ();
			GLib.GType gtype = LookupGType (typeof (InstantiationTest));
			GLib.GType.Register (gtype, typeof (InstantiationTest));
			Console.WriteLine ("Instantiating using managed constructor");
			new InstantiationTest ("Managed Instantiation Test").ShowAll ();
			Console.WriteLine ("Managed Instantiation complete");
			Console.WriteLine ("Instantiating using unmanaged construction");
			IntPtr handle = g_object_new (gtype.Val, "title", "Unmanaged Instantiation Test", IntPtr.Zero);
			gtk_widget_show (handle);
			Console.WriteLine ("Unmanaged Instantiation complete");
			Application.Run ();
			return 0;
		}


		public InstantiationTest (IntPtr raw) : base (raw)
		{
			Console.WriteLine ("IntPtr ctor invoked");
			DefaultWidth = 400;
			DefaultHeight = 200;
		}

		public InstantiationTest (string title) : base (title)
		{
			DefaultWidth = 200;
			DefaultHeight = 400;
		}

		protected override bool OnDeleteEvent (Gdk.Event ev)
		{
			Application.Quit ();
			return true;
		}

	}
}

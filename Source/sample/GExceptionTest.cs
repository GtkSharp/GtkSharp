// HelloWorld.cs - GTK Window class Test implementation
//
// Author: Mike Kestner <mkestner@speakeasy.net>
//
// (c) 2001-2002 Mike Kestner

namespace GtkSamples {

	using Gtk;
	using Gdk;
	using System;

	public class GExceptionTest  {

		public static int Main (string[] args)
		{
			Application.Init ();
			Gtk.Window win = new Gtk.Window ("GException");
			win.SetIconFromFile ("this.filename.does.not.exist");
			// Notreached, GException should throw on above call.
			return 0;
		}
	}
}

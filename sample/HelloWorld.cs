// TestWindow.cs - GTK Window class Test implementation
//
// Author: Mike Kestner <mkestner@speakeasy.net>
//
// (c) 2001 Mike Kestner

namespace GtkSamples {

	using Gtk;
	using System;

	public class HelloWorld  {

		public static int Main (string[] args)
		{
			Application.Init (ref args);
			Window win = new Window ("Gtk# Hello World");
			win.Delete += new EventHandler (delete_cb);
			win.Show ();
			Application.Run ();
			return 0;
		}

		static void delete_cb (object obj, EventArgs args)
		{
			Application.Quit ();
		}

	}
}

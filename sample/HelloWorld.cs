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
			win.DeleteEvent += new EventHandler (Window_Delete);
			win.Show ();
			Application.Run ();
			return 0;
		}

		static void Window_Delete (object obj, EventArgs args)
		{
			Application.Quit ();
		}

	}
}

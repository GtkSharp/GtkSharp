// TestWindow.cs - GTK Window class Test implementation
//
// Author: Mike Kestner <mkestner@speakeasy.net>
//
// (c) 2001 Mike Kestner

namespace GtkSamples {

	using Gtk;
	using Gdk;
	using GtkSharp;
	using System;
	using System.Drawing;

	public class HelloWorld  {

		public static int Main (string[] args)
		{
			Application.Init (ref args);
			Console.WriteLine("Creating Window");
			Gtk.Window win = new Gtk.Window (Gtk.WindowType.Toplevel);
			Console.WriteLine("Setting Title");
			win.Title = "Gtk# Hello World";
			// win.Deleted += new EventHandler (Window_Delete);
			win.Show ();
			Console.WriteLine("Entering event loop");
			Application.Run ();
			return 0;
		}

		static void Window_Delete (object obj, EventArgs args)
		{
			Application.Quit ();
		}

	}
}

// HelloWorld.cs - GTK Window class Test implementation
//
// Author: Mike Kestner <mkestner@speakeasy.net>
//
// (c) 2001-2002 Mike Kestner

namespace GtkSamples {

	using Gtk;
	using Gdk;
	using GtkSharp;
	using System;
	using System.Drawing;

	public class HelloWorld  {

		public static int Main (string[] args)
		{
			Application.Init ();
			Gtk.Window win = new Gtk.Window (Gtk.WindowType.Toplevel);
			win.Title = "Gtk# Hello World";
			win.DeleteEvent += new EventHandler (Window_Delete);
			win.ShowAll ();
			Application.Run ();
			return 0;
		}

		static void Window_Delete (object obj, EventArgs args)
		{
			SignalArgs sa = (SignalArgs) args;
			Application.Quit ();
			sa.RetVal = true;
		}

	}
}

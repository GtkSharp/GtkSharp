// ButtonApp.cs - Gtk.Button class Test implementation
//
// Author: Mike Kestner <mkestner@speakeasy.net>
//
// (c) 2001 Mike Kestner

namespace GtkSamples {

	using Gtk;
	using System;
	using System.Drawing;

	public class ButtonApp  {

		public static int Main (string[] args)
		{
			Application.Init (ref args);
			Window win = new Window ("Button Tester");
			win.Deleted += new EventHandler (Window_Delete);
			Button btn = new Button ();
			btn.Clicked += new EventHandler (btn_click);
			btn.SizeRequest = new Size (32, 24);
			btn.Show ();
			win.Add (btn);
			win.Show ();
			Application.Run ();
			return 0;
		}

		static void btn_click (object obj, EventArgs args)
		{
			Console.WriteLine ("Button Clicked");
		}

		static void Window_Delete (object obj, EventArgs args)
		{
			Application.Quit ();
		}

	}
}

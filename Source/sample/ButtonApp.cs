// ButtonApp.cs - Gtk.Button class Test implementation
//
// Author: Mike Kestner <mkestner@speakeasy.net>
//
// (c) 2001-2002 Mike Kestner

namespace GtkSamples {

	using Gtk;
	using System;

	public class ButtonApp  {

		public static int Main (string[] args)
		{
			Application.Init ();
			Window win = new Window ("Button Tester");
			win.DefaultWidth = 200;
			win.DefaultHeight = 150;
			win.DeleteEvent += new DeleteEventHandler (Window_Delete);
			Button btn = new Button ("Click Me");
			btn.Clicked += new EventHandler (btn_click);
			win.Add (btn);
			win.ShowAll ();
			Application.Run ();
			return 0;
		}

		static void btn_click (object obj, EventArgs args)
		{
			Console.WriteLine ("Button Clicked");
		}

		static void Window_Delete (object obj, DeleteEventArgs args)
		{
			Application.Quit ();
			args.RetVal = true;
		}

	}
}

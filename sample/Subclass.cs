// Subclass.cs - Widget subclass Test implementation
//
// Author: Mike Kestner <mkestner@speakeasy.net>
//
// (c) 2001-2002 Mike Kestner

namespace GtkSamples {

	using Gtk;
	using GtkSharp;
	using System;
	using System.Drawing;

	public class ButtonApp  {

		public static int Main (string[] args)
		{
			Application.Init ();
			Window win = new Window ("Button Tester");
			win.DefaultSize = new Size (200, 150);
			win.DeleteEvent += new DeleteEventHandler (Window_Delete);
			Button btn = new MyButton ();
			btn.Label = "I'm a subclassed button";
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

	public class MyButton : Gtk.Button {

		static uint gtype = 0;

		public MyButton () : base (new GLib.Type (GType)) {}

		public static new uint GType {
			get {
				if (gtype == 0)
					gtype = RegisterGType (typeof (MyButton)).Value;
				return gtype;
			}
		}
	}
}

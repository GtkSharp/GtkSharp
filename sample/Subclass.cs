// Subclass.cs - Widget subclass Test 
//
// Author: Mike Kestner <mkestner@ximian.com>
//
// (c) 2001-2003 Mike Kestner, Novell, Inc.

namespace GtkSamples {

	using Gtk;
	using System;

	public class ButtonApp  {

		public static int Main (string[] args)
		{
			Application.Init ();
			Window win = new Window ("Button Tester");
			win.DeleteEvent += new DeleteEventHandler (Quit);
			Button btn = new MyButton ();
			win.Add (btn);
			win.ShowAll ();
			Application.Run ();
			return 0;
		}

		static void Quit (object sender, DeleteEventArgs args)
		{
			Application.Quit();
		}
	}

	public class MyButton : Gtk.Button {

		static GLib.GType gtype = GLib.GType.Invalid;

		public MyButton () : base (GType) 
		{
			Label = "I'm a subclassed button";
		}

		public static new GLib.GType GType {
			get {
				if (gtype == GLib.GType.Invalid)
					gtype = RegisterGType (typeof (MyButton));
				return gtype;
			}
		}

		protected override void OnClicked ()
		{
			Console.WriteLine ("Button::Clicked default handler fired.");
		}
	}
}

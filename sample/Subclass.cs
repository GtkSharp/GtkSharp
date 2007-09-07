// Subclass.cs - Widget subclass Test 
//
// Author: Mike Kestner <mkestner@ximian.com>
//
// (c) 2001-2003 Mike Kestner, Novell, Inc.

[assembly:GLib.IgnoreClassInitializers]

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

	[Binding (Gdk.Key.Escape, "HandleBinding", "Escape")]
	[Binding (Gdk.Key.Left, "HandleBinding", "Left")]
	[Binding (Gdk.Key.Right, "HandleBinding", "Right")]
	[Binding (Gdk.Key.Up, "HandleBinding", "Up")]
	[Binding (Gdk.Key.Down, "HandleBinding", "Down")]
	public class MyButton : Gtk.Button {

		public MyButton () : base ("I'm a subclassed button") {}

		protected override void OnClicked ()
		{
			Console.WriteLine ("Button::Clicked default handler fired.");
		}

		private void HandleBinding (string text)
		{
			Console.WriteLine ("Got a bound keypress: " + text);
		}
	}
}

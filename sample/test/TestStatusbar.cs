//
// TestStatusbar.cs
//
// Author: Duncan Mak  (duncan@ximian.com)
//
// Copyright (C) 2002, Duncan Mak, Ximian Inc.
//

using System;

using Gtk;
using GtkSharp;

namespace WidgetViewer {

	public class TestStatusbar
	{
		static Window window = null;
		static Statusbar statusbar = null;
		static int counter = 1;
		static uint context_id ;

		public static Gtk.Window Create ()
		{
			window = new Window ("Statusbar");
			window.SetDefaultSize (150, 100);

			VBox box1 = new VBox (false, 0);
			window.Add (box1);

			VBox box2 = new VBox (false, 10);
			box2.BorderWidth = 10;
			box1.PackStart (box2, true, true, 0);

			statusbar = new Statusbar ();
			box1.PackEnd (statusbar, true, true, 0);
			statusbar.TextPopped += new TextPoppedHandler (statusbar_popped);

			Button button = new Button ("push");
			box2.PackStart (button, false, false, 0);
			button.Clicked += new EventHandler (statusbar_pushed);

			button = new Button ("pop");
			box2.PackStart (button, false, false, 0);
			button.Clicked += new EventHandler (pop_clicked);

			box1.PackStart (new HSeparator (), false, true, 0);

			box2 = new VBox (false, 10);
			box2.BorderWidth = 10;
			box1.PackStart (box2, false, true, 0);

			Button close_button = new Button ("Close");
			close_button.Clicked += new EventHandler (Close_Button);
			box2.PackStart (close_button, true, true, 0);
			button.CanDefault = true;
			button.GrabDefault ();
			
			window.ShowAll ();
			return window;
		}

		static void pop_clicked (object o, EventArgs args)
		{
			Console.WriteLine ("Pop");
			statusbar.Pop (context_id);
		}

		static void statusbar_popped (object o, TextPoppedArgs args)
		{
			Console.WriteLine ("statusbar_popped signal");
			Console.WriteLine (args.Text);
			Console.WriteLine (args.ContextId);
		}

		static void statusbar_pushed (object o, EventArgs args)
		{
			string content = String.Format ("Push #{0}", counter);
			context_id = statusbar.GetContextId (content);
			statusbar.Push (context_id, content);

			counter ++;
			return;
		}

		static void Close_Button (object o, EventArgs args)
		{
			window.Destroy ();
		}
	}
}

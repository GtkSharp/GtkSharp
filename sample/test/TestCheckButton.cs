//
// TestCheckButton.cs
//
// Author: Duncan Mak  (duncan@ximian.com)
//
// Copyright (C) 2002, Duncan Mak, Ximian Inc.
//

using System;

using Gtk;

namespace WidgetViewer {	
	public class TestCheckButton
	{
		static Window window = null;	       
		static CheckButton checked_button = null;
		
		public static Gtk.Window Create ()
		{
			window = new Window ("GtkCheckButton");
			window.SetDefaultSize (200, 100);

			VBox box1 = new VBox (false, 0);
			window.Add (box1);

			VBox box2 = new VBox (false, 10);
			box2.BorderWidth = 10;
			box1.PackStart (box2, true, true, 0);

			checked_button = new CheckButton ("_button1");
			box2.PackStart (checked_button, true, true, 0);

 			checked_button = new CheckButton ("button2");
 			box2.PackStart (checked_button, true, true, 0);

			checked_button = new CheckButton ("button3");
			box2.PackStart (checked_button, true, true, 0);

			checked_button = new CheckButton ("Inconsistent");
			checked_button.Inconsistent = true;
			box2.PackStart (checked_button, true, true, 0);

			HSeparator separator = new HSeparator ();

			box1.PackStart (separator, false, false, 0);

			box2 = new VBox (false, 10);
			box2.BorderWidth = 10;
			box1.PackStart (box2, false, false, 0);

			Button button = new Button (Stock.Close);
			button.Clicked += new EventHandler (OnCloseClicked);
			button.CanDefault = true;
			
			box2.PackStart (button, true, true, 0);
			button.GrabDefault ();
			return window;
		}

		static void OnCloseClicked (object o, EventArgs args)
		{
			window.Destroy ();
		}
	}
}



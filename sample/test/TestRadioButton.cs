//
// TestRadioButton.cs
//
// Author: Duncan Mak  (duncan@ximian.com)
//
// Copyright (C) 2002, Duncan Mak, Ximian Inc.
//

using System;

using Gtk;
using GtkSharp;

namespace WidgetViewer {
	public class TestRadioButton
	{
		static Window window = null;
		static RadioButton radio_button = null;
		
		public static Gtk.Window Create ()
		{
			window = new Window ("GtkRadioButton");
			window.SetDefaultSize (200, 100);

			VBox box1 = new VBox (false, 0);
			window.Add (box1);

			VBox box2 = new VBox (false, 10);
			box2.BorderWidth = 10;
			box1.PackStart (box2, true, true, 0);

			radio_button = RadioButton.NewWithLabel (new GLib.SList (IntPtr.Zero), "Button 1");
			Console.WriteLine (radio_button);
			box2.PackStart (radio_button, true, true, 0);

			radio_button = RadioButton.NewWithLabelFromWidget (radio_button, "Button 2");
			radio_button.Active = true;
			box2.PackStart (radio_button, true, true, 0);

			radio_button = RadioButton.NewWithLabelFromWidget (radio_button, "Button 3");
			box2.PackStart (radio_button, true, true, 0);

			radio_button = RadioButton.NewWithLabelFromWidget (radio_button, "Inconsistent");
			radio_button.Inconsistent = true;
			box2.PackStart (radio_button, true, true, 0);

			box1.PackStart (new HSeparator (), false, true, 0);

			box2 = new VBox (false, 10);
			box2.BorderWidth = 10;
			box1.PackStart (box2, true, true, 0);
			
			radio_button = RadioButton.NewWithLabel (new GLib.SList (IntPtr.Zero), "Button 4");
			radio_button.Mode = false;
			box2.PackStart (radio_button, true, true, 0);

			radio_button = RadioButton.NewWithLabelFromWidget (radio_button, "Button 5");
			radio_button.Active = true;
			radio_button.Mode = false;
			box2.PackStart (radio_button, true, true, 0);

			radio_button = RadioButton.NewWithLabelFromWidget (radio_button, "Button 6");
			radio_button.Mode = false;
			box2.PackStart (radio_button, true, true, 0);

			box1.PackStart (new HSeparator (), false, true, 0);

			box2 = new VBox (false, 10);
			box2.BorderWidth = 10;
			box1.PackStart (box2, false, true, 0);

			Button button = new Button ("_Close");
			button.Clicked += new EventHandler (Close_Button);
			box2.PackStart (button, true, true, 0);
			button.CanDefault = true;
			button.GrabDefault ();

			return window;
		}

		static void Close_Button (object o, EventArgs args)
		{
			SignalArgs sa = (SignalArgs) args;
			window.Destroy ();
			sa.RetVal = true;
		}
	}
}

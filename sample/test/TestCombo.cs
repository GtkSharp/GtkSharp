//
// TestCombo.cs
//
// Author: Duncan Mak  (duncan@ximian.com)
//
// Copyright (C) 2003, Duncan Mak, Ximian Inc.
//

using System;

using Gtk;

namespace WidgetViewer {	
	public class TestCombo
	{
		static Window window = null;	       
		static Gtk.Combo combo = null;
		
		public static Gtk.Window Create ()
		{
			window = new Window ("GtkCombo");
			window.SetDefaultSize (200, 100);

			VBox box1 = new VBox (false, 0);
			window.Add (box1);

			VBox box2 = new VBox (false, 10);
			box2.BorderWidth = 10;
			box1.PackStart (box2, true, true, 0);

			combo = new Gtk.Combo ();
			string[] pop = {"Foo", "Bar"};
			combo.PopdownStrings = pop;
			combo.Entry.Activated += new EventHandler (OnComboActivated);
			box2.PackStart (combo, true, true, 0);

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

		static void OnComboActivated (object o, EventArgs args)
		{
			string text = ((Gtk.Entry) o).Text;

			// combo.AppendString (text);
			// combo.SetPopdownStrings (text);
		}
	}
}



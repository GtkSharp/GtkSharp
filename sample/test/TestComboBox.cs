// TestCombo.cs
//
// Author: Mike Kestner  (mkestner@novell.com)
//
// Copyright (c) 2005, Novell, Inc.
//

using System;
using Gtk;

namespace WidgetViewer {	
	public class TestComboBox
	{
		static Window window = null;	       
		
		public static Gtk.Window Create ()
		{
			window = new Window ("GtkComboBox");
			window.SetDefaultSize (200, 100);

			VBox box1 = new VBox (false, 0);
			window.Add (box1);

			VBox box2 = new VBox (false, 10);
			box2.BorderWidth = 10;
			box1.PackStart (box2, true, true, 0);

			ComboBoxEntry combo = new Gtk.ComboBoxEntry (new string[] {"Foo", "Bar"});
			combo.Changed += new EventHandler (OnComboActivated);
			combo.Entry.Changed += new EventHandler (OnComboEntryChanged);
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
			ComboBox combo = o as ComboBox;
			TreeIter iter;
			if (combo.GetActiveIter (out iter))
				Console.WriteLine ((string)combo.Model.GetValue (iter, 0));
		}

		static void OnComboEntryChanged (object o, EventArgs args)
		{
			Entry entry = o as Entry;
			Console.WriteLine ("Entry text is: " + entry.Text);
		}
	}
}



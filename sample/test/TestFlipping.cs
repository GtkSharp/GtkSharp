//
// TestFlipping.cs
//
// Author: Duncan Mak  (duncan@ximian.com)
//
// Copyright (C) 2002, Duncan Mak, Ximian Inc.
//

using System;

using Gtk;

namespace WidgetViewer {
	public class TestFlipping {

		static Dialog window = null;
		static CheckButton check_button = null;
		static Button button = null;
		static Label label = null;

		public static Gtk.Window Create ()
		{
			window = new Dialog ();
			window.Title = "Bi-directional flipping";
			window.SetDefaultSize (200, 100);

			label = new Label ("Label direction: <b>Left-to-right</b>");
			label.UseMarkup = true;
			label.SetPadding (3, 3);
			window.VBox.PackStart (label, true, true, 0);

			check_button = new CheckButton ("Toggle label direction");
			window.VBox.PackStart (check_button, true, true, 2);

			if (window.Direction == TextDirection.Ltr)
				check_button.Active = true;

			check_button.Toggled += new EventHandler (Toggle_Flip);
			check_button.BorderWidth = 10;

			button = new Button (Stock.Close);
			button.Clicked += new EventHandler (Close_Button);
			button.CanDefault = true;
			
			window.ActionArea.PackStart (button, true, true, 0);
			button.GrabDefault ();

			window.ShowAll ();
			return window;
		}

		static void Toggle_Flip (object o, EventArgs args)
		{
			if (((CheckButton) o).Active) {
				check_button.Direction = TextDirection.Ltr;
				label.Markup = "Label direction: <b>Left-to-right</b>";
			} else {
				check_button.Direction = TextDirection.Rtl;
				label.Markup = "Label direction: <b>Right-to-left</b>";
			}
		}

		static void Close_Button (object o, EventArgs args)
		{
			window.Destroy ();
		}
	}
}

//
// TestDialog.cs
//
// Author: Duncan Mak  (duncan@ximian.com)
//
// Copyright (C) 2002, Duncan Mak, Ximian Inc.
//


using System;

using Gtk;

namespace WidgetViewer {
	public class TestDialog
	{
		static Dialog window = null;
		static Label label = null;
		
		public static Gtk.Window Create ()
		{
			window = new Dialog ();
			window.Response += new ResponseHandler (Print_Response);
			window.SetDefaultSize (200, 100);

			window.Title = "GtkDialog";
			Button button = new Button (Stock.Ok);
			button.Clicked += new EventHandler (Close_Button);
			button.CanDefault = true;
			window.ActionArea.PackStart (button, true, true, 0);
			button.GrabDefault ();

			ToggleButton toggle_button = new ToggleButton ("Toggle Label");
			toggle_button.Clicked += new EventHandler (Label_Toggle);
			window.ActionArea.PackStart (toggle_button, true, true, 0);

			toggle_button = new ToggleButton ("Toggle Separator");
			toggle_button.Clicked += new EventHandler (Separator_Toggle);
			window.ActionArea.PackStart (toggle_button, true, true, 0);

			window.ShowAll ();

			return window;
		}

		static void Close_Button (object o, EventArgs args)
		{
			window.Destroy ();
		}

		static void Print_Response (object o, ResponseArgs args)
		{
			Console.WriteLine ("Received response signal: " + args.ResponseId);
		}

		static void Label_Toggle (object o, EventArgs args)
		{
			if (label == null) {
				label = new Label ("This is Text label inside a Dialog");
				label.SetPadding (10, 10);
				window.VBox.PackStart (label, true, true, 0);
				label.Show ();
			} else {
				label.Destroy ();
				label = null;
			}
		}

		static void Separator_Toggle (object o, EventArgs args)
		{
			window.HasSeparator = (!((ToggleButton) o).Active);
		}
	}
}


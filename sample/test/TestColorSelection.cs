//
// TestColorSelection.cs
//
// Author: Duncan Mak  (duncan@ximian.com)
//
// Copyright (C) 2002, Duncan Mak, Ximian Inc.
//

using System;

using Gtk;
using GtkSharp;

namespace WidgetViewer {
	public class TestColorSelection
	{
		static ColorSelectionDialog window = null;
		static Dialog dialog = null;

		public static Gtk.Window Create ()
		{
			HBox options = new HBox (false, 0);
			CheckButton check_button = null;

			window = new ColorSelectionDialog ("Color selection dialog");
			window.ColorSelection.HasOpacityControl = true;
			window.ColorSelection.HasPalette = true;

			window.SetDefaultSize (250, 200);
			window.VBox.PackStart (options, false, false, 0);
			window.VBox.BorderWidth = 10;

			check_button = new CheckButton("Show Opacity");
			check_button.Active = true;
			options.PackStart (check_button, false, false, 0);
			check_button.Toggled += new EventHandler (Opacity_Callback);

			check_button = new CheckButton("Show Palette");
			check_button.Active = true;
			options.PackEnd (check_button, false, false, 0);
			check_button.Toggled += new EventHandler (Palette_Callback);

			window.ColorSelection.ColorChanged += new EventHandler (Color_Changed);
			window.OkButton.Clicked += new EventHandler (Color_Selection_OK);
			window.CancelButton.Clicked += new EventHandler (Color_Selection_Cancel); 

			options.ShowAll ();

			return window;
		}

		static void Opacity_Callback (object o, EventArgs args)
		{
			window.ColorSelection.HasOpacityControl = ((ToggleButton )o).Active;
		}

		static void Palette_Callback (object o, EventArgs args)
		{
			window.ColorSelection.HasPalette = ((ToggleButton )o).Active;
		}

		static void Color_Changed (object o, EventArgs args)
		{
			Gdk.Color color = window.ColorSelection.CurrentColor;
		}

		static void Color_Selection_OK (object o, EventArgs args)
		{
			Gdk.Color selected = window.ColorSelection.CurrentColor;
/*
			if (selected == null) {
				Console.WriteLine ("Color selection failed.");
				return;
			}
*/			
			Display_Result (selected);
		}

		static void Color_Selection_Cancel (object o, EventArgs args)
		{
			if (dialog != null)
				dialog.Destroy ();
			window.Destroy ();
		}

		static void Display_Result (Gdk.Color color)
		{
/*
			if (color == null)
				Console.WriteLine ("Null color");
*/			
			dialog = new Dialog ();
			dialog.Title = "Selected Color";

			DrawingArea da = new DrawingArea ();

			da.ModifyBg (StateType.Normal, color);

			Console.WriteLine (da);

			dialog.VBox.PackStart (da, true, true, 0);

			Button button = new Button ("OK");
			button.Clicked += new EventHandler (Close_Button);
			button.CanDefault = true;
			dialog.ActionArea.PackStart (button, true, true, 0);
			button.GrabDefault ();

			dialog.ShowAll ();
		}

		static void Close_Button (object o, EventArgs args)
		{
			Color_Selection_Cancel (o, args);
		}
	}
}

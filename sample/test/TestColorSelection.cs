//
// TestColorSelection.cs
//
// Author: Duncan Mak  (duncan@ximian.com)
//
// Copyright (C) 2002, Duncan Mak, Ximian Inc.
//

using System;
using System.Text;

using Gtk;

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

		static string HexFormat (Gdk.Color color)
		{
			StringBuilder s = new StringBuilder ();
			ushort[] vals = { color.Red, color.Green, color.Blue };
			char[] hexchars = {'0', '1', '2', '3', '4', '5', '6', '7', '8', '9',
									 'A', 'B', 'C', 'D', 'E', 'F'};

			s.Append ('#');
			foreach (ushort val in vals) {
				/* Convert to a range of 0-255, then lookup the
				 * digit for each half-byte */
				byte rounded = (byte) (val >> 8);
				s.Append (hexchars[(rounded & 0xf0) >> 4]);
				s.Append (hexchars[rounded & 0x0f]);
			}

			return s.ToString ();
		}
		
		static void Color_Changed (object o, EventArgs args)
		{
			Gdk.Color color = window.ColorSelection.CurrentColor;
			Console.WriteLine (HexFormat (color));
		}

		static void Color_Selection_OK (object o, EventArgs args)
		{
			Gdk.Color selected = window.ColorSelection.CurrentColor;
			window.Hide ();
			Display_Result (selected);
		}

		static void Color_Selection_Cancel (object o, EventArgs args)
		{
			window.Destroy ();
		}

		static void Dialog_Ok (object o, EventArgs args)
		{
			dialog.Destroy ();
			window.ShowAll ();
		}

		static void Display_Result (Gdk.Color color)
		{
			dialog = new Dialog ();
			dialog.Title = "Selected Color: " + HexFormat (color);
			dialog.HasSeparator = true;

			DrawingArea da = new DrawingArea ();

			da.ModifyBg (StateType.Normal, color);

			dialog.VBox.BorderWidth = 10;
			dialog.VBox.PackStart (da, true, true, 10);
			dialog.SetDefaultSize (200, 200);

			Button button = new Button (Stock.Ok);
			button.Clicked += new EventHandler (Dialog_Ok);
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

using System;

using Gtk;
using GtkSharp;

namespace WidgetViewer {
	public class TestColorSelection
	{
		static ColorSelectionDialog window = null;

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
			options.PackStart (check_button, false, false, 0);
			check_button.Toggled += new EventHandler (Opacity_Callback);

			check_button = new CheckButton("Show Palette");
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
			Gdk.Color color = window.ColorSelection.CurrentColor;
			window.ColorSelection.CurrentColor = color;
		}

		static void Color_Selection_Cancel (object o, EventArgs args)
		{
			window.Destroy ();
		}
	}
}

using System;

using Gtk;
using GtkSharp;

namespace WidgetViewer {
	public class TestDialog
	{
		static Dialog window = null;
		static Label label = null;
		
		public static Gtk.Window Create ()
		{
			window = new Dialog ();
			window.Response += new EventHandler (Print_Response);

			window.Title = "GtkDialog";
			Button button = new ToggleButton ("OK");
			button.Clicked += new EventHandler (Close_Button);
			button.CanDefault = true;
			window.ActionArea.PackStart (button, true, true, 0);
			button.GrabDefault ();

			ToggleButton toggle_button = new ToggleButton ("Toggle Label");
			toggle_button.Clicked += new EventHandler (Label_Toggle);
			window.ActionArea.PackStart (toggle_button, true, true, 0);

			toggle_button = new ToggleButton ("Toggle Separator");
			button.Clicked += new EventHandler (Separator_Toggle);
			window.ActionArea.PackStart (toggle_button, true, true, 0);

			window.ShowAll ();

			return window;
		}

		static void Close_Button (object o, EventArgs args)
		{
			SignalArgs sa = (SignalArgs) args;
			window.Destroy ();
			sa.RetVal = true;
		}

		static void Print_Response (object o, EventArgs args)
		{
			SignalArgs sa = (SignalArgs) args;
			Console.WriteLine ("Received response signal: " + sa.Args [1]);
			sa.RetVal = true;
		}

		static void Label_Toggle (object o, EventArgs args)
		{
			if (label == null) {
				Console.WriteLine ("Null label");
				label = new Label ("Text label");
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
			window.HasSeparator = ((ToggleButton) o).Active;
		}
	}
}

// SpinButton.cs - Gtk# Tutorial example
//
// Author: Alejandro Sánchez Acosta <raciel@es.gnu.org>
// 	   Cesar Octavio Lopez Nataren <cesar@ciencias.unam.mx>
//
// (c) 2002 Alejandro Sánchez Acosta


namespace GtkSharpTutorial {

	using Gtk;
	using GtkSharp;
	using System;
	using System.Drawing;

	using System.Text; 

	public class SpinButtonSample
	{
		static Gtk.Window window;
		static Frame frame;
		static HBox hbox; 
		static VBox main_vbox;
		static VBox vbox;
		static VBox vbox2;
		static SpinButton spinner;
		static SpinButton spinner1;
		static SpinButton spinner2;
		static Button button;
		static Label label;
		static Label val_label;
		static Adjustment adj;
		
		static void toggle_snap (object obj, EventArgs args)
		{
			spinner1.SnapToTicks = ((ToggleButton) obj).Active;
		}
		
		static void toggle_numeric (object obj, EventArgs args)
		{
			spinner1.Numeric = ((ToggleButton) obj).Active;
		}

		// FIXME: exception emitted when called.
		static void change_digits (object obj, EventArgs args)
		{
			spinner1.Digits = (uint) ((SpinButton) obj).ValueAsInt;
		}

		static void get_value (object obj, EventArgs args)
		{
			StringBuilder buff;
			Label label;
			SpinButton spin;
			
			spin = spinner1;
			buff =new StringBuilder ("ERROR");

			label = new Label ("1");
			label.Text = buff.ToString ();
		}
		
		static void delete_event (object obj, DeleteEventArgs args)
		{
			SignalArgs sa = (SignalArgs) args;
			Application.Quit();
			sa.RetVal = true;
		}

		static void exitbutton_event (object obj, EventArgs args)
		{
			Application.Quit();
		}

		public static void Main (string[] args)
		{
			Application.Init();
			
			window = new Gtk.Window ("SpinButton#");
			window.DeleteEvent += new DeleteEventHandler (delete_event);
			
			main_vbox = new VBox (false, 5);
			main_vbox.BorderWidth = 10;
			window.Add (main_vbox);

			frame = new Frame ("Not accelerated");
			main_vbox.PackStart (frame, true, true, 0);

			vbox = new VBox (false, 0);
			vbox.BorderWidth = 5;
			frame.Add (vbox);

			// Day, month, year spinners 

			hbox = new HBox (false, 0);
			vbox.PackStart (hbox, true, true, 5);

			vbox2 = new VBox (false, 0);
			hbox.PackStart (vbox2, true, true, 5);

			label = new Label ("Day :");
			((Gtk.Misc) label).SetAlignment ((float) 0.0, (float) 0.5);
			vbox2.PackStart (label, false, true, 0);

			adj = new Adjustment (1.0, 1.0, 31.0, 1.0, 5.0, 0.0);

			spinner = new SpinButton (adj, 0, 0);

			spinner.Wrap = true;
			vbox2.PackStart (spinner, false, true, 0);

			vbox2 = new VBox (false, 0);
			hbox.PackStart (vbox2, true, true, 5);

			label = new Label ("Month :");
			((Gtk.Misc) label).SetAlignment ((float) 0.0, (float) 0.5);

			vbox2.PackStart (label, false, true, 0);

			adj = new Adjustment (1.0, 1.0, 12.0, 1.0,
					      5.0, 0.0);
			
			spinner = new SpinButton (adj, 0, 0);
			spinner.Wrap = true;
			vbox2.PackStart (spinner, false, true, 0);

			vbox2 = new VBox (false, 0);
			hbox.PackStart (vbox2, true, true, 5);

			label = new Label ("Year :");
			((Gtk.Misc) label).SetAlignment ((float) 0.0, (float) 0.5);
			vbox2.PackStart (label, false, true, 0);

			adj = new Adjustment (1998.0, 0.0, 2100.0, 1.0, 100.0, 0.0);

			spinner = new SpinButton (adj, 0, 0);
			spinner.Wrap = false;
			spinner.SetSizeRequest (55, -1);
			vbox2.PackStart (spinner, false, true, 0);

			frame = new Frame ("Accelerated");
			main_vbox.PackStart (frame, true, true, 0);

			vbox = new VBox (false, 0);
			vbox.BorderWidth = 5;
			frame.Add (vbox);

			hbox = new HBox (false, 0);
			vbox.PackStart (hbox, false, true, 5);

			vbox2 = new VBox (false, 0);
			hbox.PackStart (vbox2, true, true, 5);

			label = new Label ("Value :");
			((Gtk.Misc) label).SetAlignment ((float) 0.0, (float) 0.5);
			vbox2.PackStart (label, false, true, 0);

			adj = new Adjustment (0.0, -10000.0, 10000.0, 0.5, 100.0, 0.0);
			
			spinner1 = new SpinButton (adj, 1.0, 2);
			spinner1.Wrap = true;
			spinner1.SetSizeRequest (100, -1);
			vbox2.PackStart (spinner1, false, true, 0);

			vbox2 = new VBox (false, 0);
			hbox.PackStart (vbox2, true, true, 5);

			label = new Label ("Digits :");
			((Gtk.Misc) label).SetAlignment ((float) 0, (float) 0.5);
			vbox2.PackStart (label, false, true, 0);

			adj = new Adjustment (2, 1, 5, 1, 1, 0);
			
			spinner2 = new SpinButton (adj, 0.0, 0);
			spinner2.Wrap = true;
			
			adj.ValueChanged += new EventHandler (change_digits);
			vbox2.PackStart (spinner2, false, true, 0);

			hbox = new HBox (false, 0);
			vbox.PackStart (hbox, false, true, 5);

			button = new CheckButton ("Snap to 0.5-ticks");
			button.Clicked += new EventHandler (toggle_snap);
			vbox.PackStart (button, true, true, 0);
			((Gtk.ToggleButton) button).Active = true;

			button = new CheckButton ("Numeric only input mode");
			button.Clicked += new EventHandler (toggle_numeric);
			vbox.PackStart (button, true, true, 0);
			((Gtk.ToggleButton) button).Active = true;

			val_label = new Label ("");

			hbox = new HBox (false, 0);
			vbox.PackStart (hbox, false, true, 5);
			button = new Button ("Value as Int");
			button.SetData ("user_data", val_label);

			button.Clicked += new EventHandler (get_value);
			hbox.PackStart (button, true, true, 5);

			button = new Button ("Value as Float");
			button.SetData ("user_data", val_label);
			button.Clicked += new EventHandler (get_value);
			hbox.PackStart (button, true, true, 5);
			label.Text = "0";

			vbox.PackStart (val_label, true, true, 0);
			val_label.Text = "0";
			
			hbox = new HBox (false, 0);
			main_vbox.PackStart (hbox, false, true, 0);

			button = new Button ("Close");
			button.Clicked += new EventHandler (exitbutton_event);
			
			hbox.PackStart (button, true, true, 5);

			window.ShowAll();
						
			Application.Run ();
		}
	}
}

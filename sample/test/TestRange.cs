//
// TestRange.cs
//
// Author: Duncan Mak  (duncan@ximian.com)
//
// Copyright (C) 2002, Duncan Mak, Ximian Inc.
//

using System;

using Gtk;

namespace WidgetViewer {

	public class TestRange
	{
		static Window window = null;
		
		public static Gtk.Window Create ()
		{
			window = new Window ("GtkRange");
			window.SetDefaultSize (250, 200);

			VBox box1 = new VBox (false, 0);
			window.Add (box1);

			VBox box2 = new VBox (false, 0);
			box2.BorderWidth = 10;
			box1.PackStart (box2, true, true, 0);

			Adjustment adjustment = new Adjustment (0.0, 0.0, 101.0, 0.1, 1.0, 1.0);

			HScale hscale = new HScale (adjustment);
			hscale.SetSizeRequest (150, -1);
			((Range) hscale).UpdatePolicy = UpdateType.Delayed;

			hscale.Digits = 1;
			hscale.DrawValue = true;
			box2.PackStart (hscale, true, true, 0);

			HScrollbar hscrollbar = new HScrollbar (adjustment);
			((Range) hscrollbar).UpdatePolicy = UpdateType.Continuous;
			box2.PackStart (hscrollbar, true, true, 0);

			hscale = new HScale (adjustment);
			hscale.DrawValue = true;
			hscale.FormatValue += new FormatValueHandler (reformat_value);

			box2.PackStart (hscale, true, true, 0);

			HBox hbox = new HBox (false, 0);
			VScale vscale = new VScale (adjustment);
			vscale.SetSizeRequest (-1, 200);
			vscale.Digits = 2;
			vscale.DrawValue = true;
			hbox.PackStart (vscale, true, true, 0);
			
			vscale = new VScale (adjustment);
			vscale.SetSizeRequest (-1, 200);
			vscale.Digits = 2;
			vscale.DrawValue = true;
			((Range) vscale).Inverted = true;
			hbox.PackStart (vscale, true, true, 0);

			vscale = new VScale (adjustment);
			vscale.DrawValue = true;
			vscale.FormatValue += new FormatValueHandler (reformat_value);
			hbox.PackStart (vscale, true, true, 0);

			box2.PackStart (hbox, true, true, 0);

			box1.PackStart (new HSeparator (), false, true, 0);

			box2 = new VBox (false, 10);
			box2.BorderWidth = 10;
			box1.PackStart (box2, false, true, 0);

			Button button = new Button (Stock.Close);
			button.Clicked += new EventHandler (Close_Button);
			box2.PackStart (button, true, true, 0);
			button.CanDefault = true;
			button.GrabDefault ();
			
			window.ShowAll ();
			return window;
		}

		static void Close_Button (object o, EventArgs args)
		{
			window.Destroy ();
		}

		static void reformat_value (object o, FormatValueArgs args)
		{
			int x = (int) args.Value;
			args.RetVal = x.ToString ();
		}
	}
}

//
// WidgetViewer.cs
//
// Author: Duncan Mak  (duncan@ximian.com)
//
// Copyright (C) 2002, Duncan Mak, Ximian Inc.
//

using System;

using Gtk;
using GtkSharp;

namespace WidgetViewer {

	public class Viewer
	{
		static Window window = null;
		static Window viewer = null;
		static Button button = null;
		
		static void Main ()
		{
			Application.Init ();
			window = new Window ("Gtk# Widget viewer");
			window.DeleteEvent += new EventHandler (Window_Delete);
			window.SetDefaultSize (250, 200);
			
			VBox box1 = new VBox (false, 0);
			window.Add (box1);

			VBox box2 = new VBox (false, 5);
			box2.BorderWidth = 10;
			box1.PackStart (box2, true, true, 0);

			button = new Button ("Check Buttons");
			button.Clicked += new EventHandler (Check_Buttons);
			box2.PackStart (button, false, false, 0);

			button = new Button ("Color Selection");
			button.Clicked += new EventHandler (Color_Selection);
			box2.PackStart (button, false, false, 0);

			button = new Button ("Dialog");
			button.Clicked += new EventHandler (Dialog);
			box2.PackStart (button, false, false, 0);

			button = new Button ("File Selection");
			button.Clicked += new EventHandler (File_Selection);
			box2.PackStart (button, false, false, 0);

			button = new Button ("Radio Buttons");
			button.Clicked += new EventHandler (Radio_Buttons);
			box2.PackStart (button, false, false, 0);

			button = new Button ("Range Controls");
			button.Clicked += new EventHandler (Range_Controls);
			box2.PackStart (button, false, false, 0);

			button = new Button ("Statusbar");
			button.Clicked += new EventHandler (Statusbar);
			box2.PackStart (button, false, false, 0);

			button = new Button ("Toolbar");
			button.Clicked += new EventHandler (Toolbar);
			box2.PackStart (button, false, false, 0);
			
			box1.PackStart (new HSeparator (), false, false, 0);
			
			box2 = new VBox (false, 10);
			box2.BorderWidth = 10;
			box1.PackStart (box2, false, false, 0);

			Button close_button = new Button ("_Close");
			close_button.Clicked += new EventHandler (Close_Button);
			box2.PackStart (close_button, true, true, 0);

			window.ShowAll ();
			Application.Run ();
		}

		static void Window_Delete (object o, EventArgs args)
		{
			SignalArgs sa = (SignalArgs) args;
			Application.Quit ();
			sa.RetVal = true;
		}

		static void Viewer_Delete (object o, EventArgs args)
		{
			SignalArgs sa = (SignalArgs) args;
			viewer.Destroy ();
			sa.RetVal = true;
		}

		static void Close_Button (object o, EventArgs args)
		{
			Window_Delete (o, args);
		}

		static void Check_Buttons (object o, EventArgs args)
		{
			viewer = TestCheckButton.Create ();
			viewer.ShowAll ();
		}

		static void Color_Selection (object o, EventArgs args)
		{
			viewer = TestColorSelection.Create ();
			viewer.ShowAll ();
		}

		static void File_Selection (object o, EventArgs args)
		{
			viewer = TestFileSelection.Create ();
			viewer.DeleteEvent += new EventHandler (Viewer_Delete);
			viewer.ShowAll ();
		}

		static void Radio_Buttons (object o, EventArgs args)
		{
			viewer = TestRadioButton.Create ();
			viewer.DeleteEvent += new EventHandler (Viewer_Delete);
			viewer.ShowAll ();
		}

		static void Range_Controls (object o, EventArgs args)
		{
			viewer = TestRange.Create ();
			viewer.DeleteEvent += new EventHandler (Viewer_Delete);
			viewer.ShowAll ();
		}

		static void Statusbar (object o, EventArgs args)
		{
			viewer = TestStatusbar.Create ();
			viewer.DeleteEvent += new EventHandler (Viewer_Delete);
			viewer.ShowAll ();
		}

		static void Toolbar (object o, EventArgs args)
		{
			viewer = TestToolbar.Create ();
			viewer.DeleteEvent += new EventHandler (Window_Delete);
			viewer.ShowAll ();
		}

		static void Dialog (object o, EventArgs args)
		{
			viewer = TestDialog.Create ();
			viewer.DeleteEvent += new EventHandler (Window_Delete);
			viewer.ShowAll ();
		}

	}
}

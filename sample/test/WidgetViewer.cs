//
// WidgetViewer.cs
//
// Author: Duncan Mak  (duncan@ximian.com)
//
// Copyright (C) 2002, Duncan Mak, Ximian Inc.
//

using System;

using Gtk;

namespace WidgetViewer {
	public class Viewer
	{
		static Window window = null;
		static Window viewer = null;
		static Button button = null;
		static VBox box2 = null;
		
		static void Main ()
		{
			Application.Init ();
			window = new Window ("Gtk# Widget viewer");
			window.DeleteEvent += new DeleteEventHandler (Window_Delete);
			window.SetDefaultSize (250, 200);
			
			VBox box1 = new VBox (false, 0);
			window.Add (box1);
			
			box2 = new VBox (false, 5);
			box2.BorderWidth = 10;
			
			Frame frame = new Frame ("Select a widget");
			frame.BorderWidth = 5;
			frame.Add (box2);
			
			box1.PackStart (frame, true, true, 0);

			AddButton ("Bi-directional flipping", new EventHandler (Flipping));
			AddButton ("Check Buttons", new EventHandler (Check_Buttons));
			AddButton ("Color Selection", new EventHandler (Color_Selection));
			AddButton ("Combo Box", new EventHandler (Combo_Box));
			AddButton ("New Combo Box", new EventHandler (New_Combo_Box));
			AddButton ("Dialog", new EventHandler (Dialog));
			AddButton ("File Selection", new EventHandler (File_Selection));
			AddButton ("Menus", new EventHandler (Menus));
			AddButton ("Radio Buttons", new EventHandler (Radio_Buttons));
			AddButton ("Range Controls", new EventHandler (Range_Controls));
			AddButton ("Size Groups", new EventHandler (Size_Groups));
			AddButton ("Statusbar", new EventHandler (Statusbar));
			AddButton ("Toolbar", new EventHandler (Toolbar));
			
			box1.PackStart (new HSeparator (), false, false, 0);
			
			box2 = new VBox (false, 10);
			box2.BorderWidth = 10;
			box1.PackStart (box2, false, false, 0);

			Button close_button = new Button (Stock.Close);
			close_button.Clicked += new EventHandler (Close_Button);
			box2.PackStart (close_button, true, true, 0);

			window.ShowAll ();
			Application.Run ();
		}

		static void AddButton (string caption, EventHandler handler)
		{
			button = new Button (caption);
			button.Clicked += handler;
			box2.PackStart (button, false, false, 0);
		}

		static void AddWindow (Window dialog)
		{
			viewer = dialog;
			viewer.DeleteEvent += new DeleteEventHandler (Viewer_Delete);
			viewer.ShowAll ();
		}

		static void Window_Delete (object o, DeleteEventArgs args)
		{
			Application.Quit ();
			args.RetVal = true;
		}

		static void Viewer_Delete (object o, DeleteEventArgs args)
		{
			viewer.Destroy ();
			viewer = null;
			args.RetVal = true;
		}

		static void Close_Button (object o, EventArgs args)
		{
			Application.Quit ();
		}

		static void Check_Buttons (object o, EventArgs args)
		{
			AddWindow (TestCheckButton.Create ());
		}

		static void Color_Selection (object o, EventArgs args)
		{
			AddWindow (TestColorSelection.Create ());
		}

		static void File_Selection (object o, EventArgs args)
		{
			//AddWindow (TestFileSelection.Create ());
		}

		static void Radio_Buttons (object o, EventArgs args)
		{
			AddWindow (TestRadioButton.Create ());
		}

		static void Range_Controls (object o, EventArgs args)
		{
			AddWindow (TestRange.Create ());
		}

		static void Statusbar (object o, EventArgs args)
		{
			AddWindow (TestStatusbar.Create ());
		}

		static void Toolbar (object o, EventArgs args)
		{
			//AddWindow (TestToolbar.Create ());
		}

		static void Dialog (object o, EventArgs args)
		{
			AddWindow (TestDialog.Create ());
		}

		static void Flipping (object o, EventArgs args)
		{
			AddWindow (TestFlipping.Create ());
		}

		static void Menus (object o, EventArgs args)
		{
			//AddWindow (TestMenus.Create ());
		}

		static void Size_Groups (object o, EventArgs args)
		{
			AddWindow (TestSizeGroup.Create ());
		}

		static void New_Combo_Box (object o, EventArgs args)
		{
			AddWindow (TestComboBox.Create ());
		}

		static void Combo_Box (object o, EventArgs args)
		{
			AddWindow (TestCombo.Create ());
		}
	}
}

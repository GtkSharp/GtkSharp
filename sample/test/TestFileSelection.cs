//
// TestFileSelection.cs
//
// Author: Duncan Mak  (duncan@ximian.com)
//
// Copyright (C) 2002, Duncan Mak, Ximian Inc.
//

using System;

using Gtk;
using GtkSharp;

namespace WidgetViewer {
	public class TestFileSelection
	{
		static FileSelection window = null;
		static ToggleButton toggle_button = null;
		static CheckButton check_button = null;
		
		public static Gtk.Window Create ()
		{
			window = new FileSelection ("File Selection Dialog");
			window.HideFileopButtons ();
			window.OkButton.Clicked += new EventHandler (file_selection_ok);
			window.CancelButton.Clicked += new EventHandler (file_selection_cancel);
			
			check_button = new CheckButton ("Show Fileops");
			check_button.Toggled += new EventHandler (show_fileops);
			window.ActionArea.PackStart (check_button, false, false, 0);

			toggle_button = new ToggleButton ("Select Multiple");
			toggle_button.Clicked += new EventHandler (select_multiple);
			window.ActionArea.PackStart (toggle_button, false, false, 0);

			window.ShowAll ();
			return window; 
		}

		static void file_selection_ok (object o, EventArgs args)
		{
			Console.WriteLine (window.Selections);
		}

		static void show_fileops (object o, EventArgs args)
		{
			if (((ToggleButton) o).Active)
				window.ShowFileopButtons ();
			else
				window.HideFileopButtons ();
		}

		static void select_multiple (object o, EventArgs args)
		{
			window.SelectMultiple = toggle_button.Active;
		}

		static void file_selection_cancel (object o, EventArgs args)
		{
			SignalArgs sa = (SignalArgs) args;
			window.Destroy ();
			sa.RetVal = true;
		}
	}
}


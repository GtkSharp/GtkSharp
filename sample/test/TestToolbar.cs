//
// TestToolbar.cs
//
// Author: Duncan Mak  (duncan@ximian.com)
//
// Copyright (C) 2002, Duncan Mak, Ximian Inc.
//

using System;

using Gtk;

namespace WidgetViewer {
	public class TestToolbar {

		static Window window = null;
		static Toolbar toolbar = null;
		static bool showTooltips = true;
		
		public static Gtk.Window Create ()
		{
			window = new Window ("Toolbar");
			window.Resizable = false;

			toolbar = new Toolbar ();
			toolbar.InsertStock (Stock.New, "Stock icon: New", "Toolbar/New",
					     new SignalFunc (set_small_icon), IntPtr.Zero, -1);

			toolbar.InsertStock (Stock.Open, "Stock icon: Open", "Toolbar/Open",
					     new SignalFunc (set_large_icon), IntPtr.Zero, -1);

			toolbar.AppendSpace ();

			toolbar.AppendItem ("Toggle tooltips", "toggle showing of tooltips", "Toolbar/Tooltips",
					    new Image (Stock.DialogInfo, IconSize.LargeToolbar),
					    new SignalFunc (toggle_tooltips));
			
			toolbar.AppendSpace ();

			toolbar.AppendItem ("Horizontal", "Horizontal layout", "Toolbar/Horizontal",
					    new Image (Stock.GoForward, IconSize.LargeToolbar),
					    new SignalFunc (set_horizontal));

			toolbar.AppendItem ("Vertical", "Vertical layout", "Toolbar/Vertical",
					    new Image (Stock.GoUp, IconSize.LargeToolbar),
					    new SignalFunc (set_vertical));
			
			toolbar.AppendSpace ();

			toolbar.AppendItem ("Icons", "Only show icons", "Toolbar/IconsOnly",
					    new Image (Stock.Home, IconSize.LargeToolbar),
					    new SignalFunc (set_icon_only));

			toolbar.AppendItem ("Text", "Only show Text", "Toolbar/TextOnly",
					    new Image (Stock.JustifyFill, IconSize.LargeToolbar),
					    new SignalFunc (set_text_only));

			toolbar.AppendItem ("Both", "Show both Icon & Text", "Toolbar/Both",
					    new Image (Stock.Index, IconSize.LargeToolbar),
					    new SignalFunc (set_both));

			toolbar.AppendItem ("Both (Horizontal)", "Show Icon & Text horizontally", "Toolbar/BothHoriz",
					    new Image (Stock.Index, IconSize.LargeToolbar),
					    new SignalFunc (set_both_horiz));

			toolbar.AppendSpace ();

			toolbar.InsertStock (Stock.Close, "Stock icon: Close", "Toolbar/Close",
					     new SignalFunc (Close_Button), IntPtr.Zero, -1);

			window.Add (toolbar);
			window.ShowAll ();
			return window;
		}

		static void set_small_icon ()
		{
			toolbar.IconSize = IconSize.SmallToolbar;
		}

		static void set_large_icon ()
		{
			toolbar.IconSize = IconSize.LargeToolbar;
		}

		static void set_icon_only ()
		{
			toolbar.ToolbarStyle = ToolbarStyle.Icons;
		}

		static void set_text_only ()
		{
			toolbar.ToolbarStyle = ToolbarStyle.Text;
		}

		static void set_horizontal ()
		{
			toolbar.Orientation = Orientation.Horizontal;
		}

		static void set_vertical ()
		{
			toolbar.Orientation = Orientation.Vertical;
		}
		
		static void set_both ()
		{
			toolbar.ToolbarStyle = ToolbarStyle.Both;
		}

		static void set_both_horiz ()
		{
			toolbar.ToolbarStyle = ToolbarStyle.BothHoriz;
		}

		static void toggle_tooltips ()
		{
			if (showTooltips == true)
				showTooltips = false;
			else
				showTooltips = true;

			toolbar.Tooltips = showTooltips;
			Console.WriteLine ("Show tooltips: " + showTooltips);
		}

		static void Close_Button ()
		{
			window.Destroy ();
		}
	}
}

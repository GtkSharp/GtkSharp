//
// TestMenus.cs
//
// Author: Duncan Mak  (duncan@ximian.com)
//
// Copyright (C) 2002, Duncan Mak, Ximian Inc.
//

using System;

using Gtk;
using GtkSharp;

namespace WidgetViewer {
	public class TestMenus {

		static Window window = null;

		public static Gtk.Window Create ()
		{
			window = new Window ("Menus");
			
			AccelGroup accel_group = new AccelGroup ();
			window.AddAccelGroup (accel_group);

			VBox box1 = new VBox (false, 0);
			window.Add (box1);

			MenuBar menubar = new MenuBar ();
			box1.PackStart (menubar, false, false, 0);

			Menu menu = Create_Menu (2, true);
			MenuItem menuitem = new MenuItem ("foo");
			menuitem.Submenu = menu;
			menubar.Append (menuitem);

			menuitem = new MenuItem ("bar");
			menuitem.Submenu = Create_Menu (3, true);
			menubar.Append (menuitem);
			
			Image image = new Image (Stock.Help, IconSize.Menu);

			menuitem = new ImageMenuItem ("Help");
			((ImageMenuItem) menuitem).Image = image;
			menuitem.Submenu = Create_Menu (4, true);
			menuitem.RightJustified = true;
			menubar.Append (menuitem);

			menubar = new MenuBar ();
			box1.PackStart (menubar, false, true, 0);
			
			menu = Create_Menu (2, true);
			
			menuitem = new MenuItem ("Second menu bar");
			menuitem.Submenu = menu;
			menubar.Append (menuitem);

			VBox box2 = new VBox (false, 10);
			box2.BorderWidth = 10;
			box1.PackStart (box2, true, true, 0);
			
			menu = Create_Menu (1, false);
			menu.AccelGroup = accel_group;

			menu.Append (new SeparatorMenuItem ());

			menuitem = new CheckMenuItem ("Accelerate Me");
			menu.Append (menuitem);
			menuitem.AddAccelerator ("activate", accel_group, 0xFFBE, 0, AccelFlags.Visible);
			
			menuitem = new CheckMenuItem ("Accelerator locked");
			menu.Append (menuitem);
			menuitem.AddAccelerator ("activate", accel_group, 0xFFBF, 0, AccelFlags.Visible | AccelFlags.Locked);

			menuitem = new CheckMenuItem ("Accelerator Frozen");
			menu.Append (menuitem);
			menuitem.AddAccelerator ("activate", accel_group, 0xFFBF, 0, AccelFlags.Visible);
			menuitem.AddAccelerator ("activate", accel_group, 0xFFC0, 0, AccelFlags.Visible);
			
			OptionMenu option_menu = new OptionMenu ();
			option_menu.Menu = menu;
			option_menu.SetHistory (3);
			box2.PackStart (option_menu, true, true, 0);
			
			box1.PackStart (new HSeparator (), false, false, 0);
			
			box2 = new VBox (false, 10);
			box2.BorderWidth = 10;
			box1.PackStart (box2, false, true, 0);
			
			Button close_button = Button.NewFromStock (Stock.Close);
			close_button.Clicked += new EventHandler (Close_Button);
			box2.PackStart (close_button, true, true, 0);
			
			close_button.CanDefault = true;
			close_button.GrabDefault ();
			
			window.ShowAll ();
			return window;
		}

		static Menu Create_Menu (int depth, bool tearoff)
		{
			if (depth < 1)
				return null;

			Menu menu = new Menu ();
			MenuItem menuitem = null;
			string label = null;
			GLib.SList group = new GLib.SList (IntPtr.Zero);

			if (tearoff) {
				menuitem = new TearoffMenuItem ();
				menu.Append (menuitem);
				menuitem.Show ();
			}

			for (int i = 0, j = 1; i < 5; i++, j++) {

				label = String.Format ("item {0} - {1}", depth, j);
				menuitem = RadioMenuItem.NewWithLabel (group, label);
				group = ((RadioMenuItem) menuitem).Group;
				menuitem = new MenuItem (label);
				menu.Append (menuitem);
				
				if (i == 3)
					menuitem.Sensitive = false;
				
				Menu child = Create_Menu ((depth - 1), true);

				if (child != null)
					menuitem.Submenu = child;
			}

			return menu;
		}

		static void Close_Button (object o, EventArgs args)
		{
			window.Destroy ();
		}
	}
}

//
// TestMenus.cs
//
// Author: Duncan Mak  (duncan@ximian.com)
//
// Copyright (C) 2002, Duncan Mak, Ximian Inc.
//

/* Menus
 *
 * There are several widgets involved in displaying menus. The
 * MenuBar widget is a horizontal menu bar, which normally appears
 * at the top of an application. The Menu widget is the actual menu
 * that pops up. Both MenuBar and Menu are subclasses of
 * MenuShell; a MenuShell contains menu items
 * (MenuItem). Each menu item contains text and/or images and can
 * be selected by the user.
 *
 * There are several kinds of menu item, including plain MenuItem,
 * CheckMenuItem which can be checked/unchecked, RadioMenuItem
 * which is a check menu item that's in a mutually exclusive group,
 * SeparatorMenuItem which is a separator bar, TearoffMenuItem
 * which allows a Menu to be torn off, and ImageMenuItem which
 * can place a Image or other widget next to the menu text.
 *
 * A MenuItem can have a submenu, which is simply a Menu to pop
 * up when the menu item is selected. Typically, all menu items in a menu bar
 * have submenus.
 *
 * The OptionMenu widget is a button that pops up a Menu when clicked.
 * It's used inside dialogs and such.
 *
 * ItemFactory provides a higher-level interface for creating menu bars
 * and menus; while you can construct menus manually, most people don't
 * do that. There's a separate demo for ItemFactory.
 * 
 */


// TODO : window width is not exactly equal
//        point on the right side

using System;
using Gtk;

namespace GtkDemo
{
	public class DemoMenus : Gtk.Window
	{
		public DemoMenus () : base ("Menus")
		{	
			this.DeleteEvent += new DeleteEventHandler (WindowDelete);
			
			AccelGroup accel_group = new AccelGroup ();
			this.AddAccelGroup (accel_group);

			VBox box1 = new VBox (false, 0);
			this.Add (box1);

			MenuBar menubar = new MenuBar ();
			box1.PackStart (menubar, false, false, 0);

			Menu menu = Create_Menu (2, true);

			MenuItem menuitem = new MenuItem ("test\nline2");
			menuitem.Submenu = menu;
			menubar.Append (menuitem);

 			MenuItem menuitem1 = new MenuItem ("foo");
 			menuitem1.Submenu = Create_Menu (3, true);
 			menubar.Append (menuitem1);

			menuitem = new MenuItem ("bar");
			menuitem.Submenu = Create_Menu (4, true);
			menuitem.RightJustified = true;
			menubar.Append (menuitem);
			
			menubar = new MenuBar ();
			box1.PackStart (menubar, false, true, 0);
			
			VBox box2 = new VBox (false, 10);
			box2.BorderWidth = 10;
			box1.PackStart (box2, true, true, 0);
			
			menu = Create_Menu (1, false);
			menu.AccelGroup = accel_group;

			menu.Append (new SeparatorMenuItem ());

			menuitem = new CheckMenuItem ("Accelerate Me");
			menu.Append (menuitem);
			AccelKey ak = new AccelKey ();
			ak.Key = (Gdk.Key) 0xFFBE;
			menuitem.AddAccelerator ("activate", accel_group, ak);
			
			menuitem = new CheckMenuItem ("Accelerator locked");
			menu.Append (menuitem);
			AccelKey ak2 = new AccelKey ();
			ak2.Key = (Gdk.Key) 0xFFBF;
			menuitem.AddAccelerator ("activate", accel_group, ak2);

			menuitem = new CheckMenuItem ("Accelerator Frozen");
			menu.Append (menuitem);
			AccelKey ak3 = new AccelKey ();
			ak3.Key = (Gdk.Key) 0xFFC0;
			menuitem.AddAccelerator ("activate", accel_group, ak2);
			menuitem.AddAccelerator ("activate", accel_group, ak3);
			
			OptionMenu option_menu = new OptionMenu ();
			option_menu.Menu = menu;
			option_menu.SetHistory (3);
			box2.PackStart (option_menu, true, true, 0);
			
			box1.PackStart (new HSeparator (), false, false, 0);
			
			box2 = new VBox (false, 10);
			box2.BorderWidth = 10;
			box1.PackStart (box2, false, true, 0);
			
			Button close_button = new Button ("close");
			close_button.Clicked += new EventHandler (Close_Button);
			box2.PackStart (close_button, true, true, 0);
			
			close_button.CanDefault = true;
			close_button.GrabDefault ();

			this.ShowAll ();
		}			

		private Menu Create_Menu (int depth, bool tearoff)
		{
			if (depth < 1)
				return null;

			Menu menu = new Menu ();
			MenuItem menuitem;
			string label;
			GLib.SList group = new GLib.SList (IntPtr.Zero);

			if (tearoff)
			{
				menuitem = new TearoffMenuItem ();
				menu.Append (menuitem);
				menuitem.Show ();
			}

			for (int i = 0, j = 1; i < 5; i++, j++)
			{
				label = String.Format ("item {0} - {1}", depth, j);
				menuitem = new RadioMenuItem (group, label);
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

		private void Close_Button (object o, EventArgs args)
		{
			this.Hide ();
			this.Destroy ();
		}
		
		private void WindowDelete (object o, DeleteEventArgs args)
		{
			this.Hide ();
			this.Destroy ();
			args.RetVal = true;
		}
	}
}

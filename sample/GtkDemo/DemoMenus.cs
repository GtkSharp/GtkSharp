/* Menus
 *
 * There are several widgets involved in displaying menus. The MenuBar
 * widget is a horizontal menu bar, which normally appears at the top
 * of an application. The Menu widget is the actual menu that pops up.
 * Both MenuBar and Menu are subclasses of MenuShell; a MenuShell
 * contains menu items (MenuItem). Each menu item contains text and/or
 * images and can be selected by the user.
 *
 * There are several kinds of menu item, including plain MenuItem,
 * CheckMenuItem which can be checked/unchecked, RadioMenuItem which
 * is a check menu item that's in a mutually exclusive group,
 * SeparatorMenuItem which is a separator bar, TearoffMenuItem which
 * allows a Menu to be torn off, and ImageMenuItem which can place a
 * Image or other widget next to the menu text.
 *
 * A MenuItem can have a submenu, which is simply a Menu to pop up
 * when the menu item is selected. Typically, all menu items in a menu
 * bar have submenus.
 *
 * UIManager provides a higher-level interface for creating menu bars
 * and menus; while you can construct menus manually, most people
 * don't do that. There's a separate demo for UIManager.
 *
 */

using System;
using Gtk;

namespace GtkDemo
{
	[Demo ("Menus", "DemoMenus.cs")]
	public class DemoMenus : Gtk.Window
	{
		public DemoMenus () : base ("Menus")
		{
			AccelGroup accel_group = new AccelGroup ();
			AddAccelGroup (accel_group);

			VBox box1 = new VBox (false, 0);
			Add (box1);

			MenuBar menubar = new MenuBar ();
			box1.PackStart (menubar, false, true, 0);

			MenuItem menuitem = new MenuItem ("test\nline2");
			menuitem.Submenu = CreateMenu (2, true);
			menubar.Append (menuitem);

 			MenuItem menuitem1 = new MenuItem ("foo");
 			menuitem1.Submenu = CreateMenu (3, true);
 			menubar.Append (menuitem1);

			menuitem = new MenuItem ("bar");
			menuitem.Submenu = CreateMenu (4, true);
			menuitem.RightJustified = true;
			menubar.Append (menuitem);

			VBox box2 = new VBox (false, 10);
			box2.BorderWidth = 10;
			box1.PackStart (box2, false, true, 0);

			Button close = new Button ("close");
			close.Clicked += new EventHandler (CloseClicked);
			box2.PackStart (close, true, true, 0);

			close.CanDefault = true;
			close.GrabDefault ();

			ShowAll ();
		}

		private Menu CreateMenu (int depth, bool tearoff)
		{
			if (depth < 1)
				return null;

			Menu menu = new Menu ();
			GLib.SList group = new GLib.SList (IntPtr.Zero);

			if (tearoff) {
				TearoffMenuItem menuitem = new TearoffMenuItem ();
				menu.Append (menuitem);
			}

			for (int i = 0, j = 1; i < 5; i++, j++) {
				RadioMenuItem menuitem = new RadioMenuItem (group, String.Format ("item {0} - {1}", depth, j));
				group = menuitem.Group;

				menu.Append (menuitem);
				if (i == 3)
					menuitem.Sensitive = false;

				menuitem.Submenu = CreateMenu ((depth - 1), true);
			}

			return menu;
		}

		private void CloseClicked (object o, EventArgs args)
		{
			Destroy ();
		}

		protected override bool OnDeleteEvent (Gdk.Event evt)
		{
			Destroy ();
			return true;
		}
	}
}

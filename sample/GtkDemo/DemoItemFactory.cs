//
// DemoItemFactoryDemo.cs - port of item_factory from gtk-demo
//
// Author: Daniel Kornhauser <dkor@alum.mit.edu>
//
// Copyright (C) 2003, Ximian Inc.

/* Item Factory
 *
 * The GtkItemFactory object allows the easy creation of menus
 * from an array of descriptions of menu items.
 */

// TODO: - unfinished

using System;
using System.IO;

using Gtk;
using Gdk;
using GtkSharp;

namespace GtkDemo
{
	public class DemoItemFactory
	{
		private Gtk.Window window;

		public DemoItemFactory ()
		{
			window = new Gtk.Window (null);
			window.DeleteEvent += new DeleteEventHandler (WindowDelete);
			Gtk.AccelGroup accelGroup = new Gtk.AccelGroup ();
			//STUCK   OUCH !!!!

			window.ShowAll ();
		}

	 	private void WindowDelete (object o, DeleteEventArgs args)
		{
			window.Hide ();
			window.Destroy ();
		}
	}
}

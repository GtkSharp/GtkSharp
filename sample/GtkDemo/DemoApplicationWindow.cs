//
// ApplicationWindow.cs, port of appwindow.c from gtk-demo
//
// Author: Daniel Kornhauser <dkor@alum.mit.edu>
//
// Copyright (C) 2003, Ximian Inc.


/* Application main window
 *
 * Demonstrates a typical application window, with menubar, toolbar, statusbar.
 */

//     : - Is this necesary? /* Set up item factory to go away with the window */

using System;

using Gtk;
using GtkSharp;

namespace GtkDemo 
{
	public class DemoApplicationWindow
	{
		private Gtk.Window window;
		// static ItemFactoryEntry items[] = { new ItemFactoryEntry ("/_File", null, 0, 0, "<Branch>" )};		

	public DemoApplicationWindow ()
		{
			window = new Gtk.Window ("Demo Application Window");
			window.DeleteEvent += new DeleteEventHandler (WindowDelete);

			Table table = new Table (1, 4, false);
			window.Add (table);

			// Create the menubar

			AccelGroup accelGroup = new AccelGroup ();
			window.AddAccelGroup (accelGroup);

			//ItemFactory itemFactory = new ItemFactory ((uint) typeof (MenuBar),"<main>", accelGroup);
			
			// static ItemFactoryEntry items[] = { new ItemFactoryEntry ("/_File", null, 0, 0, "<Branch>" )};
			


			// Set up item factory to go away with the window
			// Is this necesary ?

			// create menu items			
			//STUCK : Didn't find any examples of ItemFactory 
			
			window.ShowAll ();
		}

	private void WindowDelete (object o, DeleteEventArgs args)
		{
			window.Hide ();
			window.Destroy ();
		}
	}
}

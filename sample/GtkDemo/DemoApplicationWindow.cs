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

namespace GtkDemo 
{
	public class DemoApplicationWindow
	{
		private Gtk.Window window;
		// static ItemFactoryEntry items[] = { new ItemFactoryEntry ("/_File", null, 0, 0, "<Branch>" )};		

	public DemoApplicationWindow ()
		{
			window = new Gtk.Window ("Demo Application Window");
			window.SetDefaultSize (400, 300);
			window.DeleteEvent += new DeleteEventHandler (WindowDelete);

			VBox vbox = new VBox (false, 0);
			window.Add (vbox);

			// Create the menubar

			AccelGroup accelGroup = new AccelGroup ();
			window.AddAccelGroup (accelGroup);
			
			MenuBar menubar = CreateMenu ();
			vbox.PackStart (menubar, false, false, 0);
			
			Toolbar toolbar = CreateToolbar ();
			vbox.PackStart (toolbar, false, false, 0);
			
			TextView textview = new TextView ();
			vbox.PackStart (textview, true, true, 0);
			
			Statusbar statusbar = new Statusbar ();
			statusbar.Push (1, "Cursor at row 0 column 0 - 0 chars in document");
			vbox.PackStart (statusbar, false, false, 0);

			//ItemFactory itemFactory = new ItemFactory (typeof (MenuBar),"<main>", accelGroup);
			
			// static ItemFactoryEntry items[] = { new ItemFactoryEntry ("/_File", null, 0, 0, "<Branch>" )};
			


			// Set up item factory to go away with the window
			// Is this necesary ?

			// create menu items			
			//STUCK : Didn't find any examples of ItemFactory 
			
			window.ShowAll ();
		}
		
		private MenuBar CreateMenu ()
		{
			MenuBar menubar = new MenuBar ();
			MenuItem file = new MenuItem ("File");
			menubar.Append (file);
			return menubar;
		}
		
		private Toolbar CreateToolbar ()
		{
			Toolbar toolbar = new Toolbar ();
			
			Button open = new Button (Stock.Open);
			open.Clicked += new EventHandler (OnToolbarClicked);
			toolbar.AppendWidget (open, "Open", "Open");
			
			Button quit = new Button (Stock.Quit);
			quit.Clicked += new EventHandler (OnToolbarClicked);
			toolbar.AppendWidget (quit, "Quit", "Quit");
			
			Button gtk = new Button ("Gtk#");
			gtk.Clicked += new EventHandler (OnToolbarClicked);
			toolbar.AppendWidget (gtk, "Gtk#", "Gtk#");
			
			return toolbar;
		}
		
		private void OnToolbarClicked (object o, EventArgs args)
		{
			MessageDialog md = new MessageDialog (window, DialogFlags.DestroyWithParent, MessageType.Info, ButtonsType.Close, "You selected a toolbar button.");
			md.Run ();
			md.Hide ();
			md.Dispose ();
		}

		private void WindowDelete (object o, DeleteEventArgs args)
		{
			window.Hide ();
			window.Destroy ();
		}
	}
}

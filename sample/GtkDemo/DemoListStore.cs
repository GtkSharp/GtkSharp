//
// DemoListItem.cs, port of tree_store.c from gtk-demo
//
// Author: Daniel Kornhauser <dkor@alum.mit.edu>
//
// Copyright (C) 2003, Ximian Inc.

/* List View/List Store
 *
 * The Gtk.ListStore is used to store data in tree form, to be
 * used later on by a Gtk.ListView to display it. This demo builds
 * a simple Gtk.ListStore and displays it. If you're new to the
 * Gtk.ListView widgets and associates, look into the Gtk.ListStore
 * example first.
 */

using System;
using System.Collections;

using Gtk;
using GtkSharp;

namespace GtkDemo 
{

	public class DemoListStore
	{
		private Window window;
		ListStore store;
		public DemoListStore (){
			window = new Window ("ListStore Demo");
			window.DeleteEvent += new DeleteEventHandler (WindowDelete);

			VBox vbox = new VBox (false, 8);
			vbox.BorderWidth = 8;
			window.Add (vbox);

			vbox.PackStart(new Label ("This is the bug list (note: not based on real data, it would be nice to have a nice ODBC interface to bugzilla or so, though)."), false, false, 0);

			ScrolledWindow scrolledWindow = new ScrolledWindow ();
			scrolledWindow.ShadowType = ShadowType.EtchedIn;
			scrolledWindow.SetPolicy (PolicyType.Automatic, PolicyType.Automatic);
			vbox.PackStart (scrolledWindow, true, true, 0);
                        // create model 
			CreateModel();

			// create tree view
			TreeView treeView = new TreeView (store);
			treeView.RulesHint = true;
			treeView.SearchColumn = (int) ColumnNumber.Description;
			AddColumns (treeView);
			scrolledWindow.Add (treeView);

			// finish & show 
			window.SetDefaultSize (650, 400);
			window.ShowAll ();
		}

		//FIXME: Finish implementing this function, I don't know 
		//       why it doesn't work.
		private void ItemToggled (object o, ToggledArgs args)
		{

			Gtk.TreeIter iter;
			if (store.GetIterFromString(out iter, args.Path))
			{
				bool val = (bool) store.GetValue(iter, 0);
				Console.WriteLine("toggled {0} with value {1}", args.Path, val);
				store.SetValue(iter, 0, !val);
			}
		}

		private void AddColumns (TreeView treeView)
		{
			// column for fixed toggles
			CellRendererToggle rendererToggle = new CellRendererToggle ();
			rendererToggle.Toggled += new ToggledHandler (ItemToggled);
			TreeViewColumn column =  new TreeViewColumn("Fixed", rendererToggle, "active", 0);
			rendererToggle.Active = true;
			rendererToggle.Activatable = true;
			rendererToggle.Visible = true;
			// set this column to a fixed sizing (of 50 pixels) 
			column.Sizing = TreeViewColumnSizing.Fixed;
			column.FixedWidth = 50;
			treeView.AppendColumn(column);			
			// column for bug numbers 
			CellRendererText rendererText = new CellRendererText ();
			column = new TreeViewColumn("Bug number", rendererText, "text", ColumnNumber.Number);
			column.SortColumnId = (int) ColumnNumber.Number;
			treeView.AppendColumn(column);			
			// column for severities
			rendererText = new CellRendererText ();
			column = new TreeViewColumn("Severity", rendererText, "text", ColumnNumber.Severity);
			column.SortColumnId = (int) ColumnNumber.Severity;
			treeView.AppendColumn(column);				
			// column for description
			rendererText = new CellRendererText ();
			column = new TreeViewColumn("Description", rendererText, "text", ColumnNumber.Description);
			column.SortColumnId = (int) ColumnNumber.Description;
			treeView.AppendColumn(column);				

		}
		

		private void WindowDelete (object o, DeleteEventArgs args)
		{
			window.Hide ();
			window.Destroy ();
		}

		private void CreateModel ()
		{
			store = new ListStore (
				typeof(bool),
				typeof(int),
				typeof(string),
				typeof(string));

			foreach (Bug bug in bugs) {
				store.AppendValues(bug.Fixed,
						    bug.Number,
						    bug.Severity,
						    bug.Description);}

		}
		//FIXME: Insted of using numbert conver enum to array using
		// GetValues and then ge the Length Property
		public enum ColumnNumber
		{
			Fixed,
			Number,
			Severity,
			Description,
		}



		private static Bug[] bugs =
		{
			new Bug ( false, 60482, "Normal",     "scrollable notebooks and hidden tabs"),
			new Bug ( false, 60620, "Critical",   "gdk_window_clear_area (gdkwindow-win32.c) is not thread-safe" ),
			new Bug ( false, 50214, "Major",      "Xft support does not clean up correctly" ),
			new Bug ( true,  52877, "Major",      "GtkFileSelection needs a refresh method. " ),
			new Bug ( false, 56070, "Normal",     "Can't click button after setting in sensitive" ),
			new Bug ( true,  56355, "Normal",     "GtkLabel - Not all changes propagate correctly" ),
			new Bug ( false, 50055, "Normal",     "Rework width/height computations for TreeView" ),
			new Bug ( false, 58278, "Normal",     "gtk_dialog_set_response_sensitive () doesn't work" ),
			new Bug ( false, 55767, "Normal",     "Getters for all setters" ),
			new Bug ( false, 56925, "Normal",     "Gtkcalender size" ),
			new Bug ( false, 56221, "Normal",     "Selectable label needs right-click copy menu" ),
			new Bug ( true,  50939, "Normal",     "Add shift clicking to GtkTextView" ),
			new Bug ( false, 6112,  "Enhancement","netscape-like collapsable toolbars" ),
			new Bug ( false, 1,     "Normal",     "First bug :=)" )
		};	
	}


	public class Bug
	{
		public bool Fixed;
		public int Number;
		public string Severity;
			public string Description;
		
			public Bug ( bool status,
					int number,
					string severity,
					string description)
			{
				Fixed = status;
				Number = number;
				Severity = severity;
				Description = description;
			}
	}

}

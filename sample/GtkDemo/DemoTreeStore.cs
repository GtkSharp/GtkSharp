//
// DemoTreeItem.cs, port of tree_store.c from gtk-demo
//
// Author: Daniel Kornhauser <dkor@alum.mit.edu>
//
// Copyright (C) 2003, Ximian Inc.

/* Tree View/Tree Store
 *
 * The Gtk.TreeStore is used to store data in tree form, to be
 * used later on by a Gtk.TreeView to display it. This demo builds
 * a simple Gtk.TreeStore and displays it. If you're new to the
 * Gtk.TreeView widgets and associates, look into the Gtk.ListStore
 * example first.
 */

using System;
using System.Collections;

using Gtk;
using GLib;

namespace GtkDemo 
{
	[Demo ("TreeStore", "DemoTreeStore.cs", "Tree View")]
	public class DemoTreeStore : Gtk.Window
	{
		private TreeStore store;

		public DemoTreeStore () : base ("TreeStore Demo")
		{
			this.DeleteEvent += new DeleteEventHandler (WindowDelete);

			VBox vbox = new VBox (false, 8);
			vbox.BorderWidth = 8;
			this.Add (vbox);

			vbox.PackStart (new Label ("Jonathan's Holiday Card Planning Sheet"), false, false, 0);

			ScrolledWindow scrolledWindow = new ScrolledWindow ();
			scrolledWindow.ShadowType = ShadowType.EtchedIn;
			scrolledWindow.SetPolicy (PolicyType.Automatic, PolicyType.Automatic);
			vbox.PackStart (scrolledWindow, true, true, 0);

			// create model 
			CreateModel ();
			
			// create tree view
			TreeView treeView = new TreeView (store);
			treeView.RulesHint = true;
		        TreeSelection treeSelection = treeView.Selection;
			treeSelection.Mode = SelectionMode.Multiple;
			AddColumns (treeView);
			scrolledWindow.Add (treeView);

			// expand all rows after the treeview widget has been realized
			for (int i = 0; i < 12; i++)
			{
				treeView.ExpandRow (new TreePath (i.ToString ()), false);
			}

			this.SetDefaultSize (650, 400);
			this.ShowAll ();
		}

		private void ItemToggled (object o, ToggledArgs args)
		{
			GLib.Object cellRendererToggle = (GLib.Object) o;
			int column = (int) cellRendererToggle.Data["column"];
			
 			Gtk.TreeIter iter;
 			if (store.GetIterFromString (out iter, args.Path))
 			{
 				bool val = (bool) store.GetValue (iter, column);
 				Console.WriteLine ("toggled {0} with value {1}", args.Path, !val);
 				store.SetValue (iter, column, !val);
 			}
		}

		private void AddColumns (TreeView treeView)
		{
			// column for holiday names
			CellRendererText rendererText = new CellRendererText ();
			rendererText.Xalign = 0.0f;
			GLib.Object ugly = (GLib.Object) rendererText;
			ugly.Data ["column"] = Column.HolidayName;
			TreeViewColumn column = new TreeViewColumn ("Holiday", rendererText, 
					"text", Column.HolidayName);
			treeView.InsertColumn (column, (int) Column.HolidayName);

			// alex column
			CellRendererToggle rendererToggle = new CellRendererToggle ();
			rendererToggle.Xalign = 0.0f;
			ugly = (GLib.Object) rendererToggle;
			ugly.Data ["column"] = Column.Alex;
			rendererToggle.Toggled += new ToggledHandler (ItemToggled);
			rendererToggle.Visible = true;
			rendererToggle.Activatable = true;
			rendererToggle.Active = true;
			column = new TreeViewColumn ("Alex", rendererToggle, "active", (int) Column.Alex);
			column.Sizing = TreeViewColumnSizing.Fixed;
			column.FixedWidth = 50;
			column.Clickable = true;
			treeView.InsertColumn (column, (int) Column.Alex);

			// havoc column
			rendererToggle = new CellRendererToggle ();
			rendererToggle.Xalign = 0.0f;
			ugly = (GLib.Object) rendererToggle;
			ugly.Data ["column"] = Column.Havoc;
			rendererToggle.Toggled += new ToggledHandler (ItemToggled);
			column = new TreeViewColumn ("Havoc", rendererToggle, "active", (int) Column.Havoc);
			column.Visible = true;
			rendererToggle.Activatable = true;
			rendererToggle.Active = true;
			treeView.InsertColumn (column, (int) Column.Havoc);
			column.Sizing = TreeViewColumnSizing.Fixed;
			column.FixedWidth = 50;
			column.Clickable = true;

			// tim column
			rendererToggle = new CellRendererToggle ();
			rendererToggle.Xalign = 0.0f;
			ugly = (GLib.Object) rendererToggle;
			ugly.Data ["column"] = Column.Tim;
			rendererToggle.Toggled += new ToggledHandler (ItemToggled);
			column = new TreeViewColumn ("Tim", rendererToggle, "active", (int) Column.Tim);
			column.Visible = true;
			rendererToggle.Activatable = true;
			rendererToggle.Active = true;
			treeView.InsertColumn (column, (int) Column.Tim);
			column.Sizing = TreeViewColumnSizing.Fixed;
			column.FixedWidth = 50;
			column.Clickable = true;

			// owen column
			rendererToggle = new CellRendererToggle ();
			rendererToggle.Xalign = 0.0f;
			ugly = (GLib.Object) rendererToggle;
			ugly.Data ["column"] = Column.Owen;
			rendererToggle.Toggled += new ToggledHandler (ItemToggled);
			column = new TreeViewColumn ("Owen", rendererToggle, "active", (int) Column.Owen);
			column.Visible = true;
			rendererToggle.Activatable = true;
			rendererToggle.Active = true;
			treeView.InsertColumn (column, (int) Column.Owen);
			column.Sizing = TreeViewColumnSizing.Fixed;
			column.FixedWidth = 50;
			column.Clickable = true;

			// dave column
			rendererToggle = new CellRendererToggle ();
			rendererToggle.Xalign = 0.0f;
			ugly = (GLib.Object) rendererToggle;
			ugly.Data ["column"] = Column.Dave;
			rendererToggle.Toggled += new ToggledHandler (ItemToggled);
			column = new TreeViewColumn ("Dave", rendererToggle,  "active", (int) Column.Dave);
			column.Visible = true;
			rendererToggle.Activatable = true;
			rendererToggle.Active = true;
			treeView.InsertColumn (column, (int) Column.Dave);
			column.Sizing = TreeViewColumnSizing.Fixed;
			column.FixedWidth = 50;
			column.Clickable = true;
		}

		private void WindowDelete (object o, DeleteEventArgs args)
		{
			this.Hide ();
			this.Destroy ();
			args.RetVal = true;
		}

		private void CreateModel ()
		{
			// create tree store
			store = new TreeStore (
				typeof(string),
				typeof(bool),
				typeof(bool),
				typeof(bool),
				typeof(bool), 
				typeof(bool),
				typeof(bool), 
				typeof(bool));
	
			// add data to the tree store

			foreach (MyTreeItem month in toplevel)
			{
				TreeIter iter = store.AppendValues (month.Label, 
						false, 
						false, 
						false,
						false, 
						false, 
						false,
						false);
				
				foreach (MyTreeItem hollyday in month.Children)
				{
					store.AppendValues (iter, 
							hollyday.Label,
							hollyday.Alex,
 							hollyday.Havoc,
							hollyday.Tim,
							hollyday.Owen,
							hollyday.Dave,
							true);
				}
			}
		}
		
		// tree data
	private static MyTreeItem[] january = 
	{
		new MyTreeItem ("New Years Day", true, true, true, true, false, true, null ),
		new MyTreeItem ("Presidential Inauguration", false, true, false, true, false, false, null ),
		new MyTreeItem ("Martin Luther King Jr. day", false, true, false, true, false, false, null )		
	};

		private static MyTreeItem[] february =
		{
			new MyTreeItem ( "Presidents' Day", false, true, false, true, false, false, null ),
			new MyTreeItem ( "Groundhog Day", false, false, false, false, false, false, null ),
			new MyTreeItem ( "Valentine's Day", false, false, false, false, true, true, null )			
		};

		private static MyTreeItem[] march =
		{
			new MyTreeItem ( "National Tree Planting Day", false, false, false, false, false, false, null ),
			new MyTreeItem ( "St Patrick's Day", false, false, false, false, false, true, null )			
		};
		
		private static MyTreeItem[] april =
		{
			new MyTreeItem ( "April Fools' Day", false, false, false, false, false, true, null ),
			new MyTreeItem ( "Army Day", false, false, false, false, false, false, null ),
			new MyTreeItem ( "Earth Day", false, false, false, false, false, true, null ),
			new MyTreeItem ( "Administrative Professionals' Day", false, false, false, false, false, false, null )			
		};

		private static MyTreeItem[] may =
		{
			new MyTreeItem ( "Nurses' Day", false, false, false, false, false, false, null ),
			new MyTreeItem ( "National Day of Prayer", false, false, false, false, false, false, null ),
			new MyTreeItem ( "Mothers' Day", false, false, false, false, false, true, null ),
			new MyTreeItem ( "Armed Forces Day", false, false, false, false, false, false, null ),
			new MyTreeItem ( "Memorial Day", true, true, true, true, false, true, null )			
		};

		private static MyTreeItem[] june =
		{
			new MyTreeItem ( "June Fathers' Day", false, false, false, false, false, true, null ),
			new MyTreeItem ( "Juneteenth (Liberation of Slaves)", false, false, false, false, false, false, null ),
			new MyTreeItem ( "Flag Day", false, true, false, true, false, false, null )			
		};

		private static MyTreeItem[] july =
		{
			new MyTreeItem ( "Parents' Day", false, false, false, false, false, true, null ),
			new MyTreeItem ( "Independence Day", false, true, false, true, false, false, null )			
		};

		private static MyTreeItem[] august =
		{
			new MyTreeItem ( "Air Force Day", false, false, false, false, false, false, null ),
			new MyTreeItem ( "Coast Guard Day", false, false, false, false, false, false, null ),
			new MyTreeItem ( "Friendship Day", false, false, false, false, false, false, null )			
		};

		private static MyTreeItem[] september =
		{
			new MyTreeItem ( "Grandparents' Day", false, false, false, false, false, true, null ),
			new MyTreeItem ( "Citizenship Day or Constitution Day", false, false, false, false, false, false, null ),
			new MyTreeItem ( "Labor Day", true, true, true, true, false, true, null )			
		};

		private static MyTreeItem[] october =
		{
			new MyTreeItem ( "National Children's Day", false, false, false, false, false, false, null ),
			new MyTreeItem ( "Bosses' Day", false, false, false, false, false, false, null ),
			new MyTreeItem ( "Sweetest Day", false, false, false, false, false, false, null ),
			new MyTreeItem ( "Mother-in-Law's Day", false, false, false, false, false, false, null ),
			new MyTreeItem ( "Navy Day", false, false, false, false, false, false, null ),
			new MyTreeItem ( "Columbus Day", false, true, false, true, false, false, null ),
			new MyTreeItem ( "Halloween", false, false, false, false, false, true, null )
		};

		private static MyTreeItem[] november =
		{
			new MyTreeItem ( "Marine Corps Day", false, false, false, false, false, false, null ),
			new MyTreeItem ( "Veterans' Day", true, true, true, true, false, true, null ),
			new MyTreeItem ( "Thanksgiving", false, true, false, true, false, false, null )
		};

		private static MyTreeItem[] december =
		{
			new MyTreeItem ( "Pearl Harbor Remembrance Day", false, false, false, false, false, false, null ),
			new MyTreeItem ( "Christmas", true, true, true, true, false, true, null ),
			new MyTreeItem ( "Kwanzaa", false, false, false, false, false, false, null )
		};


		private static MyTreeItem[] toplevel =
		{
			new MyTreeItem ("January", false, false, false, false, false, false, january),
			new MyTreeItem ("February", false, false, false, false, false, false, february),
			new MyTreeItem ("March", false, false, false, false, false, false, march),
			new MyTreeItem ("April", false, false, false, false, false, false, april),
			new MyTreeItem ("May", false, false, false, false, false, false, may),
			new MyTreeItem ("June", false, false, false, false, false, false, june),
			new MyTreeItem ("July", false, false, false, false, false, false, july),
			new MyTreeItem ("August", false, false, false, false, false, false, august),
			new MyTreeItem ("September", false, false, false, false, false, false, september),
			new MyTreeItem ("October", false, false, false, false, false, false, october),
			new MyTreeItem ("November", false, false, false, false, false, false, november),
			new MyTreeItem ("December", false, false, false, false, false, false, december)
		};
		
		// TreeItem structure
		public class MyTreeItem
		{
			public string          Label;
			public bool            Alex;
			public bool            Havoc;
			public bool            Tim;
			public bool            Owen;
			public bool            Dave;
			public bool            World_holiday; /* shared by the European hackers */
			public MyTreeItem[]      Children;                                  
		
			public MyTreeItem (	
				string      label,
				bool        alex,
				bool        havoc,
				bool        tim,
				bool        owen,
				bool        dave,
				bool        world_holiday,
			MyTreeItem[]  children)
		{
			Label =          label;
			Alex  =     	 alex;     
			Havoc =     	 havoc;	     
			Tim   =     	 tim;     
			Owen  =     	 owen;     
			Dave  =      	 dave;	     
			World_holiday =  world_holiday;
			Children =       children;
		}
		
	}
		// columns
		public enum Column
		{
			HolidayName,
			Alex,
			Havoc,
			Tim,
			Owen,
			Dave,
			Visible,
			World,
			Num,
		}
	}
}

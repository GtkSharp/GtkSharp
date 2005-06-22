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

		public DemoTreeStore () : base ("Card planning sheet")
		{
			VBox vbox = new VBox (false, 8);
			vbox.BorderWidth = 8;
			Add (vbox);

			vbox.PackStart (new Label ("Jonathan's Holiday Card Planning Sheet"),
					false, false, 0);

			ScrolledWindow sw = new ScrolledWindow ();
			sw.ShadowType = ShadowType.EtchedIn;
			sw.SetPolicy (PolicyType.Automatic, PolicyType.Automatic);
			vbox.PackStart (sw, true, true, 0);

			// create model
			CreateModel ();

			// create tree view
			TreeView treeView = new TreeView (store);
			treeView.RulesHint = true;
			treeView.Selection.Mode = SelectionMode.Multiple;
			AddColumns (treeView);

			sw.Add (treeView);

			// expand all rows after the treeview widget has been realized
			treeView.Realized += new EventHandler (ExpandRows);

			SetDefaultSize (650, 400);
			ShowAll ();
		}

		private void ExpandRows (object obj, EventArgs args)
		{
			TreeView treeView = obj as TreeView;

			treeView.ExpandAll ();
		}

		ArrayList columns = new ArrayList ();

		private void ItemToggled (object sender, ToggledArgs args)
		{
			int column = columns.IndexOf (sender);

 			Gtk.TreeIter iter;
 			if (store.GetIterFromString (out iter, args.Path)) {
 				bool val = (bool) store.GetValue (iter, column);
 				store.SetValue (iter, column, !val);
 			}
		}

		private void AddColumns (TreeView treeView)
		{
			CellRendererText text;
			CellRendererToggle toggle;

			// column for holiday names
			text = new CellRendererText ();
			text.Xalign = 0.0f;
			columns.Add (text);
			TreeViewColumn column = new TreeViewColumn ("Holiday", text,
								    "text", Column.HolidayName);
			treeView.InsertColumn (column, (int) Column.HolidayName);

			// alex column
			toggle = new CellRendererToggle ();
			toggle.Xalign = 0.0f;
			columns.Add (toggle);
			toggle.Toggled += new ToggledHandler (ItemToggled);
			column = new TreeViewColumn ("Alex", toggle,
						     "active", (int) Column.Alex,
						     "visible", (int) Column.Visible,
						     "activatable", (int) Column.World);
			column.Sizing = TreeViewColumnSizing.Fixed;
			column.FixedWidth = 50;
			column.Clickable = true;
			treeView.InsertColumn (column, (int) Column.Alex);

			// havoc column
			toggle = new CellRendererToggle ();
			toggle.Xalign = 0.0f;
			columns.Add (toggle);
			toggle.Toggled += new ToggledHandler (ItemToggled);
			column = new TreeViewColumn ("Havoc", toggle,
						     "active", (int) Column.Havoc,
						     "visible", (int) Column.Visible);
			treeView.InsertColumn (column, (int) Column.Havoc);
			column.Sizing = TreeViewColumnSizing.Fixed;
			column.FixedWidth = 50;
			column.Clickable = true;

			// tim column
			toggle = new CellRendererToggle ();
			toggle.Xalign = 0.0f;
			columns.Add (toggle);
			toggle.Toggled += new ToggledHandler (ItemToggled);
			column = new TreeViewColumn ("Tim", toggle,
						     "active", (int) Column.Tim,
						     "visible", (int) Column.Visible,
						     "activatable", (int) Column.World);
			treeView.InsertColumn (column, (int) Column.Tim);
			column.Sizing = TreeViewColumnSizing.Fixed;
			column.FixedWidth = 50;
			column.Clickable = true;

			// owen column
			toggle = new CellRendererToggle ();
			toggle.Xalign = 0.0f;
			columns.Add (toggle);
			toggle.Toggled += new ToggledHandler (ItemToggled);
			column = new TreeViewColumn ("Owen", toggle,
						     "active", (int) Column.Owen,
						     "visible", (int) Column.Visible);
			treeView.InsertColumn (column, (int) Column.Owen);
			column.Sizing = TreeViewColumnSizing.Fixed;
			column.FixedWidth = 50;
			column.Clickable = true;

			// dave column
			toggle = new CellRendererToggle ();
			toggle.Xalign = 0.0f;
			columns.Add (toggle);
			toggle.Toggled += new ToggledHandler (ItemToggled);
			column = new TreeViewColumn ("Dave", toggle,
						     "active", (int) Column.Dave,
						     "visible", (int) Column.Visible);
			treeView.InsertColumn (column, (int) Column.Dave);
			column.Sizing = TreeViewColumnSizing.Fixed;
			column.FixedWidth = 50;
			column.Clickable = true;
		}

		protected override bool OnDeleteEvent (Gdk.Event evt)
		{
			Destroy ();
			return true;
		}

		private void CreateModel ()
		{
			// create tree store
			store = new TreeStore (typeof (string),
					       typeof (bool),
					       typeof (bool),
					       typeof (bool),
					       typeof (bool),
					       typeof (bool),
					       typeof (bool),
					       typeof (bool));

			// add data to the tree store
			foreach (MyTreeItem month in toplevel) {
				TreeIter iter = store.AppendValues (month.Label,
								    false,
								    false,
								    false,
								    false,
								    false,
								    false,
								    false);

				foreach (MyTreeItem holiday in month.Children) {
					store.AppendValues (iter,
							    holiday.Label,
							    holiday.Alex,
							    holiday.Havoc,
							    holiday.Tim,
							    holiday.Owen,
							    holiday.Dave,
							    true,
							    holiday.WorldHoliday);
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
			public string Label;
			public bool Alex, Havoc, Tim, Owen, Dave;
			public bool WorldHoliday; // shared by the European hackers
			public MyTreeItem[] Children;

			public MyTreeItem (string label, bool alex, bool havoc, bool tim,
					   bool owen, bool dave, bool worldHoliday,
					   MyTreeItem[] children)
			{
				Label = label;
				Alex = alex;
				Havoc = havoc;
				Tim = tim;
				Owen = owen;
				Dave = dave;
				WorldHoliday =  worldHoliday;
				Children = children;
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
		}
	}
}

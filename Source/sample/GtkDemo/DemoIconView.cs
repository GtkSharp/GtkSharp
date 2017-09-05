using System;
using System.IO;
using Gtk;

#if GTK_SHARP_2_6
namespace GtkDemo
{
	[Demo ("Icon View", "DemoIconView.cs")]
	public class DemoIconView : Window
	{
		const int COL_PATH = 0;
		const int COL_DISPLAY_NAME = 1;
		const int COL_PIXBUF = 2;
		const int COL_IS_DIRECTORY = 3;

		DirectoryInfo parent = new DirectoryInfo ("/");
		Gdk.Pixbuf dirIcon, fileIcon;
		ListStore store;
		ToolButton upButton;

		public DemoIconView () : base ("Gtk.IconView demo")
		{
			SetDefaultSize (650, 400);
			DeleteEvent += new DeleteEventHandler (OnWinDelete);

			VBox vbox = new VBox (false, 0);
			Add (vbox);

			Toolbar toolbar = new Toolbar ();
			vbox.PackStart (toolbar, false, false, 0);

			upButton = new ToolButton (Stock.GoUp);
			upButton.IsImportant = true;
			upButton.Sensitive = false;
			toolbar.Insert (upButton, -1);

			ToolButton homeButton = new ToolButton (Stock.Home);
			homeButton.IsImportant = true;
			toolbar.Insert (homeButton, -1);

			fileIcon = GetIcon (Stock.File);
			dirIcon = GetIcon (Stock.Open);

			ScrolledWindow sw = new ScrolledWindow ();
			sw.ShadowType = ShadowType.EtchedIn;
			sw.SetPolicy (PolicyType.Automatic, PolicyType.Automatic);
			vbox.PackStart (sw, true, true, 0);

			// Create the store and fill it with the contents of '/'
			store = CreateStore ();
			FillStore ();

			IconView iconView = new IconView (store);
			iconView.SelectionMode = SelectionMode.Multiple;

			upButton.Clicked += new EventHandler (OnUpClicked);
			homeButton.Clicked += new EventHandler (OnHomeClicked);

			iconView.TextColumn = COL_DISPLAY_NAME;
			iconView.PixbufColumn = COL_PIXBUF;

			iconView.ItemActivated += new ItemActivatedHandler (OnItemActivated);
			sw.Add (iconView);
			iconView.GrabFocus ();

			ShowAll ();
		}

		Gdk.Pixbuf GetIcon (string name)
		{
			return Gtk.IconTheme.Default.LoadIcon (name, 48, (IconLookupFlags) 0);
		}

		ListStore CreateStore ()
		{
			// path, name, pixbuf, is_dir
			ListStore store = new ListStore (typeof (string), typeof (string), typeof (Gdk.Pixbuf), typeof (bool));

			// Set sort column and function
			store.DefaultSortFunc = new TreeIterCompareFunc (SortFunc);
			store.SetSortColumnId (COL_DISPLAY_NAME, SortType.Ascending);

			return store;
		}

		void FillStore ()
		{
			// first clear the store
			store.Clear ();

			// Now go through the directory and extract all the file information
			if (!parent.Exists)
				return;

			foreach (DirectoryInfo di in parent.GetDirectories ())
			{
				if (!di.Name.StartsWith ("."))
					store.AppendValues (di.FullName, di.Name, dirIcon, true);
			}
			
			foreach (FileInfo file in parent.GetFiles ())
			{
				if (!file.Name.StartsWith ("."))
					store.AppendValues (file.FullName, file.Name, fileIcon, false);
			}
		}

		int SortFunc (TreeModel model, TreeIter a, TreeIter b)
		{
			// sorts folders before files
			bool a_is_dir = (bool) model.GetValue (a, COL_IS_DIRECTORY);
			bool b_is_dir = (bool) model.GetValue (b, COL_IS_DIRECTORY);
			string a_name = (string) model.GetValue (a, COL_DISPLAY_NAME);
			string b_name = (string) model.GetValue (b, COL_DISPLAY_NAME);

			if (!a_is_dir && b_is_dir)
				return 1;
			else if (a_is_dir && !b_is_dir)
				return -1;
			else
				return String.Compare (a_name, b_name);
		}

		void OnHomeClicked (object sender, EventArgs a)
		{
			parent = new DirectoryInfo (Environment.GetFolderPath (Environment.SpecialFolder.Personal));
			FillStore ();
			upButton.Sensitive = true;
		}

		void OnItemActivated (object sender, ItemActivatedArgs a)
		{
			TreeIter iter;
			store.GetIter (out iter, a.Path);
			string path = (string) store.GetValue (iter, COL_PATH);
			bool isDir = (bool) store.GetValue (iter, COL_IS_DIRECTORY);

			if (!isDir)
				return;

			// Replace parent with path and re-fill the model
			parent = new DirectoryInfo (path);
			FillStore ();

			// Sensitize the up button
			upButton.Sensitive = true;
		}

		void OnUpClicked (object sender, EventArgs a)
		{
			parent = parent.Parent;
			FillStore ();
			upButton.Sensitive = (parent.FullName == "/" ? false : true);
		}

		void OnWinDelete (object sender, DeleteEventArgs a)
		{
			Hide ();
			Dispose ();
		}
	}
}
#endif


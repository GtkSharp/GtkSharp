/* Tree View/Editable Cells
 *
 * This demo demonstrates the use of editable cells in a Gtk.TreeView. If
 * you are new to the Gtk.TreeView widgets and associates, look into
 * the Gtk.ListStore example first.
 *
 */

using System;
using System.Collections;
using Gtk;

namespace GtkDemo
{
	[Demo ("Editable Cells", "DemoEditableCells.cs", "Tree View")]
	public class DemoEditableCells : Gtk.Window
	{
		private ListStore store;
		private TreeView treeView;
		private ArrayList articles;

		public DemoEditableCells () : base ("Shopping list")
		{
			SetDefaultSize (320, 200);
			BorderWidth = 5;

			VBox vbox = new VBox (false, 5);
			Add (vbox);

			vbox.PackStart (new Label ("Shopping list (you can edit the cells!)"), false, false, 0);

			ScrolledWindow scrolledWindow = new ScrolledWindow ();
			scrolledWindow.ShadowType = ShadowType.EtchedIn;
			scrolledWindow.SetPolicy (PolicyType.Automatic, PolicyType.Automatic);
			vbox.PackStart (scrolledWindow, true, true, 0);

			// create model
			store = CreateModel ();

			// create tree view
			treeView = new TreeView (store);
			treeView.RulesHint = true;
			treeView.Selection.Mode = SelectionMode.Single;

			AddColumns ();
			scrolledWindow.Add (treeView);

			// some buttons
			HBox hbox = new HBox (true, 4);
			vbox.PackStart (hbox, false, false, 0);

			Button button = new Button ("Add item");
			button.Clicked += new EventHandler (AddItem);
			hbox.PackStart (button, true, true, 0);

			button = new Button ("Remove item");
			button.Clicked += new EventHandler (RemoveItem);
			hbox.PackStart (button, true, true, 0);

			ShowAll ();
		}

		private void AddColumns ()
		{
			CellRendererText renderer;

			// number column
			renderer = new CellRendererText ();
			renderer.Edited += new EditedHandler (NumberCellEdited);
			renderer.Editable = true;
			treeView.AppendColumn ("Number", renderer,
					       "text", (int) Column.Number);

			// product column
			renderer = new CellRendererText ();
			renderer.Edited += new EditedHandler (TextCellEdited);
			renderer.Editable = true;
			treeView.AppendColumn ("Product", renderer ,
					       "text", (int) Column.Product);
		}

		private ListStore CreateModel ()
		{
			// create array
			articles = new ArrayList ();
			AddItems ();

			// create list store
			ListStore store = new ListStore (typeof (int), typeof (string), typeof (bool));

			// add items
			foreach (Item item in articles)
				store.AppendValues (item.Number, item.Product);

			return store;
		}

		private void AddItems ()
		{
			Item foo;

			foo = new Item (3, "bottles of coke");
			articles.Add (foo);

			foo = new Item (5, "packages of noodles");
			articles.Add (foo);

			foo = new Item (2, "packages of chocolate chip cookies");
			articles.Add (foo);

			foo = new Item (1, "can vanilla ice cream");
			articles.Add (foo);

			foo = new Item (6, "eggs");
			articles.Add (foo);
		}

		protected override bool OnDeleteEvent (Gdk.Event evt)
		{
			Destroy ();
			return true;
		}

		private void NumberCellEdited (object o, EditedArgs args)
		{
			TreePath path = new TreePath (args.Path);
 			TreeIter iter;
 			store.GetIter (out iter, path);
			int i = path.Indices[0];

			Item foo;
			try {
				foo = (Item) articles[i];
 				foo.Number = int.Parse (args.NewText);
			} catch (Exception e) {
				Console.WriteLine (e);
				return;
			}

 			store.SetValue (iter, (int) Column.Number, foo.Number);
		}

		private void TextCellEdited (object o, EditedArgs args)
		{
			TreePath path = new TreePath (args.Path);
 			TreeIter iter;
 			store.GetIter (out iter, path);
			int i = path.Indices[0];

			Item foo = (Item) articles[i];
			foo.Product = args.NewText;
			store.SetValue (iter, (int) Column.Product, foo.Product);
		}

		private void AddItem (object o, EventArgs args)
		{
			Item foo = new Item (0, "Description here");
			articles.Add (foo);
			store.AppendValues (foo.Number, foo.Product);
		}

		private void RemoveItem (object o, EventArgs args)
		{
 			TreeIter iter;
 			TreeModel model;

 			if (treeView.Selection.GetSelected (out model, out iter)) {
 				int position = store.GetPath (iter).Indices[0];
 				store.Remove (ref iter);
				articles.RemoveAt (position);
			}
		}
	}

	enum Column
	{
		Number,
		Product
	};

 	struct Item
 	{
		public int Number {
			get { return number; }
			set { number = value; }
		}
		public string Product {
			get { return product; }
			set { product = value; }
		}

		private int number;
		private string product;

 		public Item (int number, string product)
 		{
 			this.number = number;
 			this.product = product;
 		}
 	}
}

//
// DemoEditableCells.cs, port of appwindow.c from gtk-demo
//
// Author: Daniel Kornhauser <dkor@alum.mit.edu>
//
// Copyright (C) 2003, Ximian Inc.


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
	public class DemoEditableCells
	{
		private Gtk.Window window;
		private ListStore store;
		private TreeView treeView;
		private ArrayList articles;

		public DemoEditableCells ()
		{
			window = new Gtk.Window ("Color Selection");
			window.SetDefaultSize (320, 200);
			window.DeleteEvent += new DeleteEventHandler (WindowDelete);

			VBox vbox = new VBox (false, 5);
			window.Add (vbox);
			
			vbox.PackStart (new Label ("Shopping list (you can edit the cells!)"), false, false, 0);
			
			ScrolledWindow scrolledWindow = new ScrolledWindow (null, null);
			scrolledWindow.ShadowType = ShadowType.In;
			scrolledWindow.SetPolicy (PolicyType.Automatic, PolicyType.Automatic);
			vbox.PackStart (scrolledWindow);

			// create tree view
			treeView = new TreeView ();
			CreateModel ();
			treeView.Model = store ; 
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

			window.ShowAll ();
		}
		


		private void AddColumns ()
		{
			CellRendererText renderer;
			
			renderer = new CellRendererText ();
			renderer.Edited += new EditedHandler (NumberCellEdited);
			renderer.Editable = true;
			// renderer.Data ("column", (int) Column.Number);
			treeView.AppendColumn ("Number", renderer, 
					"text", (int) Column.Number);
	

			// product column
			renderer = new CellRendererText ();
			renderer.Edited += new EditedHandler (TextCellEdited);
			renderer.Editable = true;
			// renderer.Data ("column", (int) Column.Product);
			treeView.AppendColumn ("Product", renderer ,
					"text", (int) Column.Product);
		}

		private void CreateModel ()
		{

			// create array
			articles = new ArrayList ();
			AddItems ();
			// create list store
			store = new ListStore (typeof (int), typeof (string), typeof (bool));
			
			// add items
			foreach (Item item in articles)
				store.AppendValues (item.Number, item.Product, item.Editable);

		}

		private void AddItems ()
		{
			Item foo;

			foo = new Item (3, "bottles of coke", true);
			articles.Add (foo);

			foo = new Item (5, "packages of noodles", true);
			articles.Add (foo);

			foo = new Item (2, "packages of chocolate chip cookies", true);
			articles.Add (foo);

			foo = new Item (1, "can of vanilla ice cream", true);
			articles.Add (foo);

			foo = new Item (6, "eggs", true);
			articles.Add (foo);
		}
		
		private void WindowDelete (object o, DeleteEventArgs args)
		{
			window.Hide ();
			window.Destroy ();
		}

		// FIXME: This is ugly. 
		//        Figure out why the following line doesn't work
		//             Console.WriteLine ("articles[i] {0}", articles[i].Number);
		//             the code would definitely look better if I havent to do the
		//             following midle step to get the Number.
		//             foo.Number = Convert.ToInt32(args.NewText);
		//,       Figure out why I'm not catching the execptions..
		private void NumberCellEdited (object o, EditedArgs args)
		{
			int i;
			try 
			{
				i = Convert.ToInt32(args.Path);
				} 
			catch (Exception e) 
			{
				Console.WriteLine ("Exeception {0}",e);
				return; // This return should exit the callback but it doesn't
			} 
 			Console.WriteLine ("--NUMBER--1");
// 			//Console.WriteLine ("Path {0}", args.Path);
// 			//Console.WriteLine ("NewText {0}", args.NewText);
// 			//Console.WriteLine ("articles[i] {0}",articles[i]);
			Item foo = (Item) articles[i];
// 			//Console.WriteLine ("foo.Number {0}", foo.Number);
// 			//Console.WriteLine ("");
 			foo.Number = Convert.ToInt32(args.NewText);
 			TreeIter iter = new TreeIter ();
// 			// How the hell do I assing the column !!!
 			store.GetIterFromString(out iter, args.Path);
// 			store.SetValue(iter, (int) Column.Number, foo.Number);
		}


		private void TextCellEdited (object o, EditedArgs args)
		{
			int i = Convert.ToInt32(args.Path);		
			// Console.WriteLine ("--PRODUCT--");
			// Console.WriteLine ("Path {0}", args.Path);
			// Console.WriteLine ("NewText {0}", args.NewText);
			// Console.WriteLine ("articles[i] {0}",articles[i]);
			Item foo = (Item) articles[i];
			// Console.WriteLine ("foo.Product {0}", foo.Product);
			// Console.WriteLine ("");
			foo.Product = args.NewText;
						TreeIter iter = new TreeIter ();
			store.GetIterFromString(out iter, args.Path);
			store.SetValue(iter, (int) Column.Product, foo.Product);
		}

		private void AddItem (object o, EventArgs args)
		{
			Item foo = new Item (0, "Description here", true);
			articles.Add (foo);
			store.AppendValues (foo.Number, foo.Product, foo.Editable);
		}

		private void RemoveItem (object o, EventArgs args)
		{
 			TreeIter iter;
 			TreeModel model;

 			if (treeView.Selection.GetSelected (out model, out iter))
			{
 				TreePath path = store.GetPath (iter);
 				store.Remove (out iter);
				//articles.RemoveAt (path.Indices[0]);

			}
		}
	}
	
	public enum Column
	{
		Number,
		Product,
		Editable,
	};
	
 	public struct Item
 	{
		public int Number {
			get {return NumberItem;}
			set {NumberItem = value;}
		}
		public string Product {
			get {return ProductItem;}
			set {ProductItem = value;}
		}
					
		public bool Editable {
			get {return EditableItem;}
			set {EditableItem = value;}
		}

		private int NumberItem;
		private string ProductItem;
		private bool EditableItem;

 		public Item (int number , string product, bool editable)
 		{
 			NumberItem = number;
 			ProductItem = product;
 			EditableItem = editable;
 		}
 	}
}

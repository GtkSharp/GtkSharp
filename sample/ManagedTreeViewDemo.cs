// ManagedTreeViewDemo.cs - Another TreeView demo
//
// Author: Rachel Hestilow <hestilow@ximian.com>
//
// (c) 2003 Rachel Hestilow

namespace GtkSamples {
	using System;
	using System.Runtime.InteropServices;

	using Gtk;

	public class TreeViewDemo {
		private static ListStore store = null;
		
		private class Pair {
			public string a, b;
			public Pair (string a, string b) {
				this.a = a;
				this.b = b;
			}
		}

		private static void PopulateStore ()
		{
			store = new ListStore (typeof (Pair));
			string[] combs = {null, "foo", "bar", "baz"};
			foreach (string a in combs) {
				foreach (string b in combs) {
					store.AppendValues (new Pair (a, b));
				}
			}
		}

		private static void CellDataA (Gtk.TreeViewColumn tree_column, Gtk.CellRenderer cell, Gtk.TreeModel tree_model, Gtk.TreeIter iter)
		{
			Pair val = (Pair) store.GetValue (iter, 0);
			((CellRendererText) cell).Text = val.a;
		}
		
		private static void CellDataB (Gtk.TreeViewColumn tree_column, Gtk.CellRenderer cell, Gtk.TreeModel tree_model, Gtk.TreeIter iter)
		{
			Pair val = (Pair) store.GetValue (iter, 0);
			((CellRendererText) cell).Text = val.b;
		}
		
		public static void Main (string[] args)
		{
			Application.Init ();

			PopulateStore ();

			Window win = new Window ("TreeView demo");
			win.DeleteEvent += new DeleteEventHandler (DeleteCB);
			win.DefaultWidth = 320;
			win.DefaultHeight = 480;

			ScrolledWindow sw = new ScrolledWindow ();
			win.Add (sw);

			TreeView tv = new TreeView (store);
			tv.HeadersVisible = true;

			tv.AppendColumn ("One", new CellRendererText (), new TreeCellDataFunc (CellDataA));
			tv.AppendColumn ("Two", new CellRendererText (), new TreeCellDataFunc (CellDataB));

			sw.Add (tv);
			win.ShowAll ();

			Application.Run ();
		}

		private static void DeleteCB (System.Object o, DeleteEventArgs args)
		{
			Application.Quit ();
		}
	}
}

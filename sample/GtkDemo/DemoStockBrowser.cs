//
// StockBrowser.cs, port of stock_browser.c from gtk-demo
//
// Author: Daniel Kornhauser <dkor@media.mit.edu>
//
// (C) 2003 Ximian, Inc.

using System;
using Gtk;

namespace GtkDemo
{	
	public class DemoStockBrowser : Gtk.Window
	{
		public DemoStockBrowser () : base ("Stock Item Browser Demo")
		{	
			this.SetDefaultSize (600, 400);
			this.DeleteEvent += new DeleteEventHandler (WindowDelete);
			this.BorderWidth = 8;

			HBox hbox = new HBox (false, 8);
			this.Add (hbox);

			ScrolledWindow scrolledWindow = new ScrolledWindow (null, null);
			scrolledWindow.SetPolicy (PolicyType.Never, PolicyType.Automatic);
			hbox.PackStart (scrolledWindow, true, true, 0);
			
			TreeView list = new TreeView ();
			list.AppendColumn ("Icon", new CellRendererPixbuf (), "pixbuf", 0);
			list.AppendColumn ("Name", new CellRendererText (), "text", 1);
			list.AppendColumn ("Label", new CellRendererText (), "text", 2);
			list.AppendColumn ("Accel", new CellRendererText (), "text", 3);			
			list.AppendColumn ("ID", new CellRendererText (), "text", 4);			
			list.Model = CreateStore ();

			list.Selection.Changed += new EventHandler (OnSelectionChanged);
			scrolledWindow.Add (list);
			
			Frame frame = new Frame ("Selected Item");
			frame.Add (new Label ("TODO"));
			hbox.PackStart (frame, false, false, 0);
			
			this.ShowAll ();
		}	

		private ListStore CreateStore ()
		{
			// image, name, label, accel, id
			ListStore store = new Gtk.ListStore (typeof (Gtk.Image), typeof(string), typeof(string), typeof(string), typeof (string));

			string[] stock_ids = Gtk.Stock.ListIds ();		

			foreach (string s in stock_ids)
			{
				Gtk.StockItem si = new StockItem ();
				if (Gtk.StockManager.Lookup (s, ref si)) {
					Image icon = new Image (s, IconSize.Menu);
					store.AppendValues (icon, GetCLSName (si.StockId), si.Label, GetKeyName (si), si.StockId);
				}
				else {
					Console.WriteLine ("StockItem '{0}' could not be found.", s);
				}
			}
			
			return store;
		}

		string GetCLSName (string stockID)
		{
			// TODO: change gtk-stock-close
			// int Gtk.Stock.Close
			return stockID;
		}

		string GetKeyName (StockItem si)
		{
			// TODO: use si.Keyval and si.Modifier
			// to produce a reasonable representation
			// of the key binding
			return "<ctl> + key";
		}

		void OnSelectionChanged (object o, EventArgs args)
		{
			TreeIter iter;
			TreeModel model;

			if (((TreeSelection) o).GetSelected (out model, out iter))
			{
				// update the frame
			}
		}
		
  		private void WindowDelete (object o, DeleteEventArgs args)
		{
			this.Hide ();
			this.Destroy ();
			args.RetVal = true;
		}
	}
}

//
// StockBrowser.cs, port of stock_browser.c from gtk-demo
//
// Author: Daniel Kornhauser <dkor@media.mit.edu>
//
// (C) 2003 Ximian, Inc.

using System;
using System.Collections;
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
			
			hbox.PackStart (CreateFrame (), false, false, 0);
			
			this.ShowAll ();
		}	

		Frame CreateFrame ()
		{
			// Icon and Item / Icon Only / ???
			// icon / blank
			// _Add / blank
			// Gtk.Stock.Cancel
			// gtk-stock-cancel
			Frame frame = new Frame ("Selected Item");
			VBox vbox = new VBox (false, 3);
			vbox.PackStart (new Label ("???"), false, true, 0);
			vbox.PackStart (new Image (), false, true, 0);
			vbox.PackStart (new Label ("_Add"), false, true, 0);
			vbox.PackStart (new Label ("Gtk.Stock.Add"), false, true, 0);
			vbox.PackStart (new Label ("gtk-stock-add"), false, true, 0);
			frame.Add (vbox);
			return frame;
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
					Gdk.Pixbuf icon = new Image (s, IconSize.Menu).RenderIcon (s, IconSize.Menu, "");
					// FIXME: si.Label needs to _AccelAware
					store.AppendValues (icon, GetCLSName (si.StockId), si.Label, GetKeyName (si), si.StockId);
				}
				else {
					//Console.WriteLine ("StockItem '{0}' could not be found.", s);
				}
			}
			
			return store;
		}

		// changes 'gtk-stock-close' into 'Gtk.Stock.Close'
		// should use StudlyCaps from gapi2xml.pl instead
		string GetCLSName (string stockID)
		{
			string cls = "";
			if (stockID.StartsWith ("gtk-"))
				cls = stockID.Substring (4, stockID.Length - 4);

			char[] split = cls.ToCharArray ();
			bool raiseNext = false;
			ArrayList tmp = new ArrayList ();
			tmp.Add (char.ToUpper (split[0]));

			for (int i = 1; i < split.Length; i ++)
			{
				if (split[i] == '-') {
					raiseNext = true;
					continue;
				}

				if (raiseNext) {
					tmp.Add (char.ToUpper (split[i]));
					raiseNext = false;
				}
				else {
					tmp.Add (split[i]);
				}
			}

			split = new char[tmp.Count];
			int j = 0;
			foreach (char c in tmp)
				split[j++] = c;

			return "Gtk.Stock." + new string (split);
		}

		// use si.Keyval and si.Modifier
		// to produce a reasonable representation
		// of the key binding
		string GetKeyName (StockItem si)
		{
			string mod = "";
			string key = "";

			switch (si.Modifier) {
				// seems to be the only one used
				case Gdk.ModifierType.ControlMask:
					mod = "<Control>";
					break;
				default:
					break;
			}

			if (si.Keyval > 0)
				key = Gdk.Keyval.Name (si.Keyval);

			return String.Format ("{0} {1}", mod, key);
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


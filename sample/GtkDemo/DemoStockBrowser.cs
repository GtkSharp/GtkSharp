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
		class StockInfo
		{
			internal string Name;
			internal string Label;
			internal string Accel;
			internal string ID;
			internal Gdk.Pixbuf Icon;
		}

		// in a real application this would be
		// split into its own file
		class StockFrame : Gtk.Frame
		{
			StockInfo info;
			Label category;
			Label name;
			Label id;
			Label label;
			Image icon;

			internal StockFrame () : base ("Selected Item")
			{
				this.SetSizeRequest (200, -1);
				// Icon and Item / Icon Only / ???
				category = new Label ("???");
				// icon / blank
				icon = new Image ("");
				// _Add / blank
				label = new Label ();
				label.UseUnderline = true;
				// Gtk.Stock.Cancel
				name = new Label ();
				// gtk-stock-cancel
				id = new Label ();

				VBox vbox = new VBox (false, 3);
				vbox.PackStart (category, false, true, 0);
				vbox.PackStart (icon, false, true, 0);
				vbox.PackStart (label, false, true, 0);
				vbox.PackStart (name, false, true, 0);
				vbox.PackStart (id, false, true, 0);

				this.Add (vbox);
				this.ShowAll ();
			}

			internal StockInfo Info
			{
				get { return info; }
				set {
					info = value;
					name.Text = info.Name;
					label.Text = info.Label;
					id.Text = info.ID;
					icon.Pixbuf = info.Icon;
				}
			}
		}

		StockFrame stockFrame;

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
			
			stockFrame = new StockFrame ();
			hbox.PackStart (stockFrame, false, false, 0);
			
			this.ShowAll ();
		}	

		private ListStore CreateStore ()
		{
			// FIXME: tremendous duplication of info
			// image, name, label, accel, id, StockInfo
			ListStore store = new Gtk.ListStore (typeof (Gdk.Pixbuf), typeof(string), typeof(string), typeof(string), typeof (string), typeof (StockInfo));

			string[] stock_ids = Gtk.Stock.ListIds ();		

			foreach (string s in stock_ids)
			{
				Gtk.StockItem si = new StockItem ();
				if (Gtk.StockManager.Lookup (s, ref si)) {
					Gdk.Pixbuf icon = this.RenderIcon (s, IconSize.Menu, "");
					StockInfo info = new StockInfo ();
					info.Icon = icon;
					info.Name = GetCLSName (si.StockId);
					info.Label = si.Label;
					info.Accel = GetKeyName (si);
					info.ID = si.StockId;

					// FIXME: si.Label needs to _AccelAware
					store.AppendValues (icon, GetCLSName (si.StockId), si.Label, GetKeyName (si), si.StockId, info);
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
				StockInfo info = (StockInfo) model.GetValue (iter, 5);
				stockFrame.Info = info;
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


/* Stock Item and Icon Browser
 *
 * This source code for this demo doesn't demonstrate anything
 * particularly useful in applications. The purpose of the "demo" is
 * just to provide a handy place to browse the available stock icons
 * and stock items.
 */

using System;
using System.Collections;
using System.Reflection;
using Gtk;

namespace GtkDemo
{
	[Demo ("Stock Item and Icon Browser", "DemoStockBrowser.cs")]
	public class DemoStockBrowser : Gtk.Window
	{
		enum Column {
			Id,
			Name,
			Label,
			Accel,
		};

		Label typeLabel, nameLabel, idLabel, accelLabel;
		Image iconImage;

		public DemoStockBrowser () : base ("Stock Icons and Items")
		{
			SetDefaultSize (-1, 500);
			BorderWidth = 8;

			HBox hbox = new HBox (false, 8);
			Add (hbox);

			ScrolledWindow sw = new ScrolledWindow ();
			sw.SetPolicy (PolicyType.Never, PolicyType.Automatic);
			hbox.PackStart (sw, false, false, 0);

			ListStore model = CreateModel ();

			TreeView treeview = new TreeView (model);
			sw.Add (treeview);

			TreeViewColumn column = new TreeViewColumn ();
			column.Title = "Name";
			CellRenderer renderer = new CellRendererPixbuf ();
			column.PackStart (renderer, false);
			column.SetAttributes (renderer, "stock_id", Column.Id);
			renderer = new CellRendererText ();
			column.PackStart (renderer, true);
			column.SetAttributes (renderer, "text", Column.Name);

			treeview.AppendColumn (column);
			treeview.AppendColumn ("Label", new CellRendererText (), "text", Column.Label);
			treeview.AppendColumn ("Accel", new CellRendererText (), "text", Column.Accel);
			treeview.AppendColumn ("ID", new CellRendererText (), "text", Column.Id);

			Alignment align = new Alignment (0.5f, 0.0f, 0.0f, 0.0f);
			hbox.PackEnd (align, false, false, 0);

			Frame frame = new Frame ("Selected Item");
			align.Add (frame);

			VBox vbox = new VBox (false, 8);
			vbox.BorderWidth = 8;
			frame.Add (vbox);

			typeLabel = new Label ();
			vbox.PackStart (typeLabel, false, false, 0);
			iconImage = new Gtk.Image ();
			vbox.PackStart (iconImage, false, false, 0);
			accelLabel = new Label ();
			vbox.PackStart (accelLabel, false, false, 0);
			nameLabel = new Label ();
			vbox.PackStart (nameLabel, false, false, 0);
			idLabel = new Label ();
			vbox.PackStart (idLabel, false, false, 0);

			treeview.Selection.Mode = Gtk.SelectionMode.Single;
			treeview.Selection.Changed += new EventHandler (SelectionChanged);

			ShowAll ();
		}

		private ListStore CreateModel ()
		{
			ListStore store = new Gtk.ListStore (typeof (string), typeof(string), typeof(string), typeof(string), typeof (string));

			string[] stockIds = Gtk.Stock.ListIds ();
			Array.Sort (stockIds);

			// Use reflection to get the list of C# names
			Hashtable idToName = new Hashtable ();
			foreach (PropertyInfo info in typeof (Gtk.Stock).GetProperties (BindingFlags.Public | BindingFlags.Static)) {
				if (info.PropertyType == typeof (string))
					idToName[info.GetValue (null, null)] = "Gtk.Stock." + info.Name;
			}

			foreach (string id in stockIds) {
				Gtk.StockItem si;
				string accel;

				si = Gtk.Stock.Lookup (id);
				if (si.Keyval != 0)
					accel = Accelerator.Name (si.Keyval, si.Modifier);
				else
					accel = "";

				store.AppendValues (id, idToName[id], si.Label, accel);
			}

			return store;
		}

		void SelectionChanged (object o, EventArgs args)
		{
			TreeSelection selection = (TreeSelection)o;
			TreeIter iter;
			TreeModel model;

			if (selection.GetSelected (out model, out iter)) {
				string id = (string) model.GetValue (iter, (int)Column.Id);
				string name = (string) model.GetValue (iter, (int)Column.Name);
				string label = (string) model.GetValue (iter, (int)Column.Label);
				string accel = (string) model.GetValue (iter, (int)Column.Accel);

				IconSize size = GetLargestSize (id);
				bool icon = (size != IconSize.Invalid);

				if (icon && label != null)
					typeLabel.Text = "Icon and Item";
				else if (icon)
					typeLabel.Text = "Icon Only";
				else if (label != null)
					typeLabel.Text = "Item Only";
				else
					typeLabel.Text = "???????";

				if (name != null)
					nameLabel.Text = name;
				else
					nameLabel.Text = "";

				idLabel.Text = id;

				if (label != null)
					accelLabel.TextWithMnemonic = label + " " + accel;
				else
					accelLabel.Text = "";

				if (icon)
					iconImage.SetFromStock (id, size);
				else
					iconImage.Pixbuf = null;
			} else {
				typeLabel.Text = "No selected item";
				nameLabel.Text = "";
				idLabel.Text = "";
				accelLabel.Text = "";
				iconImage.Pixbuf = null;
			}
		}

		// Finds the largest size at which the given image stock id is
		// available. This would not be useful for a normal application

		private IconSize GetLargestSize (string stockId)
		{
			IconSet set = IconFactory.LookupDefault (stockId);
			if (set == null)
				return IconSize.Invalid;

			IconSize[] sizes = set.Sizes;
			IconSize bestSize = IconSize.Invalid;
			int bestPixels = 0;

			foreach (IconSize size in sizes) {
				int width, height;
				Gtk.Icon.SizeLookup (size, out width, out height);
				if (width * height > bestPixels) {
					bestSize = size;
					bestPixels = width * height;
				}
			}

			return bestSize;
		}

		protected override bool OnDeleteEvent (Gdk.Event evt)
		{
			Destroy ();
			return true;
		}
	}
}

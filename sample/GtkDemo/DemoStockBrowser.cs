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
	public class DemoStockBrowser 
	{
		private Gtk.Window window;
		
		public DemoStockBrowser ()
		{	
			window = new Gtk.Window ("Stock Item Browser Demo");
			window.SetDefaultSize (600, 400);
			window.DeleteEvent += new DeleteEventHandler (WindowDelete);
			window.BorderWidth = 8;

			HBox hbox = new HBox (false, 8);
			window.Add (hbox);

			ScrolledWindow scrolledWindow = new ScrolledWindow (null, null);
			scrolledWindow.SetPolicy (PolicyType.Never, PolicyType.Automatic);
			hbox.PackStart (scrolledWindow, true, true, 0);
			
			TreeView list = new TreeView ();
			list.AppendColumn ("Icon", new CellRendererPixbuf (), "pixbuf", 0);
			list.AppendColumn ("Name", new CellRendererText (), "text", 1);
			list.AppendColumn ("Label", new CellRendererText (), "text", 2);
			list.AppendColumn ("Accel", new CellRendererText (), "text", 3);			
			list.Model = CreateStore ();
			scrolledWindow.Add (list);
			
			Frame frame = new Frame ();
			hbox.PackStart (frame, true, true, 0);
			
			window.ShowAll ();
		}	

// 		private TreeModel CreateModel ()
// 		{
// 			ListStore store;
// 			TreeModel model = new TreeModel ();
// 			return model;
// 		}

		private ListStore CreateStore ()
		{
			// image, name, label, accel
			ListStore store = new Gtk.ListStore (typeof (Gdk.Pixbuf), typeof(string), typeof(string), typeof(string));
			
			for (int i =1; i < 10; i++)
			{
			Image icon = new Image ("images/MonoIcon.png");
			store.AppendValues (icon, "Gtk.Stock.Ok", "Ok", "_Ok");
			}
			
			return store;
		}
		
  		private void WindowDelete (object o, DeleteEventArgs args)
		{
			window.Hide ();
			window.Destroy ();
		}
	}
}

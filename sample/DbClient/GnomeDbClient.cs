using System;
using Gda;
using GnomeDb;
using GdaSharp;
using Gtk;
using GtkSharp;

class GnomeDbClient {

	static Gtk.Window window;
	static Toolbar toolbar;
	static Browser browser;
	static VBox box;
	static Gda.Client client = null;
	static Gda.Connection cnc = null;
	
	static void Main (string [] args)
	{
		GnomeDb.Application.Init ();

		/* create the UI */
		window = new Gtk.Window ("GNOME-DB client");
		window.DeleteEvent += new DeleteEventHandler (Window_Delete);
		box = new VBox (false, 0);
		window.Add (box);
		
		toolbar = new Toolbar ();
		toolbar.ToolbarStyle = ToolbarStyle.BothHoriz;
		toolbar.AppendItem ("Change database", "Select another database to browse", String.Empty,
				    new Gtk.Image (Gtk.Stock.Add, IconSize.LargeToolbar),
				    new SignalFunc (DB_connect));
		box.PackStart (toolbar, false, false, 0);

		browser = new GnomeDb.Browser ();
		box.PackStart (browser, true, true, 0);
		
		window.ShowAll ();
		GnomeDb.Application.Run ();
	}

	static void Client_Event (object o, EventNotificationArgs args)
	{
		switch (args.Event) {
		case ClientEvent.Error :
			System.Console.WriteLine ("There's been an error");
			break;
		}
	}
	
	static void Window_Delete (object o, DeleteEventArgs args)
	{
		GnomeDb.Application.Quit ();
		args.RetVal = true;
	}

	static void DB_connect ()
	{
		GnomeDb.LoginDialog dialog;

		dialog = new GnomeDb.LoginDialog ("Select data source");
		if (dialog.Run () == true) {
			if (client == null) {
				client = new Gda.Client ();
				client.EventNotification += new GdaSharp.EventNotificationHandler (Client_Event);
			}

			cnc = client.OpenConnection (dialog.Dsn, dialog.Username, dialog.Password,
						     Gda.ConnectionOptions.Only);
			if (cnc != null)
				browser.Connection = cnc;
		}
		dialog.Destroy ();
	}
}


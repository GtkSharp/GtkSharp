using System;
using System.Data;
using System.Data.SqlClient;

using Gtk;
using Gdk;
using Gnome;
using GtkSharp;

public class GtkDbClient {

	static Window window;
	static Notebook notebook = null;
	
	static void Main ()
	{
		Application.Init ();
		window = new Window ("Database client");
		window.DeleteEvent += new DeleteEventHandler (Window_Delete);
		VBox box = new VBox (false, 0);
		notebook = new Notebook ();

		box.PackStart (CreateMenu (), false, false, 0);
		box.PackStart (notebook, false, false, 0);
		
		AddPage ("Browse", new Label ("Browse"));
		AddPage ("Insert", new Label ("Insert"));
		AddPage ("Remove", new Label ("Remove"));
		AddPage ("Update", new Label ("Update"));

		window.Add (box);
		window.ShowAll ();
		Application.Run ();
	}

	static Gtk.MenuBar CreateMenu ()
	{
		MenuBar mb = new MenuBar ();
		Menu file_menu = new Menu ();		
		
		ImageMenuItem close_item = new ImageMenuItem ("Close");
		close_item.Image = new Image (Stock.Close, IconSize.Menu);
		ImageMenuItem quit_item = new ImageMenuItem ("Quit");
		quit_item.Image = new Image (Stock.Quit, IconSize.Menu);

		quit_item.Activated += new EventHandler (Quit_Activated);
		
		file_menu.Append (new SeparatorMenuItem ());
		file_menu.Append (close_item);
		file_menu.Append (quit_item);
	
		MenuItem file_item = new MenuItem ("_File");
		file_item.Submenu = file_menu;

		mb.Append (file_item);

		Menu help_menu = new Menu ();
		MenuItem help_item = new MenuItem ("_Help");
		help_item.Submenu = help_menu;
		MenuItem about = new MenuItem ("About");
		about.Activated += new EventHandler (About_Box);
		help_menu.Append (about);
		mb.Append (help_item);

		return mb;
	}
	
	static void AddPage (string title, Widget child)
	{
		notebook.AppendPage (child, new Label (title));
	}

	static void About_Box (object o, EventArgs args)
	{
		string [] authors = new string [] {
			"Rodrigo Moya (rodrigo@ximian.com",
			"Gonzalo Paniagua (gonzalo@ximian.com)",
			"Duncan Mak (duncan@ximian.com)",
		};

		string [] documenters = new string [] {};

		Gnome.About about = new Gnome.About ("Gtk# Db Client", "0.1",
						     "Copyright (C) 2002, Ximian Inc.",
						     "A Sample Database client",
						     authors, documenters, "", new Pixbuf ());
		about.Show ();
	}

	static void Window_Delete (object o, DeleteEventArgs args)
	{
		Application.Quit ();
		args.RetVal = true;
	}

	static void Quit_Activated (object o, EventArgs args)
	{
		Application.Quit ();
	}
}

using System;
using System.Data;
using System.Data.SqlClient;

using Gtk;
using Gdk;
using Gnome;
using GtkSharp;

public class BrowseTab : VBox 
{
	private Button goFirst;
	private Button goPrev;
	private Button goNext;
	private Button goLast;
	private TextView id;
	private TextView nameData;
	private TextView address;

	public BrowseTab () : base (false, 0)
	{
		Table table = new Table (3, 3, true);
		table.Attach (new Label ("ID: "), 0, 1, 0, 1);
		table.Attach (new Label ("Name: "), 0, 1, 1, 2);
		table.Attach (new Label ("Address: "), 0, 1, 2, 3);
		table.Attach (ID, 1, 2, 0, 1);
		table.Attach (NameData, 1, 2, 1, 2);
		table.Attach (Address, 1, 2, 2, 3);

		HBox hbox = new HBox (false, 0);
		hbox.PackStart (ButtonFirst);
		hbox.PackStart (ButtonPrev);
		hbox.PackStart (ButtonNext);
		hbox.PackStart (ButtonLast);

		this.PackStart (table, false, false, 0);
		this.PackStart (hbox, false, false, 0);
	}

	public Widget ButtonFirst
	{
		get {
			if (goFirst == null)
				goFirst = Button.NewFromStock (Stock.GotoFirst);
			return goFirst;
		}
	}
	
	public Widget ButtonPrev
	{
		get {
			if (goPrev == null)
				goPrev = Button.NewFromStock (Stock.GoBack);
			return goPrev;
		}
	}

	public Widget ButtonNext
	{
		get {
			if (goNext == null)
				goNext = Button.NewFromStock (Stock.GoForward);
			return goNext;
		}
	}

	public Widget ButtonLast
	{
		get {
			if (goLast == null)
				goLast = Button.NewFromStock (Stock.GotoLast);
			return goLast;
		}
	}

	public Widget ID
	{
		get {
			if (id == null)
				id = new TextView ();
			return id;
		}
	}

	public Widget NameData
	{
		get {
			if (nameData == null)
				nameData = new TextView ();
			return nameData;
		}
	}

	public Widget Address
	{
		get {
			if (address == null)
				address = new TextView ();
			return address;
		}
	}
}

public class GtkDbClient {

	static Window window;
	static Notebook notebook = null;
	static BrowseTab browse;
	
	static void Main ()
	{
		Application.Init ();
		window = new Window ("Database client");
		window.DeleteEvent += new DeleteEventHandler (Window_Delete);
		VBox box = new VBox (false, 0);
		notebook = new Notebook ();

		box.PackStart (CreateMenu (), false, false, 0);
		box.PackStart (notebook, false, false, 0);
		
		browse = new BrowseTab ();
		AddPage ("Browse", browse);
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

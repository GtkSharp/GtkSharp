using System;
using System.Collections;
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

class DbData
{
	private uint id;
	private string name;
	private string address;
	
	private DbData () {}

	public DbData (uint id, string name, string address)
	{
		this.id = id;
		this.name = name;
		this.address = address;
	}

	public uint Id
	{
		get { return id; }
	}

	public string Name
	{
		get { return name; }
	}

	public string Address
	{
		get { return address; }
	}
}

class IdConnection : IDisposable
{
	private SqlConnection cnc;
	private bool disposed;

	public IdConnection ()
	{
		cnc = new SqlConnection ();
		string connectionString = "hostaddr=127.0.0.1;" +
					  "user=monotest;" +
					  "password=monotest;" +
					  "dbname=monotest";
						  
		cnc.ConnectionString = connectionString;
		try {
			cnc.Open ();
		} catch (Exception){
			cnc = null;
			throw;
		}
	}

	public void Insert (uint id, string name, string address)
	{
		string insertCmd = String.Format ("INSERT INTO customers VALUES ({0}, '{1}', '{2}')",
						   id, name.Trim (), address.Trim ());
		IDbCommand insertCommand = cnc.CreateCommand();
		insertCommand.CommandText = insertCmd;
		insertCommand.ExecuteNonQuery ();
	}

	public void Delete (int id)
	{
		string deleteCmd = String.Format ("DELETE FROM customers WHERE id = {0}", id);
		IDbCommand deleteCommand = cnc.CreateCommand();
		deleteCommand.CommandText = deleteCmd;
		deleteCommand.ExecuteNonQuery ();
	}

	public bool Update (int id, string name, string address)
	{
		string updateCmd = String.Format ("UPDATE customers SET name = '{1}', address = '{2}' WHERE id = {0}",
						   id, name.Trim (), address.Trim ());
		IDbCommand updateCommand = cnc.CreateCommand();
		updateCommand.CommandText = updateCmd;
		bool updated = false;
		return (updateCommand.ExecuteNonQuery () != 0);
	}

	public ArrayList SelectAll ()
	{
		IDbCommand selectCommand = cnc.CreateCommand();
		string selectCmd = "SELECT id, name, address FROM customers ORDER by id";
		selectCommand.CommandText = selectCmd;
		IDataReader reader = selectCommand.ExecuteReader ();
		return FillDataList (reader);
	}

	public DbData Select (int id)
	{
		IDbCommand selectCommand = cnc.CreateCommand();
		string selectCmd = "SELECT id, name, address FROM customers WHERE id = " + id;
		selectCommand.CommandText = selectCmd;
		IDataReader reader = selectCommand.ExecuteReader ();
		ArrayList list = FillDataList (reader);
		return (DbData) list [0];
	}

	private ArrayList FillDataList (IDataReader reader)
	{
		ArrayList list = new ArrayList ();
		while (reader.Read ()) {
			DbData data = new DbData ((uint) reader.GetValue (0),
						  (string) reader.GetValue (1),
						  (string) reader.GetValue (2));
			list.Add (data);
		}
		return list;
	}

	protected virtual void Dispose (bool exp)
	{
		if (!disposed && cnc != null) {
			disposed = true;
			try {
				cnc.Close ();
			} catch (Exception) {
			}
			cnc = null;
		}
	}

	public void Dispose ()
	{
		Dispose (true);
		GC.SuppressFinalize (this);
	}

	~IdConnection ()
	{
		Dispose (false);
	}
}


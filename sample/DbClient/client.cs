using System;
using System.Collections;
using System.Drawing;
using System.Data;
using System.Data.SqlClient;

using Gtk;
using GtkSharp;

enum DialogType
{
	Insert,
	Delete,
	Update
}

class Client {

	static Window window;
	static Dialog dialog;
	static Toolbar toolbar;
	static Table tableau;
	static Entry id_entry;
	static Entry name_entry;
	static Entry address_entry;
	static Statusbar status;
	static Stack statusIds;
	static VBox box;
	static IdConnection conn;
	
	static void Main ()
	{
		Application.Init ();
		window = new Window ("Database client");
		window.DeleteEvent += new DeleteEventHandler (Window_Delete);
		window.DefaultSize = new Size (300, 200);

		box = new VBox (false, 0);
		window.Add (box);

		box.PackStart (CreateMenu (), false, false, 0);
		toolbar = new Toolbar ();
		PackToolbar ();
		box.PackStart (toolbar, false, false, 0);

		UpdateView ();

		status = new Statusbar ();
		box.PackEnd (status, false, false, 0);
		window.ShowAll ();
		Application.Run ();
	}

	static uint context_id = 0;
	static void PushMessage (string message)
	{
		if (statusIds == null)
			statusIds = new Stack ();
		if (status != null)
			statusIds.Push (status.Push (context_id++, message));
	}

	static void PopMessage ()
	{
		if (statusIds == null || statusIds.Count == 0)
			return;
		status.Pop ((uint) statusIds.Pop ());
	}
	
	static void PackToolbar ()
	{
		toolbar.AppendItem ("Insert", "Insert a row", String.Empty,
				    new Gtk.Image (Stock.Add, IconSize.LargeToolbar),
				    new SignalFunc (Db_Insert), IntPtr.Zero);
		
		toolbar.AppendItem ("Remove", "Remove a row", String.Empty,
				    new Gtk.Image (Stock.Remove, IconSize.LargeToolbar),
				    new SignalFunc (Db_Remove), IntPtr.Zero);

		toolbar.AppendItem ("Update", "Update a row", String.Empty,
				    new Gtk.Image (Stock.Italic, IconSize.LargeToolbar),
				    new SignalFunc (Db_Update), IntPtr.Zero);

		toolbar.AppendItem ("Refresh", "Refresh the view", String.Empty,
				    new Gtk.Image (Stock.Refresh, IconSize.LargeToolbar),
				    new SignalFunc (UpdateView), IntPtr.Zero);

		toolbar.AppendSpace ();		

		toolbar.InsertStock (Stock.Quit, "Quit", String.Empty,
				     new SignalFunc (Quit), IntPtr.Zero, -1);

		toolbar.ToolbarStyle = ToolbarStyle.BothHoriz;
	}

	static void Window_Delete (object o, DeleteEventArgs args)
	{
		Application.Quit ();
		args.RetVal = true;
	}

	static void UpdateView ()
	{
		if (tableau != null)
			tableau.Destroy ();

		PopMessage ();
		PushMessage ("");
		ArrayList dataList = Conn.SelectAll ();
		
		tableau = new Gtk.Table ((uint) dataList.Count + 1, 3, false);
		DrawTitles (tableau);
		tableau.ColSpacings = 10;
		uint i = 1;
		foreach (Record r in dataList) {
			tableau.Attach (new Label (r.ID.ToString ()), 0, 1, i, i + 1);
			tableau.Attach (new Label (r.Name), 1, 2, i, i + 1);
			tableau.Attach (new Label (r.Address), 2, 3, i, i + 1);
			i++;
		}
		
		tableau.Show ();
		box.PackStart (tableau, false, false, 0);
		box.ShowAll ();
	}

	static void DrawTitles (Gtk.Table t)
	{
		Label label = null;

		label = new Label (String.Empty);
		label.Markup = "<big><b>ID</b></big>";
		label.UseMarkup = true;

		t.Attach (label, 0, 1, 0, 1);

		label = new Label (String.Empty);
		label.Markup = "<big><b>Name</b></big>";
		label.UseMarkup = true;
		t.Attach (label, 1, 2, 0, 1);

		label = new Label (String.Empty);
		label.Markup = "<big><b>Address</b></big>";
		label.UseMarkup = true;
		t.Attach (label, 2, 3, 0, 1);
	}

	static void Db_Insert ()
	{
		if (dialog != null) {
			return;
		}
		dialog = new Dialog ();
		dialog.Title = "Insert row";
		dialog.BorderWidth = 3;
		dialog.VBox.BorderWidth = 5;
		dialog.HasSeparator = false;

		Frame frame = new Frame ("Insert a row");
		frame.Add (MakeDialog (Stock.DialogInfo, DialogType.Insert));
		dialog.VBox.PackStart (frame, true, true, 0);

		Button button = null;
		button = Button.NewFromStock (Stock.Add);
		button.Clicked += new EventHandler (Insert_Action);
		button.CanDefault = true;
		dialog.ActionArea.PackStart (button, true, true, 0);
		button.GrabDefault ();

		button = Button.NewFromStock (Stock.Cancel);
		button.Clicked += new EventHandler (Dialog_Cancel);
		dialog.ActionArea.PackStart (button, true, true, 0);
		dialog.Modal = true;

		dialog.ShowAll ();
	}

	static void Db_Remove ()
	{
		if (dialog != null) {
			return;
		}
		dialog = new Dialog ();
		dialog.Title = "Remove row";
		dialog.BorderWidth = 3;		
		dialog.VBox.BorderWidth = 5;
		dialog.HasSeparator = false;

		Frame frame = new Frame ("Remove a row");
		frame.Add (MakeDialog (Stock.DialogWarning, DialogType.Delete));
		dialog.VBox.PackStart (frame, true, true, 0);

		Button button = null;
		button = Button.NewFromStock (Stock.Remove);
		button.Clicked += new EventHandler (Remove_Action);
		button.CanDefault = true;
		dialog.ActionArea.PackStart (button, true, true, 0);
		button.GrabDefault ();

		button = Button.NewFromStock (Stock.Cancel);
		button.Clicked += new EventHandler (Dialog_Cancel);
		dialog.ActionArea.PackStart (button, true, true, 0);

		dialog.ShowAll ();

	}

	static Widget MakeDialog (string image, DialogType type)
	{
		HBox hbox = new HBox (false, 2);
		hbox.BorderWidth = 5;
		hbox.PackStart (new Gtk.Image (image, IconSize.Dialog), true, true, 0);
		
		Table table = new Table (3, 3, false);
		hbox.PackStart (table);
		table.ColSpacings = 4;
		table.RowSpacings = 4;
		Label label = null;

		label = Label.NewWithMnemonic ("_ID");
		table.Attach (label, 0, 1, 0, 1);
		id_entry = new Entry ();
		table.Attach (id_entry, 1, 2, 0, 1);

		label = Label.NewWithMnemonic ("_Name");
		table.Attach (label, 0, 1, 1, 2);
		name_entry = new Entry ();
		if (type == DialogType.Delete)
			name_entry.Sensitive = false;
		table.Attach (name_entry, 1, 2, 1, 2);

		label = Label.NewWithMnemonic ("_Address");
		table.Attach (label, 0, 1, 2, 3);
		address_entry = new Entry ();
		if (type == DialogType.Delete)
			address_entry.Sensitive = false;
		table.Attach (address_entry, 1, 2, 2, 3);

		return hbox ;
	}

	static void Db_Update ()
	{
		if (dialog != null) {
			return;
		}
		dialog = new Dialog ();
		dialog.Title = "Update row";
		dialog.BorderWidth = 3;		
		dialog.VBox.BorderWidth = 5;
		dialog.HasSeparator = false;

		Frame frame = new Frame ("Update row");
		frame.Add (MakeDialog (Stock.DialogWarning, DialogType.Update));
		dialog.VBox.PackStart (frame, true, true, 0);

		Button button = null;
		button = Button.NewFromStock (Stock.Apply);
		button.Clicked += new EventHandler (Update_Action);
		button.CanDefault = true;
		dialog.ActionArea.PackStart (button, true, true, 0);
		button.GrabDefault ();

		button = Button.NewFromStock (Stock.Cancel);
		button.Clicked += new EventHandler (Dialog_Cancel);
		dialog.ActionArea.PackStart (button, true, true, 0);

		dialog.ShowAll ();
	}

	static void Quit ()
	{
		Application.Quit ();
	}

	static void Insert_Action (object o, EventArgs args)
	{
		try {
			Conn.Insert (UInt32.Parse (id_entry.Text), name_entry.Text, address_entry.Text);
			UpdateView ();
		} catch (Exception e) {
			PushMessage (e.Message);
		}
		dialog.Destroy ();
		dialog = null;
	}

	static void Remove_Action (object o, EventArgs args)
	{
		try {
			Conn.Delete (UInt32.Parse (id_entry.Text));
			UpdateView ();
		} catch (Exception e) {
			PushMessage (e.Message);
		}
		dialog.Destroy ();
		dialog = null;
	}

	static void Update_Action (object o, EventArgs args)
	{
		try {
			Conn.Update (UInt32.Parse (id_entry.Text), name_entry.Text, address_entry.Text);
			UpdateView ();
		} catch (Exception e) {
			PushMessage (e.Message);
		}
		dialog.Destroy ();
		dialog = null;
	}

	static void Dialog_Cancel (object o, EventArgs args)
	{
		dialog.Destroy ();
		dialog = null;
	}

		static Gtk.MenuBar CreateMenu ()
	{
		MenuBar mb = new MenuBar ();
		Menu file_menu = new Menu ();		
		
		ImageMenuItem quit_item = new ImageMenuItem ("Quit");
		quit_item.Image = new Gtk.Image (Stock.Quit, IconSize.Menu);
		quit_item.Activated += new EventHandler (Quit_Activated);

		file_menu.Append (new SeparatorMenuItem ());
		file_menu.Append (quit_item);
	
		MenuItem file_item = new MenuItem ("_File");
		file_item.Submenu = file_menu;
		mb.Append (file_item);

		Menu action_menu = new Menu ();

		ImageMenuItem insert_item = new ImageMenuItem ("Insert");
		insert_item.Image = new Gtk.Image (Stock.Add, IconSize.Menu);
		insert_item.Activated += new EventHandler (Insert_Activated);

		ImageMenuItem remove_item = new ImageMenuItem ("Remove");
		remove_item.Image = new Gtk.Image (Stock.Remove, IconSize.Menu);
		remove_item.Activated += new EventHandler (Remove_Activated);

		ImageMenuItem update_item = new ImageMenuItem ("Update");
		update_item.Image = new Gtk.Image (Stock.Italic, IconSize.Menu);
		update_item.Activated += new EventHandler (Update_Activated);

		action_menu.Append (insert_item);
		action_menu.Append (remove_item);
		action_menu.Append (update_item);

		Menu help_menu = new Menu ();
		MenuItem help_item = new MenuItem ("_Help");
		help_item.Submenu = help_menu;
		MenuItem about = new MenuItem ("About");
		about.Activated += new EventHandler (About_Box);
		help_menu.Append (about);
		mb.Append (help_item);

		return mb;
	}

	static void Insert_Activated (object o, EventArgs args)
	{
		Db_Insert ();
	}

	static void Remove_Activated (object o, EventArgs args)
	{
		Db_Remove ();
	}

	static void Update_Activated (object o, EventArgs args)
	{
		Db_Update ();
	}

	static void Quit_Activated (object o, EventArgs args)
	{
		Application.Quit ();
	}

	static void About_Box (object o, EventArgs args)
	{
		string [] authors = new string [] {
			"Gonzalo Paniagua (gonzalo@ximian.com)",
			"Duncan Mak (duncan@ximian.com)",
		};

		string [] documenters = new string [] {};

		Gnome.About about = new Gnome.About ("Database Client", "0.1",
						     "Copyright (C) 2002, Ximian Inc.",
						     "A Sample Database client",
						     authors, documenters, "", new Gdk.Pixbuf ());
		about.Show ();
	}

	
	static IdConnection Conn
	{
		get {
			if (conn == null)
				conn = new IdConnection ();
			return conn;
		}
	}
}

struct Record {
	public uint ID;
	public string Name;
	public string Address;

	public Record (uint i, string s, string t)
	{
		ID = i;
		Name = s;
		Address = t;
	}
}

class IdConnection : IDisposable
{
	private SqlConnection cnc;
	private bool disposed;

	public IdConnection ()
	{
		cnc = new SqlConnection ();
		string connectionString = "hostaddr=80.24.221.71;" +
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

	public void Delete (uint id)
	{
		string deleteCmd = String.Format ("DELETE FROM customers WHERE id = {0}", id);
		IDbCommand deleteCommand = cnc.CreateCommand();
		deleteCommand.CommandText = deleteCmd;
		deleteCommand.ExecuteNonQuery ();
	}

	public bool Update (uint id, string name, string address)
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

	public Record Select (uint id)
	{
		IDbCommand selectCommand = cnc.CreateCommand();
		string selectCmd = "SELECT id, name, address FROM customers WHERE id = " + id;
		selectCommand.CommandText = selectCmd;
		IDataReader reader = selectCommand.ExecuteReader ();
		ArrayList list = FillDataList (reader);
		return (Record) list [0];
	}

	private ArrayList FillDataList (IDataReader reader)
	{
		ArrayList list = new ArrayList ();
		while (reader.Read ()) {
			Record data = new Record (UInt32.Parse (reader.GetValue (0).ToString ()),
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


using System;
using System.Collections;
using System.Drawing;
using System.Data;
using System.Data.SqlClient;

using Gtk;
using GtkSharp;

class Client {

	static Window window = null;
	static Dialog dialog = null;
	static Toolbar toolbar = null;
	static Table tableau = null;
	static Entry id_entry = null;
	static Entry name_entry = null;
	static Entry address_entry = null;
	static VBox box = null;
	static IdConnection conn = null;
	
	static void Main ()
	{
		Application.Init ();
		window = new Window ("Database client");
		window.DeleteEvent += new DeleteEventHandler (Window_Delete);
		window.DefaultSize = new Size (300, 200);

		box = new VBox (false, 0);
		window.Add (box);

		toolbar = new Toolbar ();
		PackToolbar ();
		box.PackStart (toolbar, false, false, 0);

		UpdateView ();

		window.ShowAll ();
		Application.Run ();
	}

	static void PackToolbar ()
	{
		toolbar.AppendItem ("Insert", "Insert a row", String.Empty,
				    new Gtk.Image (Stock.Add, IconSize.LargeToolbar),
				    new SignalFunc (Db_Insert), IntPtr.Zero);
		
		toolbar.AppendItem ("Remove", "Remove a row", String.Empty,
				    new Gtk.Image (Stock.Remove, IconSize.LargeToolbar),
				    new SignalFunc (Db_Remove), IntPtr.Zero);

		toolbar.AppendItem ("Edit", "Edit a row", String.Empty,
				    new Gtk.Image (Stock.Italic, IconSize.LargeToolbar),
				    new SignalFunc (Db_Edit), IntPtr.Zero);

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

		ArrayList dataList = Conn.SelectAll ();
		
		tableau = new Gtk.Table ((uint) dataList.Count + 1, 3, true);
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

		t.Attach (label, 0, 1, 0, 1, AttachOptions.Expand, AttachOptions.Expand, 1, 1);

		label = new Label (String.Empty);
		label.Markup = "<big><b>Name</b></big>";
		label.UseMarkup = true;
		t.Attach (label, 0, 2, 0, 1, AttachOptions.Expand, AttachOptions.Expand, 1, 1);

		label = new Label (String.Empty);
		label.Markup = "<big><b>Address</b></big>";
		label.UseMarkup = true;
		t.Attach (label, 0, 3, 0, 1, AttachOptions.Expand, AttachOptions.Expand, 1, 1);
	}

	static void Db_Insert ()
	{
		dialog = new Dialog ();
		dialog.Title = "Insert row";
		dialog.BorderWidth = 3;
		dialog.VBox.BorderWidth = 5;
		dialog.HasSeparator = false;

		Frame frame = new Frame ("Insert a row");
		frame.Add (DrawForm (Stock.DialogInfo));
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
		dialog = new Dialog ();
		dialog.Title = "Remove row";
		dialog.BorderWidth = 3;		
		dialog.VBox.BorderWidth = 5;
		dialog.HasSeparator = false;

		Frame frame = new Frame ("Remove a row");
		frame.Add (DrawForm (Stock.DialogWarning));
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

	static Widget DrawForm (string image)
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
		table.Attach (name_entry, 1, 2, 1, 2);

		label = Label.NewWithMnemonic ("_Address");
		table.Attach (label, 0, 1, 2, 3);
		address_entry = new Entry ();
		table.Attach (address_entry, 1, 2, 2, 3);

		return hbox ;
	}

	static void Db_Edit ()
	{
	}

	static void Quit ()
	{
		Application.Quit ();
	}

	static void Insert_Action (object o, EventArgs args)
	{
		Conn.Insert (UInt32.Parse (id_entry.Text), name_entry.Text, address_entry.Text);
		UpdateView ();
		dialog.Destroy ();
	}

	static void Remove_Action (object o, EventArgs args)
	{
		Conn.Delete (UInt32.Parse (id_entry.Text));
		UpdateView ();
		dialog.Destroy ();
	}

	static void Dialog_Cancel (object o, EventArgs args)
	{
		dialog.Destroy ();
		dialog = null;
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


using System;
using System.Drawing;
using Gtk;
using GtkSharp;

class Client {

	static Window window = null;
	static Dialog dialog = null;
	static Toolbar toolbar = null;
	static Table tableau = null;
	
	static void Main ()
	{
		Application.Init ();
		window = new Window ("Database client");
		window.DeleteEvent += new DeleteEventHandler (Window_Delete);
		window.DefaultSize = new Size (300, 200);

		VBox box = new VBox (false, 0);
		window.Add (box);

		toolbar = new Toolbar ();
		PackToolbar ();
		box.PackStart (toolbar, false, false, 0);

		tableau = CreateView ();
		box.PackStart (tableau, false, false, 0);

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

		toolbar.AppendItem ("Update", "Update the view", String.Empty,
				    new Gtk.Image (Stock.Refresh, IconSize.LargeToolbar),
				    new SignalFunc (Db_Update), IntPtr.Zero);

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

	static Gtk.Table CreateView ()
	{
		Table t = new Gtk.Table (0, 2, true);
		DrawTitles (t);
		t.ColSpacings = 10;

		return t;
	}

	static void DrawTitles (Table t)
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

	static void UpdateView (Record [] records)
	{
		
	}

	static void Db_Insert ()
	{
		dialog = new Dialog ();
		dialog.Title = "Insert row";
		dialog.VBox.BorderWidth = 5;		

		dialog.VBox.PackStart (DrawForm (Stock.Add), true, true, 0);

		Button button = null;
		button = new Button ("_Insert");
		button.Clicked += new EventHandler (Insert_Action);
		button.CanDefault = true;
		dialog.ActionArea.PackStart (button, true, true, 0);
		button.GrabDefault ();

		button = new Button ("_Cancel");
		button.Clicked += new EventHandler (Dialog_Cancel);
		dialog.ActionArea.PackStart (button, true, true, 0);

		dialog.ShowAll ();
	}

	static void Db_Remove ()
	{
		dialog = new Dialog ();
		dialog.Title = "Remove row";
		dialog.VBox.BorderWidth = 5;

		dialog.VBox.PackStart (DrawForm (Stock.Remove), true, true, 0);

		Button button = null;
		button = new Button ("_Remove");
		button.Clicked += new EventHandler (Remove_Action);
		button.CanDefault = true;
		dialog.ActionArea.PackStart (button, true, true, 0);
		button.GrabDefault ();

		button = new Button ("_Cancel");
		button.Clicked += new EventHandler (Dialog_Cancel);
		dialog.ActionArea.PackStart (button, true, true, 0);

		dialog.ShowAll ();

	}

	static Widget DrawForm (string image)
	{
		HBox hbox = new HBox (false, 2);
		hbox.PackStart (new Gtk.Image (image, IconSize.Dialog), true, true, 0);
		
		Table table = new Table (3, 3, false);
		hbox.PackStart (table);
		table.ColSpacings = 4;
		table.RowSpacings = 4;
		Label label = null;
		Gtk.Entry entry = null;

		label = Label.NewWithMnemonic ("_ID");
		table.Attach (label, 0, 1, 0, 1);
		entry = new Entry ();
		table.Attach (entry, 1, 2, 0, 1);

		label = Label.NewWithMnemonic ("_Name");
		table.Attach (label, 0, 1, 1, 2);
		entry = new Entry ();
		table.Attach (entry, 1, 2, 1, 2);

		label = Label.NewWithMnemonic ("_Address");
		table.Attach (label, 0, 1, 2, 3);
		entry = new Entry ();
		table.Attach (entry, 1, 2, 2, 3);
		

		return hbox ;
	}

	static void Db_Update ()
	{
	}

	static void Quit ()
	{
		Application.Quit ();
	}

	static void Insert_Action (object o, EventArgs args)
	{
	}

	static void Remove_Action (object o, EventArgs args)
	{
	}

	static void Dialog_Cancel (object o, EventArgs args)
	{
		dialog.Destroy ();
		dialog = null;
	}

	
}

struct Record {

	public int ID;
	public string Name;
	public string Address;

	public Record (int i, string s, string t)
	{
		ID = i;
		Name = s;
		Address = t;
	}
}

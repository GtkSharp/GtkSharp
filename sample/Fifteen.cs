using System;
using System.Drawing;

using Gtk;
using Gdk;
using Gnome;
using GtkSharp;


public class Fifteen
{
	const int SCRAMBLE_MOVES = 256;
	static Gtk.Window window = null;
	static Canvas canvas = null;
	static BoardPiece [] board;
		
	static void Main (string [] args)
	{
		if (args.Length > 0 && args [0] == "debug")
			BoardPiece.Debug = true;
		Program fifteen = new Program ("Fifteen", "0.1", Modules.UI, args);
		
		window = new Gtk.Window ("Fifteen #");
		VBox box = new VBox (false, 0);
		window.Add (box);
		window.DefaultSize = new Size (300, 300);
		
		VBox vbox = new VBox (false, 4);
		vbox.BorderWidth = 4;

		MenuBar menu = Create_Menu ();
		box.PackStart (menu, false, false, 0);
		box.PackStart (vbox, false, false, 0);
		window.DeleteEvent += new DeleteEventHandler (Window_Delete);

		Alignment alignment = new Alignment (0.5f, 0.5f, 0.0f, 0.0f);
		vbox.PackStart (alignment, true, true, 4);

		Frame frame = new Frame (null);
		frame.ShadowType = ShadowType.In;
		alignment.Add (frame);

		canvas = new Canvas ();
		canvas.SetSizeRequest (BoardPiece.PIECE_SIZE * 4 + 1, BoardPiece.PIECE_SIZE * 4 + 1);
		canvas.SetScrollRegion (0, 0, BoardPiece.PIECE_SIZE * 4 + 1, BoardPiece.PIECE_SIZE * 4 + 1);
		frame.Add (canvas);
		
		board = new BoardPiece [16];

		for (int i = 0; i < 15; i ++) {					
			int y = i /4;
			int x = i % 4;
			board [i] = new BoardPiece (canvas.Root (), board, x, y, i);
		}

		box.PackStart (new HSeparator (), false, false, 0);
		HBox hbox = new HBox (false, 4);
		box.PackStart (hbox, true, false, 4);

		// Scramble button here.
		Button scramble = new Button ("_Scramble");
		scramble.BorderWidth = 8;
		scramble.Clicked += new EventHandler (Scramble);
		scramble.CanDefault = true;
		hbox.PackStart (scramble, true, true, 4);
		scramble.GrabDefault ();

		// Close button
		Button close = new Button ("_Close");
		close.BorderWidth = 8;
		close.Clicked += new EventHandler (Quit);
		hbox.PackStart (close, true, true, 4);
		
		window.ShowAll ();
		fifteen.Run ();
	}

	static void Quit (object o, EventArgs args)
	{
		Application.Quit ();
	}

	static Gtk.MenuBar Create_Menu ()
	{
		MenuBar mb = new MenuBar ();
		Menu file_menu = new Menu ();

		ImageMenuItem scramble_item = new ImageMenuItem ("_Scramble");
		scramble_item.Image = new Gtk.Image (Gtk.Stock.Refresh, IconSize.Menu);
		scramble_item.Activated += new EventHandler (Scramble);
		
		ImageMenuItem quit_item = new ImageMenuItem ("_Quit");
		quit_item.Image = new Gtk.Image (Gtk.Stock.Quit, IconSize.Menu);
		quit_item.Activated += new EventHandler (Quit);

		file_menu.Append (scramble_item);
		file_menu.Append (new SeparatorMenuItem ());
		file_menu.Append (quit_item);
	
		MenuItem file_item = new MenuItem ("_File");
		file_item.Submenu = file_menu;
		mb.Append (file_item);

		Menu help_menu = new Menu ();
		MenuItem help_item = new MenuItem ("_Help");
		help_item.Submenu = help_menu;

		ImageMenuItem about = new ImageMenuItem (Gnome.Stock.About, new AccelGroup ());
		about.Activated += new EventHandler (About_Box);
		help_menu.Append (about);
		mb.Append (help_item);

		return mb;
	}

	static void About_Box (object o, EventArgs args)
	{
		string [] authors = new string [] {
			"Duncan Mak (duncan@ximian.com)",
		};

		string [] documenters = new string [] {};
		string translaters = null;
		Pixbuf pixbuf = new Pixbuf ("pixmaps/gnome-color-browser.png");

		Gnome.About about = new Gnome.About ("Fifteen #", "0.1",
						     "Copyright (C) 2002, Ximian Inc.",
						     "A C# port of Fifteen, a gnomecanvas demo",
						     authors, documenters, translaters,
						     pixbuf);
		about.Show ();
	}


	static void Scramble (object o, EventArgs args)
	{
		Random rand = new Random ();
		int blank;
		int position;		

		// find blank spot
		for (position = 0; position < 16; position ++)
			if (board [position] == null)
				break;

		for (int i = 0; i < SCRAMBLE_MOVES; i++) {
		retry:
			int dir = rand.Next (4);
			int x = 0, y = 0;

			if ((dir == 0) && (position > 3)) // up
				y = -1;
			else if ((dir == 1) && (position < 12)) // down
				y = 1;
			else if ((dir == 2) && ((position % 4) != 0)) // left
				x = -1;
			else if ((dir == 3) && ((position % 4) != 3)) // right
				x = 1;
			else 
				goto retry;

			int old_position = position + y * 4 + x;
			board [position] = board [old_position];
			board [position].Position = position;
			board [old_position] = null;
			board [position].Move (-x * BoardPiece.PIECE_SIZE, -y * BoardPiece.PIECE_SIZE);
			canvas.UpdateNow ();
			position = old_position;
		}			
	}

	static void Window_Delete (object o, DeleteEventArgs args)
	{
		Application.Quit ();
		args.RetVal = true;
	}
}

public class BoardPiece : Gnome.CanvasGroup
{
	public static int PIECE_SIZE = 50;
	public int Number;
	int position;
	public CanvasText Text;
	public BoardPiece [] Board;
	public static bool Debug = false;
	
	public BoardPiece (CanvasGroup group, BoardPiece [] board, int x, int y, int i)
		: base (group, CanvasGroup.GType)
	{
		this.X = (x * PIECE_SIZE);
		this.Y = (y * PIECE_SIZE);
		this.Board = board;
		this.Number = i;
		this.Position = i;		

		CanvasRect rect = new CanvasRect (this);
		rect.X1 = 0.0;
		rect.Y2 = 0.0;
		rect.X2 = PIECE_SIZE;
		rect.Y2 = PIECE_SIZE;
		rect.FillColor = Color;
		rect.OutlineColor = "black";
		rect.WidthPixels = 0;
		
		CanvasText text = new CanvasText (this);
		text.Text = (i + 1).ToString ();
		text.X = PIECE_SIZE / 2.0;
		text.Y = PIECE_SIZE / 2.0;
		text.Font = "Sans Bold 24";
		text.Anchor = AnchorType.Center;
		text.FillColor = "black";

		this.Text = text;
		this.CanvasEvent += new GnomeSharp.CanvasEventHandler (Piece_Event);
	}

	public string Color {
		get {
			int y = Number / 4;
			int x = Number % 4;

			int r = ((4 - x) * 255) / 4;
			int g = ((4 - y) * 255) / 4;
			int b = 128;

			return String.Format ("#{0:x2}{1:x2}{2:x2}", r, g, b);
		}
	}

	public int Position {
		get {
			return position;
		}

		set {
			Board [value] = this;
			position = value;
		}
	}

	static void Piece_Event (object o, GnomeSharp.CanvasEventArgs args)
	{
		BoardPiece piece = (BoardPiece) o;
		Canvas canvas = piece.Canvas;
		double dx = 0.0, dy = 0.0;

		switch (args.Event.Type) {
		case EventType.EnterNotify:
			piece.Text.FillColor = "white";
			args.RetVal = true;
			break;

		case EventType.LeaveNotify:
			piece.Text.FillColor = "black";
			args.RetVal = true;
			break;

		case EventType.ButtonPress:
			int y = piece.Position / 4; 
			int x = piece.Position % 4;
			Print_Position ("from", piece.Position, true);
			
			bool toMove = true;

			if ((y > 0) && (piece.Board [(y - 1) * 4 + x] == null)) {
				dx = 0.0;
				dy = -1.0;
				y --;
			} else if ((y < 3) && (piece.Board [(y + 1) * 4 + x] == null)) {
				dx = 0.0;
				dy = 1.0;
				y ++;
			} else if ((x > 0) && (piece.Board [y * 4 + x - 1] == null)) {
				dx = -1.0;
				dy = 0.0;
				x --;
			} else if ((x < 3) && (piece.Board [y * 4 + x + 1] == null)) {
				dx = 1.0;
				dy = 0.0;
				x ++;
			} else 
				toMove = false;

			if (toMove) {
				int new_position = y * 4 + x;
				Print_Position ("to", new_position, false);
 				piece.Board [piece.Position] = null;
				piece.Position = new_position;
				piece.Move (dx * PIECE_SIZE, dy * PIECE_SIZE);
				canvas.UpdateNow ();
			} else
				Print_Position ("to", piece.Position, false);

			args.RetVal = true;
			break;

		default:
			args.RetVal = false;
			break;
		}
	}

	static void Print_Position (string text, int position, bool newLine)
	{
		if (!Debug)
			return;
		else {
			int x = position / 4;
			int y = position % 4;
			string output = String.Format (" {0} ({1}, {2})", text, x + 1, y + 1);
			if (newLine)
				Console.Write (output);
			else
				Console.WriteLine (output);
		}
	}

}


using System;
using System.Drawing;

using Gtk;
using Gdk;
using Gnome;
using GtkSharp;


public class Fifteen
{
	const int PIECE_SIZE = 50;
	const int SCRAMBLE_MOVES = 256;
	static Window window = null;
	static Canvas canvas = null;
	static BoardPiece [] board;
	static bool debug = false;
	
	static void Main (string [] args)
	{
		if (args.Length > 0 && args [0] == "debug")
			debug = true;
		
		Application.Init ();
		window = new Window ("Fifteen #");
		VBox vbox = new VBox (false, 4);
		vbox.BorderWidth = 4;
		window.DefaultSize = new Size (300, 300);
		window.Add (vbox);
		window.DeleteEvent += new DeleteEventHandler (Window_Delete);

		Alignment alignment = new Alignment (0.5f, 0.5f, 0.0f, 0.0f);
		vbox.PackStart (alignment, true, true, 0);

		Frame frame = new Frame (null);
		frame.ShadowType = ShadowType.In;
		alignment.Add (frame);

		canvas = new Canvas ();
		canvas.SetSizeRequest (PIECE_SIZE * 4 + 1, PIECE_SIZE * 4 + 1);
		canvas.SetScrollRegion (0, 0, PIECE_SIZE * 4 + 1, PIECE_SIZE * 4 + 1);
		frame.Add (canvas);
		
		board = new BoardPiece [16];

		for (int i = 0; i < 15; i ++) {					
			int y = i /4;
			int x = i % 4;

			board [i] = new BoardPiece (canvas.Root ());
			board [i].X = (x * PIECE_SIZE);
			board [i].Y = (y * PIECE_SIZE);

			CanvasRect rect = new CanvasRect (board [i]);
			rect.X1 = 0.0;
			rect.Y2 = 0.0;
			rect.X2 = PIECE_SIZE;
			rect.Y2 = PIECE_SIZE;
			rect.FillColor = Get_Color (i);
			rect.OutlineColor = "black";
			rect.WidthPixels = 0;

			CanvasText text = new CanvasText (board [i]);
			text.Text = (i + 1).ToString ();
			text.X = PIECE_SIZE / 2.0;
			text.Y = PIECE_SIZE / 2.0;
			text.Font = "Sans Bold 24";
			text.Anchor = AnchorType.Center;
			text.FillColor = "black";

			board [i].Number = i;
			board [i].Position = i;
			board [i].Text = text;
			board [i].CanvasEvent += new CanvasEventHandler (Piece_Event);
		}

		// Scramble button here.
		Button button = new Button ("Scamble");
		vbox.PackStart (button, false, false, 0);
		button.Clicked += new EventHandler (Scramble);

		window.ShowAll ();
		Application.Run ();
	}

	static string Get_Color (int piece)
	{
		int y = piece / 4;
		int x = piece % 4;

		int r = ((4 - x) * 255) / 4;
		int g = ((4 - y) * 255) / 4;
		int b = 128;

		return String.Format ("#{0:x2}{1:x2}{2:x2}", r, g, b);
	}

	static void Piece_Event (object o, CanvasEventArgs args)
	{
		BoardPiece piece = (BoardPiece) o;
		Canvas canvas = piece.Canvas;
		double dx = 0.0, dy = 0.0;

		switch (args.Event.Type) {
		case EventType.EnterNotify:
			piece.Text.FillColor = "white";
			break;

		case EventType.LeaveNotify:
			piece.Text.FillColor = "black";
			break;

		case EventType.ButtonPress:
			int y = piece.Position / 4; 
			int x = piece.Position % 4;
			Print_Position ("from", piece.Position, true);
			
			bool toMove = true;

			if ((y > 0) && (board [(y - 1) * 4 + x] == null)) {
				dx = 0.0;
				dy = -1.0;
				y --;
			} else if ((y < 3) && (board [(y + 1) * 4 + x] == null)) {
				dx = 0.0;
				dy = 1.0;
				y ++;
			} else if ((x > 0) && (board [y * 4 + x - 1] == null)) {
				dx = -1.0;
				dy = 0.0;
				x --;
			} else if ((x < 3) && (board [y * 4 + x + 1] == null)) {
				dx = 1.0;
				dy = 0.0;
				x ++;
			} else 
				toMove = false;

			if (toMove) {
				int new_position = y * 4 + x;
				Print_Position ("to", new_position, false);
				board [piece.Position] = null;
				board [new_position] = piece;
				piece.Position = new_position;
				piece.Move (dx * PIECE_SIZE, dy * PIECE_SIZE);
			} else
				Print_Position ("to", piece.Position, false);

			break;

		default:
			break;
		}

		args.RetVal = false;
	}

	static void Print_Position (string text, int position, bool newLine)
	{
		if (!debug)
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
			board [position].Move (-x * PIECE_SIZE, -y * PIECE_SIZE);
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
	public int Number;
	public int Position;
	public CanvasText Text;
	
	public BoardPiece (CanvasGroup group)
		: base (group, CanvasGroup.Type)
	{
	}
}


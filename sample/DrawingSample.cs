//
// Sample program demostrating using Cairo with Gtk#
//
using Gtk;
using System;
using System.Drawing;

class X {
	static DrawingArea a, b;
	
	static void Main ()
	{
		Application.Init ();
		Gtk.Window w = new Gtk.Window ("System.Drawing and Gtk#");

		// Custom widget sample
		a = new PrettyGraphic ();

		// Event-based drawing
		b = new DrawingArea ();
		b.ExposeEvent += ExposeHandler;
		b.SizeAllocated += SizeAllocatedHandler;

		Button c = new Button ("Quit");
		c.Clicked += quit;

		MovingText m = new MovingText ();
		
		Box box = new HBox (true, 0);
		box.Add (a);
		box.Add (b);
		box.Add (m);
		box.Add (c);
		w.Add (box);
		
		w.ShowAll ();
		Application.Run ();
	}

	static void quit (object obj, EventArgs args)
	{
		Application.Quit ();
	}

	static Gdk.Rectangle rect;
		
	static void SizeAllocatedHandler (object obj, SizeAllocatedArgs args)
	{
		rect = args.Allocation;
	}

	static void ExposeHandler (object obj, ExposeEventArgs args)
	{
		Gdk.EventExpose ev = args.Event;
		Gdk.Window window = ev.Window;
		
		using (Graphics g = Gdk.Graphics.FromDrawable (window)){
			g.TranslateTransform (ev.Area.X, ev.Area.Y);
			using (Pen p = new Pen (Color.Red)){
				g.DrawPie (p, 0, 0, rect.Width, rect.Height, 50, 90);
			}
		}
	}
}

//
// A sample using inheritance to draw
//
class PrettyGraphic : DrawingArea {

	public PrettyGraphic ()
	{
		SetSizeRequest (200, 200);
	}
			       
	protected override bool OnExposeEvent (Gdk.EventExpose args)
	{
		Gdk.Window win = args.Window;
		Gdk.Rectangle area = args.Area;

		using (Graphics g = Gdk.Graphics.FromDrawable (args.Window)){
			//Console.WriteLine ("{0} and {1}", -args.Area.X, -args.Area.Y);
			//g.TranslateTransform (-args.Area.X, -args.Area.Y);
			Pen p = new Pen (Color.Blue, 1.0f);

			for (int i = 0; i < 600; i += 60)
				for (int j = 0; j < 600; j += 60)
					g.DrawLine (p, i, 0, 0, j);
		}
		return true;
	}
}

class MovingText : DrawingArea {
	static int d = 0;

	public MovingText ()
	{
		Gtk.Timeout.Add (50, new Function (Forever));
		SetSizeRequest (300, 200);
	}

	bool Forever ()
	{
		QueueDraw ();
		return true;
	}
	
	protected override bool OnExposeEvent (Gdk.EventExpose args)
	{
		Gdk.Window win = args.Window;
		Gdk.Rectangle area = args.Area;

		using (Graphics g = Gdk.Graphics.FromDrawable (args.Window)){
			Brush back = new SolidBrush (Color.White);
			Brush fore = new SolidBrush (Color.Red);
			Font f = new Font ("Times", 20);

			g.FillRectangle (back, 0, 0, 400, 400);
			g.TranslateTransform (150, 100); 
			g.RotateTransform (d);
			d += 3;
			g.DrawString ("Mono", f, fore, 0, 0);
		}

		return true;
	}
	
}

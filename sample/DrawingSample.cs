//
// Sample program demostrating using System.Drawing with Gtk#
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
		b.ExposeEvent += new ExposeEventHandler (ExposeHandler);
		b.SizeAllocated += new SizeAllocatedHandler (SizeAllocatedHandler);

		Button c = new Button ("Quit");
		c.Clicked += new EventHandler (quit);

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
		
		using (Graphics g = Gtk.DotNet.Graphics.FromDrawable (window)){
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
		using (Graphics g = Gtk.DotNet.Graphics.FromDrawable (args.Window)){
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
	Font f;
	
	public MovingText ()
	{
		GLib.Timeout.Add (20, new GLib.TimeoutHandler (Forever));
		SetSizeRequest (300, 200);
		f = new Font ("Times", 20);
	}

	bool Forever ()
	{
		QueueDraw ();
		return true;
	}
	
	protected override bool OnExposeEvent (Gdk.EventExpose args)
	{
		using (Graphics g = Gtk.DotNet.Graphics.FromDrawable (args.Window)){
			using (Brush back = new SolidBrush (Color.White), 
			       fore = new SolidBrush (Color.Red)){

					g.FillRectangle (back, 0, 0, 400, 400);
					g.TranslateTransform (150, 100); 
					g.RotateTransform (d);
					d += 3;
					g.DrawString ("Mono", f, fore, 0, 0);
			}
		}

		return true;
	}
	
}

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
		Gtk.Window w = new Gtk.Window ("Hello");

		// Custom widget sample
		a = new PrettyGraphic ();

		// Event-based drawing
		b = new DrawingArea ();
		b.ExposeEvent += ExposeHandler;
		b.SizeAllocated += SizeAllocatedHandler;
		
		Box box = new HBox (true, 0);
		box.Add (a);
		//box.Add (b);
		w.Add (box);
		
		w.ShowAll ();
		Application.Run ();
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
			Console.WriteLine ("{0} and {1}", -ev.Area.X, -ev.Area.Y);
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

	protected override bool OnExposeEvent (Gdk.EventExpose args)
	{
		Gdk.Window win = args.Window;
		Gdk.Rectangle area = args.Area;

		using (Graphics g = Gdk.Graphics.FromDrawable (args.Window)){
			//Console.WriteLine ("{0} and {1}", -args.Area.X, -args.Area.Y);
			//g.TranslateTransform (-args.Area.X, -args.Area.Y);
			Pen p = new Pen (Color.Blue, 1.0f);
			Pen q = new Pen (Color.Red, 1.0f);

			g.DrawLine (p, 0, 0, 100, 100);
			g.DrawLine (q, 0, 0, 100, 100);
			return true;
			
			for (int i = 0; i < 600; i += 60)
				for (int j = 0; j < 600; j += 60)
					g.DrawLine (p, i, 0, 0, j);
		}
		return true;
	}
}

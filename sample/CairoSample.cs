//
// Sample program demostrating using Cairo with Gtk#
//
using Gtk;
using Gdk;
using System;
using GtkSharp;
using Cairo;

class X {
	static DrawingArea a, b;
	
	static void Main ()
	{
		Application.Init ();
		Gtk.Window w = new Gtk.Window ("Hello");

		a = new DrawingArea ();
		a.ExposeEvent += new ExposeEventHandler (LineExposeHandler);

		b = new DrawingArea ();
		b.ExposeEvent += new ExposeEventHandler (CirclesExposeHandler);
		b.SizeAllocated += new SizeAllocatedHandler (SizeAllocatedHandler);
		
		Box box = new HBox (true, 0);
		//box.Add (a);
		box.Add (b);
		w.Add (box);
		
		w.ShowAll ();
		Application.Run ();
	}

	static void LineExposeHandler (object obj, ExposeEventArgs args)
	{
		int offx, offy;

		using (Cairo.Graphics o = args.Event.window.CairoGraphics (out offx, out offy)){
			o.SetRGBColor (1, 0, 0);
			o.Translate (-offx, -offy);
			o.MoveTo (0, 0);
			o.LineTo (100, 100);
			o.Stroke ();
		}
	}

	static Rectangle rect;
		
	static void SizeAllocatedHandler (object obj, SizeAllocatedArgs args)
	{
		rect = args.Allocation;
	}

	static void CirclesExposeHandler (object obj, ExposeEventArgs args)
	{
		Rectangle area = args.Event.area;
		Gdk.Window window = args.Event.window;
		Pixmap p = new Pixmap (window, area.width, area.height, -1);

		int x, y;
		//Cairo.Object o = p.CairoGraphics ();
		using (Cairo.Graphics o = window.CairoGraphics (out x, out y))
		{
			o.Translate (-area.x, -area.y);
			DrawCircles (o, rect);
			
			//using (Gdk.GC gc = new Gdk.GC (window)){
			//window.DrawDrawable (gc, p, 0, 0, area.x, area.y, area.height, area.width);
			//}
		}
	}
	
	static void DrawCircles (Cairo.Graphics o, Gdk.Rectangle rect)
	{
		FillChecks (o, rect);
	}

	const int CS = 32;

	static void FillChecks (Cairo.Graphics o, Gdk.Rectangle rect)
	{
		Surface check;
		// Draw the check
		o.Save ();
		using (check = Surface.CreateSimilar (o.TargetSurface, Format.RGB24, 2 * CS, 2 * CS)){
#if true
			o.Save ();
			check.Repeat = 1;
			
			o.TargetSurface = check;
			o.Operator = Operator.Src;
			o.SetRGBColor (0.4, 0.4, 0.4);
			
			// Clear the background
			o.Rectangle (0, 0, 2*CS, 2*CS);
			o.Fill ();
			o.SetRGBColor (0.7, 0.7, 0.7);
			o.Rectangle (0, CS, CS, CS *2);
			o.Fill ();
			o.Rectangle (CS, 0, CS*2, CS);
			o.Fill ();
			o.Restore ();

			// Fill the surface with the check
			o.SetPattern (check);
			o.Rectangle (0, 0, rect.width, rect.height);
			o.Fill ();
#endif
		}
		o.Restore ();
		o.SetRGBColor (1, 0, 0);
		o.Alpha = 0.5;
		Console.WriteLine (rect);
		o.MoveTo (0, 0);
		o.LineTo (rect.width, rect.height);
		o.Stroke ();
	}
}

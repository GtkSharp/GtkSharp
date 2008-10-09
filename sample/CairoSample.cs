using System;
using Gtk;
using Cairo;

class Knockout : DrawingArea
{
	static void Main ()
	{
		Application.Init ();
		new Knockout ();
		Application.Run ();
	}

	Knockout ()
	{
		Window win = new Window ("Cairo with Gtk#");
		win.SetDefaultSize (400, 400);
		win.DeleteEvent += new DeleteEventHandler (OnQuit);
		win.Add (this);
		win.ShowAll ();
	}

	void OvalPath (Context cr, double xc, double yc, double xr, double yr)
	{
		Matrix m = cr.Matrix;

		cr.Translate (xc, yc);
		cr.Scale (1.0, yr / xr);
		cr.MoveTo (xr, 0.0);
		cr.Arc (0, 0, xr, 0, 2 * Math.PI);
		cr.ClosePath ();

		cr.Matrix = m;
	}

	void FillChecks (Context cr, int x, int y, int width, int height)
	{
		int CHECK_SIZE = 32;
		
		cr.Save ();
		Surface check = cr.Target.CreateSimilar (Content.Color, 2 * CHECK_SIZE, 2 * CHECK_SIZE);
		
		// draw the check
		using (Context cr2 = new Context (check)) {
			cr2.Operator = Operator.Source;
			cr2.Color = new Color (0.4, 0.4, 0.4);
			cr2.Rectangle (0, 0, 2 * CHECK_SIZE, 2 * CHECK_SIZE);
			cr2.Fill ();

			cr2.Color = new Color (0.7, 0.7, 0.7);
			cr2.Rectangle (x, y, CHECK_SIZE, CHECK_SIZE);
			cr2.Fill ();

			cr2.Rectangle (x + CHECK_SIZE, y + CHECK_SIZE, CHECK_SIZE, CHECK_SIZE);
			cr2.Fill ();
		}

		// Fill the whole surface with the check
		SurfacePattern check_pattern = new SurfacePattern (check);
		check_pattern.Extend = Extend.Repeat;
		cr.Source = check_pattern;
		cr.Rectangle (0, 0, width, height);
		cr.Fill ();

		check_pattern.Destroy ();
		check.Destroy ();
		cr.Restore ();
	}

	void Draw3Circles (Context cr, int xc, int yc, double radius, double alpha)
	{
		double subradius = radius * (2 / 3.0 - 0.1);

		cr.Color = new Color (1.0, 0.0, 0.0, alpha);
		OvalPath (cr, xc + radius / 3.0 * Math.Cos (Math.PI * 0.5), yc - radius / 3.0 * Math.Sin (Math.PI * 0.5), subradius, subradius);
		cr.Fill ();

		cr.Color = new Color (0.0, 1.0, 0.0, alpha);
		OvalPath (cr, xc + radius / 3.0 * Math.Cos (Math.PI * (0.5 + 2 / 0.3)), yc - radius / 3.0 * Math.Sin (Math.PI * (0.5 + 2 / 0.3)), subradius, subradius);
		cr.Fill ();

		cr.Color = new Color (0.0, 0.0, 1.0, alpha);
    	OvalPath (cr, xc + radius / 3.0 * Math.Cos (Math.PI * (0.5 + 4 / 0.3)), yc - radius / 3.0 * Math.Sin (Math.PI * (0.5 + 4 / 0.3)), subradius, subradius);
		cr.Fill ();
	}

	void Draw (Context cr, int width, int height)
	{
		double radius = 0.5 * Math.Min (width, height) - 10;
		int xc = width / 2;
		int yc = height / 2;

		Surface overlay = cr.Target.CreateSimilar (Content.ColorAlpha, width, height);
		Surface punch   = cr.Target.CreateSimilar (Content.Alpha, width, height);
		Surface circles = cr.Target.CreateSimilar (Content.ColorAlpha, width, height);

		FillChecks (cr, 0, 0, width, height);
		cr.Save ();

		// Draw a black circle on the overlay
		using (Context cr_overlay = new Context (overlay)) {
			cr_overlay.Color = new Color (0.0, 0.0, 0.0);
			OvalPath (cr_overlay, xc, yc, radius, radius);
			cr_overlay.Fill ();

			// Draw 3 circles to the punch surface, then cut
			// that out of the main circle in the overlay
			using (Context cr_tmp = new Context (punch))
				Draw3Circles (cr_tmp, xc, yc, radius, 1.0);

			cr_overlay.Operator = Operator.DestOut;
			cr_overlay.SetSourceSurface (punch, 0, 0);
			cr_overlay.Paint ();

			// Now draw the 3 circles in a subgroup again
			// at half intensity, and use OperatorAdd to join up
			// without seams.
			using (Context cr_circles = new Context (circles)) {
				cr_circles.Operator = Operator.Over;
				Draw3Circles (cr_circles, xc, yc, radius, 0.5);
			}

			cr_overlay.Operator = Operator.Add;
			cr_overlay.SetSourceSurface (circles, 0, 0);
			cr_overlay.Paint ();
		}

		cr.SetSourceSurface (overlay, 0, 0);
		cr.Paint ();

		overlay.Destroy ();
		punch.Destroy ();
		circles.Destroy ();
	}

	protected override bool OnExposeEvent (Gdk.EventExpose e)
	{
		using (Context cr = Gdk.CairoHelper.Create (e.Window)) {
			int w, h;
			e.Window.GetSize (out w, out h);
			Draw (cr, w, h);
		}
		return true;
	}

	void OnQuit (object sender, DeleteEventArgs e)
	{
		Application.Quit ();
	}
}


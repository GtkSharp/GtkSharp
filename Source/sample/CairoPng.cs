using System;
using System.Diagnostics;
using System.IO;
using Cairo;

public class CairoPng
{
	public void CreatePng ()
	{
		const int Width = 480;
		const int Height = 160;
		const int CheckSize = 10;
		const int Spacing = 2;

		// Create an Image-based surface with data stored in ARGB32 format and a context,
		// "using" is used here to ensure that they are Disposed once we are done
		using (ImageSurface surface = new ImageSurface (Format.ARGB32, Width, Height))
		using (Context cr = new Cairo.Context (surface))
		{
			// Start drawing a checkerboard
			int i, j, xcount, ycount;
			xcount = 0;
			i = Spacing;
			while (i < Width) {
				j = Spacing;
				ycount = xcount % 2; // start with even/odd depending on row
				while (j < Height) {
					if (ycount % 2 != 0)
						cr.SetSourceRGB (1, 0, 0);
					else
						cr.SetSourceRGB (1, 1, 1);
					// If we're outside the clip, this will do nothing.
					cr.Rectangle (i, j, CheckSize, CheckSize);
					cr.Fill ();

					j += CheckSize + Spacing;
					++ycount;
				}
				i += CheckSize + Spacing;
				++xcount;
			}

			// Select a font to draw with
			cr.SelectFontFace ("serif", FontSlant.Normal, FontWeight.Bold);
			cr.SetFontSize (64.0);

			// Select a color (blue)
			cr.SetSourceRGB (0, 0, 1);

			// Draw
			cr.MoveTo (20, 100);
			cr.ShowText ("Hello, World");

			surface.WriteToPng ("test.png");
		}
	}

	static void Main ()
	{
		var app = new CairoPng ();
		int iterations = 100;

		for (int loop = 0; loop < 10; loop++) {
			Stopwatch stop_watch = new Stopwatch ();
			stop_watch.Start ();
			Console.Write ("Starting iterations, {0} bytes used...\t", Process.GetCurrentProcess().PrivateMemorySize64);
			for (int i = 0; i < iterations; i++) {
				app.CreatePng ();
			}
			stop_watch.Stop ();
			Console.WriteLine ("Created {0} PNG files in {1}ms", iterations, stop_watch.ElapsedMilliseconds);
			System.Threading.Thread.Sleep (1000);
		}
	}
}


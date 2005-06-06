using System;
using Gtk;
using Pango;

#if GTK_SHARP_2_6
namespace GtkDemo
{
	[Demo ("Rotated Text", "DemoRotatedText.cs")]
	public class DemoRotatedText : Window
	{
		const int RADIUS = 150;
		const int N_WORDS = 10;

		public DemoRotatedText () : base ("Rotated text")
		{
			DrawingArea drawingArea = new DrawingArea ();
			Gdk.Color white = new Gdk.Color (0xff, 0xff, 0xff);

			// This overrides the background color from the theme
			drawingArea.ModifyBg (StateType.Normal, white);
			drawingArea.ExposeEvent += new ExposeEventHandler (RotatedTextExposeEvent);

			this.Add (drawingArea);
			this.DeleteEvent += new DeleteEventHandler (OnWinDelete);
			this.SetDefaultSize (2 * RADIUS, 2 * RADIUS);
			this.ShowAll ();
		}

		void RotatedTextExposeEvent (object sender, ExposeEventArgs a)
		{
			DrawingArea drawingArea = sender as DrawingArea;

			int width = drawingArea.Allocation.Width;
			int height = drawingArea.Allocation.Height;

			double deviceRadius;

			// Get the default renderer for the screen, and set it up for drawing 
			Gdk.PangoRenderer renderer = Gdk.PangoRenderer.GetDefault (drawingArea.Screen);
			renderer.Drawable = drawingArea.GdkWindow;
			renderer.Gc = drawingArea.Style.BlackGC;

			// Set up a transformation matrix so that the user space coordinates for
			// the centered square where we draw are [-RADIUS, RADIUS], [-RADIUS, RADIUS]
			// We first center, then change the scale
			deviceRadius = Math.Min (width, height) / 2;
			Matrix matrix = Pango.Matrix.Identity;
			matrix.Translate (deviceRadius + (width - 2 * deviceRadius) / 2, deviceRadius + (height - 2 * deviceRadius) / 2);
			matrix.Scale (deviceRadius / RADIUS, deviceRadius / RADIUS);

			// Create a PangoLayout, set the font and text
			Context context = drawingArea.CreatePangoContext ();
			Pango.Layout layout = new Pango.Layout (context);
			layout.SetText ("Text");
			FontDescription desc = FontDescription.FromString ("Sans Bold 27");
			layout.FontDescription = desc;

			// Draw the layout N_WORDS times in a circle
			for (int i = 0; i < N_WORDS; i++)
			{
				Gdk.Color color = new Gdk.Color ();
				Matrix rotatedMatrix = matrix;
				int w, h;
				double angle = (360 * i) / N_WORDS;

				// Gradient from red at angle == 60 to blue at angle == 300
				color.Red = (ushort) (65535 * (1 + Math.Cos ((angle - 60) * Math.PI / 180)) / 2);
				color.Green = 0;
				color.Blue = (ushort) (65535 - color.Red);

				renderer.SetOverrideColor (RenderPart.Foreground, color);

				rotatedMatrix.Rotate (angle);
				context.Matrix = rotatedMatrix;

				// Inform Pango to re-layout the text with the new transformation matrix
				layout.ContextChanged ();
				layout.GetSize (out w, out h);
				renderer.DrawLayout (layout, - w / 2, (int) (- RADIUS * Pango.Scale.PangoScale));
			}

			// Clean up default renderer, since it is shared
			renderer.SetOverrideColor (RenderPart.Foreground, Gdk.Color.Zero);
			renderer.Drawable = null;
			renderer.Gc = null;
		}

		void OnWinDelete (object sender, DeleteEventArgs a)
		{
			this.Hide ();
			this.Dispose ();
		}
	}
}
#endif


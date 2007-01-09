/* Pixbufs
 *
 * A Pixbuf represents an image, normally in RGB or RGBA format.
 * Pixbufs are normally used to load files from disk and perform
 * image scaling.
 *
 * This demo is not all that educational, but looks cool. It was written
 * by Extreme Pixbuf Hacker Federico Mena Quintero. It also shows
 * off how to use DrawingArea to do a simple animation.
 *
 * Look at the Image demo for additional pixbuf usage examples.
 *
 */

using Gdk;
using Gtk;

using System;
using System.Runtime.InteropServices; // for Marshal.Copy

namespace GtkDemo
{
	[Demo ("Pixbuf", "DemoPixbuf.cs")]
	public class DemoPixbuf : Gtk.Window
	{
		const int FrameDelay = 50;
		const int CycleLen = 60;
		const string BackgroundName = "background.jpg";

		static string[] ImageNames = {
			"apple-red.png",
			"gnome-applets.png",
			"gnome-calendar.png",
			"gnome-foot.png",
			"gnome-gmush.png",
			"gnome-gimp.png",
			"gnome-gsame.png",
			"gnu-keys.png"
		};

		// background image
		static Pixbuf background;
		static int backWidth, backHeight;

		// images
		static Pixbuf[] images;

		// current frame
		Pixbuf frame;
		int frameNum;

		// drawing area
		DrawingArea drawingArea;

		uint timeoutId;

		static DemoPixbuf ()
		{
			// Load the images for the demo

			background = Gdk.Pixbuf.LoadFromResource (BackgroundName);

			backWidth = background.Width;
			backHeight = background.Height;

			images = new Pixbuf[ImageNames.Length];

			int i = 0;
			foreach (string im in ImageNames)
				images[i++] = Gdk.Pixbuf.LoadFromResource (im);
		}

		// Expose callback for the drawing area
		void Expose (object o, ExposeEventArgs args)
		{
			Widget widget = (Widget) o;
			Gdk.Rectangle area = args.Event.Area;
			byte[] pixels;
			int rowstride;

			rowstride = frame.Rowstride;
			pixels = new byte[(frame.Height - area.Y) * rowstride];
			IntPtr src = (IntPtr)(frame.Pixels.ToInt64 () + rowstride * area.Y + area.X * 3);
			Marshal.Copy (src, pixels, 0, pixels.Length);

			widget.GdkWindow.DrawRgbImageDithalign (widget.Style.BlackGC,
								area.X, area.Y, area.Width, area.Height,
								Gdk.RgbDither.Normal,
								pixels, rowstride,
								area.X, area.Y);
			args.RetVal = true;
		}

		// timeout handler to regenerate the frame
		bool timeout ()
		{
			background.CopyArea (0, 0, backWidth, backHeight, frame, 0, 0);

			double f = (double) (frameNum % CycleLen) / CycleLen;

			int xmid = backWidth / 2;
			int ymid = backHeight / 2;

			double radius = Math.Min (xmid, ymid) / 2;

			for (int i = 0; i < images.Length; i++) {
				double ang = 2 * Math.PI * (double) i / images.Length - f * 2 * Math.PI;

				int iw = images[i].Width;
				int ih = images[i].Height;

				double r = radius + (radius / 3) * Math.Sin (f * 2 * Math.PI);

				int xpos = (int) Math.Floor (xmid + r * Math.Cos (ang) -
							     iw / 2.0 + 0.5);
				int ypos = (int) Math.Floor (ymid + r * Math.Sin (ang) -
							     ih / 2.0 + 0.5);

				double k = (i % 2 == 1) ? Math.Sin (f * 2 * Math.PI) :
					Math.Cos (f * 2 * Math.PI);
				k = 2 * k * k;
				k = Math.Max (0.25, k);

				Rectangle r1, r2, dest;

				r1 = new Rectangle (xpos, ypos, (int) (iw * k), (int) (ih * k));
				r2 = new Rectangle (0, 0, backWidth, backHeight);

				dest = Rectangle.Intersect (r1, r2);
				if (!dest.IsEmpty) {
					images[i].Composite (frame, dest.X, dest.Y,
							     dest.Width, dest.Height,
							     xpos, ypos, k, k,
							     InterpType.Nearest,
							     (int) ((i % 2 == 1) ?
								    Math.Max (127, Math.Abs (255 * Math.Sin (f * 2 * Math.PI))) :
								    Math.Max (127, Math.Abs (255 * Math.Cos (f * 2 * Math.PI)))));
				}
			}

			drawingArea.QueueDraw ();
			frameNum++;

			return true;
		}

		public DemoPixbuf () : base ("Pixbufs")
		{
			Resizable = false;
			SetSizeRequest (backWidth, backHeight);

			frame = new Pixbuf (Colorspace.Rgb, false, 8, backWidth, backHeight);

			drawingArea = new DrawingArea ();
			drawingArea.ExposeEvent += new ExposeEventHandler (Expose);

			Add (drawingArea);
			timeoutId = GLib.Timeout.Add (FrameDelay, new GLib.TimeoutHandler(timeout));

			ShowAll ();
		}

		protected override void OnDestroyed ()
		{
			if (timeoutId != 0) {
				GLib.Source.Remove (timeoutId);
				timeoutId = 0;
			}
		}

		protected override bool OnDeleteEvent (Gdk.Event evt)
		{
			Destroy ();
			return true;
		}
	}
}

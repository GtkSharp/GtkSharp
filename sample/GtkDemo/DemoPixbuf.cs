// DemoPixbuf.cs: Pixbuf demonstration
//
// Gtk# port of pixbuf demo in gtk-demo app.
//
// Author: Yves Kurz <yves@magnific.ch>
//
// <c> 2003 Yves Kurz

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

namespace GtkDemo 
{

	public class DemoPixbuf : Gtk.Window
{

	const int FrameDelay = 50;
	const int CycleLen = 60;
	const string BackgroundName = "images/background.jpg";
	string [] ImageNames = {
		"images/apple-red.png",
		"images/gnome-applets.png",
		"images/gnome-calendar.png",
		"images/gnome-foot.png",
		"images/gnome-gmush.png",
		"images/gnome-gimp.png",
		"images/gnome-gsame.png",
		"images/gnu-keys.png"
	};
		
	// current frame
	Pixbuf frame;
	int frameNum;
	
	// background image
	Pixbuf background;
	int backWidth, backHeight;

	// images
	Pixbuf[] images;

	// drawing area
	DrawingArea drawingArea;




	string FindFile (string name)
	{
		return name;
	}
	
	// Loads the images for the demo 
	void LoadPixbuf ()
	{
		background = new Pixbuf (FindFile (BackgroundName));

		backWidth = background.Width;
		backHeight = background.Height;

		images = new Pixbuf[ImageNames.Length];
		
		for (int i = 0; i < ImageNames.Length; i++) 
			images[i] = new Pixbuf (FindFile (ImageNames[i]));
	}


	// Expose callback for the drawing area
	void Expose (object o, ExposeEventArgs args)
	{

		        EventExpose ev = args.Event;
			Widget widget = (Widget) o;
			Gdk.Rectangle area = ev.Area;

			frame.RenderToDrawableAlpha(
					widget.GdkWindow,
					0, 0,
					0, 0,
					backWidth, backHeight,
					Gdk.PixbufAlphaMode.Full, 8,
					RgbDither.Normal,
					100, 100);


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
			double ang = 2 * Math.PI * i / images.Length - f * 2 *
				Math.PI;

			int iw = images[i].Width;
			int ih = images[i].Height;

			double r = radius + (radius / 3) * Math.Sin (f * 2 * 
				Math.PI);

			int xpos = (int) Math.Floor (xmid + r * Math.Cos (ang) -
				iw / 2 + 0.5);
			int ypos = (int) Math.Floor (ymid + r * Math.Sin (ang) -
				ih / 2 + 0.5);
			
			double k = (i % 2 == 1) ? Math.Sin (f * 2 * Math.PI) : 
				Math.Cos (f * 2 * Math.PI);
			k = 2 * k * k;
			k = Math.Max (0.25, k);

			Rectangle r1, r2, dest;

			r1 = new Rectangle (xpos, ypos,(int) (iw * k),
				(int) (ih * k));

/* FIXME: Why is that code not working (in the original gtk-demo it works

			r2 = new Rectangle (0, 0, backWidth, backHeight);

			dest = new Rectangle (0, 0, 0, 0);
			r1.Intersect (r2, dest);

			images[i].Composite (frame, dest.x, dest.y, dest.width,
				dest.height, xpos, ypos, k, k,
				InterpType.Nearest, (int) ((i % 2 == 1)
				? Math.Max (127, Math.Abs (255 * Math.Sin (f *
					2 * Math.PI)))
				: Math.Max (127, Math.Abs (255 * Math.Cos (f *
					2 * Math.PI)))));
*/
			images[i].Composite (frame, r1.X, r1.Y, r1.Width,
				r1.Height, xpos, ypos, k, k,
				InterpType.Nearest, (int) ((i % 2 == 1)
				? Math.Max (127, Math.Abs (255 * Math.Sin (f *
					2 * Math.PI)))
				: Math.Max (127, Math.Abs (255 * Math.Cos (f *
					2 * Math.PI)))));
		}

		drawingArea.QueueDraw ();
		frameNum++;
		
		return true;
	}


	private Gtk.Window window;

	public DemoPixbuf () : base ("Gdk Pixbuf Demo")
	{
		//window = new DemoPixbuf ();
		//window.DeleteEvent += new DeleteEventHandler (WindowDelete);

		try {
			LoadPixbuf ();
		} catch (Exception e) {
			MessageDialog md = new MessageDialog (this,
					DialogFlags.DestroyWithParent,
					MessageType.Error,
					ButtonsType.Close,
					"Error: \n" + e.Message);
			md.Run ();

			throw;
		}

		frame = new Pixbuf (Colorspace.Rgb, true, 8, backWidth, 
				backHeight);

		drawingArea = new DrawingArea ();
		drawingArea.ExposeEvent += new ExposeEventHandler (Expose);

		Add (drawingArea);
		GLib.Timeout.Add (FrameDelay, new GLib.TimeoutHandler(timeout)); 

		this.SetDefaultSize (backWidth, backHeight);
//		this.Resizable = false;
		ShowAll ();
	}

	
	static void windowDelete (object obj, DeleteEventArgs args)
	{
		Application.Quit ();
	}


}
}


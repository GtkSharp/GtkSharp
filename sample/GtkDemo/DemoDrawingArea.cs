/* Drawing Area
 *
 * GtkDrawingArea is a blank area where you can draw custom displays
 * of various kinds.
 *
 * This demo has two drawing areas. The checkerboard area shows
 * how you can just draw something; all you have to do is write
 * a signal handler for ExposeEvent, as shown here.
 *
 * The "scribble" area is a bit more advanced, and shows how to handle
 * events such as button presses and mouse motion. Click the mouse
 * and drag in the scribble area to draw squiggles. Resize the window
 * to clear the area.
 */

using System;
using Gtk;
using Gdk;

namespace GtkDemo
{
	[Demo ("Drawing Area", "DemoDrawingArea.cs")]
	public class DemoDrawingArea : Gtk.Window
	{
		private Pixmap pixmap = null;

		public DemoDrawingArea () : base ("Drawing Area")
		{
			BorderWidth = 8;

			VBox vbox = new VBox (false, 8);
			vbox.BorderWidth = 8;
			Add (vbox);

			// Create the checkerboard area
			Label label = new Label ("<u>Checkerboard pattern</u>");
			label.UseMarkup = true;
			vbox.PackStart (label, false, false, 0);

			Frame frame = new Frame ();
			frame.ShadowType = ShadowType.In;
			vbox.PackStart (frame, true, true, 0);

			DrawingArea da = new DrawingArea ();
			// set a minimum size
			da.SetSizeRequest (100,100);
			frame.Add (da);
			da.ExposeEvent += new ExposeEventHandler (CheckerboardExpose);

			// Create the scribble area
			label = new Label ("<u>Scribble area</u>");
			label.UseMarkup = true;
			vbox.PackStart (label, false, false, 0);

			frame = new Frame ();
			frame.ShadowType = ShadowType.In;
			vbox.PackStart (frame, true, true, 0);

			da = new DrawingArea ();
			// set a minimum size
			da.SetSizeRequest (100, 100);
			frame.Add (da);

			// Signals used to handle backing pixmap
			da.ExposeEvent += new ExposeEventHandler (ScribbleExpose);
			da.ConfigureEvent += new ConfigureEventHandler (ScribbleConfigure);

			// Event signals
			da.MotionNotifyEvent += new MotionNotifyEventHandler (ScribbleMotionNotify);
			da.ButtonPressEvent += new ButtonPressEventHandler (ScribbleButtonPress);


			// Ask to receive events the drawing area doesn't normally
			// subscribe to
			da.Events |= EventMask.LeaveNotifyMask | EventMask.ButtonPressMask |
				EventMask.PointerMotionMask | EventMask.PointerMotionHintMask;

			ShowAll ();
		}

		protected override bool OnDeleteEvent (Gdk.Event evt)
		{
			Destroy ();
			return true;
		}

		private void CheckerboardExpose (object o, ExposeEventArgs args)
		{
			const int CheckSize = 10;
			const int Spacing = 2;

			DrawingArea da = o as DrawingArea;

			// It would be a bit more efficient to keep these
			// GC's around instead of recreating on each expose, but
			// this is the lazy/slow way.
			Gdk.GC gc1 = new Gdk.GC (da.GdkWindow);
			gc1.RgbFgColor = new Gdk.Color (117, 0, 117);

			Gdk.GC gc2 = new Gdk.GC (da.GdkWindow);
			gc2.RgbFgColor = new Gdk.Color (255, 255, 255);

			int i, j, xcount, ycount;
			Gdk.Rectangle alloc = da.Allocation;

			// Start redrawing the Checkerboard
			xcount = 0;
			i = Spacing;
			while (i < alloc.Width) {
				j = Spacing;
				ycount = xcount % 2; // start with even/odd depending on row
				while (j < alloc.Height) {
					Gdk.GC gc;
					if (ycount % 2 != 0)
						gc = gc1;
					else
						gc = gc2;
					da.GdkWindow.DrawRectangle (gc, true, i, j,
								    CheckSize, CheckSize);

					j += CheckSize + Spacing;
					++ycount;
				}
				i += CheckSize + Spacing;
				++xcount;
			}

			// return true because we've handled this event, so no
			// further processing is required.
			args.RetVal = true;
		}

		private void ScribbleExpose (object o, ExposeEventArgs args)
		{
			Widget widget = o as Widget;
			Gdk.Window window = widget.GdkWindow;
 			Rectangle area = args.Event.Area;

			// We use the "ForegroundGC" for the widget since it already exists,
			// but honestly any GC would work. The only thing to worry about
			// is whether the GC has an inappropriate clip region set.
			window.DrawDrawable (widget.Style.ForegroundGC (StateType.Normal),
					     pixmap,
					     area.X, area.Y,
					     area.X, area.Y,
					     area.Width, area.Height);
		}

		// Create a new pixmap of the appropriate size to store our scribbles
		private void ScribbleConfigure (object o, ConfigureEventArgs args)
		{
			Widget widget = o as Widget;
			Rectangle allocation = widget.Allocation;

			pixmap = new Pixmap (widget.GdkWindow, allocation.Width, allocation.Height, -1);

			// Initialize the pixmap to white
			pixmap.DrawRectangle (widget.Style.WhiteGC, true, 0, 0,
					      allocation.Width, allocation.Height);

			// We've handled the configure event, no need for further processing.
			args.RetVal = true;
		}

		private void ScribbleMotionNotify (object o, MotionNotifyEventArgs args)
		{

			// paranoia check, in case we haven't gotten a configure event
			if (pixmap == null)
				return;

			// This call is very important; it requests the next motion event.
			// If you don't call Window.GetPointer() you'll only get a single
			// motion event. The reason is that we specified PointerMotionHintMask
			// in widget.Events. If we hadn't specified that, we could just use
			// args.Event.X, args.Event.Y as the pointer location. But we'd also
			// get deluged in events. By requesting the next event as we handle
			// the current one, we avoid getting a huge number of events faster
			// than we can cope.

			int x, y;
			ModifierType state;
			args.Event.Window.GetPointer (out x, out y, out state);

			if ((state & ModifierType.Button1Mask) != 0)
				DrawBrush (o as Widget, x, y);

			// We've handled it, stop processing
			args.RetVal = true;
		}

		// Draw a rectangle on the screen
		private void DrawBrush (Widget widget, double x, double y)
		{
			Rectangle update_rect = new Rectangle ((int)x - 3, (int)y - 3, 6, 6);

			// Paint to the pixmap, where we store our state
			pixmap.DrawRectangle (widget.Style.BlackGC, true,
					      update_rect.X, update_rect.Y,
					      update_rect.Width, update_rect.Height);
			widget.GdkWindow.InvalidateRect (update_rect, false);
		}

		private void ScribbleButtonPress (object o, ButtonPressEventArgs args)
		{
			// paranoia check, in case we haven't gotten a configure event
			if (pixmap == null)
				return;

			EventButton ev = args.Event;
			if (ev.Button == 1)
				DrawBrush (o as Widget, ev.X, ev.Y);

			// We've handled the event, stop processing
			args.RetVal = true;
		}
	}
}

/* Drawing Area
 *
 * GtkDrawingArea is a blank area where you can draw custom displays
 * of various kinds.
 *
 * This demo has two drawing areas. The checkerboard area shows
 * how you can just draw something; all you have to do is write
 * a signal handler for the Drawn event, as shown here.
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
		private Cairo.Surface surface = null;

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
			da.Drawn += new DrawnHandler (CheckerboardDrawn);

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
			da.Drawn += new DrawnHandler (ScribbleDrawn);
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

		private void CheckerboardDrawn (object o, DrawnArgs args)
		{
			const int CheckSize = 10;
			const int Spacing = 2;

			Widget widget = o as Widget;
			Cairo.Context cr = args.Cr;

			int i, j, xcount, ycount;

			// At the start of a draw handler, a clip region has been set on
			// the Cairo context, and the contents have been cleared to the
			// widget's background color.
			
			Rectangle alloc = widget.Allocation;
			// Start redrawing the Checkerboard
			xcount = 0;
			i = Spacing;
			while (i < alloc.Width) {
				j = Spacing;
				ycount = xcount % 2; // start with even/odd depending on row
				while (j < alloc.Height) {
					if (ycount % 2 != 0)
						cr.SetSourceRGB (0.45777, 0, 0.45777);
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

			// return true because we've handled this event, so no
			// further processing is required.
			args.RetVal = true;
		}

		private void ScribbleDrawn (object o, DrawnArgs args)
		{
			Cairo.Context cr = args.Cr;
			
			cr.SetSourceSurface (surface, 0, 0);
			cr.Paint ();
		}

		// Create a new surface of the appropriate size to store our scribbles
		private void ScribbleConfigure (object o, ConfigureEventArgs args)
		{
			Widget widget = o as Widget;
			
			if (surface != null)
				surface.Dispose ();

			var allocation = widget.Allocation;

			surface = widget.Window.CreateSimilarSurface (Cairo.Content.Color, allocation.Width, allocation.Height);
			var cr = new Cairo.Context (surface);
			
			cr.SetSourceRGB (1, 1, 1);
			cr.Paint ();
			((IDisposable)cr).Dispose ();

			// We've handled the configure event, no need for further processing.
			args.RetVal = true;
		}

		private void ScribbleMotionNotify (object o, MotionNotifyEventArgs args)
		{

			// paranoia check, in case we haven't gotten a configure event
			if (surface == null)
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
			var update_rect = new Gdk.Rectangle ((int)x - 3, (int)y - 3, 6, 6);
			var cr = new Cairo.Context (surface);
			
			Gdk.CairoHelper.Rectangle (cr, update_rect);
			cr.Fill ();

			((IDisposable)cr).Dispose ();
			
			widget.Window.InvalidateRect (update_rect, false);
		}

		private void ScribbleButtonPress (object o, ButtonPressEventArgs args)
		{
			// paranoia check, in case we haven't gotten a configure event
			if (surface == null)
				return;

			EventButton ev = args.Event;
			if (ev.Button == 1)
				DrawBrush (o as Widget, ev.X, ev.Y);

			// We've handled the event, stop processing
			args.RetVal = true;
		}
	}
}

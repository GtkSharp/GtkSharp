//
// DemoDrawingArea.cs, port of drawingarea.c from gtk-demo
//
// Author: Daniel Kornhauser <dkor@alum.mit.edu>
//         Rachel Hestilow <hestilow@ximian.com> 
//
// Copyright (C) 2003, Ximian Inc.
//

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
		private static Pixmap pixmap = null;
		private static DrawingArea drawingArea;
		private static DrawingArea drawingArea1;

		public DemoDrawingArea () : base ("Drawing Area")
		{
			this.DeleteEvent += new DeleteEventHandler (WindowDelete);
			this.BorderWidth = 8;
		
			VBox vbox = new VBox (false, 8);
			vbox.BorderWidth = 8;
			this.Add (vbox);
			
			// Create the checkerboard area
			Label label = new Label ();
			label.Markup =  "<u>Checkerboard pattern</u>";
			vbox.PackStart (label, false, false, 0);
			
			Frame frame = new Frame ();
			frame.ShadowType = ShadowType.In;
			vbox.PackStart (frame, true, true, 0);

			drawingArea = new DrawingArea ();
			// set a minimum size
			drawingArea.SetSizeRequest (100,100);
			frame.Add (drawingArea);
			drawingArea.ExposeEvent += new ExposeEventHandler (CheckerboardExpose);

			// Create the scribble area
			Label label1 = new Label ("<u>Scribble area</u>");
			label1.UseMarkup = true;
			vbox.PackStart (label1, false, false, 0);
			
			Frame frame1 = new Frame ();
			frame1.ShadowType = ShadowType.In;
			vbox.PackStart (frame1, true, true, 0);

			drawingArea1 = new DrawingArea ();
			// set a minimun size
			drawingArea1.SetSizeRequest (100, 100);
			frame1.Add (drawingArea1);
			// Signals used to handle backing pixmap
			drawingArea1.ExposeEvent += new ExposeEventHandler (ScribbleExpose);
			drawingArea1.ConfigureEvent += new ConfigureEventHandler (ScribbleConfigure);
			// Event signals
			drawingArea1.MotionNotifyEvent += new MotionNotifyEventHandler (ScribbleMotionNotify);
			drawingArea1.ButtonPressEvent += new ButtonPressEventHandler (ScribbleButtonPress);

			// Ask to receive events the drawing area doesn't normally
			// subscribe to

			drawingArea1.Events = EventMask.LeaveNotifyMask | EventMask.ButtonPressMask |
			                      EventMask.PointerMotionMask | EventMask.PointerMotionHintMask;
			
			this.ShowAll ();
		}
			
		private void WindowDelete (object o, DeleteEventArgs args)
		{
			this.Hide ();
			this.Destroy ();
			args.RetVal = true;
		}
		
		private void CheckerboardExpose (object o, ExposeEventArgs args)
		{			

			// Defining the size of the Checks
			const int CheckSize = 10;
			const int Spacing = 2;

			// Defining the color of the Checks
			int i, j, xcount, ycount;
			Gdk.GC gc, gc1, gc2;
			Gdk.Color color = new Gdk.Color (); 
			EventExpose eventExpose = args.Event;
			Gdk.Window window = eventExpose.Window;
			gc1 = new Gdk.GC (window);

			color.Red = 30000;
			color.Green = 0;
			color.Blue = 30000;
			gc1.RgbFgColor = color;

			gc2 = new Gdk.GC (window);
			color.Red = 65535;
			color.Green = 65535;
			color.Blue = 65535;
			gc2.RgbFgColor = color;

			// Start redrawing the Checkerboard			
			xcount = 0;
			i = Spacing;
			while (i < drawingArea.Allocation.Width){
				j = Spacing;
				ycount = xcount % 2; //start with even/odd depending on row
				while (j < drawingArea.Allocation.Height){
					gc = new Gdk.GC (window);
					if (ycount % 2 != 0){
						gc = gc1;}
					else{
						gc = gc2;}
					window.DrawRectangle(gc, true, i, j, CheckSize, CheckSize);
					
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
			// We use the "ForegroundGC" for the widget since it already exists,
			// but honestly any GC would work. The only thing to worry about
			// is whether the GC has an inappropriate clip region set.

			EventExpose eventExpose = args.Event;
			Gdk.Window window = eventExpose.Window;
 			Rectangle area = eventExpose.Area;
			
			window.DrawDrawable (drawingArea1.Style.ForegroundGC(StateType.Normal),
					pixmap,
					area.X, area.Y,
					area.X, area.Y,
					area.Width, area.Height);
			args.RetVal = false;
		}
		
		// Create a new pixmap of the appropriate size to store our scribbles 
		private void ScribbleConfigure (object o, ConfigureEventArgs args)
		{
			EventConfigure eventConfigure = args.Event;
			Gdk.Window window = eventConfigure.Window;
			Rectangle allocation = drawingArea1.Allocation;
			pixmap = new Pixmap (window,
					allocation.Width,
					allocation.Height,
					-1);  	
			// Initialize the pixmap to white
			pixmap.DrawRectangle (drawingArea1.Style.WhiteGC, true, 0, 0,
					allocation.Width, allocation.Height);
			// We've handled the configure event, no need for further processing.
			args.RetVal = true;
		}
		
		private void ScribbleMotionNotify (object o, MotionNotifyEventArgs args)
		{

			// This call is very important; it requests the next motion event.
			// If you don't call Window.GetPointer() you'll only get
			// a single motion event. The reason is that we specified
			// PointerMotionHintMask in drawingArea1.Events.
			// If we hadn't specified that, we could just use ExposeEvent.x, ExposeEvent.y
			// as the pointer location. But we'd also get deluged in events.
			// By requesting the next event as we handle the current one,
			// we avoid getting a huge number of events faster than we
			// can cope.

			int x, y;
			ModifierType state;
			EventMotion ev = args.Event;
			Gdk.Window window = ev.Window;

			if (ev.IsHint) {
				ModifierType s;
				window.GetPointer (out x, out y, out s);
				state = s;
			} else {
				x = (int) ev.X;
				y = (int) ev.Y;
				state = (ModifierType) ev.State;
			}

			if ((state & ModifierType.Button1Mask) != 0 && pixmap != null)
				DrawBrush (x, y);
			/* We've handled it, stop processing */
			args.RetVal = true;
		}

		// Draw a rectangle on the screen
		static void DrawBrush (double x, double y)
		{
			Rectangle update_rect = new Rectangle ();
			update_rect.X = (int) x - 3;
			update_rect.Y = (int) y - 3;
			update_rect.Width = 6;
			update_rect.Height = 6;

			//Paint to the pixmap, where we store our state
			pixmap.DrawRectangle (drawingArea1.Style.BlackGC, true,
					update_rect.X, update_rect.Y,
					update_rect.Width, update_rect.Height);
			drawingArea1.QueueDrawArea (update_rect.X, update_rect.Y,
					update_rect.Width, update_rect.Height);
		}
	
		private void ScribbleButtonPress (object o, ButtonPressEventArgs args)
		{
			EventButton ev = args.Event;
			if (ev.Button == 1 && pixmap != null)
				DrawBrush (ev.X, ev.Y);
			//We've handled the event, stop processing
			args.RetVal = true;
		}
	}
}

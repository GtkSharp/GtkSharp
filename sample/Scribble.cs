// Scribble.cs - port of Gtk+ scribble demo 
//
// Author: Rachel Hestilow <hestilow@ximian.com> 
//
// (c) 2002 Rachel Hestilow

namespace GtkSamples {

	using Gtk;
	using Gdk;
	using GtkSharp;
	using System;

	public class Scribble {
		private static Gtk.DrawingArea darea;
		private static Gdk.Pixmap pixmap = null;

		public static int Main (string[] args)
		{
			Application.Init ();
			Gtk.Window win = new Gtk.Window ("Scribble demo");
			win.DeleteEvent += new DeleteEventHandler (Window_Delete);

			darea = new Gtk.DrawingArea ();
			darea.SetSizeRequest (200, 200);
			win.Add (darea);
			
			darea.ExposeEvent += new ExposeEventHandler (ExposeEvent);
			darea.ConfigureEvent += new ConfigureEventHandler (ConfigureEvent);
			darea.MotionNotifyEvent += new MotionNotifyEventHandler (MotionNotifyEvent);
			darea.ButtonPressEvent += new ButtonPressEventHandler (ButtonPressEvent);
			darea.Events = (int)EventMask.ExposureMask |
							   (int)EventMask.LeaveNotifyMask |
							   (int)EventMask.ButtonPressMask |
							   (int)EventMask.PointerMotionMask |
							   (int)EventMask.PointerMotionHintMask;

			win.ShowAll ();
			Application.Run ();
			return 0;
		}

		static void Window_Delete (object obj, DeleteEventArgs args)
		{
			SignalArgs sa = (SignalArgs) args;
			Application.Quit ();
			sa.RetVal = true;
		}

		static void ExposeEvent (object obj, ExposeEventArgs args)
		{
			Gdk.EventExpose ev = args.Event;
			Gdk.Window window = ev.window;
			// FIXME: mcs bug
			Gdk.Rectangle area = ev.area;
			// FIXME: array marshalling not done yet so no FG */
			window.DrawDrawable (darea.Style.BlackGC,
									   pixmap,
									   area.x, area.y,
									   area.x, area.y,
									   area.width, area.height);

			SignalArgs sa = (SignalArgs) args;
			sa.RetVal = false;
		}
		
		static void ConfigureEvent (object obj, ConfigureEventArgs args)
		{
			Gdk.EventConfigure ev = args.Event;
			Gdk.Window window = ev.window;
			Gdk.Rectangle allocation = darea.Allocation;

		Console.WriteLine ("Darea=[{0}]" , darea);
			pixmap = new Gdk.Pixmap (window,
									       allocation.width,
									       allocation.height,
											 -1);
			Console.WriteLine ("Darea.Style={0}", darea.Style);
			pixmap.DrawRectangle (darea.Style.WhiteGC, 1, 0, 0,
									    allocation.width, allocation.height);

			SignalArgs sa = (SignalArgs) args;
			sa.RetVal = true;
		}

		static void DrawBrush (double x, double y)
		{
			Gdk.Rectangle update_rect = new Gdk.Rectangle ();
			update_rect.x = (int) x - 5;
			update_rect.y = (int) y - 5;
			update_rect.width = 10;
			update_rect.height = 10;
			
			pixmap.DrawRectangle (darea.Style.BlackGC, 1,
										 update_rect.x, update_rect.y,
										 update_rect.width, update_rect.height);
			darea.QueueDrawArea (update_rect.x, update_rect.y,
										update_rect.width, update_rect.height);
		}
	
		static void ButtonPressEvent (object obj, ButtonPressEventArgs args)
		{
			Gdk.EventButton ev = args.Event;
			if (ev.button == 1 && pixmap != null)
				DrawBrush (ev.x, ev.y);

			SignalArgs sa = (SignalArgs) args;
			sa.RetVal = true;
		}
		
		static void MotionNotifyEvent (object obj, MotionNotifyEventArgs args)
		{
			int x, y;
			Gdk.ModifierType state;
			Gdk.EventMotion ev = args.Event;
			Gdk.Window window = ev.window;

			if (ev.is_hint != 0) {
				int s;
				window.GetPointer (out x, out y, out s);
				state = (Gdk.ModifierType) s;
			} else {
				x = (int) ev.x;
				y = (int) ev.y;
				state = (Gdk.ModifierType) ev.state;
			}

			if ((state & Gdk.ModifierType.Button1Mask) != 0 && pixmap != null)
				DrawBrush (x, y);

			SignalArgs sa = (SignalArgs) args;
			sa.RetVal = true;
		}
	}
}


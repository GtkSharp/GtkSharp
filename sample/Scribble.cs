// Scribble.cs - port of Gtk+ scribble demo 
//
// Author: Rachel Hestilow <hestilow@ximian.com> 
//
// (c) 2002 Rachel Hestilow

namespace GtkSamples {

	using Gtk;
	using Gdk;
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
			darea.Events = EventMask.ExposureMask | EventMask.LeaveNotifyMask |
				       EventMask.ButtonPressMask | EventMask.PointerMotionMask |
				       EventMask.PointerMotionHintMask;

			win.ShowAll ();
			Application.Run ();
			return 0;
		}

		static void Window_Delete (object obj, DeleteEventArgs args)
		{
			Application.Quit ();
			args.RetVal = true;
		}

		static void ExposeEvent (object obj, ExposeEventArgs args)
		{
			Gdk.Rectangle area = args.Event.Area;
			args.Event.Window.DrawDrawable (darea.Style.BlackGC,
							pixmap,
							area.X, area.Y,
							area.X, area.Y,
							area.Width, area.Height);

			args.RetVal = false;
		}
		
		static void ConfigureEvent (object obj, ConfigureEventArgs args)
		{
			Gdk.EventConfigure ev = args.Event;
			Gdk.Window window = ev.Window;
			Gdk.Rectangle allocation = darea.Allocation;

			pixmap = new Gdk.Pixmap (window, allocation.Width, allocation.Height, -1);
			pixmap.DrawRectangle (darea.Style.WhiteGC, true, 0, 0,
					      allocation.Width, allocation.Height);

			args.RetVal = true;
		}

		static void DrawBrush (double x, double y, bool black)
		{
			Gdk.Rectangle update_rect = new Gdk.Rectangle ();
			update_rect.X = (int) x - 5;
			update_rect.Y = (int) y - 5;
			update_rect.Width = 10;
			update_rect.Height = 10;
			
			pixmap.DrawRectangle (black ? darea.Style.BlackGC : darea.Style.WhiteGC, true,
										 update_rect.X, update_rect.Y,
										 update_rect.Width, update_rect.Height);
			darea.QueueDrawArea (update_rect.X, update_rect.Y,
										update_rect.Width, update_rect.Height);
		}
	
		static void ButtonPressEvent (object obj, ButtonPressEventArgs args)
		{
			Gdk.EventButton ev = args.Event;
			if (ev.Button == 1 && pixmap != null)
				DrawBrush (ev.X, ev.Y, true);
			else if (ev.Button == 3 && pixmap != null)
				DrawBrush (ev.X, ev.Y, false);
			args.RetVal = true;
		}
		
		static void MotionNotifyEvent (object obj, MotionNotifyEventArgs args)
		{
			int x, y;
			Gdk.ModifierType state;
			Gdk.EventMotion ev = args.Event;
			Gdk.Window window = ev.Window;

			if (ev.IsHint) {
				Gdk.ModifierType s;
				window.GetPointer (out x, out y, out s);
				state = s;
			} else {
				x = (int) ev.X;
				y = (int) ev.Y;
				state = ev.State;
			}

			if ((state & Gdk.ModifierType.Button1Mask) != 0 && pixmap != null)
				DrawBrush (x, y, true);
			else if ((state & Gdk.ModifierType.Button3Mask) != 0 && pixmap != null)
				DrawBrush (x, y, false);

			args.RetVal = true;
		}
	}
}


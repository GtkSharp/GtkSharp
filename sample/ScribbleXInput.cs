// ScribbleXInput.cs - port of Gtk+ scribble demo 
//
// Author: Manuel V. Santos <mvsl@telefonica.net> 
//
// (c) 2002 Rachel Hestilow
// (c) 2004 Manuel V. Santos

namespace GtkSamples {

	using Gtk;
	using Gdk;
	using System;

	public class ScribbleXInput {
		private static Gtk.Window win;
		private static Gtk.VBox vBox;
		private static Gtk.Button inputButton;
		private static Gtk.Button quitButton;
		private static Gtk.DrawingArea darea;
		private static Gdk.Pixmap pixmap = null;
		private static Gtk.InputDialog inputDialog = null;

		public static int Main (string[] args) {
			Application.Init ();
			win = new Gtk.Window ("Scribble XInput Demo");
			win.DeleteEvent += new DeleteEventHandler (WindowDelete);

			vBox = new VBox (false, 0);
			win.Add (vBox);

			darea = new Gtk.DrawingArea ();
			darea.SetSizeRequest (200, 200);
			darea.ExtensionEvents=ExtensionMode.Cursor;
			vBox.PackStart (darea, true, true, 0);
			
			darea.ExposeEvent += new ExposeEventHandler (ExposeEvent);
			darea.ConfigureEvent += new ConfigureEventHandler (ConfigureEvent);
			darea.MotionNotifyEvent += new MotionNotifyEventHandler (MotionNotifyEvent);
			darea.ButtonPressEvent += new ButtonPressEventHandler (ButtonPressEvent);
			darea.Events = EventMask.ExposureMask | EventMask.LeaveNotifyMask |
				       EventMask.ButtonPressMask | EventMask.PointerMotionMask;

			inputButton = new Button("Input Dialog");
			vBox.PackStart (inputButton, false, false, 0);

			inputButton.Clicked += new EventHandler (InputButtonClicked);

			quitButton = new Button("Quit");
			vBox.PackStart (quitButton, false, false, 0);

			quitButton.Clicked += new EventHandler (QuitButtonClicked);
			
			win.ShowAll ();
			Application.Run ();
			return 0;
		}

		static void InputButtonClicked (object obj, EventArgs args) {
			if (inputDialog == null) {
				inputDialog = new InputDialog ();
				inputDialog.SaveButton.Hide ();
				inputDialog.CloseButton.Clicked += new EventHandler(InputDialogClose);
				inputDialog.DeleteEvent += new DeleteEventHandler(InputDialogDelete);
			}
			inputDialog.Present ();
		}

		static void QuitButtonClicked (object obj, EventArgs args) {
			Application.Quit ();
		}

		static void WindowDelete (object obj, DeleteEventArgs args) {
			Application.Quit ();
			args.RetVal = true;
		}

		static void InputDialogClose (object obj, EventArgs args) {
			inputDialog.Hide ();
		}

		static void InputDialogDelete (object obj, DeleteEventArgs args) {
			inputDialog.Hide ();
			args.RetVal = true;
		}

		static void ExposeEvent (object obj, ExposeEventArgs args) {
			Gdk.Rectangle area = args.Event.Area;
			args.Event.Window.DrawDrawable (darea.Style.ForegroundGC(darea.State),
							pixmap,
							area.X, area.Y,
							area.X, area.Y,
							area.Width, area.Height);

			args.RetVal = false;
		}
		
		static void ConfigureEvent (object obj, ConfigureEventArgs args) {
			Gdk.EventConfigure ev = args.Event;
			Gdk.Window window = ev.Window;
			Gdk.Rectangle allocation = darea.Allocation;

			pixmap = new Gdk.Pixmap (window, allocation.Width, allocation.Height, -1);
			pixmap.DrawRectangle (darea.Style.WhiteGC, true, 0, 0,
					      allocation.Width, allocation.Height);

			args.RetVal = true;
		}

		static void DrawBrush (Widget widget, InputSource source, 
					double x, double y, double pressure) {
			Gdk.GC gc;
			switch (source) {
				case InputSource.Mouse:
					gc = widget.Style.BlackGC;
					break;
				case InputSource.Pen:
					gc = widget.Style.BlackGC;
					break;
				case InputSource.Eraser:
					gc = widget.Style.WhiteGC;
					break;
				default:
					gc = widget.Style.BlackGC;
					break;
    			}

			Gdk.Rectangle update_rect = new Gdk.Rectangle ();
			update_rect.X = (int) (x - 10.0d * pressure);
			update_rect.Y = (int) (y - 10.0d * pressure);
			update_rect.Width = (int) (20.0d * pressure);
			update_rect.Height = (int) (20.0d * pressure);
			
			pixmap.DrawRectangle (gc, true, 
						update_rect.X, update_rect.Y,
						update_rect.Width, update_rect.Height);
			darea.QueueDrawArea (update_rect.X, update_rect.Y,
						update_rect.Width, update_rect.Height);
			
		}
	
		static void ButtonPressEvent (object obj, ButtonPressEventArgs args) {
			Gdk.EventButton ev = args.Event;

			if (ev.Button == 1 && pixmap != null) {
   				double pressure;
				ev.Device.GetAxis (ev.Axes, AxisUse.Pressure, out pressure);
 				DrawBrush ((Widget) obj, ev.Device.Source, ev.X, ev.Y, pressure);
			}
			args.RetVal = true;
		}
		
		static void MotionNotifyEvent (object obj, MotionNotifyEventArgs args) {
			Gdk.EventMotion ev = args.Event;
			Widget widget = (Widget) obj;
			if ((ev.State & Gdk.ModifierType.Button1Mask) != 0 && pixmap != null) {
   				double pressure;
				if (!ev.Device.GetAxis (ev.Axes, AxisUse.Pressure, out pressure)) {
					pressure = 0.5;
				}
 				DrawBrush (widget, ev.Device.Source, ev.X, ev.Y, pressure);
			}
			args.RetVal = true;
		}
	}
}


// GTK.Application.cs - GTK Main Event Loop class implementation
//
// Author: Mike Kestner <mkestner@speakeasy.net>
//
// (c) 2001 Mike Kestner

namespace Gtk {

	using System;
	using System.Runtime.InteropServices;
	using Gdk;

	public class Application {

		//
		// Disables creation of instances.
		//
		private Application ()
		{
		}
		
		[DllImport("libgtk-win32-2.0-0.dll")]
		static extern void gtk_init (int argc, IntPtr argv);

		public static void Init ()
		{
			gtk_init (0, new IntPtr(0));
		}

		[DllImport("libgtk-win32-2.0-0.dll")]
		static extern void gtk_init (ref int argc, ref String[] argv);
		[DllImport("libgtk-win32-2.0-0.dll")]
		static extern bool gtk_init_check (ref int argc, ref String[] argv);

		public static void Init (ref string[] args)
		{
			int argc = args.Length;
			gtk_init (ref argc, ref args);
		}

		public static bool InitCheck (ref string[] args)
		{
			int argc = args.Length;
			return gtk_init_check (ref argc, ref args);
		}

		[DllImport("libgtk-win32-2.0-0.dll")]
		static extern void gtk_main ();

		public static void Run ()
		{
			gtk_main ();
		}

		[DllImport("libgtk-win32-2.0-0.dll")]
		static extern bool gtk_events_pending ();


		public static bool EventsPending ()
		{
			return gtk_events_pending ();
		}

		[DllImport("libgtk-win32-2.0-0.dll")]
		static extern void gtk_main_iteration ();

		[DllImport("libgtk-win32-2.0-0.dll")]
		static extern bool gtk_main_iteration_do (bool blocking);

		public static void RunIteration ()
		{
			gtk_main_iteration ();
		}

		public static bool RunIteration (bool blocking)
		{
			return gtk_main_iteration_do (blocking);
		}
		
		[DllImport("libgtk-win32-2.0-0.dll")]
		static extern void gtk_main_quit ();

		public static void Quit ()
		{
			gtk_main_quit ();
		}


		[DllImport("libgtk-win32-2.0-0.dll")]
		static extern IntPtr gtk_get_current_event ();

		public static object CurrentEvent {
			get {
				IntPtr handle = gtk_get_current_event ();
				Gdk.EventType type;

				type = (Gdk.EventType) Marshal.ReadInt32 (handle);
				switch (type){
				case EventType.Delete:
				case EventType.Destroy:
					// Fixme: do not know what this maps to.
					break;
					
				case EventType.Expose:
					return Marshal.PtrToStructure (handle, typeof (Gdk.EventExpose));
					
				case EventType.MotionNotify:
					return Marshal.PtrToStructure (handle, typeof (Gdk.EventMotion));
					
				case EventType.ButtonPress:
				case EventType.TwoButtonPress:
				case EventType.ThreeButtonPress:
				case EventType.ButtonRelease:
					return Marshal.PtrToStructure (handle, typeof (Gdk.EventButton));
					
				case EventType.KeyPress:
				case EventType.KeyRelease:
					return Marshal.PtrToStructure (handle, typeof (Gdk.EventKey));
					
				case EventType.EnterNotify:
				case EventType.LeaveNotify:
					// FIXME: Do not know what this maps to.
					break;
					
				case EventType.FocusChange:
					return Marshal.PtrToStructure (handle, typeof (Gdk.EventFocus));
					
				case EventType.Configure:
					return Marshal.PtrToStructure (handle, typeof (Gdk.EventConfigure));
					
				case EventType.Map:
				case EventType.Unmap:
					// FIXME: Do not know what this maps to.
					break;
					
				case EventType.PropertyNotify:
					return Marshal.PtrToStructure (handle, typeof (Gdk.EventProperty));
					
				case EventType.SelectionClear:
				case EventType.SelectionRequest:
				case EventType.SelectionNotify:
					return Marshal.PtrToStructure (handle, typeof (Gdk.EventSelection));
					      
				case EventType.ProximityIn:
				case EventType.ProximityOut:
					return Marshal.PtrToStructure (handle, typeof (Gdk.EventProximity));
					      
				case EventType.DragEnter:
				case EventType.DragLeave:
				case EventType.DragMotion:
				case EventType.DragStatus:
				case EventType.DropFinished:
					return Marshal.PtrToStructure (handle, typeof (Gdk.EventDND));
					
				case EventType.ClientEvent:
					return Marshal.PtrToStructure (handle, typeof (Gdk.EventClient));
					
				case EventType.VisibilityNotify:
					return Marshal.PtrToStructure (handle, typeof (Gdk.EventVisibility));
					
				case EventType.NoExpose:
					return Marshal.PtrToStructure (handle, typeof (Gdk.EventNoExpose));
					
				case EventType.Scroll:
					return Marshal.PtrToStructure (handle, typeof (Gdk.EventScroll));
					
				case EventType.WindowState:
					return Marshal.PtrToStructure (handle, typeof (Gdk.EventWindowState));
					
				case EventType.Setting:
					return Marshal.PtrToStructure (handle, typeof (Gdk.EventSetting));
				}
				return null;
			}
		}
	}
}

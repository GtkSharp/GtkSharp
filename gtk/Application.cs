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
		static extern void gtk_init (ref int argc, ref IntPtr argv);

		[DllImport("libgtk-win32-2.0-0.dll")]
		static extern bool gtk_init_check (ref int argc, ref IntPtr argv);

		public static void Init ()
		{
			IntPtr argv = new IntPtr(0);
			int argc = 0;

			gtk_init (ref argc, ref argv);
		}

		static bool do_init (string progname, ref string[] args, bool check)
		{
			bool res = false;
			string[] progargs = new string[args.Length + 1];

			progargs[0] = progname;
			args.CopyTo (progargs, 1);

			IntPtr buf = GLibSharp.Marshaller.ArgvToArrayPtr (progargs);
			int argc = progargs.Length;

			if (check)
				res = gtk_init_check (ref argc, ref buf);
			else
				gtk_init (ref argc, ref buf);

			// copy back the resulting argv, minus argv[0], which we're
			// not interested in.

			if (argc == 0)
				args = new string[0];
			else {
				progargs = GLibSharp.Marshaller.ArrayPtrToArgv (buf, argc);
				args = new string[argc - 1];
				Array.Copy (progargs, 1, args, 0, argc - 1);
			}

			return res;
		}

		public static void Init (string progname, ref string[] args)
		{
			do_init (progname, ref args, false);
		}

		public static bool InitCheck (string progname, ref string[] args)
		{
			return do_init (progname, ref args, true);
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

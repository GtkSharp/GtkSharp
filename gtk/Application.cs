// GTK.Application.cs - GTK Main Event Loop class implementation
//
// Author: Mike Kestner <mkestner@speakeasy.net>
//
// (c) 2001 Mike Kestner

namespace Gtk {

	using System;
	using System.Runtime.InteropServices;

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
	}
}

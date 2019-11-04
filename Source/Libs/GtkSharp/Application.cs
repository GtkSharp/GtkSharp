// GTK.Application.cs - GTK Main Event Loop class implementation
//
// Author: Mike Kestner <mkestner@speakeasy.net>
//
// Copyright (c) 2001 Mike Kestner
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of version 2 of the Lesser GNU General 
// Public License as published by the Free Software Foundation.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public
// License along with this program; if not, write to the
// Free Software Foundation, Inc., 59 Temple Place - Suite 330,
// Boston, MA 02111-1307, USA.

namespace Gtk {

	using System;
	using System.Reflection;
	using System.Runtime.InteropServices;
	using System.Threading;
	using Gdk;

	public partial class Application {

		const int WS_EX_TOOLWINDOW = 0x00000080;
		const int WS_OVERLAPPEDWINDOW = 0x00CF0000;

        static Application ()
		{
			if (!GLib.Thread.Supported)
				GLib.Thread.Init ();
		}
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_gtk_init(ref int argc, ref IntPtr argv);
		static d_gtk_init gtk_init = FuncLoader.LoadFunction<d_gtk_init>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_init"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate bool d_gtk_init_check(ref int argc, ref IntPtr argv);
		static d_gtk_init_check gtk_init_check = FuncLoader.LoadFunction<d_gtk_init_check>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_init_check"));

		static void SetPrgname ()
		{
			var args = Environment.GetCommandLineArgs ();
			if (args != null && args.Length > 0)
				GLib.Global.ProgramName = System.IO.Path.GetFileNameWithoutExtension (args [0]);
		}

		public static void Init ()
		{
			SetPrgname ();
			IntPtr argv = new IntPtr(0);
			int argc = 0;

			gtk_init (ref argc, ref argv);

			SynchronizationContext.SetSynchronizationContext (new GLib.GLibSynchronizationContext ());
		}

		static bool do_init (string progname, ref string[] args, bool check)
		{
			SetPrgname ();
			bool res = false;
			string[] progargs = new string[args.Length + 1];

			progargs[0] = progname;
			args.CopyTo (progargs, 1);

			GLib.Argv argv = new GLib.Argv (progargs);
			IntPtr buf = argv.Handle;
			int argc = progargs.Length;

			if (check)
				res = gtk_init_check (ref argc, ref buf);
			else
				gtk_init (ref argc, ref buf);

			if (buf != argv.Handle)
				throw new Exception ("init returned new argv handle");

			// copy back the resulting argv, minus argv[0], which we're
			// not interested in.

			if (argc <= 1)
				args = new string[0];
			else {
				progargs = argv.GetArgs (argc);
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
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_gtk_main();
		static d_gtk_main gtk_main = FuncLoader.LoadFunction<d_gtk_main>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_main"));

		public static void Run ()
		{
			gtk_main ();
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate bool d_gtk_events_pending();
		static d_gtk_events_pending gtk_events_pending = FuncLoader.LoadFunction<d_gtk_events_pending>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_events_pending"));


		public static bool EventsPending ()
		{
			return gtk_events_pending ();
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_gtk_main_iteration();
		static d_gtk_main_iteration gtk_main_iteration = FuncLoader.LoadFunction<d_gtk_main_iteration>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_main_iteration"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate bool d_gtk_main_iteration_do(bool blocking);
		static d_gtk_main_iteration_do gtk_main_iteration_do = FuncLoader.LoadFunction<d_gtk_main_iteration_do>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_main_iteration_do"));

		public static void RunIteration ()
		{
			gtk_main_iteration ();
		}

		public static bool RunIteration (bool blocking)
		{
			return gtk_main_iteration_do (blocking);
		}
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_gtk_main_quit();
		static d_gtk_main_quit gtk_main_quit = FuncLoader.LoadFunction<d_gtk_main_quit>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_main_quit"));

		public static void Quit ()
		{
			gtk_main_quit ();
		}

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_gtk_get_current_event();
		static d_gtk_get_current_event gtk_get_current_event = FuncLoader.LoadFunction<d_gtk_get_current_event>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_get_current_event"));

		public static Gdk.Event CurrentEvent {
			get {
				return Gdk.Event.GetEvent (gtk_get_current_event ());
			}
		}

		internal class InvokeCB {
			EventHandler d;
			object sender;
			EventArgs args;
			
			internal InvokeCB (EventHandler d)
			{
				this.d = d;
				args = EventArgs.Empty;
				sender = this;
			}
			
			internal InvokeCB (EventHandler d, object sender, EventArgs args)
			{
				this.d = d;
				this.args = args;
				this.sender = sender;
			}
			
			internal bool Invoke ()
			{
				d (sender, args);
				return false;
			}
		}
		
		public static void Invoke (EventHandler d)
		{
			InvokeCB icb = new InvokeCB (d);
			
			GLib.Timeout.Add (0, new GLib.TimeoutHandler (icb.Invoke));
		}

		public static void Invoke (object sender, EventArgs args, EventHandler d)
		{
			InvokeCB icb = new InvokeCB (d, sender, args);
			
			GLib.Timeout.Add (0, new GLib.TimeoutHandler (icb.Invoke));
		}
	}
}


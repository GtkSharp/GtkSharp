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
		
		// Gtk 4 changed both of these to take no arguments: it no longer parses
		// or strips command-line options, so there is no argc/argv to hand it or
		// to read back. Declaring them with the Gtk 3 signature passed two extra
		// arguments to a niladic function, and left do_init trying to recover
		// options that were never consumed.
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_gtk_init();
		static d_gtk_init gtk_init = FuncLoader.LoadFunction<d_gtk_init>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_init"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate bool d_gtk_init_check();
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
			gtk_init ();

			SynchronizationContext.SetSynchronizationContext (new GLib.GLibSynchronizationContext ());
		}

		// args is left untouched: Gtk 4 consumes no arguments. The parameter is
		// kept by reference so existing callers still compile.
		public static void Init (string progname, ref string[] args)
		{
			GLib.Global.ProgramName = progname;
			gtk_init ();

			SynchronizationContext.SetSynchronizationContext (new GLib.GLibSynchronizationContext ());
		}

		public static bool InitCheck (string progname, ref string[] args)
		{
			GLib.Global.ProgramName = progname;
			bool res = gtk_init_check ();

			if (res)
				SynchronizationContext.SetSynchronizationContext (new GLib.GLibSynchronizationContext ());

			return res;
		}
		// Gtk 4 removed the whole gtk_main family -- gtk_main, gtk_main_quit,
		// gtk_events_pending, gtk_main_iteration and gtk_main_iteration_do are
		// all gone, because a Gtk 4 application drives a GLib main loop through
		// GApplication rather than a Gtk-owned one. Loading those symbols
		// produced null delegates -- FuncLoader.LoadFunction returns default(T)
		// for a missing export -- so Application.Run threw a
		// NullReferenceException and no Gtk 4 application could start.
		//
		// These now drive a GLib main loop directly, which is what gtk_main did
		// anyway, and keeps the existing API working.

		static GLib.MainLoop main_loop;

		static GLib.MainLoop MainLoop {
			get {
				if (main_loop == null)
					main_loop = new GLib.MainLoop ();
				return main_loop;
			}
		}

		public static void Run ()
		{
			MainLoop.Run ();
		}

		public static bool EventsPending ()
		{
			return GLib.MainContext.Pending ();
		}

		public static void RunIteration ()
		{
			GLib.MainContext.Iteration ();
		}

		public static bool RunIteration (bool blocking)
		{
			return GLib.MainContext.Iteration (blocking);
		}

		public static void Quit ()
		{
			if (main_loop != null && main_loop.IsRunning)
				main_loop.Quit ();
		}

		// CurrentEvent: gtk_get_current_event is gone in Gtk 4. An event controller receives the event it is handling directly.

		internal class InvokeCB {
			System.EventHandler d;
			object sender;
			System.EventArgs args;
			
			internal InvokeCB (System.EventHandler d)
			{
				this.d = d;
				args = System.EventArgs.Empty;
				sender = this;
			}
			
			internal InvokeCB (System.EventHandler d, object sender, System.EventArgs args)
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
		
		// System.EventHandler, qualified: Gtk 4 introduces a Gtk.EventHandler
		// that would otherwise win name resolution inside namespace Gtk.
		public static void Invoke (System.EventHandler d)
		{
			InvokeCB icb = new InvokeCB (d);
			
			GLib.Timeout.Add (0, new GLib.TimeoutHandler (icb.Invoke));
		}

		public static void Invoke (object sender, System.EventArgs args, System.EventHandler d)
		{
			InvokeCB icb = new InvokeCB (d, sender, args);
			
			GLib.Timeout.Add (0, new GLib.TimeoutHandler (icb.Invoke));
		}
	}
}


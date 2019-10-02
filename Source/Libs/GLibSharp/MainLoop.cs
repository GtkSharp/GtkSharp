// GLib.MainLoop.cs - g_main_loop class implementation
//
// Author: Jeroen Zwartepoorte <jeroen@xs4all.nl>
//
// Copyright (c) 2004 Jeroen Zwartepoorte
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

using System;
using System.Runtime.InteropServices;

namespace GLib {
	public class MainLoop {
		private IntPtr handle;
	
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_main_loop_new(IntPtr context, bool isRunning);
		static d_g_main_loop_new g_main_loop_new = FuncLoader.LoadFunction<d_g_main_loop_new>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_main_loop_new"));

		public MainLoop () : this (MainContext.Default) { }

		public MainLoop (MainContext context) : this (context, false) { }

		public MainLoop (MainContext context, bool is_running)
		{
			handle = g_main_loop_new (context.Handle, is_running);
		}
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_g_main_loop_unref(IntPtr loop);
		static d_g_main_loop_unref g_main_loop_unref = FuncLoader.LoadFunction<d_g_main_loop_unref>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_main_loop_unref"));

		~MainLoop ()
		{
			g_main_loop_unref (handle);
			handle = IntPtr.Zero;
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate bool d_g_main_loop_is_running(IntPtr loop);
		static d_g_main_loop_is_running g_main_loop_is_running = FuncLoader.LoadFunction<d_g_main_loop_is_running>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_main_loop_is_running"));

		public bool IsRunning {
			get {
				return g_main_loop_is_running (handle);
			}
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_g_main_loop_run(IntPtr loop);
		static d_g_main_loop_run g_main_loop_run = FuncLoader.LoadFunction<d_g_main_loop_run>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_main_loop_run"));

		public void Run ()
		{
			g_main_loop_run (handle);
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_g_main_loop_quit(IntPtr loop);
		static d_g_main_loop_quit g_main_loop_quit = FuncLoader.LoadFunction<d_g_main_loop_quit>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_main_loop_quit"));

		public void Quit ()
		{
			g_main_loop_quit (handle);
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_main_loop_get_context(IntPtr loop);
		static d_g_main_loop_get_context g_main_loop_get_context = FuncLoader.LoadFunction<d_g_main_loop_get_context>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_main_loop_get_context"));

		public MainContext Context {
			get {
				return new MainContext (g_main_loop_get_context (handle));
			}
		}


		public override bool Equals (object o)
		{
			if (!(o is MainLoop))
				return false;

			return handle == (o as MainLoop).handle;
		}

		public override int GetHashCode ()
		{
			return handle.GetHashCode ();
		}
	}
}



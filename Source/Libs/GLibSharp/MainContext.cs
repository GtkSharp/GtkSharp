// GLib.MainContext.cs - mainContext class implementation
//
// Author: Radek Doulik <rodo@matfyz.cz>
//
// Copyright (c) 2003 Radek Doulik
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


namespace GLib {

	using System;
	using System.Runtime.InteropServices;

        public class MainContext {
		IntPtr handle;
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_main_context_new();
		static d_g_main_context_new g_main_context_new = FuncLoader.LoadFunction<d_g_main_context_new>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_main_context_new"));

		public MainContext ()
		{
			handle = g_main_context_new ();
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_g_main_context_ref(IntPtr raw);
		static d_g_main_context_ref g_main_context_ref = FuncLoader.LoadFunction<d_g_main_context_ref>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_main_context_ref"));

		public MainContext (IntPtr raw)
		{
			handle = raw;
			g_main_context_ref (handle);
		}

		public IntPtr Handle {
			get {
				return handle;
			}
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_g_main_context_unref(IntPtr raw);
		static d_g_main_context_unref g_main_context_unref = FuncLoader.LoadFunction<d_g_main_context_unref>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_main_context_unref"));

		~MainContext ()
		{
			g_main_context_unref (handle);
			handle = IntPtr.Zero;
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_main_context_default();
		static d_g_main_context_default g_main_context_default = FuncLoader.LoadFunction<d_g_main_context_default>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_main_context_default"));

		public static MainContext Default {
			get {
				return new MainContext (g_main_context_default ());
			}
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_main_context_thread_default();
		static d_g_main_context_thread_default g_main_context_thread_default = FuncLoader.LoadFunction<d_g_main_context_thread_default>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_main_context_thread_default"));

		public MainContext ThreadDefault {
			get {
				IntPtr raw = g_main_context_thread_default ();
				// NULL is returned if the thread-default main context is the default context. We'd rather not adopt this strange bahaviour.
				return raw == IntPtr.Zero ? Default : new MainContext (raw);
			}
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_g_main_context_push_thread_default(IntPtr raw);
		static d_g_main_context_push_thread_default g_main_context_push_thread_default = FuncLoader.LoadFunction<d_g_main_context_push_thread_default>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_main_context_push_thread_default"));

		public void PushThreadDefault ()
		{
			g_main_context_push_thread_default (handle);
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_g_main_context_pop_thread_default(IntPtr raw);
		static d_g_main_context_pop_thread_default g_main_context_pop_thread_default = FuncLoader.LoadFunction<d_g_main_context_pop_thread_default>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_main_context_pop_thread_default"));

		public void PopThreadDefault ()
		{
			g_main_context_pop_thread_default (handle);
		}

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate bool d_g_main_context_iteration(IntPtr raw, bool may_block);
		static d_g_main_context_iteration g_main_context_iteration = FuncLoader.LoadFunction<d_g_main_context_iteration>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_main_context_iteration"));

		public bool RunIteration (bool may_block)
		{
			return g_main_context_iteration (handle, may_block);
		}

		public bool RunIteration ()
		{
			return RunIteration (false);
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate bool d_g_main_context_pending(IntPtr raw);
		static d_g_main_context_pending g_main_context_pending = FuncLoader.LoadFunction<d_g_main_context_pending>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_main_context_pending"));

		public bool HasPendingEvents
		{
			get {
				return g_main_context_pending (handle);
			}
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_g_main_context_wakeup(IntPtr raw);
		static d_g_main_context_wakeup g_main_context_wakeup = FuncLoader.LoadFunction<d_g_main_context_wakeup>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_main_context_wakeup"));

		public void Wakeup ()
		{
			g_main_context_wakeup (handle);
		}


		public override bool Equals (object o)
		{
			if (!(o is MainContext))
				return false;

			return Handle == (o as MainContext).Handle;
		}

		public override int GetHashCode ()
		{
			return Handle.GetHashCode ();
		}

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate int d_g_main_depth();
		static d_g_main_depth g_main_depth = FuncLoader.LoadFunction<d_g_main_depth>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_main_depth"));
		public static int Depth {
			get { return g_main_depth (); }
		}


		public static bool Iteration ()
		{
			return Iteration (false);
		}

		public static bool Iteration (bool may_block)
		{
			return Default.RunIteration (may_block);
		}

		public static bool Pending ()
		{
			return Default.HasPendingEvents;
		}
	}
}


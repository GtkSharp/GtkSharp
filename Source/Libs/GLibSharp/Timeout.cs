// GLib.Timeout.cs - Timeout class implementation
//
// Author(s):
//	Mike Kestner <mkestner@speakeasy.net>
//	Stephane Delcroix <stephane@delcroix.org>
//
// Copyright (c) 2002 Mike Kestner
// Copyright (c) 2009 Novell, Inc.
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
	using System.Collections.Generic;
	using System.Runtime.InteropServices;

	public delegate bool TimeoutHandler ();

	public class Timeout {

		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		delegate bool TimeoutHandlerInternal ();

		internal class TimeoutProxy : SourceProxy {
			public TimeoutProxy (TimeoutHandler real)
			{
				real_handler = real;
				proxy_handler = new TimeoutHandlerInternal (Handler);
			}

			public bool Handler ()
			{
				try {
					TimeoutHandler timeout_handler = (TimeoutHandler) real_handler;

					bool cont = timeout_handler ();
					if (!cont)
					{
						lock (this)
						{
							Remove ();
						}
					}
					return cont;
				} catch (Exception e) {
					ExceptionManager.RaiseUnhandledException (e, false);
				}
				return false;
			}
		}
		
		private Timeout () {} 
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate uint d_g_timeout_add(uint interval, TimeoutHandlerInternal d, IntPtr data);
		static d_g_timeout_add g_timeout_add = FuncLoader.LoadFunction<d_g_timeout_add>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_timeout_add"));

		public static uint Add (uint interval, TimeoutHandler hndlr)
		{
			TimeoutProxy p = new TimeoutProxy (hndlr);
			lock (p)
			{
				p.ID = g_timeout_add (interval, (TimeoutHandlerInternal) p.proxy_handler, IntPtr.Zero);
				Source.AddSourceHandler (p.ID, p);
			}

			return p.ID;
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate uint d_g_timeout_add_full(int priority, uint interval, TimeoutHandlerInternal d, IntPtr data, DestroyNotify notify);
		static d_g_timeout_add_full g_timeout_add_full = FuncLoader.LoadFunction<d_g_timeout_add_full>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_timeout_add_full"));

		public static uint Add (uint interval, TimeoutHandler hndlr, Priority priority)
		{
			TimeoutProxy p = new TimeoutProxy (hndlr);
			lock (p)
			{
				p.ID = g_timeout_add_full ((int)priority, interval, (TimeoutHandlerInternal) p.proxy_handler, IntPtr.Zero, null);
				Source.AddSourceHandler (p.ID, p);
			}

			return p.ID;
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate uint d_g_timeout_add_seconds(uint interval, TimeoutHandlerInternal d, IntPtr data);
		static d_g_timeout_add_seconds g_timeout_add_seconds = FuncLoader.LoadFunction<d_g_timeout_add_seconds>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_timeout_add_seconds"));

		public static uint AddSeconds (uint interval, TimeoutHandler hndlr)
		{
			TimeoutProxy p = new TimeoutProxy (hndlr);
			lock (p)
			{
				p.ID = g_timeout_add_seconds (interval, (TimeoutHandlerInternal) p.proxy_handler, IntPtr.Zero);
				Source.AddSourceHandler (p.ID, p);
			}

			return p.ID;
		}

		public static void Remove (uint id)
		{
			Source.Remove (id);
		}

		public static bool Remove (TimeoutHandler hndlr)
		{
			return Source.RemoveSourceHandler (hndlr);
		}
	}
}



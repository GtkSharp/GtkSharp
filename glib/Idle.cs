// GLib.Idle.cs - Idle class implementation
//
// Author(s):
//	Mike Kestner <mkestner@speakeasy.net>
//	Rachel Hestilow <hestilow@ximian.com>
//	Stephane Delcroix <stephane@delcroix.org>
//
// Copyright (c) 2002 Mike Kestner
// Copyright (c) Rachel Hestilow
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

	public delegate bool IdleHandler ();

	public class Idle {

		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		delegate bool IdleHandlerInternal ();

		internal class IdleProxy : SourceProxy {
			public IdleProxy (IdleHandler real)
			{
				real_handler = real;
				proxy_handler = new IdleHandlerInternal (Handler);
			}

			public bool Handler ()
			{
				try {
					IdleHandler idle_handler = (IdleHandler) real_handler;

					bool cont = idle_handler ();
					if (!cont)
						Remove ();
					return cont;
				} catch (Exception e) {
					ExceptionManager.RaiseUnhandledException (e, false);
				}
				return false;
			}
		}
		
		private Idle ()
		{
		}
		
		[DllImport (Global.GLibNativeDll, CallingConvention = CallingConvention.Cdecl)]
		static extern uint g_idle_add (IdleHandlerInternal d, IntPtr data);

		public static uint Add (IdleHandler hndlr)
		{
			IdleProxy p = new IdleProxy (hndlr);
			p.ID = g_idle_add ((IdleHandlerInternal) p.proxy_handler, IntPtr.Zero);
			Source.AddSourceHandler (p.ID, p);

			return p.ID;
		}

		[DllImport (Global.GLibNativeDll, CallingConvention = CallingConvention.Cdecl)]
		static extern uint g_idle_add_full (int priority, IdleHandlerInternal d, IntPtr data, DestroyNotify notify);

		public static uint Add (IdleHandler hndlr, Priority priority)
		{
			IdleProxy p = new IdleProxy (hndlr);
			p.ID = g_idle_add_full ((int)priority, (IdleHandlerInternal)p.proxy_handler, IntPtr.Zero, null);
			Source.AddSourceHandler (p.ID, p);

			return p.ID;
		}
		
		public static void Remove (uint id)
		{
			Source.Remove (id);
		}

		public static bool Remove (IdleHandler hndlr)
		{
			return Source.RemoveSourceHandler (hndlr);
		}
	}
}


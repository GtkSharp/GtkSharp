// GLib.Timeout.cs - Timeout class implementation
//
// Author: Mike Kestner <mkestner@speakeasy.net>
//
// Copyright (c) 2002 Mike Kestner
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

	public delegate bool TimeoutHandler ();

	public class Timeout {

		[CDeclCallback]
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
						Remove ();
					return cont;
				} catch (Exception e) {
					ExceptionManager.RaiseUnhandledException (e, false);
				}
				return false;
			}
		}
		
		private Timeout () {} 
		[DllImport("libglib-2.0-0.dll")]
		static extern uint g_timeout_add (uint interval, TimeoutHandlerInternal d, IntPtr data);

		public static uint Add (uint interval, TimeoutHandler hndlr)
		{
			TimeoutProxy p = new TimeoutProxy (hndlr);

			uint code = g_timeout_add (interval, (TimeoutHandlerInternal) p.proxy_handler, IntPtr.Zero);
			lock (Source.source_handlers)
				Source.source_handlers [code] = p;

			return code;
		}
	}
}


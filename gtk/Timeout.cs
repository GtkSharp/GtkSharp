// Gtk.Timeout.cs - Gtk Timeout implementation.
//
// Author: Mike Kestner <mkestner@novell.com>
//
// Copyright (c) 2005 Novell, Inc.
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
	using System.Runtime.InteropServices;

	[Obsolete ("Replaced by GLib.Timeout")]
	public class Timeout {

		[Obsolete ("Replaced by GLib.Source.Remove")]
		public static void Remove (uint timeout_handler_id) 
		{
			GLib.Source.Remove (timeout_handler_id);
		}

		[Obsolete ("Replaced by GLib.Timeout.Add")]
		public static uint AddFull (uint interval, Gtk.Function function, Gtk.CallbackMarshal marshal, IntPtr data, Gtk.DestroyNotify destroy) 
		{
			if (marshal != null || destroy != null)
				Console.WriteLine ("marshal, data, and destroy parameters ignored by Gtk.Timeout.AddFull ().");
			return Add (interval, function);
		}

		class GTimeoutProxy {

			GLib.TimeoutHandler handler;
			Function function;

			public GTimeoutProxy (Gtk.Function function)
			{
				this.function = function;
				handler = new GLib.TimeoutHandler (Invoke);
			}

			public GLib.TimeoutHandler Handler {
				get {
					return handler;
				}
			}

			bool Invoke ()
			{
				return function ();
			}
		}

		[Obsolete ("Replaced by GLib.Timeout.Add")]
		public static uint Add (uint interval, Gtk.Function function) 
		{
			GTimeoutProxy proxy = new GTimeoutProxy (function);
			return GLib.Timeout.Add (interval, proxy.Handler);
		}
	}
}


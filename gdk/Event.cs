// Gdk.Event.cs - Custom event wrapper 
//
// Authors: Rachel Hestilow <hestilow@ximian.com> 
//          Mike Kestner <mkestner@ximian.com>
//
// Copyright (c) 2002 Rachel Hestilow
// Copyright (c) 2004 Novell, Inc.
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


namespace Gdk {

	using System;
	using System.Runtime.InteropServices;

	public class Event : GLib.IWrapper {

		[DllImport("gdksharpglue-2")]
		static extern EventType gtksharp_gdk_event_get_event_type (IntPtr evt);

		[DllImport("gdksharpglue-2")]
		static extern IntPtr gtksharp_gdk_event_get_window (IntPtr evt);

		[DllImport("gdksharpglue-2")]
		static extern sbyte gtksharp_gdk_event_get_send_event (IntPtr evt);

		[DllImport("libgdk-win32-2.0-0.dll")]
		static extern IntPtr gdk_event_get_type ();

		IntPtr raw;

		public Event(IntPtr raw) 
		{
			this.raw = raw;
		}

		public IntPtr Handle {
			get {
				return raw;
			}
		}

		public static GLib.GType GType {
			get {
				return new GLib.GType (gdk_event_get_type ());
			}
		}

		public EventType Type {
			get {
				return gtksharp_gdk_event_get_event_type (Handle);
			}
		}

		public Window Window {
			get {
				return GLib.Object.GetObject (gtksharp_gdk_event_get_window (Handle)) as Window;
			}
		}

		public bool SendEvent {
			get {
				return gtksharp_gdk_event_get_send_event (Handle) == 0 ? false : true;
			}
		}
	}
}


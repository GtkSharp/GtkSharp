// Gdk.EventScroll.cs - Custom scroll event wrapper 
//
// Author:  Mike Kestner <mkestner@ximian.com>
//
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

	public class EventScroll : Event {

		[DllImport("gdksharpglue-2.0")]
		static extern uint gtksharp_gdk_event_scroll_get_time (IntPtr evt);

		[DllImport("gdksharpglue-2.0")]
		static extern double gtksharp_gdk_event_scroll_get_x (IntPtr evt);

		[DllImport("gdksharpglue-2.0")]
		static extern double gtksharp_gdk_event_scroll_get_y (IntPtr evt);

		[DllImport("gdksharpglue-2.0")]
		static extern double gtksharp_gdk_event_scroll_get_x_root (IntPtr evt);

		[DllImport("gdksharpglue-2.0")]
		static extern double gtksharp_gdk_event_scroll_get_y_root (IntPtr evt);

		[DllImport("gdksharpglue-2.0")]
		static extern uint gtksharp_gdk_event_scroll_get_state (IntPtr evt);

		[DllImport("gdksharpglue-2.0")]
		static extern ScrollDirection gtksharp_gdk_event_scroll_get_direction (IntPtr evt);

		[DllImport("gdksharpglue-2.0")]
		static extern IntPtr gtksharp_gdk_event_scroll_get_device (IntPtr evt);

		public EventScroll (IntPtr raw) : base (raw) {} 

		public uint Time {
			get {
				return gtksharp_gdk_event_scroll_get_time (Handle);
			}
		}

		public ModifierType State {
			get {
				return (ModifierType) gtksharp_gdk_event_scroll_get_state (Handle);
			}
		}

		public double X {
			get {
				return gtksharp_gdk_event_scroll_get_x (Handle);
			}
		}

		public double Y {
			get {
				return gtksharp_gdk_event_scroll_get_y (Handle);
			}
		}

		public double XRoot {
			get {
				return gtksharp_gdk_event_scroll_get_x_root (Handle);
			}
		}

		public double YRoot {
			get {
				return gtksharp_gdk_event_scroll_get_y_root (Handle);
			}
		}

		public ScrollDirection Direction {
			get {
				return gtksharp_gdk_event_scroll_get_direction (Handle);
			}
		}

		public Device Device {
			get {
				return GLib.Object.GetObject (gtksharp_gdk_event_scroll_get_device (Handle)) as Device;
			}
		}
	}
}


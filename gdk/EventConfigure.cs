// Gdk.EventConfigure.cs - Custom configure event wrapper 
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

	public class EventConfigure : Event {

		[DllImport("gdksharpglue")]
		static extern int gtksharp_gdk_event_configure_get_x (IntPtr evt);

		[DllImport("gdksharpglue")]
		static extern int gtksharp_gdk_event_configure_get_y (IntPtr evt);

		[DllImport("gdksharpglue")]
		static extern int gtksharp_gdk_event_configure_get_width (IntPtr evt);

		[DllImport("gdksharpglue")]
		static extern int gtksharp_gdk_event_configure_get_height (IntPtr evt);

		public EventConfigure (IntPtr raw) : base (raw) {} 

		public int X {
			get {
				return gtksharp_gdk_event_configure_get_x (Handle);
			}
		}

		public int Y {
			get {
				return gtksharp_gdk_event_configure_get_y (Handle);
			}
		}

		public int Width {
			get {
				return gtksharp_gdk_event_configure_get_width (Handle);
			}
		}

		public int Height {
			get {
				return gtksharp_gdk_event_configure_get_height (Handle);
			}
		}
	}
}


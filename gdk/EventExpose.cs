// Gdk.EventExpose.cs - Custom expose event wrapper 
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

	public class EventExpose : Event {

		[DllImport("gdksharpglue")]
		static extern Rectangle gtksharp_gdk_event_expose_get_area (IntPtr evt);

		[DllImport("gdksharpglue")]
		static extern IntPtr gtksharp_gdk_event_expose_get_region (IntPtr evt);

		[DllImport("gdksharpglue")]
		static extern int gtksharp_gdk_event_expose_get_count (IntPtr evt);

		public EventExpose (IntPtr raw) : base (raw) {} 

		public Rectangle Area {
			get {
				return gtksharp_gdk_event_expose_get_area (Handle);
			}
		}

		public int Count {
			get {
				return gtksharp_gdk_event_expose_get_count (Handle);
			}
		}

		public Region Region {
			get {
				return new Region (gtksharp_gdk_event_expose_get_region (Handle));
			}
		}
	}
}


// Gdk.EventExpose.cs - Custom expose event wrapper 
//
// Author:  Mike Kestner <mkestner@ximian.com>
//
// (c) 2004 Novell, Inc.

namespace Gdk {

	using System;
	using System.Runtime.InteropServices;

	public class EventExpose : Event {

		[DllImport("gtksharpglue")]
		static extern Rectangle gtksharp_gdk_event_expose_get_area (IntPtr evt);

		[DllImport("gtksharpglue")]
		static extern IntPtr gtksharp_gdk_event_expose_get_region (IntPtr evt);

		[DllImport("gtksharpglue")]
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


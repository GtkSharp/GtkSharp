// Gdk.EventConfigure.cs - Custom configure event wrapper 
//
// Author:  Mike Kestner <mkestner@ximian.com>
//
// (c) 2004 Novell, Inc.

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


// Gdk.EventScroll.cs - Custom scroll event wrapper 
//
// Author:  Mike Kestner <mkestner@ximian.com>
//
// (c) 2004 Novell, Inc.

namespace Gdk {

	using System;
	using System.Runtime.InteropServices;

	public class EventScroll : Event {

		[DllImport("gtksharpglue")]
		static extern uint gtksharp_gdk_event_scroll_get_time (IntPtr evt);

		[DllImport("gtksharpglue")]
		static extern double gtksharp_gdk_event_scroll_get_x (IntPtr evt);

		[DllImport("gtksharpglue")]
		static extern double gtksharp_gdk_event_scroll_get_y (IntPtr evt);

		[DllImport("gtksharpglue")]
		static extern double gtksharp_gdk_event_scroll_get_x_root (IntPtr evt);

		[DllImport("gtksharpglue")]
		static extern double gtksharp_gdk_event_scroll_get_y_root (IntPtr evt);

		[DllImport("gtksharpglue")]
		static extern uint gtksharp_gdk_event_scroll_get_state (IntPtr evt);

		[DllImport("gtksharpglue")]
		static extern ScrollDirection gtksharp_gdk_event_scroll_get_direction (IntPtr evt);

		[DllImport("gtksharpglue")]
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


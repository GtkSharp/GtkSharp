// Gdk.EventProximity.cs - Custom proximity event wrapper 
//
// Author:  Mike Kestner <mkestner@ximian.com>
//
// (c) 2004 Novell, Inc.

namespace Gdk {

	using System;
	using System.Runtime.InteropServices;

	public class EventProximity : Event {

		[DllImport("gdksharpglue")]
		static extern uint gtksharp_gdk_event_proximity_get_time (IntPtr evt);

		[DllImport("gdksharpglue")]
		static extern IntPtr gtksharp_gdk_event_proximity_get_device (IntPtr evt);

		public EventProximity (IntPtr raw) : base (raw) {} 

		public Device Device {
			get {
				return GLib.Object.GetObject (gtksharp_gdk_event_proximity_get_device (Handle)) as Device;
			}
		}

		public uint Time {
			get {
				return gtksharp_gdk_event_proximity_get_time (Handle);
			}
		}
	}
}


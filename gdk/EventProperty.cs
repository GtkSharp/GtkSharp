// Gdk.EventProperty.cs - Custom property event wrapper 
//
// Author:  Mike Kestner <mkestner@ximian.com>
//
// (c) 2004 Novell, Inc.

namespace Gdk {

	using System;
	using System.Runtime.InteropServices;

	public class EventProperty : Event {

		[DllImport("gdksharpglue")]
		static extern uint gtksharp_gdk_event_property_get_time (IntPtr evt);

		[DllImport("gdksharpglue")]
		static extern IntPtr gtksharp_gdk_event_property_get_atom (IntPtr evt);

		[DllImport("gdksharpglue")]
		static extern PropertyState gtksharp_gdk_event_property_get_state (IntPtr evt);

		public EventProperty (IntPtr raw) : base (raw) {} 

		public Atom Atom {
			get {
				return new Atom (gtksharp_gdk_event_property_get_atom (Handle));
			}
		}

		public PropertyState State {
			get {
				return gtksharp_gdk_event_property_get_state (Handle);
			}
		}

		public uint Time {
			get {
				return gtksharp_gdk_event_property_get_time (Handle);
			}
		}
	}
}


// Gdk.EventVisibility.cs - Custom visibility event wrapper 
//
// Author:  Mike Kestner <mkestner@ximian.com>
//
// (c) 2004 Novell, Inc.

namespace Gdk {

	using System;
	using System.Runtime.InteropServices;

	public class EventVisibility : Event {

		[DllImport("gtksharpglue")]
		static extern VisibilityState gtksharp_gdk_event_visibility_get_state (IntPtr evt);

		public EventVisibility (IntPtr raw) : base (raw) {} 

		public VisibilityState State {
			get {
				return gtksharp_gdk_event_visibility_get_state (Handle);
			}
		}
	}
}


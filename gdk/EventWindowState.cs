// Gdk.EventWindowState.cs - Custom WindowState event wrapper 
//
// Author:  Mike Kestner <mkestner@ximian.com>
//
// (c) 2004 Novell, Inc.

namespace Gdk {

	using System;
	using System.Runtime.InteropServices;

	public class EventWindowState : Event {

		[DllImport("gtksharpglue")]
		static extern WindowState gtksharp_gdk_event_window_state_get_changed_mask (IntPtr evt);

		[DllImport("gtksharpglue")]
		static extern WindowState gtksharp_gdk_event_window_state_get_new_window_state (IntPtr evt);

		public EventWindowState (IntPtr raw) : base (raw) {} 

		public WindowState ChangedMask {
			get {
				return gtksharp_gdk_event_window_state_get_changed_mask (Handle);
			}
		}

		public WindowState NewWindowState {
			get {
				return gtksharp_gdk_event_window_state_get_new_window_state (Handle);
			}
		}
	}
}


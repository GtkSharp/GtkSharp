// Gdk.EventFocus.cs - Custom focus event wrapper 
//
// Author:  Mike Kestner <mkestner@ximian.com>
//
// (c) 2004 Novell, Inc.

namespace Gdk {

	using System;
	using System.Runtime.InteropServices;

	public class EventFocus : Event {

		[DllImport("gdksharpglue")]
		static extern short gtksharp_gdk_event_focus_get_in (IntPtr evt);

		public EventFocus (IntPtr raw) : base (raw) {} 

		public bool In {
			get {
				return gtksharp_gdk_event_focus_get_in (Handle) == 0 ? false : true;
			}
		}
	}
}


// Gdk.Event.cs - Custom event wrapper 
//
// Author: Rachel Hestilow <hestilow@ximian.com> 
//
// (c) 2002 Rachel Hestilow


namespace Gdk {

	using System;
	using System.Collections;
	using System.Runtime.InteropServices;

	public class Event : GLib.Boxed {

		[DllImport("gtksharpglue")]
		static extern EventType gtksharp_gdk_event_get_event_type (IntPtr evt);

		public Event(IntPtr raw) : base(raw) {}

		public EventType Type {
			get {
				return gtksharp_gdk_event_get_event_type (Handle);
			}
		}

		public bool IsValid {
			get {
				return (Handle != IntPtr.Zero);
			}
		}
	}

}

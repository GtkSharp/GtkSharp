// Gdk.Event.cs - Custom event wrapper 
//
// Authors: Rachel Hestilow <hestilow@ximian.com> 
//          Mike Kestner <mkestner@ximian.com>
//
// (c) 2002 Rachel Hestilow
// (c) 2004 Novell, Inc.

namespace Gdk {

	using System;
	using System.Runtime.InteropServices;

	public class Event : GLib.IWrapper {

		[DllImport("gtksharpglue")]
		static extern EventType gtksharp_gdk_event_get_event_type (IntPtr evt);

		[DllImport("gtksharpglue")]
		static extern IntPtr gtksharp_gdk_event_get_window (IntPtr evt);

		[DllImport("gtksharpglue")]
		static extern sbyte gtksharp_gdk_event_get_send_event (IntPtr evt);

		[DllImport("libgdk-win32-2.0-0.dll")]
		static extern IntPtr gdk_event_get_type ();

		IntPtr raw;

		public Event(IntPtr raw) 
		{
			this.raw = raw;
		}

		public IntPtr Handle {
			get {
				return raw;
			}
		}

		public static GLib.GType GType {
			get {
				return new GLib.GType (gdk_event_get_type ());
			}
		}

		public EventType Type {
			get {
				return gtksharp_gdk_event_get_event_type (Handle);
			}
		}

		public Window Window {
			get {
				return GLib.Object.GetObject (gtksharp_gdk_event_get_window (Handle)) as Window;
			}
		}

		public bool SendEvent {
			get {
				return gtksharp_gdk_event_get_send_event (Handle) == 0 ? false : true;
			}
		}
	}
}


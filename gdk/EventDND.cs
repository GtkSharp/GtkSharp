// Gdk.EventDND.cs - Custom dnd event wrapper 
//
// Author:  Mike Kestner <mkestner@ximian.com>
//
// (c) 2004 Novell, Inc.

namespace Gdk {

	using System;
	using System.Runtime.InteropServices;

	public class EventDND : Event {

		[DllImport("gdksharpglue")]
		static extern uint gtksharp_gdk_event_dnd_get_time (IntPtr evt);

		[DllImport("gdksharpglue")]
		static extern IntPtr gtksharp_gdk_event_dnd_get_context (IntPtr evt);

		[DllImport("gdksharpglue")]
		static extern short gtksharp_gdk_event_dnd_get_x_root (IntPtr evt);

		[DllImport("gdksharpglue")]
		static extern short gtksharp_gdk_event_dnd_get_y_root (IntPtr evt);

		public EventDND (IntPtr raw) : base (raw) {} 

		public DragContext Context {
			get {
				return GLib.Object.GetObject (gtksharp_gdk_event_dnd_get_context (Handle)) as DragContext;
			}
		}

		public uint Time {
			get {
				return gtksharp_gdk_event_dnd_get_time (Handle);
			}
		}

		public short XRoot {
			get {
				return gtksharp_gdk_event_dnd_get_x_root (Handle);
			}
		}

		public short YRoot {
			get {
				return gtksharp_gdk_event_dnd_get_y_root (Handle);
			}
		}
	}
}


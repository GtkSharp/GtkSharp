// Gdk.EventKey.cs - Custom key event wrapper 
//
// Author:  Mike Kestner <mkestner@ximian.com>
//
// (c) 2004 Novell, Inc.

namespace Gdk {

	using System;
	using System.Runtime.InteropServices;

	public class EventKey : Event {

		[DllImport("gtksharpglue")]
		static extern uint gtksharp_gdk_event_key_get_time (IntPtr evt);

		[DllImport("gtksharpglue")]
		static extern uint gtksharp_gdk_event_key_get_state (IntPtr evt);

		[DllImport("gtksharpglue")]
		static extern uint gtksharp_gdk_event_key_get_keyval (IntPtr evt);

		[DllImport("gtksharpglue")]
		static extern ushort gtksharp_gdk_event_key_get_hardware_keycode (IntPtr evt);

		[DllImport("gtksharpglue")]
		static extern byte gtksharp_gdk_event_key_get_group (IntPtr evt);

		public EventKey (IntPtr raw) : base (raw) {} 

		public uint Time {
			get {
				return gtksharp_gdk_event_key_get_time (Handle);
			}
		}

		public ModifierType State {
			get {
				return (ModifierType) gtksharp_gdk_event_key_get_state (Handle);
			}
		}

		public uint KeyValue {
			get {
				return gtksharp_gdk_event_key_get_keyval (Handle);
			}
		}

		public ushort HardwareKeycode {
			get {
				return gtksharp_gdk_event_key_get_hardware_keycode (Handle);
			}
		}

		public byte Group {
			get {
				return gtksharp_gdk_event_key_get_group (Handle);
			}
		}
	}
}


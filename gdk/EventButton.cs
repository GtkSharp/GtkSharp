// Gdk.EventButton.cs - Custom button event wrapper 
//
// Author:  Mike Kestner <mkestner@ximian.com>
//
// (c) 2004 Novell, Inc.

namespace Gdk {

	using System;
	using System.Runtime.InteropServices;

	public class EventButton : Event {

		[DllImport("gdksharpglue")]
		static extern uint gtksharp_gdk_event_button_get_time (IntPtr evt);

		[DllImport("gdksharpglue")]
		static extern double gtksharp_gdk_event_button_get_x (IntPtr evt);

		[DllImport("gdksharpglue")]
		static extern double gtksharp_gdk_event_button_get_y (IntPtr evt);

		[DllImport("gdksharpglue")]
		static extern double gtksharp_gdk_event_button_get_x_root (IntPtr evt);

		[DllImport("gdksharpglue")]
		static extern double gtksharp_gdk_event_button_get_y_root (IntPtr evt);

		[DllImport("gdksharpglue")]
		static extern uint gtksharp_gdk_event_button_get_state (IntPtr evt);

		[DllImport("gdksharpglue")]
		static extern uint gtksharp_gdk_event_button_get_button (IntPtr evt);

		[DllImport("gdksharpglue")]
		static extern IntPtr gtksharp_gdk_event_button_get_device (IntPtr evt);

		[DllImport("gdksharpglue")]
		static extern IntPtr gtksharp_gdk_event_button_get_axes (IntPtr evt);

		public EventButton (IntPtr raw) : base (raw) {} 

		public uint Time {
			get {
				return gtksharp_gdk_event_button_get_time (Handle);
			}
		}

		public ModifierType State {
			get {
				return (ModifierType) gtksharp_gdk_event_button_get_state (Handle);
			}
		}

		public double X {
			get {
				return gtksharp_gdk_event_button_get_x (Handle);
			}
		}

		public double Y {
			get {
				return gtksharp_gdk_event_button_get_y (Handle);
			}
		}

		public double XRoot {
			get {
				return gtksharp_gdk_event_button_get_x_root (Handle);
			}
		}

		public double YRoot {
			get {
				return gtksharp_gdk_event_button_get_y_root (Handle);
			}
		}

		public uint Button {
			get {
				return gtksharp_gdk_event_button_get_button (Handle);
			}
		}

		public Device Device {
			get {
				return GLib.Object.GetObject (gtksharp_gdk_event_button_get_device (Handle)) as Device;
			}
		}

		public double[] Axes {
			get {
				double[] result = new double [2];
				IntPtr axes = gtksharp_gdk_event_button_get_axes (Handle);
				Marshal.Copy (result, 0, axes, 2);
				return result;
			}
		}
	}
}


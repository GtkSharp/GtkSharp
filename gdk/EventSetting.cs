// Gdk.EventSetting.cs - Custom Setting event wrapper 
//
// Author:  Mike Kestner <mkestner@ximian.com>
//
// (c) 2004 Novell, Inc.

namespace Gdk {

	using System;
	using System.Runtime.InteropServices;

	public class EventSetting : Event {

		[DllImport("gtksharpglue")]
		static extern SettingAction gtksharp_gdk_event_setting_get_action (IntPtr evt);

		[DllImport("gtksharpglue")]
		static extern IntPtr gtksharp_gdk_event_setting_get_name (IntPtr evt);

		public EventSetting (IntPtr raw) : base (raw) {} 

		public SettingAction Action {
			get {
				return gtksharp_gdk_event_setting_get_action (Handle);
			}
		}

		public string Name {
			get {
				return Marshal.PtrToStringAnsi (gtksharp_gdk_event_setting_get_name (Handle));
			}
		}
	}
}


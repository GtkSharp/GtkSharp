// Gdk.EventSetting.cs - Custom Setting event wrapper 
//
// Author:  Mike Kestner <mkestner@ximian.com>
//
// Copyright (c) 2004 Novell, Inc.
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of version 2 of the Lesser GNU General 
// Public License as published by the Free Software Foundation.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public
// License along with this program; if not, write to the
// Free Software Foundation, Inc., 59 Temple Place - Suite 330,
// Boston, MA 02111-1307, USA.


namespace Gdk {

	using System;
	using System.Runtime.InteropServices;

	public class EventSetting : Event {

		[DllImport("gdksharpglue-2")]
		static extern SettingAction gtksharp_gdk_event_setting_get_action (IntPtr evt);

		[DllImport("gdksharpglue-2")]
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


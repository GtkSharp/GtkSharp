// Gdk.EventKey.cs - Custom key event wrapper 
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

	public class EventKey : Event {

		[DllImport("gdksharpglue")]
		static extern uint gtksharp_gdk_event_key_get_time (IntPtr evt);

		[DllImport("gdksharpglue")]
		static extern uint gtksharp_gdk_event_key_get_state (IntPtr evt);

		[DllImport("gdksharpglue")]
		static extern uint gtksharp_gdk_event_key_get_keyval (IntPtr evt);

		[DllImport("gdksharpglue")]
		static extern ushort gtksharp_gdk_event_key_get_hardware_keycode (IntPtr evt);

		[DllImport("gdksharpglue")]
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

		public Key Key {
			get {
				return (Key) gtksharp_gdk_event_key_get_keyval (Handle);
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


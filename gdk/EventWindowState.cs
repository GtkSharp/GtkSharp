// Gdk.EventWindowState.cs - Custom WindowState event wrapper 
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

	public class EventWindowState : Event {

		[DllImport("gdksharpglue")]
		static extern WindowState gtksharp_gdk_event_window_state_get_changed_mask (IntPtr evt);

		[DllImport("gdksharpglue")]
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


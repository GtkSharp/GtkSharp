// Gdk.EventProperty.cs - Custom property event wrapper 
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

	public class EventProperty : Event {

		[DllImport("gdksharpglue-2")]
		static extern uint gtksharp_gdk_event_property_get_time (IntPtr evt);

		[DllImport("gdksharpglue-2")]
		static extern IntPtr gtksharp_gdk_event_property_get_atom (IntPtr evt);

		[DllImport("gdksharpglue-2")]
		static extern PropertyState gtksharp_gdk_event_property_get_state (IntPtr evt);

		public EventProperty (IntPtr raw) : base (raw) {} 

		public Atom Atom {
			get {
				return new Atom (gtksharp_gdk_event_property_get_atom (Handle));
			}
		}

		public PropertyState State {
			get {
				return gtksharp_gdk_event_property_get_state (Handle);
			}
		}

		public uint Time {
			get {
				return gtksharp_gdk_event_property_get_time (Handle);
			}
		}
	}
}


// Gdk.EventSelection.cs - Custom selection event wrapper 
//
// Author:  Mike Kestner <mkestner@ximian.com>
//
// (c) 2004 Novell, Inc.

namespace Gdk {

	using System;
	using System.Runtime.InteropServices;

	public class EventSelection : Event {

		[DllImport("gdksharpglue")]
		static extern uint gtksharp_gdk_event_selection_get_time (IntPtr evt);

		[DllImport("gdksharpglue")]
		static extern IntPtr gtksharp_gdk_event_selection_get_selection (IntPtr evt);

		[DllImport("gdksharpglue")]
		static extern IntPtr gtksharp_gdk_event_selection_get_target (IntPtr evt);

		[DllImport("gdksharpglue")]
		static extern IntPtr gtksharp_gdk_event_selection_get_property (IntPtr evt);

		[DllImport("gdksharpglue")]
		static extern uint gtksharp_gdk_event_selection_get_requestor (IntPtr evt);

		public EventSelection (IntPtr raw) : base (raw) {} 

		public Atom Property {
			get {
				return new Atom (gtksharp_gdk_event_selection_get_property (Handle));
			}
		}

		public Atom Selection {
			get {
				return new Atom (gtksharp_gdk_event_selection_get_selection (Handle));
			}
		}

		public Atom Target {
			get {
				return new Atom (gtksharp_gdk_event_selection_get_target (Handle));
			}
		}

		public uint Requestor {
			get {
				return gtksharp_gdk_event_selection_get_requestor (Handle);
			}
		}

		public uint Time {
			get {
				return gtksharp_gdk_event_selection_get_time (Handle);
			}
		}
	}
}


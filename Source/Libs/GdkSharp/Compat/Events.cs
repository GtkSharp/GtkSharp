// Gdk event compatibility - the Gtk 3 GdkEvent* structs, over Gtk 4 event controllers
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of version 2 of the Lesser GNU General
// Public License as published by the Free Software Foundation.

namespace Gdk {

	using System;

	/// <summary>
	/// The Gtk 3 GdkEvent family, re-provided as plain managed records.
	/// </summary>
	/// <remarks>
	/// <para>In Gtk 3 these were views onto a native GdkEvent union that an application could
	/// also allocate and inject. Gtk 4 removed both halves: GdkEvent is an opaque refcounted
	/// object with accessor functions, and there is no public constructor at all - an
	/// application cannot synthesize input any more.</para>
	/// <para>So these are NOT wrappers. They are values, populated by
	/// <c>Gtk.Widget</c>'s compatibility events from the real <see cref="Event"/> the
	/// controller was handling (via <c>Gtk.EventController.GetCurrentEvent</c>) plus the
	/// coordinates the controller reports. Where Gtk 4 no longer supplies a field, it is
	/// documented here rather than silently defaulted.</para>
	/// </remarks>
	public class EventAny {

		/// <summary>The Gtk 4 event this was built from, when there was one. May be null.</summary>
		public Event Event { get; set; }

		public EventType Type { get; set; }

		/// <summary>Event timestamp in milliseconds, or 0 when no native event was available.</summary>
		public uint Time { get; set; }

		public ModifierType State { get; set; }
	}

	/// <summary>Stands in for GdkEventButton.</summary>
	public class EventButton : EventAny {

		/// <summary>X in the receiving widget's coordinates.</summary>
		public double X { get; set; }

		/// <summary>Y in the receiving widget's coordinates.</summary>
		public double Y { get; set; }

		/// <summary>1 left, 2 middle, 3 right - unchanged from Gtk 3.</summary>
		public uint Button { get; set; }

		/// <summary>
		/// 1 for a single click, 2 for the second click of a double click, 3 for the third.
		/// </summary>
		/// <remarks>
		/// This is where Gtk 3's <c>EventType.TwoButtonPress</c> / <c>ThreeButtonPress</c> went.
		/// Gtk 4 deleted both enum members - a click is always <c>EventType.ButtonPress</c> - and
		/// GtkGestureClick reports the repeat count as a separate n_press argument instead.
		/// Code switching on the event type to tell single from double click must switch on this.
		/// </remarks>
		public int NPress { get; set; }

		/// <summary>
		/// X in root/screen coordinates. Gtk 4 has no equivalent - a client cannot learn
		/// the pointer's position on the screen - so this mirrors <see cref="X"/>.
		/// </summary>
		public double XRoot { get; set; }

		/// <summary>See <see cref="XRoot"/>.</summary>
		public double YRoot { get; set; }
	}

	/// <summary>Stands in for GdkEventMotion.</summary>
	public class EventMotion : EventAny {

		public double X { get; set; }

		public double Y { get; set; }

		public double XRoot { get; set; }

		public double YRoot { get; set; }
	}

	/// <summary>Stands in for GdkEventCrossing (enter-notify / leave-notify).</summary>
	public class EventCrossing : EventAny {

		public double X { get; set; }

		public double Y { get; set; }

		public CrossingMode Mode { get; set; }

		public NotifyType Detail { get; set; }
	}

	/// <summary>Stands in for GdkEventKey.</summary>
	public class EventKey : EventAny {

		/// <summary>The keyval, as <see cref="Gdk.Key"/>.</summary>
		public Key Key { get; set; }

		/// <summary>The raw keyval, for callers that compare numerically.</summary>
		public uint KeyValue { get; set; }

		/// <summary>The hardware keycode.</summary>
		public ushort HardwareKeycode { get; set; }
	}

	/// <summary>Stands in for GdkEventScroll.</summary>
	public class EventScroll : EventAny {

		public double X { get; set; }

		public double Y { get; set; }

		/// <summary>
		/// Gtk 4's scroll controller reports deltas, so this is
		/// <see cref="ScrollDirection.Smooth"/> unless the deltas are axis-aligned, in which
		/// case the matching discrete direction is reported as Gtk 3 would have.
		/// </summary>
		public ScrollDirection Direction { get; set; }

		public double DeltaX { get; set; }

		public double DeltaY { get; set; }
	}

	/// <summary>Stands in for GdkEventConfigure.</summary>
	/// <remarks>
	/// Gtk 4 has no configure-event: a widget learns its geometry from its size_allocate
	/// vfunc. <c>Gtk.Widget</c>'s compatibility ConfigureEvent raises this from there.
	/// </remarks>
	public class EventConfigure : EventAny {

		public int X { get; set; }

		public int Y { get; set; }

		public int Width { get; set; }

		public int Height { get; set; }
	}

	/// <summary>Stands in for GdkEventWindowState.</summary>
	/// <remarks>
	/// Built from GtkWindow's Maximized / Fullscreened properties, which are what is left of
	/// GdkWindowState in Gtk 4.
	/// </remarks>
	public class EventWindowState : EventAny {

		public WindowState ChangedMask { get; set; }

		public WindowState NewWindowState { get; set; }
	}

	/// <summary>Stands in for GdkEventFocus.</summary>
	public class EventFocus : EventAny {

		public bool In { get; set; }
	}

	/// <summary>
	/// Stands in for GdkWindowState, which Gtk 4 replaced with GtkWindow properties and
	/// GdkToplevelState.
	/// </summary>
	[Flags]
	public enum WindowState {
		Withdrawn = 1 << 0,
		Iconified = 1 << 1,
		Maximized = 1 << 2,
		Sticky = 1 << 3,
		Fullscreen = 1 << 4,
		Above = 1 << 5,
		Below = 1 << 6,
		Focused = 1 << 7,
		Tiled = 1 << 8,
	}

	/// <summary>
	/// Stands in for GdkEventMask. Gtk 4 has no event masks - a widget receives what its
	/// controllers ask for - so these values are accepted and ignored.
	/// </summary>
	/// <remarks>
	/// Kept as a real flags enum rather than deleted so that Gtk 3 code expressing intent
	/// (<c>AddEvents((int)(EventMask.ButtonPressMask | ...))</c>) still says what it meant.
	/// The values are Gtk 3's, so anything that persisted one still round-trips.
	/// </remarks>
	[Flags]
	public enum EventMask {
		ExposureMask = 1 << 1,
		PointerMotionMask = 1 << 2,
		PointerMotionHintMask = 1 << 3,
		ButtonMotionMask = 1 << 4,
		Button1MotionMask = 1 << 5,
		Button2MotionMask = 1 << 6,
		Button3MotionMask = 1 << 7,
		ButtonPressMask = 1 << 8,
		ButtonReleaseMask = 1 << 9,
		KeyPressMask = 1 << 10,
		KeyReleaseMask = 1 << 11,
		EnterNotifyMask = 1 << 12,
		LeaveNotifyMask = 1 << 13,
		FocusChangeMask = 1 << 14,
		StructureMask = 1 << 15,
		PropertyChangeMask = 1 << 16,
		VisibilityNotifyMask = 1 << 17,
		ProximityInMask = 1 << 18,
		ProximityOutMask = 1 << 19,
		SubstructureMask = 1 << 20,
		ScrollMask = 1 << 21,
		TouchMask = 1 << 22,
		SmoothScrollMask = 1 << 23,
		TouchpadGestureMask = 1 << 24,
		TabletPadMask = 1 << 25,
		AllEventsMask = 0x3FFFFFE,
	}
}

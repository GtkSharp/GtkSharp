// Gtk 3 input signal arguments, over Gtk 4 event controllers
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of version 2 of the Lesser GNU General
// Public License as published by the Free Software Foundation.

namespace Gtk {

	using System;

	/// <summary>
	/// Base of the Gtk 3 input signal argument classes.
	/// </summary>
	/// <remarks>
	/// <para>Gtk 3 delivered input as widget signals whose handler returned a boolean through
	/// <c>args.RetVal</c>: true meant "handled, stop propagating". Gtk 4 delivers input through
	/// event controllers instead, and the equivalent of claiming an event is
	/// <c>Gtk.Gesture.SetState(EventSequenceState.Claimed)</c> (or returning true from
	/// GtkEventControllerKey::key-pressed).</para>
	/// <para>So <see cref="RetVal"/> is honoured, not decorative: <c>Gtk.Widget</c>'s
	/// compatibility events read it back after every handler and claim the sequence when it is
	/// true. What is NOT reproduced is Gtk 3's bottom-up delivery order through the GdkWindow
	/// stack - Gtk 4 runs controllers in capture and bubble phases, and these are attached in the
	/// bubble phase, which is the closer of the two.</para>
	/// <para>These do not derive from <c>GLib.SignalArgs</c>. That class reads its values out of
	/// a native GValue array, and there is no native signal emission behind any of this.</para>
	/// </remarks>
	public class CompatSignalArgs : EventArgs {

		/// <summary>
		/// Set to true by a handler that has consumed the event, exactly as in Gtk 3.
		/// </summary>
		public object RetVal { get; set; }

		internal bool Handled {
			get { return RetVal is bool && (bool)RetVal; }
		}
	}

	public class ButtonPressEventArgs : CompatSignalArgs {
		public Gdk.EventButton Event { get; set; }
	}

	public class ButtonReleaseEventArgs : CompatSignalArgs {
		public Gdk.EventButton Event { get; set; }
	}

	public class MotionNotifyEventArgs : CompatSignalArgs {
		public Gdk.EventMotion Event { get; set; }
	}

	public class EnterNotifyEventArgs : CompatSignalArgs {
		public Gdk.EventCrossing Event { get; set; }
	}

	public class LeaveNotifyEventArgs : CompatSignalArgs {
		public Gdk.EventCrossing Event { get; set; }
	}

	public class ScrollEventArgs : CompatSignalArgs {
		public Gdk.EventScroll Event { get; set; }
	}

	public class KeyPressEventArgs : CompatSignalArgs {
		public Gdk.EventKey Event { get; set; }
	}

	public class KeyReleaseEventArgs : CompatSignalArgs {
		public Gdk.EventKey Event { get; set; }
	}

	public class FocusInEventArgs : CompatSignalArgs {
		public Gdk.EventFocus Event { get; set; }
	}

	public class FocusOutEventArgs : CompatSignalArgs {
		public Gdk.EventFocus Event { get; set; }
	}

	/// <summary>Stands in for Gtk 3's GtkWidget::focus signal arguments.</summary>
	public class FocusedArgs : CompatSignalArgs {
		public DirectionType Direction { get; set; }
	}

	public class ConfigureEventArgs : CompatSignalArgs {
		public Gdk.EventConfigure Event { get; set; }
	}

	public class WindowStateEventArgs : CompatSignalArgs {
		public Gdk.EventWindowState Event { get; set; }
	}

	public class DeleteEventArgs : CompatSignalArgs {
		public Gdk.EventAny Event { get; set; }
	}

	/// <summary>
	/// Stands in for GtkWidget::size-allocate's arguments.
	/// </summary>
	/// <remarks>
	/// Gtk 4 has no size-allocate SIGNAL - only the vfunc, which reports a width, a height and a
	/// baseline in the widget's own coordinates. The rectangle here therefore always has X and Y
	/// of zero, which is what a Gtk 3 handler saw for a windowless widget anyway.
	/// </remarks>
	public class SizeAllocatedArgs : CompatSignalArgs {
		public Gdk.Rectangle Allocation { get; set; }
	}

	public delegate void ButtonPressEventHandler(object o, ButtonPressEventArgs args);
	public delegate void ButtonReleaseEventHandler(object o, ButtonReleaseEventArgs args);
	public delegate void MotionNotifyEventHandler(object o, MotionNotifyEventArgs args);
	public delegate void EnterNotifyEventHandler(object o, EnterNotifyEventArgs args);
	public delegate void LeaveNotifyEventHandler(object o, LeaveNotifyEventArgs args);
	public delegate void ScrollEventHandler(object o, ScrollEventArgs args);
	public delegate void KeyPressEventHandler(object o, KeyPressEventArgs args);
	public delegate void KeyReleaseEventHandler(object o, KeyReleaseEventArgs args);
	public delegate void FocusInEventHandler(object o, FocusInEventArgs args);
	public delegate void FocusOutEventHandler(object o, FocusOutEventArgs args);
	public delegate void FocusedHandler(object o, FocusedArgs args);
	public delegate void ConfigureEventHandler(object o, ConfigureEventArgs args);
	public delegate void WindowStateEventHandler(object o, WindowStateEventArgs args);
	public delegate void DeleteEventHandler(object o, DeleteEventArgs args);
	public delegate void SizeAllocatedHandler(object o, SizeAllocatedArgs args);
}

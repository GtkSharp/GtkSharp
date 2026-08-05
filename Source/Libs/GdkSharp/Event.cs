// Gdk.Event.cs - Gdk Event class customizations
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of version 2 of the Lesser GNU General
// Public License as published by the Free Software Foundation.

namespace Gdk {

	using System;

	// Gtk 4 turned GdkEvent from a union of structs into a type hierarchy --
	// GdkButtonEvent, GdkKeyEvent, GdkMotionEvent and the rest -- but those are
	// GLib *fundamental* types (glib:fundamental="1" in the gir), not GObject
	// descendants. GapiCodegen's ObjectGen assumes GObject: it emits Handle,
	// CreateNativeObject and a base(IntPtr) chain-up. This class supplies that
	// surface over a plain handle so the generated subclasses compile.
	//
	// Events are never constructed from managed code -- GDK delivers them to an
	// EventController -- so CreateNativeObject is deliberately a hard failure
	// rather than a stub that appears to work.
	public partial class Event : GLib.IWrapper {

		IntPtr handle;

		public Event (IntPtr raw)
		{
			handle = raw;
		}

		public IntPtr Handle {
			get { return handle; }
		}

		public IntPtr OwnedHandle {
			get { return handle; }
		}

		// The generated code calls this wherever it would call GetObject on a
		// GObject-derived type.
		public static Event GetEvent (IntPtr raw)
		{
			return raw == IntPtr.Zero ? null : new Event (raw);
		}

		protected void CreateNativeObject (string[] names, GLib.Value[] vals)
		{
			throw new InvalidOperationException (
				"Gdk events are created by GDK and delivered to an event controller; " +
				"they cannot be constructed from managed code.");
		}
	}
}

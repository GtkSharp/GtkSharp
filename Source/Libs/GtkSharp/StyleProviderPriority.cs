// StyleProviderPriority.cs - the GTK_STYLE_PROVIDER_PRIORITY_* constants
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of version 2 of the Lesser GNU General
// Public License as published by the Free Software Foundation.

namespace Gtk {

	/// <summary>
	/// The priorities <see cref="StyleContext.AddProvider"/> and
	/// <see cref="StyleContext.AddProviderForDisplay"/> expect, in the order Gtk
	/// applies them: a higher number wins.
	/// </summary>
	/// <remarks>
	/// These are <c>&lt;constant&gt;</c> elements in Gtk-4.0.gir, and GirToGapi
	/// emits no constants at all -- so none of Gtk's 98 reach the api.xml, and
	/// nothing generated declares these. Gdk's 2459 (the GDK_KEY_* keyvals) are
	/// covered because someone hand-wrote Gdk.Key; these were simply missing, to
	/// the point that the CSS example in Docs/getting-started.md named
	/// Gtk.StyleProviderPriority.Application and did not compile.
	///
	/// Declared as constants rather than an enum because AddProvider takes a
	/// uint: any value in between is legal, and an enum would imply otherwise.
	/// </remarks>
	public static class StyleProviderPriority {

		/// <summary>For a fallback style sheet: anything else overrides it.</summary>
		public const uint Fallback = 1;

		/// <summary>The priority the current theme is loaded at.</summary>
		public const uint Theme = 200;

		/// <summary>Style information from GtkSettings, above the theme so that a
		/// setting can override it.</summary>
		public const uint Settings = 400;

		/// <summary>The one an application should use for its own style sheet.</summary>
		public const uint Application = 600;

		/// <summary>Reserved for the user's own style sheet, above everything an
		/// application sets.</summary>
		public const uint User = 800;
	}
}

// Gtk.StateType - the Gtk 3 widget state enum
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of version 2 of the Lesser GNU General
// Public License as published by the Free Software Foundation.

namespace Gtk {

	/// <summary>
	/// Stands in for GtkStateType, which Gtk 4 removed in favour of the
	/// <see cref="StateFlags"/> bitmask.
	/// </summary>
	/// <remarks>
	/// The two differ in kind, not just in spelling: a Gtk 3 widget was in exactly one state,
	/// while a Gtk 4 widget carries a set of them (hover and focus and active at once). Anything
	/// that stores or switches on a single state still compiles against this; anything that needs
	/// to know a widget's actual state should read <c>StateFlags</c> and stop round-tripping
	/// through here. <see cref="StateTypeExtensions.ToStateFlags"/> converts.
	/// </remarks>
	public enum StateType {
		Normal,
		Active,
		Prelight,
		Selected,
		Insensitive,
		Inconsistent,
		Focused,
	}

	public static class StateTypeExtensions {

		/// <summary>The <see cref="StateFlags"/> bit that corresponds to a Gtk 3 state.</summary>
		public static StateFlags ToStateFlags(this StateType state)
		{
			switch (state) {
			case StateType.Active:
				return StateFlags.Active;
			case StateType.Prelight:
				return StateFlags.Prelight;
			case StateType.Selected:
				return StateFlags.Selected;
			case StateType.Insensitive:
				return StateFlags.Insensitive;
			case StateType.Inconsistent:
				return StateFlags.Inconsistent;
			case StateType.Focused:
				return StateFlags.Focused;
			default:
				return StateFlags.Normal;
			}
		}
	}
}

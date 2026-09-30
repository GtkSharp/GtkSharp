// Gtk.ToolButton - the Gtk 3 toolbar button, over a plain Gtk 4 GtkButton
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of version 2 of the Lesser GNU General
// Public License as published by the Free Software Foundation.

namespace Gtk {

	using System;

	/// <summary>
	/// Stands in for GtkToolButton, which Gtk 4 removed along with the rest of GtkToolbar.
	/// </summary>
	/// <remarks>
	/// <para>Gtk 4's guidance is explicit about the replacement: a toolbar is a GtkBox with the
	/// "toolbar" style class, and a tool button is an ordinary GtkButton in it. There is no
	/// behaviour left in GtkToolButton that GtkButton does not have - the tool-item machinery it
	/// carried (proxy menu items, toolbar overflow, icon-size negotiation) went away with the
	/// toolbar that drove it.</para>
	/// <para>So this is a GtkButton with GtkToolButton's constructors and property names, and
	/// nothing more. It is a rename with a compatibility surface, not an emulation.</para>
	/// </remarks>
	public class ToolButton : Button {

		string _label;
		Widget _iconWidget;
		string _iconName;

		public ToolButton() : base()
		{
		}

		/// <param name="iconWidget">The widget shown as the icon, or null.</param>
		/// <param name="label">The button's label. Empty for an icon-only button.</param>
		public ToolButton(Widget iconWidget, string label) : base()
		{
			IconWidget = iconWidget;
			Label = label;
		}

		protected ToolButton(IntPtr raw) : base(raw)
		{
		}

		/// <summary>Creates a tool button showing a themed icon.</summary>
		public static ToolButton FromIconName(string iconName)
		{
			var button = new ToolButton();
			button.IconName = iconName;
			return button;
		}

		/// <summary>
		/// The button's text.
		/// </summary>
		/// <remarks>
		/// Kept separately rather than delegated to <c>Button.Label</c>, because setting an icon
		/// widget replaces the button's child - and Button.Label reads that child. Without this
		/// the label would silently come back as null the moment an icon was set, which is the
		/// normal case for a toolbar.
		/// </remarks>
		public new string Label {
			get { return _label; }
			set {
				_label = value;

				if (_iconWidget == null && string.IsNullOrEmpty(_iconName))
					base.Label = value ?? string.Empty;
				else
					TooltipText = value;
			}
		}

		/// <summary>The widget shown in place of an icon.</summary>
		public Widget IconWidget {
			get { return _iconWidget; }
			set {
				_iconWidget = value;

				if (value != null) {
					Child = value;

					// The label has nowhere to show once a custom child owns the button, so it
					// becomes the tooltip - which is what an icon-only toolbar button wants
					// anyway, and what Gtk 3 did when the toolbar style hid labels.
					if (!string.IsNullOrEmpty(_label))
						TooltipText = _label;
				}
			}
		}

		/// <summary>The name of a themed icon to show.</summary>
		public new string IconName {
			get { return _iconName; }
			set {
				_iconName = value;
				base.IconName = value;

				if (!string.IsNullOrEmpty(_label))
					TooltipText = _label;
			}
		}
	}
}

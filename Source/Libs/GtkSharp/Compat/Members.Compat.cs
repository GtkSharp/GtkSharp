// Gtk 3 members that Gtk 4 renamed, folded away or replaced, on types that still exist
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of version 2 of the Lesser GNU General
// Public License as published by the Free Software Foundation.

namespace Gtk {

	using System;

	/// <summary>
	/// Stands in for GtkShadowType, which Gtk 4 removed.
	/// </summary>
	/// <remarks>
	/// Gtk 3 drew a widget's frame from this enum; Gtk 4 draws frames from CSS, so there is no
	/// shadow style to pick. The one place the distinction survives is GtkScrolledWindow, which
	/// kept a boolean "has-frame" - so a scrolled window maps None to false and everything else to
	/// true, and the other widgets accept the value and ignore it.
	/// </remarks>
	public enum ShadowType {
		None,
		In,
		Out,
		EtchedIn,
		EtchedOut,
	}

	public partial class Widget {

		/// <summary>
		/// Stands in for GtkContainer:border-width - space kept clear inside a container, around
		/// its children.
		/// </summary>
		/// <remarks>
		/// <para>Gtk 4 removed it with GtkContainer. The replacement is CSS padding, which is what
		/// this sets: padding, not margin. The difference is not pedantic - margin is space
		/// OUTSIDE the widget, so on anything that paints (a frame, a coloured container) a margin
		/// would shrink the painted area and move the border inward, where border-width kept the
		/// frame where it was and pushed only the children in.</para>
		/// <para>Each widget that uses this gets one CssProvider, created on first set and reused
		/// afterwards.</para>
		/// </remarks>
		public uint BorderWidth {
			get { return _borderWidth; }
			set {
				if (_borderWidth == value && _borderWidthProvider != null)
					return;

				_borderWidth = value;

				if (_borderWidthProvider == null) {
					_borderWidthProvider = new CssProvider();
					StyleContext.AddProvider(_borderWidthProvider, StyleProviderPriority.Application);
				}

				_borderWidthProvider.LoadFromData(
					string.Format(System.Globalization.CultureInfo.InvariantCulture,
						"* {{ padding: {0}px; }}", value));
			}
		}

		uint _borderWidth;
		CssProvider _borderWidthProvider;

		/// <summary>
		/// Stands in for GtkWidget:margin, the Gtk 3 shorthand that set all four margins.
		/// </summary>
		/// <remarks>
		/// Gtk 4 kept the four individual properties and dropped the shorthand. The getter reports
		/// MarginTop, which is what the shorthand's getter did when the four agreed and is
		/// meaningless when they do not - so treat it as write-mostly, as Gtk 3 code does.
		/// </remarks>
		public int Margin {
			get { return MarginTop; }
			set { MarginStart = MarginEnd = MarginTop = MarginBottom = value; }
		}

		/// <summary>
		/// Stands in for GtkWidget:accessible, the ATK object Gtk 3 handed out.
		/// </summary>
		/// <remarks>
		/// Gtk 4 has no ATK: accessibility is built into GtkWidget itself, and a name or
		/// description is pushed with gtk_accessible_update_property rather than read and written
		/// through a separate object. See <see cref="AccessibleInfo"/> for what that costs.
		/// </remarks>
		public AccessibleInfo Accessible {
			get { return _accessible ?? (_accessible = new AccessibleInfo(this)); }
		}

		AccessibleInfo _accessible;

		/// <summary>
		/// Stands in for gtk_widget_get_toplevel: the window this widget is in, or null.
		/// </summary>
		/// <remarks>
		/// Gtk 4 replaced it with gtk_widget_get_root, which returns a GtkRoot - an interface,
		/// because a widget's root may be something other than a GtkWindow. Callers that only ever
		/// meant "my window" get that here; anything that needs the general case should use Root.
		/// </remarks>
		public Window Toplevel {
			get { return Root as Window; }
		}
	}

	/// <summary>
	/// Stands in for the Gtk 3 AtkObject reached through <c>Widget.Accessible</c>, over Gtk 4's
	/// GtkAccessible.
	/// </summary>
	/// <remarks>
	/// <para>The Gtk 3 name and description map onto GTK_ACCESSIBLE_PROPERTY_LABEL and
	/// GTK_ACCESSIBLE_PROPERTY_DESCRIPTION.</para>
	/// <para>The asymmetry to know about: Gtk 4 lets an application WRITE these and not read them.
	/// A widget's accessible label is normally derived from the widget itself (a button's label, a
	/// label's text) and only overridden by an explicit update; there is no getter for either the
	/// derived value or the override. So the getters here report what was set THROUGH THIS OBJECT,
	/// and null before anything was.</para>
	/// <para>That turns out to be the useful behaviour for the common pattern - save the old
	/// value, set a new one, restore the old - because restoring null resets the property, and
	/// resetting is exactly how Gtk 4 spells "go back to the widget-derived default".</para>
	/// </remarks>
	public sealed class AccessibleInfo {

		readonly Widget _widget;
		string _name;
		string _description;

		internal AccessibleInfo(Widget widget)
		{
			_widget = widget;
		}

		/// <summary>The accessible label. GTK_ACCESSIBLE_PROPERTY_LABEL.</summary>
		public string Name {
			get { return _name; }
			set {
				_name = value;
				Update(AccessibleProperty.Label, value);
			}
		}

		/// <summary>The accessible description. GTK_ACCESSIBLE_PROPERTY_DESCRIPTION.</summary>
		public string Description {
			get { return _description; }
			set {
				_description = value;
				Update(AccessibleProperty.Description, value);
			}
		}

		void Update(AccessibleProperty property, string value)
		{
			var accessible = _widget as IAccessible;

			if (accessible == null)
				return;

			if (value == null) {
				accessible.ResetProperty(property);
				return;
			}

			// InitValue, not new GLib.Value(string): gtk_accessible_update_property reads the
			// GValue as the exact type the property declares, and a GValue built from the managed
			// string is not guaranteed to be it. See the note in Accessible.cs.
			//
			// GLib.Value is a mutable struct, so it cannot be a `using` variable - assigning to
			// .Val on one would be a compile error - hence the explicit try/finally.
			GLib.Value boxed = property.InitValue();

			try {
				boxed.Val = value;
				accessible.UpdateProperty(property, boxed);
			} finally {
				boxed.Dispose();
			}
		}
	}

	public partial class TextTag {

		/// <summary>Stands in for GtkTextTag:foreground-gdk, which Gtk 4 removed with GdkColor.</summary>
		public Gdk.Color ForegroundGdk {
			get { return Gdk.Color.FromRGBA(ForegroundRgba); }
			set { ForegroundRgba = value.ToRGBA(); }
		}
	}

	public partial class CellRendererText {

		/// <summary>Stands in for GtkCellRendererText:foreground-gdk. See <see cref="TextTag.ForegroundGdk"/>.</summary>
		public Gdk.Color ForegroundGdk {
			get { return Gdk.Color.FromRGBA(ForegroundRgba); }
			set { ForegroundRgba = value.ToRGBA(); }
		}
	}

	public partial class Image {

		/// <summary>
		/// The pixbuf last set through this property, or null.
		/// </summary>
		/// <remarks>
		/// <para>Gtk 4's gtk_image_set_from_pixbuf is write-only and deprecated: an image stores a
		/// GdkPaintable internally and a pixbuf handed to it is converted to a texture on the way
		/// in, so there is no pixbuf left to give back. The Gtk 3 binding's getter is therefore
		/// gone.</para>
		/// <para>This remembers what was set, which is what code reading the property back is
		/// really asking for ("what did I put here?"). It does NOT report anything for an image
		/// filled from an icon name, a file or a paintable - those never had a pixbuf.</para>
		/// </remarks>
		public Gdk.Pixbuf Pixbuf {
			get { return _pixbuf; }
			set {
				_pixbuf = value;
				PixbufSource = value;
			}
		}

		Gdk.Pixbuf _pixbuf;
	}

	/// <summary>
	/// Stands in for GtkReliefStyle, which Gtk 4 removed.
	/// </summary>
	/// <remarks>
	/// A button's relief is a CSS matter now: the "flat" style class is what None used to mean,
	/// and Normal is a button's default appearance.
	/// </remarks>
	public enum ReliefStyle {
		Normal,
		Half,
		None,
	}

	public partial class Button {

		/// <summary>Stands in for GtkButton:relief. See <see cref="ReliefStyle"/>.</summary>
		public ReliefStyle Relief {
			get { return HasCssClass("flat") ? ReliefStyle.None : ReliefStyle.Normal; }
			set {
				if (value == ReliefStyle.Normal)
					RemoveCssClass("flat");
				else
					AddCssClass("flat");
			}
		}

		/// <summary>
		/// Stands in for GtkButton:image-position. Accepted and ignored.
		/// </summary>
		/// <remarks>
		/// Gtk 4 removed the button's built-in image/label pair entirely - a button has one child,
		/// and an icon beside a label is a GtkBox you build yourself, where the order IS the
		/// position. There is nothing left for this to set.
		/// </remarks>
		public PositionType ImagePosition { get; set; }

		/// <inheritdoc cref="ImagePosition"/>
		public void SetImagePosition(PositionType position)
		{
			ImagePosition = position;
		}
	}

	public partial class Label {

		/// <summary>Stands in for GtkLabel:line-wrap, renamed to "wrap" in Gtk 4.</summary>
		public bool LineWrap {
			get { return Wrap; }
			set { Wrap = value; }
		}
	}

	public partial class Stack {

		/// <summary>
		/// Stands in for GtkStack:homogeneous, which Gtk 4 split per axis.
		/// </summary>
		/// <remarks>
		/// Reading reports true only when both axes are homogeneous, which is what the single
		/// Gtk 3 property meant.
		/// </remarks>
		public bool Homogeneous {
			get { return Hhomogeneous && Vhomogeneous; }
			set { Hhomogeneous = Vhomogeneous = value; }
		}
	}

	public partial class ScrolledWindow {

		/// <summary>Stands in for GtkScrolledWindow:shadow-type. See <see cref="ShadowType"/>.</summary>
		public ShadowType ShadowType {
			get { return HasFrame ? ShadowType.In : ShadowType.None; }
			set { HasFrame = value != ShadowType.None; }
		}
	}

	public partial class Viewport {

		/// <summary>
		/// Stands in for GtkViewport:shadow-type. Accepted and ignored: Gtk 4's viewport draws no
		/// frame at all, and there is no has-frame to map onto as there is for a scrolled window.
		/// </summary>
		public ShadowType ShadowType { get; set; }

		/// <summary>Sets the viewport's child, as gtk_container_add did.</summary>
		public void Add(Widget widget)
		{
			Child = widget;
		}

		public void Remove(Widget widget)
		{
			if (widget != null && Child == widget)
				Child = null;
		}
	}

	public partial class Frame {

		/// <summary>
		/// Stands in for GtkFrame:shadow-type. Accepted and ignored - a Gtk 4 frame's border comes
		/// from CSS.
		/// </summary>
		public ShadowType ShadowType { get; set; }

		/// <summary>Sets the frame's child, as gtk_container_add did.</summary>
		public void Add(Widget widget)
		{
			Child = widget;
		}

		public void Remove(Widget widget)
		{
			if (widget != null && Child == widget)
				Child = null;
		}
	}

	public partial class Button {

		/// <summary>Sets the button's child, as gtk_container_add did.</summary>
		public void Add(Widget widget)
		{
			Child = widget;
		}

		public void Remove(Widget widget)
		{
			if (widget != null && Child == widget)
				Child = null;
		}
	}

	public partial class Revealer {

		/// <summary>Sets the revealer's child, as gtk_container_add did.</summary>
		public void Add(Widget widget)
		{
			Child = widget;
		}

		public void Remove(Widget widget)
		{
			if (widget != null && Child == widget)
				Child = null;
		}
	}

	public partial class CheckButton {

		/// <summary>Sets the check button's child, as gtk_container_add did.</summary>
		public void Add(Widget widget)
		{
			Child = widget;
		}

		public void Remove(Widget widget)
		{
			if (widget != null && Child == widget)
				Child = null;
		}
	}

	public partial class Overlay {

		/// <summary>
		/// Sets the overlay's main child, as gtk_container_add did.
		/// </summary>
		/// <remarks>
		/// The MAIN child, not an overlay child - which is what gtk_container_add meant for a
		/// GtkOverlay too, since overlay children went in through gtk_overlay_add_overlay. Gtk 4
		/// keeps that split as SetChild and AddOverlay; only the first lost its Gtk 3 spelling.
		/// </remarks>
		public void Add(Widget widget)
		{
			Child = widget;
		}

		public void Remove(Widget widget)
		{
			if (widget == null)
				return;

			if (Child == widget)
				Child = null;
			else if (widget.Parent == this)
				RemoveOverlay(widget);
		}
	}

	public partial class Expander {

		/// <summary>Sets the expander's child, as gtk_container_add did.</summary>
		public void Add(Widget widget)
		{
			Child = widget;
		}

		public void Remove(Widget widget)
		{
			if (widget != null && Child == widget)
				Child = null;
		}
	}

	public partial class Box {

		/// <summary>
		/// Stands in for the GtkBox child properties Gtk 3 exposed as a Box.BoxChild record.
		/// </summary>
		/// <remarks>
		/// Gtk 4 has no child properties at all. What is left of these is a position - settable by
		/// reordering - and the expand/fill flags, which became properties of the child widget
		/// itself; see <c>Box.PackStart</c>. This is a view onto the box, not a stored record, so
		/// it cannot go stale the way the Gtk 3 one could.
		/// </remarks>
		public sealed class BoxChild {

			readonly Box _box;
			readonly Widget _child;

			internal BoxChild(Box box, Widget child)
			{
				_box = box;
				_child = child;
			}

			/// <summary>The child's index among its siblings.</summary>
			public int Position {
				get {
					var children = _box.Children;

					for (int i = 0; i < children.Length; i++) {
						if (children[i] == _child)
							return i;
					}

					return -1;
				}
				set { _box.ReorderChild(_child, value); }
			}

			/// <summary>See <see cref="Box.PackStart"/>: this now lives on the child.</summary>
			public bool Expand {
				get {
					return _box.Orientation == Orientation.Horizontal
						? _child.Hexpand
						: _child.Vexpand;
				}
				set {
					if (_box.Orientation == Orientation.Horizontal)
						_child.Hexpand = value;
					else
						_child.Vexpand = value;
				}
			}

			/// <summary>See <see cref="Box.PackStart"/>: this now lives on the child.</summary>
			public bool Fill {
				get {
					var align = _box.Orientation == Orientation.Horizontal
						? _child.Halign
						: _child.Valign;

					return align == Align.Fill;
				}
				set {
					var align = value ? Align.Fill : Align.Start;

					if (_box.Orientation == Orientation.Horizontal)
						_child.Halign = align;
					else
						_child.Valign = align;
				}
			}
		}

		/// <summary>The packing view for one of this box's children.</summary>
		public BoxChild this[Widget child] {
			get { return new BoxChild(this, child); }
		}

		/// <summary>Moves a child to a given index, as gtk_box_reorder_child did.</summary>
		/// <remarks>
		/// Gtk 4 replaced it with gtk_box_reorder_child_after, which names a sibling rather than a
		/// position - so the sibling to sit after is the child currently at index - 1, and a null
		/// sibling means "first".
		/// </remarks>
		public void ReorderChild(Widget child, int position)
		{
			if (child == null || child.Parent != this)
				return;

			if (position <= 0) {
				ReorderChildAfter(child, null);
				return;
			}

			var children = Children;

			// Index among the OTHER children: the child being moved still occupies a slot, so
			// using the raw list would place it one short whenever it starts before the target.
			int seen = 0;

			foreach (var sibling in children) {
				if (sibling == child)
					continue;

				if (seen == position - 1) {
					ReorderChildAfter(child, sibling);
					return;
				}

				seen++;
			}

			// Past the end: leave it where Gtk 3 would have, at the back.
			if (children.Length > 1)
				ReorderChildAfter(child, children[children.Length - 1]);
		}

		/// <summary>Sets a child's expand/fill flags, as gtk_box_set_child_packing did.</summary>
		/// <remarks>
		/// The padding and pack-type arguments are gone: padding is a margin on the child (set it
		/// directly), and pack type is a matter of which end the child was appended to.
		/// </remarks>
		public void SetChildPacking(Widget child, bool expand, bool fill, uint padding, PackType packType)
		{
			if (child == null || child.Parent != this)
				return;

			var packing = this[child];
			packing.Expand = expand;
			packing.Fill = fill;

			if (padding != 0)
				child.MarginStart = child.MarginEnd = child.MarginTop = child.MarginBottom = (int)padding;
		}
	}

}

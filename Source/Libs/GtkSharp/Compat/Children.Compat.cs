// Gtk 3 child management (Add / Remove / Children / PackStart) on the Gtk 4 widgets that lost it
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of version 2 of the Lesser GNU General
// Public License as published by the Free Software Foundation.

namespace Gtk {

	using System;
	using System.Collections.Generic;

	/// <summary>
	/// Walks a Gtk 4 widget's children.
	/// </summary>
	/// <remarks>
	/// Gtk 4 has no child list to hand back: children are a linked list reached through
	/// GetFirstChild and GetNextSibling. Enumerating it into an array is what the Gtk 3
	/// <c>Children</c> property did, and it also makes the result safe to mutate while iterating -
	/// which matters, because the overwhelmingly common use of <c>Children</c> is to remove them.
	/// </remarks>
	static class ChildEnumerator {

		public static Widget[] ChildrenOf(Widget parent)
		{
			var children = new List<Widget>();

			for (Widget child = parent.FirstChild; child != null; child = child.NextSibling)
				children.Add(child);

			return children.ToArray();
		}
	}

	/// <summary>
	/// Decides whether a child may be attached, and detaches it from a previous parent first.
	/// </summary>
	/// <remarks>
	/// <para>Every Gtk 4 attach primitive - gtk_box_append, gtk_fixed_put, gtk_grid_attach - asserts
	/// that the child has no parent, and <b>does nothing at all when the assertion fails</b>. The
	/// child is then silently missing from the tree, which surfaces much later as content that does
	/// not render rather than as an error at the call site.</para>
	/// <para>gtk_container_add, which these shims stand in for, did not behave that way: re-adding a
	/// child to the container it was already in was a warned no-op, so Gtk 3 code treats a repeat
	/// Add as harmless. That case is preserved exactly here.</para>
	/// <para>The other case - the child is parented somewhere ELSE - is deliberately an improvement
	/// on Gtk 3 rather than a copy of it. Gtk 3 refused and warned, leaving the caller with a child
	/// still in its old parent; here it is detached and moved, which is what a caller writing
	/// <c>newParent.Add(child)</c> plainly means. Detaching goes through the owning widget's own
	/// remove where there is one, not gtk_widget_unparent, because a container keeps bookkeeping of
	/// its own that unparent would leave stale.</para>
	/// <para><see cref="Container.Add"/> has always had the first half of this guard; these
	/// partials simply never did.</para>
	/// </remarks>
	static class ChildAttach {

		/// <summary>
		/// True when the caller should go ahead and attach <paramref name="child"/>.
		/// </summary>
		public static bool Prepare(Widget child, Widget newParent)
		{
			if (child == null)
				return false;

			Widget parent = child.Parent;

			if (parent == newParent)
				return false;

			if (parent != null)
				Detach(parent, child);

			return true;
		}

		static void Detach(Widget parent, Widget child)
		{
			switch (parent) {
			case Container container:
				container.Remove(child);
				break;
			case Box box:
				box.Remove(child);
				break;
			case Grid grid:
				grid.Remove(child);
				break;
			case Fixed fixd:
				fixd.Remove(child);
				break;
			default:
				child.Unparent();
				break;
			}
		}
	}

	public partial class Box {

		/// <summary>Appends a child, as gtk_box_pack_start did.</summary>
		/// <remarks>
		/// <para>Gtk 4 replaced pack_start's expand/fill/padding arguments with per-child
		/// properties, and the mapping is exact: expand becomes the child's Hexpand or Vexpand
		/// along the box's orientation, fill becomes Halign/Valign of Fill rather than Start, and
		/// padding becomes a margin on the two edges perpendicular to nothing - i.e. all four,
		/// which is what pack_start's padding did.</para>
		/// <para>Setting them on the CHILD is the part worth noticing: in Gtk 3 these were
		/// container child properties, owned by the box, and a child moved to another container
		/// left them behind. Here they travel with the child.</para>
		/// </remarks>
		public void PackStart(Widget child, bool expand, bool fill, uint padding)
		{
			if (!ChildAttach.Prepare(child, this))
				return;

			ApplyPacking(child, expand, fill, padding);
			Append(child);
		}

		/// <summary>Prepends a child, as gtk_box_pack_end did.</summary>
		public void PackEnd(Widget child, bool expand, bool fill, uint padding)
		{
			if (!ChildAttach.Prepare(child, this))
				return;

			ApplyPacking(child, expand, fill, padding);
			Prepend(child);
		}

		void ApplyPacking(Widget child, bool expand, bool fill, uint padding)
		{
			if (Orientation == Orientation.Horizontal) {
				child.Hexpand = expand;
				child.Halign = fill ? Align.Fill : Align.Start;
			} else {
				child.Vexpand = expand;
				child.Valign = fill ? Align.Fill : Align.Start;
			}

			if (padding == 0)
				return;

			child.MarginStart = child.MarginEnd = child.MarginTop = child.MarginBottom = (int)padding;
		}

		/// <summary>Appends a child, as gtk_container_add did for a box.</summary>
		public void Add(Widget widget)
		{
			if (ChildAttach.Prepare(widget, this))
				Append(widget);
		}

		// No Remove: gtk_box_remove survived into Gtk 4 and is generated already.

		/// <summary>The children, as gtk_container_get_children returned them.</summary>
		public Widget[] Children {
			get { return ChildEnumerator.ChildrenOf(this); }
		}

		/// <summary>
		/// Stands in for gtk_container_get_children's Gtk 3 binding name on a box.
		/// </summary>
		/// <remarks>
		/// GtkContainer had both Children and AllChildren; they differed only for GtkTextView and
		/// friends, which kept internal children out of the first. A box has no internal children,
		/// so the two were always the same list here.
		/// </remarks>
		public Widget[] AllChildren {
			get { return Children; }
		}
	}

	/// <remarks>
	/// IEnumerable is declared here and nowhere else: it is what makes a C# collection
	/// initializer - <c>new Fixed { child, child }</c> - legal, which Gtk 3 code uses because
	/// GtkContainer's binding implemented it. Partial declarations union their interface lists, so
	/// this adds it to the generated class without touching generated code.
	/// </remarks>
	public partial class Fixed : System.Collections.IEnumerable {

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return ChildEnumerator.ChildrenOf(this).GetEnumerator();
		}
	}

	public partial class Fixed {

		/// <summary>
		/// Adds a child at the origin, as gtk_container_add did for a GtkFixed.
		/// </summary>
		/// <remarks>
		/// Gtk 3's GtkFixed inherited container_add and dropped the child at (0, 0); code that
		/// then called Move relied on exactly that. gtk_fixed_put with a zero offset is the same
		/// thing spelled out.
		/// </remarks>
		public void Add(Widget widget)
		{
			if (ChildAttach.Prepare(widget, this))
				Put(widget, 0, 0);
		}

		public Widget[] Children {
			get { return ChildEnumerator.ChildrenOf(this); }
		}
	}

	public partial class Grid {

		/// <summary>
		/// Adds a child in the first free row of the first column, as gtk_container_add did for a
		/// GtkGrid.
		/// </summary>
		public void Add(Widget widget)
		{
			if (!ChildAttach.Prepare(widget, this))
				return;

			Attach(widget, 0, ChildEnumerator.ChildrenOf(this).Length, 1, 1);
		}

		public Widget[] Children {
			get { return ChildEnumerator.ChildrenOf(this); }
		}
	}

	public partial class Window {

		/// <summary>Sets the window's child, as gtk_container_add did for a GtkWindow.</summary>
		/// <remarks>
		/// A Gtk 3 window was a GtkBin: one child, and adding a second was a warning rather than
		/// a second child. Gtk 4 says so in the API - there is only SetChild - so this is a
		/// rename, not a behaviour change.
		/// </remarks>
		public void Add(Widget widget)
		{
			Child = widget;
		}

		/// <summary>Removes the window's child.</summary>
		public void Remove(Widget widget)
		{
			if (widget != null && Child == widget)
				Child = null;
		}

		/// <summary>The window's child, as a one- or zero-element array.</summary>
		public Widget[] Children {
			get { return Child == null ? new Widget[0] : new[] { Child }; }
		}
	}

	public partial class ScrolledWindow {

		/// <summary>Sets the scrolled window's child, as gtk_container_add did.</summary>
		/// <remarks>
		/// In Gtk 3 this also wrapped a non-scrollable child in a GtkViewport automatically.
		/// Gtk 4's gtk_scrolled_window_set_child does the same, so the convenience survives.
		/// </remarks>
		public void Add(Widget widget)
		{
			Child = widget;
		}

		public void Remove(Widget widget)
		{
			if (widget != null && Child == widget)
				Child = null;
		}

		public Widget[] Children {
			get { return Child == null ? new Widget[0] : new[] { Child }; }
		}
	}
}

// Gtk.EventBox - the Gtk 3 GtkEventBox, over a plain Gtk 4 widget
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of version 2 of the Lesser GNU General
// Public License as published by the Free Software Foundation.

namespace Gtk {

	using System;

	/// <summary>
	/// Stands in for GtkEventBox, which Gtk 4 removed.
	/// </summary>
	/// <remarks>
	/// <para>GtkEventBox existed for one reason: in Gtk 3 only a widget owning a GdkWindow could
	/// receive input, and most widgets did not own one, so wrapping them in an event box gave
	/// them a window to receive events on. Gtk 4 has no per-widget GdkWindows and every widget
	/// can take an event controller, so the whole problem is gone and with it the class.</para>
	/// <para>What is left worth preserving is the shape of the API: a container that draws a
	/// background and hands out input events. Both come from elsewhere now - the events from
	/// <see cref="Widget"/>'s compatibility events, the drawing from
	/// <see cref="OnDrawn"/> over the snapshot vfunc - so this class is thin.</para>
	/// </remarks>
	public class EventBox : Container {

		/// <summary>
		/// Stands in for GtkEventBox:visible-window. Accepted and ignored.
		/// </summary>
		/// <remarks>
		/// It selected whether the event box got its own GdkWindow or shared its parent's. There
		/// are no GdkWindows in Gtk 4, and a widget receives input either way, so there is nothing
		/// left for it to select. Kept as a settable property because Gtk 3 code sets it in a
		/// constructor and would otherwise not compile.
		/// </remarks>
		public bool VisibleWindow { get; set; }

		public EventBox() : base()
		{
		}

		/// <inheritdoc cref="Container(IntPtr)"/>
		protected EventBox(IntPtr raw) : base(raw)
		{
		}

		/// <summary>
		/// True: a GtkEventBox filled itself with its child.
		/// </summary>
		/// <remarks>
		/// <para>GtkEventBox was a GtkBin, and gtk_event_box_size_allocate handed the child the box's
		/// whole allocation less the border width. <see cref="Container"/> defaults to GtkFixed's
		/// answer instead - the child's own request - which for an event box leaves content pinned at
		/// its natural size inside a correctly sized parent, and content laid out far too small reads
		/// as content that is not there at all.</para>
		/// <para>MEASURED: a window of 800x600 holding an event box holding an event box holding a
		/// Grid allocated the outer box 800x561 and the inner one 44x88 - its natural size - and
		/// everything below collapsed to match. The collapse is the lesser half: deriving a child's
		/// size request from its own allocation is an ordinary pattern for a framework that owns its
		/// layout, and it becomes self-amplifying the moment the parent's allocation is a function of
		/// the child's request. Measured against a consumer that does exactly that, a page's height
		/// climbed 4048 -> 8440 -> 12832 -> 17224, 72px a frame, with no upper bound, inside a window
		/// that stayed 600px tall.</para>
		/// </remarks>
		protected override bool ChildrenFillAllocation {
			get { return true; }
		}

		/// <summary>
		/// Stands in for GtkWidget::draw, the Gtk 3 drawing vfunc, and receives a Cairo context
		/// in the widget's own coordinates.
		/// </summary>
		/// <remarks>
		/// <para>Gtk 4 draws through GskRenderNodes rather than Cairo: the vfunc is snapshot, and
		/// it appends nodes to a tree that a renderer replays, possibly on the GPU. A Cairo node
		/// is still one of the node kinds, and <c>Gtk.Snapshot.AppendCairo</c> is how you get
		/// one - so Cairo drawing keeps working, at the cost of rasterising into a texture rather
		/// than being composited.</para>
		/// <para>The context arrives clipped to the bounds passed to AppendCairo, i.e. this
		/// widget's allocation. That is a genuine improvement on Gtk 3, where a windowless widget
		/// received a context clipped to the damage region of the SHARED parent GdkWindow and an
		/// unguarded <c>Paint()</c> would happily cover its siblings.</para>
		/// <para>Returning true means "handled, do not draw the default" - the Gtk 3 convention.
		/// Children are snapshotted by the base implementation either way.</para>
		/// </remarks>
		protected virtual bool OnDrawn(Cairo.Context cr)
		{
			return false;
		}

		protected override void OnSnapshot(Snapshot snapshot)
		{
			CompatVFunc.Snapshot(this, snapshot, OnDrawn);
			base.OnSnapshot(snapshot);
		}
	}
}

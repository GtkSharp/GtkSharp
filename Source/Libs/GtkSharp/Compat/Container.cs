// Gtk.Container - the Gtk 3 GtkContainer, over Gtk 4 widget parenting
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of version 2 of the Lesser GNU General
// Public License as published by the Free Software Foundation.

namespace Gtk {

	using System;
	using System.Collections.Generic;

	/// <summary>
	/// Stands in for GtkContainer, which Gtk 4 removed.
	/// </summary>
	/// <remarks>
	/// <para>Gtk 4 has no container class at all: any widget may hold children, attached with
	/// <c>Widget.Parent</c> and detached with <c>Unparent</c>, and "being a container" just means
	/// measuring and allocating them from the measure and size_allocate vfuncs.</para>
	/// <para>The layout implemented here is GtkFixed's - each child sits at an (x, y) offset and
	/// gets the size it asked for - because that is what Gtk 3 code reaching for a bare container
	/// and positioning children by hand was relying on. It is deliberately NOT derived from
	/// <see cref="Fixed"/>: Fixed installs a GtkFixedLayout, and a layout manager answers the
	/// measure vfunc INSTEAD of the widget, so an <c>OnMeasure</c> override on a Fixed subclass is
	/// never called. Deriving from bare <see cref="Widget"/> keeps the vfuncs reachable, which is
	/// what subclasses here need.</para>
	/// </remarks>
	public class Container : Widget {

		sealed class Placement {
			public Widget Child;
			public int X;
			public int Y;
		}

		readonly List<Placement> _placements = new List<Placement>();

		public Container() : base()
		{
		}

		/// <remarks>
		/// Every managed GObject subclass that native code may hand back needs an (IntPtr)
		/// constructor - Gtk.Builder is the usual way to find out, and it finds out at run time,
		/// by throwing. Subclasses of this one need their own; this is the link in the chain.
		/// </remarks>
		public Container(IntPtr raw) : base(raw)
		{
		}

		/// <summary>Adds a child at the origin, as gtk_container_add did.</summary>
		public virtual void Add(Widget widget)
		{
			if (widget == null || Find(widget) != null)
				return;

			_placements.Add(new Placement { Child = widget });
			widget.Parent = this;
			QueueResize();
		}

		/// <summary>Removes a child, as gtk_container_remove did.</summary>
		public virtual void Remove(Widget widget)
		{
			var placement = Find(widget);

			if (placement == null)
				return;

			_placements.Remove(placement);
			widget.Unparent();
			QueueResize();
		}

		/// <summary>Moves an existing child, as gtk_fixed_move did.</summary>
		public void Move(Widget widget, int x, int y)
		{
			var placement = Find(widget);

			if (placement == null)
				return;

			if (placement.X == x && placement.Y == y)
				return;

			placement.X = x;
			placement.Y = y;

			// QueueAllocate, not QueueResize: the child's position changed, its size request did
			// not, and asking for a full resize from an animation loop that moves a child every
			// frame is how the Gtk 3 version of this became a measurable performance problem.
			QueueAllocate();
		}

		/// <summary>Adds a child at a given position, as gtk_fixed_put did.</summary>
		public void Put(Widget widget, int x, int y)
		{
			Add(widget);
			Move(widget, x, y);
		}

		/// <summary>The children, in the order they were added.</summary>
		public Widget[] Children {
			get {
				var children = new Widget[_placements.Count];

				for (int i = 0; i < _placements.Count; i++)
					children[i] = _placements[i].Child;

				return children;
			}
		}

		/// <summary>
		/// The single child, for the Gtk 3 GtkBin API. Null when there is none; the first when
		/// there are several.
		/// </summary>
		public Widget Child {
			get { return _placements.Count == 0 ? null : _placements[0].Child; }
			set {
				foreach (var placement in Children)
					Remove(placement);

				if (value != null)
					Add(value);
			}
		}

		Placement Find(Widget widget)
		{
			foreach (var placement in _placements) {
				if (placement.Child == widget)
					return placement;
			}

			return null;
		}

		// ------------------------------------------------------------ measure / allocate

		/// <summary>
		/// Stands in for GtkWidget's Gtk 3 get_preferred_width vfunc. Override to change the
		/// width this container asks for.
		/// </summary>
		protected virtual void OnGetPreferredWidth(out int minimumWidth, out int naturalWidth)
		{
			MeasureChildren(Orientation.Horizontal, out minimumWidth, out naturalWidth);
		}

		/// <summary>
		/// Stands in for GtkWidget's Gtk 3 get_preferred_height vfunc.
		/// </summary>
		protected virtual void OnGetPreferredHeight(out int minimumHeight, out int naturalHeight)
		{
			MeasureChildren(Orientation.Vertical, out minimumHeight, out naturalHeight);
		}

		protected override void OnMeasure(Orientation orientation, int forSize,
		                                  out int minimum, out int natural,
		                                  out int minimumBaseline, out int naturalBaseline)
		{
			if (orientation == Orientation.Horizontal)
				OnGetPreferredWidth(out minimum, out natural);
			else
				OnGetPreferredHeight(out minimum, out natural);

			// -1 means "no baseline", which is what a container full of arbitrary children has.
			minimumBaseline = naturalBaseline = -1;
		}

		void MeasureChildren(Orientation orientation, out int minimum, out int natural)
		{
			minimum = 0;
			natural = 0;

			foreach (var placement in _placements) {
				if (!placement.Child.Visible)
					continue;

				int childMinimum, childNatural, ignored;
				placement.Child.Measure(orientation, -1, out childMinimum, out childNatural,
					out ignored, out ignored);

				int offset = orientation == Orientation.Horizontal ? placement.X : placement.Y;

				minimum = Math.Max(minimum, offset + childMinimum);
				natural = Math.Max(natural, offset + childNatural);
			}
		}

		/// <summary>
		/// Stands in for the Gtk 3 size_allocate vfunc. The rectangle's origin is always (0, 0):
		/// Gtk 4 allocates in the widget's own coordinates, so there is no parent-relative
		/// position to report.
		/// </summary>
		protected virtual void OnSizeAllocated(Gdk.Rectangle allocation)
		{
			AllocateChildren();
			RaiseSizeAllocated(allocation);
		}

		protected override void OnSizeAllocate(int width, int height, int baseline)
		{
			OnSizeAllocated(new Gdk.Rectangle(0, 0, width, height));
		}

		/// <summary>
		/// The size a child is given: false - GtkFixed's answer, whatever the child asks for - by
		/// default, true for the space left inside this widget.
		/// </summary>
		/// <remarks>
		/// Gtk 3 had both answers, and the difference is load-bearing. GtkFixed gave a child its own
		/// request, which is what code positioning children by hand relies on, so it stays the
		/// default here; the GtkBin subclasses - GtkEventBox, GtkFrame, GtkViewport - gave their
		/// single child the bin's whole allocation. <see cref="EventBox"/> is one of the latter and
		/// overrides this.
		/// </remarks>
		protected virtual bool ChildrenFillAllocation {
			get { return false; }
		}

		/// <summary>
		/// Allocates every child at its recorded position, at the size it asks for - or, when
		/// <see cref="ChildrenFillAllocation"/> is set, at the space left inside this widget.
		/// Subclasses that override <see cref="OnSizeAllocated"/> and do their own layout need not
		/// call this.
		/// </summary>
		protected void AllocateChildren()
		{
			foreach (var placement in _placements) {
				if (!placement.Child.Visible)
					continue;

				int width, height, ignored;

				if (ChildrenFillAllocation) {
					// Width/Height, not the child's request: gtk_widget_allocate stores this widget's
					// size before it invokes the size_allocate vfunc, so these are the allocation
					// currently being handed out.
					width = Math.Max(0, Width - placement.X);
					height = Math.Max(0, Height - placement.Y);

					// Measured anyway, and not optionally: Gtk 4 refuses to propagate an allocation to
					// a widget it has not measured, whatever size the caller chose.
					placement.Child.Measure(Orientation.Horizontal, -1, out ignored, out ignored, out ignored, out ignored);
					placement.Child.Measure(Orientation.Vertical, width, out ignored, out ignored, out ignored, out ignored);
				} else {
					placement.Child.Measure(Orientation.Horizontal, -1, out ignored, out width, out ignored, out ignored);
					placement.Child.Measure(Orientation.Vertical, width, out ignored, out height, out ignored, out ignored);
				}

				Gsk.Transform transform = null;

				if (placement.X != 0 || placement.Y != 0) {
					var offset = new Graphene.Point();
					offset.Init(placement.X, placement.Y);
					transform = new Gsk.Transform().Translate(offset);
				}

				placement.Child.Allocate(width, height, -1, transform);
			}
		}

		SizeAllocatedHandler _sizeAllocated;

		/// <summary>Stands in for GtkWidget::size-allocate, which Gtk 4 removed as a signal.</summary>
		public event SizeAllocatedHandler SizeAllocated {
			add { _sizeAllocated += value; }
			remove { _sizeAllocated -= value; }
		}

		protected void RaiseSizeAllocated(Gdk.Rectangle allocation)
		{
			if (_sizeAllocated != null)
				_sizeAllocated(this, new SizeAllocatedArgs { Allocation = allocation });
		}

		// ------------------------------------------------------------ teardown

		/// <remarks>
		/// Gtk 4 warns ("finalized while it still has children") if a widget is finalized with
		/// children still parented to it, so a container has to let go of them explicitly.
		/// </remarks>
		protected override void Dispose(bool disposing)
		{
			if (disposing) {
				foreach (var placement in _placements) {
					if (placement.Child.Handle != IntPtr.Zero)
						placement.Child.Unparent();
				}

				_placements.Clear();
			}

			base.Dispose(disposing);
		}
	}
}

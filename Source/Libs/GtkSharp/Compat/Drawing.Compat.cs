// Gtk 3 draw / size-allocate vfuncs on the Gtk 4 widgets that Gtk 3 code subclasses
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of version 2 of the Lesser GNU General
// Public License as published by the Free Software Foundation.

namespace Gtk {

	using System;

	/// <summary>
	/// The shared bodies of the <c>OnDrawn</c> / <c>OnSizeAllocated</c> compatibility overrides.
	/// </summary>
	/// <remarks>
	/// <para>Gtk 3's drawing vfunc was <c>draw(cr)</c> and its layout vfunc was
	/// <c>size_allocate(rect)</c>. Gtk 4 renamed and reshaped both: drawing is
	/// <c>snapshot(GtkSnapshot)</c>, which builds a GskRenderNode tree rather than painting, and
	/// allocation is <c>size_allocate(width, height, baseline)</c> in the widget's own
	/// coordinates. Cairo survives as one node kind, reached through
	/// <c>Gtk.Snapshot.AppendCairo</c>.</para>
	/// <para>These live on each concrete widget class rather than on <see cref="Widget"/> because
	/// a partial class cannot override its own virtual method: <c>Gtk.Widget.OnSnapshot</c> is
	/// declared in Widget's generated half, so the override has to sit in a SUBCLASS. Hence one
	/// small partial per widget that Gtk 3 code is known to subclass, all forwarding here.</para>
	/// <para>One caveat that cannot be fixed from this side: a widget with a GtkLayoutManager -
	/// <see cref="Box"/>, <see cref="Grid"/> and <see cref="Fixed"/> all have one - has its
	/// MEASURE vfunc answered by the layout manager, so an <c>OnMeasure</c> override on such a
	/// subclass is never called. Snapshot and size_allocate are unaffected, which is why only
	/// those two are offered here. Code that needs to control a Box subclass's preferred size
	/// must set a size request or drop the layout manager.</para>
	/// </remarks>
	static class CompatVFunc {

		/// <summary>
		/// Runs a Gtk 3 <c>draw</c> handler against a Cairo context covering the widget, then
		/// lets the widget snapshot itself and its children as usual.
		/// </summary>
		/// <remarks>
		/// The Gtk 3 convention is preserved: the handler runs BEFORE the default drawing, and
		/// its return value is "I handled this". Unlike Gtk 3, the context is clipped to this
		/// widget's own bounds rather than to the damage region of a shared parent GdkWindow, so
		/// an unguarded <c>Paint()</c> can no longer cover a sibling.
		/// </remarks>
		public static void Snapshot(Widget widget, Gtk.Snapshot snapshot, Func<Cairo.Context, bool> onDrawn)
		{
			int width = widget.Width;
			int height = widget.Height;

			if (width <= 0 || height <= 0)
				return;

			// Alloc, not new: Graphene.Rect is a GLib.Opaque over a heap-allocated
			// graphene_rect_t, so the only managed constructor takes the pointer.
			var bounds = Graphene.Rect.Alloc();
			bounds.Init(0, 0, width, height);

			using (var cr = snapshot.AppendCairo(bounds))
				onDrawn(cr);
		}

		/// <summary>
		/// The rectangle a Gtk 3 size_allocate handler expected.
		/// </summary>
		/// <remarks>
		/// X and Y are always zero. In Gtk 3 they were the widget's position in its PARENT's
		/// coordinates; Gtk 4 allocates in the widget's own, with position carried separately by
		/// a transform, so there is no parent-relative origin left to report. A handler that read
		/// them to learn where it sits must ask its parent instead.
		/// </remarks>
		public static Gdk.Rectangle Allocation(int width, int height)
		{
			return new Gdk.Rectangle(0, 0, width, height);
		}
	}

	public partial class Window {

		/// <inheritdoc cref="EventBox.OnDrawn"/>
		protected virtual bool OnDrawn(Cairo.Context cr)
		{
			return false;
		}

		protected override void OnSnapshot(Snapshot snapshot)
		{
			CompatVFunc.Snapshot(this, snapshot, OnDrawn);
			base.OnSnapshot(snapshot);
		}

		/// <summary>Stands in for the Gtk 3 size_allocate vfunc. See <see cref="CompatVFunc"/>.</summary>
		protected virtual void OnSizeAllocated(Gdk.Rectangle allocation)
		{
		}

		protected override void OnSizeAllocate(int width, int height, int baseline)
		{
			base.OnSizeAllocate(width, height, baseline);

			var allocation = CompatVFunc.Allocation(width, height);

			OnSizeAllocated(allocation);

			if (_sizeAllocated != null)
				_sizeAllocated(this, new SizeAllocatedArgs { Allocation = allocation });
		}

		SizeAllocatedHandler _sizeAllocated;

		/// <summary>Stands in for GtkWidget::size-allocate, which Gtk 4 removed as a signal.</summary>
		public event SizeAllocatedHandler SizeAllocated {
			add { _sizeAllocated += value; }
			remove { _sizeAllocated -= value; }
		}
	}

	public partial class Box {

		/// <inheritdoc cref="EventBox.OnDrawn"/>
		protected virtual bool OnDrawn(Cairo.Context cr)
		{
			return false;
		}

		protected override void OnSnapshot(Snapshot snapshot)
		{
			CompatVFunc.Snapshot(this, snapshot, OnDrawn);
			base.OnSnapshot(snapshot);
		}

		/// <summary>Stands in for the Gtk 3 size_allocate vfunc. See <see cref="CompatVFunc"/>.</summary>
		protected virtual void OnSizeAllocated(Gdk.Rectangle allocation)
		{
		}

		protected override void OnSizeAllocate(int width, int height, int baseline)
		{
			// Chain up FIRST: a Box lays its children out through a GtkBoxLayout, and that runs
			// from the base implementation. A handler that inspects child geometry would
			// otherwise see the previous frame's.
			base.OnSizeAllocate(width, height, baseline);

			var allocation = CompatVFunc.Allocation(width, height);

			OnSizeAllocated(allocation);

			if (_sizeAllocated != null)
				_sizeAllocated(this, new SizeAllocatedArgs { Allocation = allocation });
		}

		SizeAllocatedHandler _sizeAllocated;

		/// <summary>Stands in for GtkWidget::size-allocate, which Gtk 4 removed as a signal.</summary>
		public event SizeAllocatedHandler SizeAllocated {
			add { _sizeAllocated += value; }
			remove { _sizeAllocated -= value; }
		}
	}

	public partial class Fixed {

		/// <inheritdoc cref="EventBox.OnDrawn"/>
		protected virtual bool OnDrawn(Cairo.Context cr)
		{
			return false;
		}

		protected override void OnSnapshot(Snapshot snapshot)
		{
			CompatVFunc.Snapshot(this, snapshot, OnDrawn);
			base.OnSnapshot(snapshot);
		}

		/// <summary>Stands in for the Gtk 3 size_allocate vfunc. See <see cref="CompatVFunc"/>.</summary>
		protected virtual void OnSizeAllocated(Gdk.Rectangle allocation)
		{
		}

		protected override void OnSizeAllocate(int width, int height, int baseline)
		{
			base.OnSizeAllocate(width, height, baseline);

			var allocation = CompatVFunc.Allocation(width, height);

			OnSizeAllocated(allocation);

			if (_sizeAllocated != null)
				_sizeAllocated(this, new SizeAllocatedArgs { Allocation = allocation });
		}

		SizeAllocatedHandler _sizeAllocated;

		/// <summary>Stands in for GtkWidget::size-allocate, which Gtk 4 removed as a signal.</summary>
		public event SizeAllocatedHandler SizeAllocated {
			add { _sizeAllocated += value; }
			remove { _sizeAllocated -= value; }
		}
	}

	public partial class Grid {

		/// <inheritdoc cref="EventBox.OnDrawn"/>
		protected virtual bool OnDrawn(Cairo.Context cr)
		{
			return false;
		}

		protected override void OnSnapshot(Snapshot snapshot)
		{
			CompatVFunc.Snapshot(this, snapshot, OnDrawn);
			base.OnSnapshot(snapshot);
		}

		/// <summary>Stands in for the Gtk 3 size_allocate vfunc. See <see cref="CompatVFunc"/>.</summary>
		protected virtual void OnSizeAllocated(Gdk.Rectangle allocation)
		{
		}

		protected override void OnSizeAllocate(int width, int height, int baseline)
		{
			base.OnSizeAllocate(width, height, baseline);

			var allocation = CompatVFunc.Allocation(width, height);

			OnSizeAllocated(allocation);

			if (_sizeAllocated != null)
				_sizeAllocated(this, new SizeAllocatedArgs { Allocation = allocation });
		}

		SizeAllocatedHandler _sizeAllocated;

		/// <summary>Stands in for GtkWidget::size-allocate, which Gtk 4 removed as a signal.</summary>
		public event SizeAllocatedHandler SizeAllocated {
			add { _sizeAllocated += value; }
			remove { _sizeAllocated -= value; }
		}
	}

	public partial class Button {

		/// <inheritdoc cref="EventBox.OnDrawn"/>
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

	public partial class Frame {

		/// <inheritdoc cref="EventBox.OnDrawn"/>
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

	public partial class DrawingArea {

		/// <summary>
		/// Stands in for GtkWidget::draw on a drawing area.
		/// </summary>
		/// <remarks>
		/// Gtk 4's own answer here is <c>SetDrawFunc</c>, which is preferable for new code - it
		/// hands out a Cairo context sized to the content area without any subclassing. This
		/// override exists because Gtk 3 code subclasses GtkDrawingArea and overrides draw, and
		/// the two coexist: a draw func, if one is set, runs from the base snapshot below.
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

		/// <summary>Stands in for the Gtk 3 size_allocate vfunc. See <see cref="CompatVFunc"/>.</summary>
		protected virtual void OnSizeAllocated(Gdk.Rectangle allocation)
		{
		}

		protected override void OnSizeAllocate(int width, int height, int baseline)
		{
			base.OnSizeAllocate(width, height, baseline);

			var allocation = CompatVFunc.Allocation(width, height);

			OnSizeAllocated(allocation);

			if (_sizeAllocated != null)
				_sizeAllocated(this, new SizeAllocatedArgs { Allocation = allocation });
		}

		SizeAllocatedHandler _sizeAllocated;

		/// <summary>Stands in for GtkWidget::size-allocate, which Gtk 4 removed as a signal.</summary>
		public event SizeAllocatedHandler SizeAllocated {
			add { _sizeAllocated += value; }
			remove { _sizeAllocated -= value; }
		}
	}
}

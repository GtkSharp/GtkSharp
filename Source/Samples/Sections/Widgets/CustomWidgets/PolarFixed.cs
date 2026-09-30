// adopted from: https://github.com/mono/gtk-sharp/commits/2.99.3/sample/PolarFixed.cs
// This is a completely pointless widget, but it shows how to do custom layout.
//
// Gtk 3 said "how to subclass a container". Gtk 4 has no GtkContainer: any
// widget may hold children, and laying them out means overriding measure and
// size_allocate (or writing a GtkLayoutManager). The arithmetic below is
// unchanged from the Gtk 3 version; what changed is how it is delivered:
//
//   - children are attached with Widget.Parent and detached with Unparent,
//     instead of Container.Add/Remove and a ContainerChild record
//   - OnMeasure replaces OnGetPreferredWidth/Height and reports one orientation
//     at a time
//   - OnSizeAllocate positions each child with a Gsk.Transform rather than by
//     handing it an allocation rectangle in parent coordinates
//   - BorderWidth and HasWindow are gone; margins are ordinary properties and
//     widgets no longer own a GdkWindow

using System;
using System.Collections.Generic;
using Gtk;

namespace Samples
{
	class PolarFixed : Widget
	{
		readonly IList<PolarFixedChild> children = new List<PolarFixedChild>();

		// The per-child placement record. In Gtk 3 this derived from
		// Container.ContainerChild and was reachable through child properties;
		// Gtk 4 has no child properties, so it is a plain object the widget
		// keeps for itself.
		public class PolarFixedChild
		{
			readonly PolarFixed parent;
			double theta;
			uint r;

			public PolarFixedChild(PolarFixed parent, Widget child, double theta, uint r)
			{
				this.parent = parent;
				Child = child;
				this.theta = theta;
				this.r = r;
			}

			public Widget Child { get; }

			// QueueResize from the setters, so moving a widget is just a matter
			// of changing its placement.

			public double Theta {
				get { return theta; }
				set {
					theta = value;
					parent.QueueResize();
				}
			}

			public uint R {
				get { return r; }
				set {
					r = value;
					parent.QueueResize();
				}
			}
		}

		public PolarFixedChild this[Widget w] {
			get {
				foreach (PolarFixedChild pfc in children) {
					if (pfc.Child == w)
						return pfc;
				}

				return null;
			}
		}

		// our own adder method
		public void Put(Widget w, double theta, uint r)
		{
			children.Add(new PolarFixedChild(this, w, theta, r));
			w.Parent = this;
			QueueResize();
		}

		public void Move(Widget w, double theta, uint r)
		{
			PolarFixedChild pfc = this[w];
			if (pfc != null) {
				pfc.Theta = theta;
				pfc.R = r;
			}
		}

		public void Remove(Widget w)
		{
			PolarFixedChild pfc = this[w];
			if (pfc != null) {
				pfc.Child.Unparent();
				children.Remove(pfc);
				QueueResize();
			}
		}

		// Gtk 4 warns if a widget is finalized while it still has children, so
		// the parent has to let go of them explicitly.
		protected override void Dispose(bool disposing)
		{
			if (disposing) {
				foreach (PolarFixedChild pfc in children)
					pfc.Child.Unparent();
				children.Clear();
			}

			base.Dispose(disposing);
		}

		// Handle size request. Gtk 4 asks for one orientation at a time, and
		// wants a baseline too -- -1 meaning "this widget has none".
		protected override void OnMeasure(Orientation orientation, int for_size,
		                                  out int minimum, out int natural,
		                                  out int minimum_baseline, out int natural_baseline)
		{
			int size = 0;

			foreach (PolarFixedChild pfc in children) {
				int child_width, child_height;
				MeasureChild(pfc.Child, out child_width, out child_height);

				// Figure out where we're going to put it
				int x = (int) (Math.Cos(pfc.Theta) * pfc.R) + child_width / 2;
				int y = (int) (Math.Sin(pfc.Theta) * pfc.R) + child_height / 2;

				// Update our own size request to fit it
				int extent = orientation == Orientation.Horizontal ? 2 * x : 2 * y;
				if (size < extent)
					size = extent;
			}

			minimum = natural = size;
			minimum_baseline = natural_baseline = -1;
		}

		static void MeasureChild(Widget child, out int width, out int height)
		{
			int ignored;
			child.Measure(Orientation.Horizontal, -1, out ignored, out width, out ignored, out ignored);
			child.Measure(Orientation.Vertical, -1, out ignored, out height, out ignored, out ignored);
		}

		// Size allocation. Note that the size received may be smaller than what we
		// requested. Some widgets take that into account by giving some or all
		// of their children less room than they asked for. Others (like this one)
		// just let their children get placed partly out-of-bounds.
		protected override void OnSizeAllocate(int width, int height, int baseline)
		{
			// Figure out where the center of the grid will be. Gtk 4 allocates
			// in widget-local coordinates, so the origin is already ours.
			int cx = width / 2;
			int cy = height / 2;

			foreach (PolarFixedChild pfc in children) {
				int child_width, child_height;
				MeasureChild(pfc.Child, out child_width, out child_height);

				int x = (int) (Math.Cos(pfc.Theta) * pfc.R) - child_width / 2;
				int y = (int) (Math.Sin(pfc.Theta) * pfc.R) + child_height / 2;

				// A child is positioned by the transform it is allocated with,
				// rather than by an allocation rectangle in parent coordinates.
				var offset = new Graphene.Point();
				offset.Init(cx + x, cy - y);
				var transform = new Gsk.Transform().Translate(offset);

				pfc.Child.Allocate(child_width, child_height, -1, transform);
			}
		}
	}
}

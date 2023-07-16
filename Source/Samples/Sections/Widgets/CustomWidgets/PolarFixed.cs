// adopted from: https://github.com/mono/gtk-sharp/commits/2.99.3/sample/PolarFixed.cs
// This is a completely pointless widget, but it shows how to subclass container...

using System;
using System.Collections.Generic;
using Gtk;
using Gdk;

namespace Samples
{
	class PolarFixed : Container
	{
        readonly IList<PolarFixedChild> children;

		public PolarFixed()
		{
			children = new List<PolarFixedChild>();
			HasWindow = false;
		}

		// The child properties object
		public class PolarFixedChild : Container.ContainerChild
		{
			double theta;
			uint r;

			public PolarFixedChild(PolarFixed parent, Widget child, double theta, uint r) : base(parent, child)
			{
				this.theta = theta;
				this.r = r;
			}

			// We call parent.QueueResize() from the property setters here so that you
			// can move the widget around just by changing its child properties (just
			// like with a native container class).

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

		// Override the child properties accessor to return the right object from
		// "children".
		public override ContainerChild this[Widget w] {
			get {
				foreach (PolarFixedChild pfc in children) {
					if (pfc.Child == w)
						return pfc;
				}

				return null;
			}
		}

		// Indicate the kind of children the container will accept. Most containers
		// will accept any kind of child, so they should return Gtk.Widget.GType.
		// The default is "GLib.GType.None", which technically means that no (new)
		// children can be added to the container, though Container.Add does not
		// enforce this.
		protected override GLib.GType OnChildType()
		{
			return Gtk.Widget.GType;
		}

		// Implement gtk_container_forall(), which is also used by
		// Gtk.Container.Children and Gtk.Container.AllChildren.
		protected override void ForAll(bool include_internals, Callback callback)
		{
			base.ForAll(include_internals, callback);
			foreach (PolarFixedChild pfc in children)
				callback(pfc.Child);
		}

		// Invoked by Container.Add (w). It's good practice to have this do *something*,
		// even if it's not something terribly useful.
		protected override void OnAdded(Widget w)
		{
			Put(w, 0.0, 0);
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
			PolarFixedChild pfc = (PolarFixedChild) this[w];
			if (pfc != null) {
				pfc.Theta = theta;
				pfc.R = r;
			}
		}

		// invoked by Container.Remove (w)
		protected override void OnRemoved(Widget w)
		{
			PolarFixedChild pfc = (PolarFixedChild) this[w];
			if (pfc != null) {
				pfc.Child.Unparent();
				children.Remove(pfc);
				QueueResize();
			}
		}

		// Handle size request
		protected override void OnGetPreferredHeight(out int minimal_height, out int natural_height)
		{
			Requisition req = new Requisition();
			OnSizeRequested(ref req);
			minimal_height = natural_height = req.Height;
		}

		protected override void OnGetPreferredWidth(out int minimal_width, out int natural_width)
		{
			Requisition req = new Requisition();
			OnSizeRequested(ref req);
			minimal_width = natural_width = req.Width;
		}

		void OnSizeRequested(ref Requisition req)
		{
			int child_width, child_minwidth, child_height, child_minheight;
			int x, y;

			req.Width = req.Height = 0;
			foreach (PolarFixedChild pfc in children) {
				// Recursively request the size of each child
				pfc.Child.GetPreferredWidth(out child_minwidth, out child_width);
				pfc.Child.GetPreferredHeight(out child_minheight, out child_height);

				// Figure out where we're going to put it
				x = (int) (Math.Cos(pfc.Theta) * pfc.R) + child_width / 2;
				y = (int) (Math.Sin(pfc.Theta) * pfc.R) + child_height / 2;

				// Update our own size request to fit it
				if (req.Width < 2 * x)
					req.Width = 2 * x;
				if (req.Height < 2 * y)
					req.Height = 2 * y;
			}

			// Take Container.BorderWidth into account
			req.Width += (int) (2 * BorderWidth);
			req.Height += (int) (2 * BorderWidth);
		}

		// Size allocation. Note that the allocation received may be smaller than what we
		// requested. Some containers will take that into account by giving some or all
		// of their children a smaller allocation than they requested. Other containers
		// (like this one) just let their children get placed partly out-of-bounds if they
		// aren't allocated enough room.
		protected override void OnSizeAllocated(Rectangle allocation)
		{
			Requisition childReq, childMinReq;
			int cx, cy, x, y;

			// This sets the "Allocation" property. For widgets that
			// have a GdkWindow, it also calls GdkWindow.MoveResize()
			base.OnSizeAllocated(allocation);

			// Figure out where the center of the grid will be
			cx = allocation.X + (allocation.Width / 2);
			cy = allocation.Y + (allocation.Height / 2);

			foreach (PolarFixedChild pfc in children) {
				pfc.Child.GetPreferredSize(out childMinReq, out childReq);

				x = (int) (Math.Cos(pfc.Theta) * pfc.R) - childReq.Width / 2;
				y = (int) (Math.Sin(pfc.Theta) * pfc.R) + childReq.Height / 2;

				allocation.X = cx + x;
				allocation.Width = childReq.Width;
				allocation.Y = cy - y;
				allocation.Height = childReq.Height;

				pfc.Child.SizeAllocate(allocation);
			}
		}
	}
}
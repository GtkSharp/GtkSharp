// Copyright (c) 2011 Novell, Inc.
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of version 2 of the Lesser GNU General 
// Public License as published by the Free Software Foundation.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public
// License along with this program; if not, write to the
// Free Software Foundation, Inc., 59 Temple Place - Suite 330,
// Boston, MA 02111-1307, USA.


using System;

namespace Gtk {

	public partial class StyleContext {

		public void RenderActivity (Cairo.Context cr, double x, double y, double width, double height)
		{
			Render.Activity (this, cr, x, y, width, height);
		}

		public void RenderArrow (Cairo.Context cr, double angle, double x, double y, double size)
		{
			Render.Arrow (this, cr, angle, x, y, size);
		}

		public void RenderBackground (Cairo.Context cr, double x, double y, double width, double height)
		{
			Render.Background (this, cr, x, y, width, height);
		}

		public void RenderCheck (Cairo.Context cr, double x, double y, double width, double height)
		{
			Render.Check (this, cr, x, y, width, height);
		}

		public void RenderExtension (Cairo.Context cr, double x, double y, double width, double height, Gtk.PositionType gap_side)
		{
			Render.Extension (this, cr, x, y, width, height, gap_side);
		}

		public void RenderExpander (Cairo.Context cr, double x, double y, double width, double height)
		{
			Render.Expander (this, cr, x, y, width, height);
		}

		public void RenderFocus (Cairo.Context cr, double x, double y, double width, double height)
		{
			Render.Focus (this, cr, x, y, width, height);
		}

		public void RenderFrame (Cairo.Context cr, double x, double y, double width, double height)
		{
			Render.Frame (this, cr, x, y, width, height);
		}

		public void RenderFrameGap (Cairo.Context cr, double x, double y, double width, double height, Gtk.PositionType gap_side, double xy0_gap, double xy1_gap)
		{
			Render.FrameGap (this, cr, x, y, width, height, gap_side, xy0_gap, xy1_gap);
		}

		public void RenderHandle (Cairo.Context cr, double x, double y, double width, double height)
		{
			Render.Handle (this, cr, x, y, width, height);
		}

		public Gdk.Pixbuf RenderIconPixbuf (Gtk.IconSource source, Gtk.IconSize size)
		{
			return Render.IconPixbuf (this, source, size);
		}

		public void RenderLayout (Cairo.Context cr, double x, double y, Pango.Layout layout)
		{
			Render.Layout (this, cr, x, y, layout);
		}

		public void RenderLine (Cairo.Context cr, double x0, double y0, double x1, double y1)
		{
			Render.Line (this, cr, x0, y0, x1, y1);
		}

		public void RenderOption (Cairo.Context cr, double x, double y, double width, double height)
		{
			Render.Option (this, cr, x, y, width, height);
		}

		public void RenderSlider (Cairo.Context cr, double x, double y, double width, double height, Gtk.Orientation orientation)
		{
			Render.Slider (this, cr, x, y, width, height, orientation);
		}
	}
}

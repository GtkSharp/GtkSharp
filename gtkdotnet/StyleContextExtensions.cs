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
using System.Drawing;

namespace Gtk.DotNet {

	public static class StyleContextExtensions {

		public static void RenderActivity (this Gtk.StyleContext style, Cairo.Context cr, RectangleF rect)
		{
			style.RenderActivity (cr, rect.X, rect.Y, rect.Width, rect.Height);
		}

		public static void RenderArrow (this Gtk.StyleContext style, Cairo.Context cr, double angle, PointF location, double size)
		{
			style.RenderArrow (cr, angle, location.X, location.Y, size);
		}

		public static void RenderBackground (this Gtk.StyleContext style, Cairo.Context cr, RectangleF rect)
		{
			style.RenderBackground (cr, rect.X, rect.Y, rect.Width, rect.Height);
		}

		public static void RenderCheck (this Gtk.StyleContext style, Cairo.Context cr, RectangleF rect)
		{
			style.RenderCheck (cr, rect.X, rect.Y, rect.Width, rect.Height);
		}

		public static void RenderExtension (this Gtk.StyleContext style, Cairo.Context cr, RectangleF rect, Gtk.PositionType gap_side)
		{
			style.RenderExtension (cr, rect.X, rect.Y, rect.Width, rect.Height, gap_side);
		}

		public static void RenderExpander (this Gtk.StyleContext style, Cairo.Context cr, RectangleF rect)
		{
			style.RenderExpander (cr, rect.X, rect.Y, rect.Width, rect.Height);
		}

		public static void RenderFocus (this Gtk.StyleContext style, Cairo.Context cr, RectangleF rect)
		{
			style.RenderFocus (cr, rect.X, rect.Y, rect.Width, rect.Height);
		}

		public static void RenderFrame (this Gtk.StyleContext style, Cairo.Context cr, RectangleF rect)
		{
			style.RenderFrame (cr, rect.X, rect.Y, rect.Width, rect.Height);
		}

		public static void RenderFrameGap (this Gtk.StyleContext style, Cairo.Context cr, RectangleF rect, Gtk.PositionType gap_side, double xy0_gap, double xy1_gap)
		{
			style.RenderFrameGap (cr, rect.X, rect.Y, rect.Width, rect.Height, gap_side, xy0_gap, xy1_gap);
		}

		public static void RenderHandle (this Gtk.StyleContext style, Cairo.Context cr, RectangleF rect)
		{
			style.RenderHandle (cr, rect.X, rect.Y, rect.Width, rect.Height);
		}

		public static void RenderLayout (this Gtk.StyleContext style, Cairo.Context cr, PointF location, Pango.Layout layout)
		{
			style.RenderLayout (cr, location.X, location.Y, layout);
		}

		public static void RenderLine (this Gtk.StyleContext style, Cairo.Context cr, PointF pt0, PointF pt1)
		{
			style.RenderLine (cr, pt0.X, pt0.Y, pt1.X, pt1.Y);
		}

		public static void RenderOption (this Gtk.StyleContext style, Cairo.Context cr, RectangleF rect)
		{
			style.RenderOption (cr, rect.X, rect.Y, rect.Width, rect.Height);
		}

		public static void RenderSlider (this Gtk.StyleContext style, Cairo.Context cr, RectangleF rect, Gtk.Orientation orientation)
		{
			style.RenderSlider (cr, rect.X, rect.Y, rect.Width, rect.Height, orientation);
		}
	}
}

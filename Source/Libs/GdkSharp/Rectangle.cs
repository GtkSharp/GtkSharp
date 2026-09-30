// Authors:
//	Jasper van Putten <Jaspervp@gmx.net>
//	Ben Maurer <bmaurer@novell.com>
// Contains lots of c&p from System.Drawing
//
// Copyright (c) 2002 Jasper van Putten
// Copyright (c) 2005 Novell, Inc
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
using System.Runtime.InteropServices;

namespace Gdk {

	public partial struct Rectangle {

		// X, Y, Width, Height and the struct layout come from the generated half:
		// Gtk 4 introspects GdkRectangle, where Gtk 3 only aliased it to
		// cairo_rectangle_int_t and left the whole type to this file.

		public Rectangle (int x, int y, int width, int height)
		{
			this.X = x;
			this.Y = y;
			this.Width = width;
			this.Height = height;
		}

		public Rectangle (Point loc, Size sz) : this (loc.X, loc.Y, sz.Width, sz.Height) {}

		public static Rectangle FromLTRB (int left, int top, int right, int bottom)
		{
			return new Rectangle (left, top, right - left, bottom - top);
		}

		// Equals, GetHashCode and the GLib.Value conversions come from the
		// generated half; only == and != are kept, since they are defined in
		// terms of Location and Size, which live here.
		public static bool operator == (Rectangle r1, Rectangle r2)
		{
			return ((r1.Location == r2.Location) && (r1.Size == r2.Size));
		}

		public static bool operator != (Rectangle r1, Rectangle r2)
		{
			return !(r1 == r2);
		}

		public override string ToString ()
		{
			return String.Format ("{0}x{1}+{2}+{3}", Width, Height, X, Y);
		}

		// Hit Testing / Intersection / Union
		public bool Contains (Rectangle rect)
		{
			return (rect == Intersect (this, rect));
		}

		public bool Contains (Point pt)
		{
			return Contains (pt.X, pt.Y);
		}

		public bool Contains (int x, int y)
		{
			return ((x >= Left) && (x <= Right) && (y >= Top) && (y <= Bottom));
		}

		public bool IntersectsWith (Rectangle r)
		{
			return !((Left > r.Right) || (Right < r.Left) ||
	    		(Top > r.Bottom) || (Bottom < r.Top));
		}

		public static Rectangle Union (Rectangle r1, Rectangle r2)
		{
			return FromLTRB (Math.Min (r1.Left, r2.Left),
			 		Math.Min (r1.Top, r2.Top),
			 		Math.Max (r1.Right, r2.Right),
			 		Math.Max (r1.Bottom, r2.Bottom));
		}

		public void Intersect (Rectangle r)
		{
			this = Intersect (this, r);
		}

		public static Rectangle Intersect (Rectangle r1, Rectangle r2)
		{
			Rectangle r;
			if (!r1.Intersect (r2, out r))
				return new Rectangle ();
			
			return r;
		}

		// Position/Size
		public int Top {
			get { return Y; }
		}
		public int Bottom {
			get { return Y + Height - 1; }
		}
		public int Right {
			get { return X + Width - 1; }
		}
		public int Left {
			get { return X; }
		}

		public bool IsEmpty {
			get { return (Width == 0) || (Height == 0); }
		}

		public Size Size {
			get { return new Size (Width, Height); }
			set {
				Width = value.Width;
				Height = value.Height;
			}
		}

		public Point Location {
			get {
				return new Point (X, Y);
			}
			set {
				X = value.X;
				Y = value.Y;
			}
		}

		// Inflate and Offset
		public void Inflate (Size sz)
		{
			Inflate (sz.Width, sz.Height);
		}

		public void Inflate (int width, int height)
		{
			X -= width;
			Y -= height;
			Width += width * 2;
			Height += height * 2;
		}

		public static Rectangle Inflate (Rectangle rect, int x, int y)
		{
			Rectangle r = rect;
			r.Inflate (x, y);
			return r;
		}

		public static Rectangle Inflate (Rectangle rect, Size sz)
		{
			return Inflate (rect, sz.Width, sz.Height);
		}

		public void Offset (int dx, int dy)
		{
			X += dx;
			Y += dy;
		}

		public void Offset (Point dr)
		{
			Offset (dr.X, dr.Y);
		}

		public static Rectangle Offset (Rectangle rect, int dx, int dy)
		{
			Rectangle r = rect;
			r.Offset (dx, dy);
			return r;
		}

		public static Rectangle Offset (Rectangle rect, Point dr)
		{
			return Offset (rect, dr.X, dr.Y);
		}
		// GType, the native Union and Intersect, New and Zero all come from the
		// generated half now: Gtk 4 introspects GdkRectangle, where Gtk 3 only
		// aliased it to cairo_rectangle_int_t and left every one of those to be
		// bound by hand here. What stays is the managed geometry API that has no
		// C counterpart.
	}
}

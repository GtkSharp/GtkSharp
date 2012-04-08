// Gdk.Point.cs - Gdk Point class customizations
//
// Authors:
//	Jasper van Putten <Jaspervp@gmx.net>
//	Martin Willemoes Hansen <mwh@sysrq.dk>
//	Ben Maurer <bmaurer@novell.com>
// Contains lots of c&p from System.Drawing
//
// Copyright (c) 2002 Jasper van Putten
// Copyright (c) 2003 Martin Willemoes Hansen
// Copyright (c) 2005 Novell, Inc
//
// This code is inserted after the automatically generated code.
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

namespace Gdk {

	using System;

	public partial struct Point {

		public override string ToString ()
		{
			return String.Format ("({0},{1})", X, Y);
		}

		public Point (int x, int y)
		{
			this.X = x;
			this.Y = y;
		}

		public Point (Size sz)
		{
			this.X = sz.Width;
			this.Y = sz.Height;
		}

		public void Offset (int dx, int dy)
		{
			X += dx;
			Y += dy;
		}

		public bool IsEmpty {
			get {
				return ((X == 0) && (Y == 0));
			}
		}

		public static explicit operator Size (Point pt)
		{
			return new Size (pt.X, pt.Y);
		}

		public static Point operator + (Point pt, Size sz)
		{
			return new Point (pt.X + sz.Width, pt.Y + sz.Height);
		}

		public static Point operator - (Point pt, Size sz)
		{
			return new Point (pt.X - sz.Width, pt.Y - sz.Height);
		}

		public static bool operator == (Point pt_a, Point pt_b)
		{
			return ((pt_a.X == pt_b.X) && (pt_a.Y == pt_b.Y));
		}

		public static bool operator != (Point pt_a, Point pt_b)
		{
			return ((pt_a.X != pt_b.X) || (pt_a.Y != pt_b.Y));
		}
	}
}


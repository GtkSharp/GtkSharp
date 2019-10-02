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

	[StructLayout(LayoutKind.Sequential)]
	public struct Rectangle {

		public int X;
		public int Y;
		public int Width;
		public int Height;

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

		public override bool Equals (object o)
		{
			if (!(o is Rectangle))
				return false;

			return (this == (Rectangle) o);
		}

		public override int GetHashCode ()
		{
			return (Height + Width) ^ X + Y;
		}

		public static bool operator == (Rectangle r1, Rectangle r2)
		{
			return ((r1.Location == r2.Location) && (r1.Size == r2.Size));
		}

		public static bool operator != (Rectangle r1, Rectangle r2)
		{
			return !(r1 == r2);
		}
		
		public static explicit operator GLib.Value (Gdk.Rectangle boxed)
		{
			GLib.Value val = GLib.Value.Empty;
			val.Init (Gdk.Rectangle.GType);
			val.Val = boxed;
			return val;
		}

		public static explicit operator Gdk.Rectangle (GLib.Value val)
		{
			return (Gdk.Rectangle) val.Val;
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
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_gdk_rectangle_get_type();
		static d_gdk_rectangle_get_type gdk_rectangle_get_type = FuncLoader.LoadFunction<d_gdk_rectangle_get_type>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gdk), "gdk_rectangle_get_type"));

		public static GLib.GType GType { 
			get {
				IntPtr raw_ret = gdk_rectangle_get_type();
				GLib.GType ret = new GLib.GType(raw_ret);
				return ret;
			}
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_gdk_rectangle_union(ref Rectangle src1, ref Rectangle src2, out Rectangle dest);
		static d_gdk_rectangle_union gdk_rectangle_union = FuncLoader.LoadFunction<d_gdk_rectangle_union>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gdk), "gdk_rectangle_union"));

		public Gdk.Rectangle Union (Gdk.Rectangle src)
		{
			Gdk.Rectangle dest;
			gdk_rectangle_union (ref this, ref src, out dest);
			return dest;
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate bool d_gdk_rectangle_intersect(ref Rectangle src1, ref Rectangle src2, out Rectangle dest);
		static d_gdk_rectangle_intersect gdk_rectangle_intersect = FuncLoader.LoadFunction<d_gdk_rectangle_intersect>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gdk), "gdk_rectangle_intersect"));

		public bool Intersect (Gdk.Rectangle src, out Gdk.Rectangle dest)
		{
			return gdk_rectangle_intersect (ref this, ref src, out dest);
		}

		public static Rectangle New (IntPtr raw)
		{
			return (Gdk.Rectangle) Marshal.PtrToStructure (raw, typeof (Gdk.Rectangle));
		}

		public static Rectangle Zero;
	}
}


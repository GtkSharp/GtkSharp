// Size.cs
//
// Author:  Mike Kestner (mkestner@speakeasy.net)
//
// (C) 2001 Mike Kestner

using System;

namespace Gdk {
	
	public struct Size { 
		
		int width, height;

		public static readonly Size Empty;

		public static Size operator + (Size sz1, Size sz2)
		{
			return new Size (sz1.Width + sz2.Width, 
					 sz1.Height + sz2.Height);
		}
		
		public static bool operator == (Size sz_a, Size sz_b)
		{
			return ((sz_a.Width == sz_b.Width) && 
				(sz_a.Height == sz_b.Height));
		}
		
		public static bool operator != (Size sz_a, Size sz_b)
		{
			return ((sz_a.Width != sz_b.Width) || 
				(sz_a.Height != sz_b.Height));
		}
		
		public static Size operator - (Size sz1, Size sz2)
		{
			return new Size (sz1.Width - sz2.Width, 
					 sz1.Height - sz2.Height);
		}
		
		public static explicit operator Point (Size sz)
		{
			return new Point (sz.Width, sz.Height);
		}

		public Size (Point pt)
		{
			width = pt.X;
			height = pt.Y;
		}

		public Size (int width, int height)
		{
			this.width = width;
			this.height = height;
		}

		public bool IsEmpty {
			get {
				return ((width == 0) && (height == 0));
			}
		}

		public int Width {
			get {
				return width;
			}
			set {
				width = value;
			}
		}

		public int Height {
			get {
				return height;
			}
			set {
				height = value;
			}
		}

		public override bool Equals (object o)
		{
			if (!(o is Size))
				return false;

			return (this == (Size) o);
		}

		public override int GetHashCode ()
		{
			return width^height;
		}

		public override string ToString ()
		{
			return String.Format ("{{Width={0}, Height={1}}}", width, height);
		}
	}
}

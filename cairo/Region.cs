// Copyright (C) 2011 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Runtime.InteropServices;

namespace Cairo
{

	[StructLayout(LayoutKind.Sequential)]
	public struct RectangleInt {
		public int X;
		public int Y;
		public int Width;
		public int Height;
	}

	public enum RegionOverlap {
		In,
		Out,
		Part,
	}
		
	public class Region : IDisposable {

		const string libname = "libcairo.dll";

		IntPtr handle;
		public IntPtr Handle {
			get { return handle; }
		}

		~Region ()
		{
			Dispose (false);
		}

		[DllImport (libname)]
		static extern IntPtr cairo_region_reference (IntPtr region);

		public Region (IntPtr handle)
		{
			handle = cairo_region_reference (handle);
		}

		[DllImport (libname)]
		static extern IntPtr cairo_region_create ();

		public Region ()
		{
			handle = cairo_region_create ();
		}

		[DllImport (libname)]
		static extern IntPtr cairo_region_create_rectangle (ref RectangleInt rect);

		public Region (RectangleInt rect)
		{
			handle = cairo_region_create_rectangle (ref rect);
		}

		[DllImport (libname)]
		static extern IntPtr cairo_region_create_rectangles (RectangleInt[] rects, int count);

		public Region (RectangleInt[] rects)
		{
			handle = cairo_region_create_rectangles (rects, rects.Length);
		}

		[DllImport (libname)]
		static extern IntPtr cairo_region_copy (IntPtr original);

		public Region Copy ()
		{
			return new Region (cairo_region_copy (Handle));
		}

		[DllImport (libname)]
		static extern void cairo_region_destroy (IntPtr region);

		public void Dispose ()
		{
			Dispose (true);
		}

		void Dispose (bool disposing)
		{
			if (handle != IntPtr.Zero)
				cairo_region_destroy (Handle);
			handle = IntPtr.Zero;
			if (disposing)
				GC.SuppressFinalize (this);
		}

		[DllImport (libname)]
		static extern bool cairo_region_equal (IntPtr a, IntPtr b);

		public override bool Equals (object obj)
		{
			return (obj is Region) && cairo_region_equal (Handle, (obj as Region).Handle);
		}

		[DllImport (libname)]
		static extern Status cairo_region_status (IntPtr region);

		public Status Status {
			get { return cairo_region_status (Handle); }
		}

		[DllImport (libname)]
		static extern void cairo_region_get_extents (IntPtr region, out RectangleInt extents);

		public RectangleInt Extents {
			get {
				RectangleInt result;
				cairo_region_get_extents (Handle, out result);
				return result;
			}
		}

		[DllImport (libname)]
		static extern int cairo_region_num_rectangles (IntPtr region); 

		public int NumRectangles {
			get { return cairo_region_num_rectangles (Handle); }
		}

		[DllImport (libname)]
		static extern void cairo_region_get_rectangle (IntPtr region, int nth, out RectangleInt rectangle);

		public RectangleInt GetRectangle (int nth)
		{
			RectangleInt val;
			cairo_region_get_rectangle (Handle, nth, out val);
			return val;
		}

		[DllImport (libname)]
		static extern bool cairo_region_is_empty (IntPtr region);

		public bool IsEmpty {
			get { return cairo_region_is_empty (Handle); }
		}

		[DllImport (libname)]
		static extern RegionOverlap cairo_region_contains_rectangle (IntPtr region, ref RectangleInt rectangle);

		public RegionOverlap ContainsPoint (RectangleInt rectangle)
		{
			return cairo_region_contains_rectangle (Handle, ref rectangle);
		}

		[DllImport (libname)]
		static extern bool cairo_region_contains_point (IntPtr region, int x, int y);

		public bool ContainsPoint (int x, int y)
		{
			return cairo_region_contains_point (Handle, x, y);
		}

		[DllImport (libname)]
		static extern void cairo_region_translate (IntPtr region, int dx, int dy);

		public void Translate (int dx, int dy)
		{
			cairo_region_translate (Handle, dx, dy);
		}

		[DllImport (libname)]
		static extern Status cairo_region_subtract (IntPtr dst, IntPtr other);

		public Status Subtract (Region other)
		{
			return cairo_region_subtract (Handle, other.Handle);
		}

		[DllImport (libname)]
		static extern Status cairo_region_subtract_rectangle (IntPtr dst, ref RectangleInt rectangle);

		public Status SubtractRectangle (RectangleInt rectangle)
		{
			return cairo_region_subtract_rectangle (Handle, ref rectangle);
		}

		[DllImport (libname)]
		static extern Status cairo_region_intersect (IntPtr dst, IntPtr other);

		public Status Intersect (Region other)
		{
			return cairo_region_intersect (Handle, other.Handle);
		}

		[DllImport (libname)]
		static extern Status cairo_region_intersect_rectangle (IntPtr dst, ref RectangleInt rectangle);

		public Status IntersectRectangle (RectangleInt rectangle)
		{
			return cairo_region_intersect_rectangle (Handle, ref rectangle);
		}

		[DllImport (libname)]
		static extern Status cairo_region_union (IntPtr dst, IntPtr other);

		public Status Union (Region other)
		{
			return cairo_region_union (Handle, other.Handle);
		}

		[DllImport (libname)]
		static extern Status cairo_region_union_rectangle (IntPtr dst, ref RectangleInt rectangle);

		public Status UnionRectangle (RectangleInt rectangle)
		{
			return cairo_region_union_rectangle (Handle, ref rectangle);
		}

		[DllImport (libname)]
		static extern Status cairo_region_xor (IntPtr dst, IntPtr other);

		public Status Xor (Region other)
		{
			return cairo_region_xor (Handle, other.Handle);
		}

		[DllImport (libname)]
		static extern Status cairo_region_xor_rectangle (IntPtr dst, ref RectangleInt rectangle);

		public Status XorRectangle (RectangleInt rectangle)
		{
			return cairo_region_xor_rectangle (Handle, ref rectangle);
		}
	}
}

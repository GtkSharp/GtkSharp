// RoundedRect.cs - Gsk RoundedRect struct customizations
//
// This code is inserted after the automatically generated code.
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of version 2 of the Lesser GNU General
// Public License as published by the Free Software Foundation.

namespace Gsk {

	using System;
	using System.Runtime.InteropServices;

	/// <summary>A rectangle with a size for each of its four corners.</summary>
	/// <remarks>
	/// The layout is the C one, and has to be exact -- GSK reads these fields
	/// directly rather than through accessors:
	///
	/// <code>
	/// struct GskRoundedRect {
	///     graphene_rect_t bounds;      // origin.x, origin.y, size.width, size.height
	///     graphene_size_t corner[4];   // top-left, top-right, bottom-right, bottom-left
	/// };
	/// </code>
	///
	/// Twelve floats, 48 bytes. The generated struct had an IntPtr for the bounds
	/// and a managed array for the corners -- 40 bytes, in the wrong order -- so
	/// the fields are removed in GskSharp.metadata and declared here instead.
	/// Because they are declared in one place, sequential layout is their
	/// declaration order and nothing else.
	/// </remarks>
	public partial struct RoundedRect {

		/// <summary>The left edge of <see cref="Bounds"/>.</summary>
		public float X;
		/// <summary>The top edge of <see cref="Bounds"/>.</summary>
		public float Y;
		/// <summary>The width of <see cref="Bounds"/>.</summary>
		public float Width;
		/// <summary>The height of <see cref="Bounds"/>.</summary>
		public float Height;

		/// <summary>Horizontal radius of the top-left corner.</summary>
		public float TopLeftWidth;
		/// <summary>Vertical radius of the top-left corner.</summary>
		public float TopLeftHeight;
		/// <summary>Horizontal radius of the top-right corner.</summary>
		public float TopRightWidth;
		/// <summary>Vertical radius of the top-right corner.</summary>
		public float TopRightHeight;
		/// <summary>Horizontal radius of the bottom-right corner.</summary>
		public float BottomRightWidth;
		/// <summary>Vertical radius of the bottom-right corner.</summary>
		public float BottomRightHeight;
		/// <summary>Horizontal radius of the bottom-left corner.</summary>
		public float BottomLeftWidth;
		/// <summary>Vertical radius of the bottom-left corner.</summary>
		public float BottomLeftHeight;

		/// <summary>A rectangle with the same radius on all four corners.</summary>
		public RoundedRect (float x, float y, float width, float height, float radius)
			: this (x, y, width, height, radius, radius, radius, radius)
		{
		}

		/// <summary>A rectangle with a radius per corner, clockwise from the top left.</summary>
		public RoundedRect (float x, float y, float width, float height,
		                    float topLeft, float topRight, float bottomRight, float bottomLeft)
		{
			X = x; Y = y; Width = width; Height = height;

			TopLeftWidth = TopLeftHeight = topLeft;
			TopRightWidth = TopRightHeight = topRight;
			BottomRightWidth = BottomRightHeight = bottomRight;
			BottomLeftWidth = BottomLeftHeight = bottomLeft;
		}

		/// <summary>
		/// The rectangle the corners are cut from. Reading allocates a new
		/// graphene_rect_t, because the C struct stores four floats rather than
		/// anything a Graphene.Rect could wrap in place.
		/// </summary>
		public Graphene.Rect Bounds {
			get {
				var bounds = Graphene.Rect.Alloc ();
				bounds.Init (X, Y, Width, Height);
				return bounds;
			}
			set {
				if (value == null)
					throw new ArgumentNullException ("value");

				X = value.X;
				Y = value.Y;
				Width = value.Width;
				Height = value.Height;
			}
		}

		// The six methods below all return their own receiver. The generated
		// wrappers marshalled the struct to unmanaged memory, called through, read
		// the result back into "this" -- and then freed that memory before
		// constructing the return value out of it. The receiver came out right and
		// the returned value was read from freed memory. Passing "ref this" needs
		// no copy at all now that the struct is blittable, and there is nothing
		// left to free. All six are hidden in GskSharp.metadata.

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_gsk_rounded_rect_init(ref RoundedRect raw, IntPtr bounds, IntPtr top_left, IntPtr top_right, IntPtr bottom_right, IntPtr bottom_left);
		static d_gsk_rounded_rect_init gsk_rounded_rect_init = FuncLoader.LoadFunction<d_gsk_rounded_rect_init>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gsk), "gsk_rounded_rect_init"));

		/// <summary>Sets every field, and returns the rectangle it set them on.</summary>
		public RoundedRect Init (Graphene.Rect bounds, Graphene.Size top_left, Graphene.Size top_right, Graphene.Size bottom_right, Graphene.Size bottom_left)
		{
			gsk_rounded_rect_init (ref this,
				bounds == null ? IntPtr.Zero : bounds.Handle,
				top_left == null ? IntPtr.Zero : top_left.Handle,
				top_right == null ? IntPtr.Zero : top_right.Handle,
				bottom_right == null ? IntPtr.Zero : bottom_right.Handle,
				bottom_left == null ? IntPtr.Zero : bottom_left.Handle);

			return this;
		}

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_gsk_rounded_rect_init_copy(ref RoundedRect raw, ref RoundedRect src);
		static d_gsk_rounded_rect_init_copy gsk_rounded_rect_init_copy = FuncLoader.LoadFunction<d_gsk_rounded_rect_init_copy>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gsk), "gsk_rounded_rect_init_copy"));

		/// <summary>Copies <paramref name="src"/> over this rectangle.</summary>
		public RoundedRect InitCopy (RoundedRect src)
		{
			gsk_rounded_rect_init_copy (ref this, ref src);
			return this;
		}

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_gsk_rounded_rect_init_from_rect(ref RoundedRect raw, IntPtr bounds, float radius);
		static d_gsk_rounded_rect_init_from_rect gsk_rounded_rect_init_from_rect = FuncLoader.LoadFunction<d_gsk_rounded_rect_init_from_rect>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gsk), "gsk_rounded_rect_init_from_rect"));

		/// <summary>Sets the bounds and gives all four corners the same radius.</summary>
		public RoundedRect InitFromRect (Graphene.Rect bounds, float radius)
		{
			gsk_rounded_rect_init_from_rect (ref this, bounds == null ? IntPtr.Zero : bounds.Handle, radius);
			return this;
		}

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_gsk_rounded_rect_normalize(ref RoundedRect raw);
		static d_gsk_rounded_rect_normalize gsk_rounded_rect_normalize = FuncLoader.LoadFunction<d_gsk_rounded_rect_normalize>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gsk), "gsk_rounded_rect_normalize"));

		/// <summary>Turns a negative size positive and shrinks corners that would
		/// otherwise overlap.</summary>
		public RoundedRect Normalize ()
		{
			gsk_rounded_rect_normalize (ref this);
			return this;
		}

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_gsk_rounded_rect_offset(ref RoundedRect raw, float dx, float dy);
		static d_gsk_rounded_rect_offset gsk_rounded_rect_offset = FuncLoader.LoadFunction<d_gsk_rounded_rect_offset>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gsk), "gsk_rounded_rect_offset"));

		/// <summary>Moves the rectangle, leaving its corners alone.</summary>
		public RoundedRect Offset (float dx, float dy)
		{
			gsk_rounded_rect_offset (ref this, dx, dy);
			return this;
		}

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_gsk_rounded_rect_shrink(ref RoundedRect raw, float top, float right, float bottom, float left);
		static d_gsk_rounded_rect_shrink gsk_rounded_rect_shrink = FuncLoader.LoadFunction<d_gsk_rounded_rect_shrink>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gsk), "gsk_rounded_rect_shrink"));

		/// <summary>Insets each edge by the given amount, adjusting the corners to
		/// match. Negative values grow the rectangle.</summary>
		public RoundedRect Shrink (float top, float right, float bottom, float left)
		{
			gsk_rounded_rect_shrink (ref this, top, right, bottom, left);
			return this;
		}

		// Generated with noequals/nohash: a struct whose fields all live in this
		// file would otherwise get an Equals that compares nothing and returns
		// true for every pair.

		public bool Equals (RoundedRect other)
		{
			return X == other.X && Y == other.Y
				&& Width == other.Width && Height == other.Height
				&& TopLeftWidth == other.TopLeftWidth && TopLeftHeight == other.TopLeftHeight
				&& TopRightWidth == other.TopRightWidth && TopRightHeight == other.TopRightHeight
				&& BottomRightWidth == other.BottomRightWidth && BottomRightHeight == other.BottomRightHeight
				&& BottomLeftWidth == other.BottomLeftWidth && BottomLeftHeight == other.BottomLeftHeight;
		}

		// Folded in order rather than XOR-ed, for the reason StructBase.GenHashCode
		// gives: XOR is commutative, so every permutation of the same floats would
		// otherwise collide -- and a rounded rectangle is mostly permutations.
		public override int GetHashCode ()
		{
			unchecked {
				int hash = typeof (RoundedRect).FullName.GetHashCode ();

				hash = hash * 397 ^ X.GetHashCode ();
				hash = hash * 397 ^ Y.GetHashCode ();
				hash = hash * 397 ^ Width.GetHashCode ();
				hash = hash * 397 ^ Height.GetHashCode ();
				hash = hash * 397 ^ TopLeftWidth.GetHashCode ();
				hash = hash * 397 ^ TopLeftHeight.GetHashCode ();
				hash = hash * 397 ^ TopRightWidth.GetHashCode ();
				hash = hash * 397 ^ TopRightHeight.GetHashCode ();
				hash = hash * 397 ^ BottomRightWidth.GetHashCode ();
				hash = hash * 397 ^ BottomRightHeight.GetHashCode ();
				hash = hash * 397 ^ BottomLeftWidth.GetHashCode ();
				hash = hash * 397 ^ BottomLeftHeight.GetHashCode ();

				return hash;
			}
		}

		public override string ToString ()
		{
			return string.Format (
				"Gsk.RoundedRect ({0}, {1}, {2}x{3}; corners {4}, {5}, {6}, {7})",
				X, Y, Width, Height,
				TopLeftWidth, TopRightWidth, BottomRightWidth, BottomLeftWidth);
		}
	}
}

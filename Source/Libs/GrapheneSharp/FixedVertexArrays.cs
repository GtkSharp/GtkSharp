// Graphene's four fixed-size arrays of boxed elements, bound by hand.
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of version 2 of the GNU General Public
// License as published by the Free Software Foundation.

namespace Graphene {

	using System;
	using System.Runtime.InteropServices;

	/// <summary>
	/// The one array shape GapiCodegen cannot express: N boxed structs laid end
	/// to end.
	/// </summary>
	/// <remarks>
	/// <para>
	/// <c>graphene_rect_get_vertices (r, graphene_vec2_t vertices[4])</c> wants
	/// sixty-four contiguous bytes. Every one of these types is bound as a
	/// class, so a <c>Graphene.Vec2[]</c> marshals as an array of *pointers* -
	/// four addresses, not four vectors - and there is no attribute that says
	/// otherwise. <c>Parameters.Validate</c> therefore drops the four methods
	/// with a warning rather than emitting code that overruns a buffer, and
	/// they are written out here instead.
	/// </para>
	/// <para>
	/// What they replaced was worse than absent: the parameter came out as the
	/// *element* type, so the binding passed a single <c>Vec2</c>'s handle and
	/// let graphene write four vectors through it.
	/// </para>
	/// </remarks>
	static class FixedArray {

		/// <summary>
		/// Allocates room for <paramref name="count"/> elements of
		/// <paramref name="size"/> bytes, which the caller frees.
		/// </summary>
		public static IntPtr Alloc (int count, int size)
		{
			return GLib.Marshaller.Malloc ((ulong) (count * size));
		}

		public static unsafe void Copy (IntPtr source, IntPtr destination, int size)
		{
			Buffer.MemoryCopy ((void*) source, (void*) destination, size, size);
		}

		/// <summary>
		/// Splits a filled buffer into <paramref name="count"/> independently
		/// owned wrappers, then releases the buffer.
		/// </summary>
		/// <remarks>
		/// Each element is copied into its own allocation rather than wrapped in
		/// place: a wrapper over an interior pointer would free the middle of
		/// somebody else's block, and every one of these arrays outlives the
		/// call that produced it.
		/// </remarks>
		public static T[] Split<T> (IntPtr buffer, int count, int size) where T : GLib.Opaque
		{
			var result = new T [count];

			for (int i = 0; i < count; i++) {
				IntPtr element = GLib.Marshaller.Malloc ((ulong) size);
				Copy (buffer + i * size, element, size);
				result [i] = (T) GLib.Opaque.GetOpaque (element, typeof (T), true);
			}

			GLib.Marshaller.Free (buffer);
			return result;
		}

		/// <summary>
		/// Packs <paramref name="items"/> into one contiguous buffer, which the
		/// caller frees.
		/// </summary>
		public static IntPtr Pack (GLib.Opaque[] items, int count, int size, string name)
		{
			if (items == null || items.Length != count)
				throw new ArgumentException ("must have exactly " + count + " elements", name);

			IntPtr buffer = Alloc (count, size);
			for (int i = 0; i < count; i++) {
				if (items [i] == null) {
					GLib.Marshaller.Free (buffer);
					throw new ArgumentNullException (name + "[" + i + "]");
				}
				Copy (items [i].Handle, buffer + i * size, size);
			}

			return buffer;
		}
	}

	public partial class Rect {

		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		delegate void d_graphene_rect_get_vertices (IntPtr raw, IntPtr vertices);
		static d_graphene_rect_get_vertices graphene_rect_get_vertices =
			FuncLoader.LoadFunction<d_graphene_rect_get_vertices> (
				FuncLoader.GetProcAddress (GLibrary.Load (Library.Graphene), "graphene_rect_get_vertices"));

		/// <summary>
		/// The rectangle's four corners, in the order graphene documents:
		/// top-left, top-right, bottom-right, bottom-left.
		/// </summary>
		public Graphene.Vec2[] GetVertices ()
		{
			int size = (int) Graphene.Vec2.abi_info.Size;
			IntPtr buffer = FixedArray.Alloc (4, size);
			graphene_rect_get_vertices (Handle, buffer);
			return FixedArray.Split<Graphene.Vec2> (buffer, 4, size);
		}
	}

	public partial class Box {

		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		delegate void d_graphene_box_get_vertices (IntPtr raw, IntPtr vertices);
		static d_graphene_box_get_vertices graphene_box_get_vertices =
			FuncLoader.LoadFunction<d_graphene_box_get_vertices> (
				FuncLoader.GetProcAddress (GLibrary.Load (Library.Graphene), "graphene_box_get_vertices"));

		/// <summary>
		/// The box's eight corners.
		/// </summary>
		public Graphene.Vec3[] GetVertices ()
		{
			int size = (int) Graphene.Vec3.abi_info.Size;
			IntPtr buffer = FixedArray.Alloc (8, size);
			graphene_box_get_vertices (Handle, buffer);
			return FixedArray.Split<Graphene.Vec3> (buffer, 8, size);
		}
	}

	public partial class Frustum {

		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		delegate void d_graphene_frustum_get_planes (IntPtr raw, IntPtr planes);
		static d_graphene_frustum_get_planes graphene_frustum_get_planes =
			FuncLoader.LoadFunction<d_graphene_frustum_get_planes> (
				FuncLoader.GetProcAddress (GLibrary.Load (Library.Graphene), "graphene_frustum_get_planes"));

		/// <summary>
		/// The frustum's six clipping planes.
		/// </summary>
		public Graphene.Plane[] GetPlanes ()
		{
			int size = (int) Graphene.Plane.abi_info.Size;
			IntPtr buffer = FixedArray.Alloc (6, size);
			graphene_frustum_get_planes (Handle, buffer);
			return FixedArray.Split<Graphene.Plane> (buffer, 6, size);
		}
	}

	public partial class Quad {

		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		delegate IntPtr d_graphene_quad_init_from_points (IntPtr raw, IntPtr points);
		static d_graphene_quad_init_from_points graphene_quad_init_from_points =
			FuncLoader.LoadFunction<d_graphene_quad_init_from_points> (
				FuncLoader.GetProcAddress (GLibrary.Load (Library.Graphene), "graphene_quad_init_from_points"));

		/// <summary>
		/// Initialises the quad from exactly four points, in order.
		/// </summary>
		public Graphene.Quad InitFromPoints (Graphene.Point[] points)
		{
			int size = (int) Graphene.Point.abi_info.Size;
			IntPtr buffer = FixedArray.Pack (points, 4, size, "points");

			try {
				IntPtr raw_ret = graphene_quad_init_from_points (Handle, buffer);
				return raw_ret == IntPtr.Zero
					? null
					: (Graphene.Quad) GLib.Opaque.GetOpaque (raw_ret, typeof (Graphene.Quad), false);
			} finally {
				GLib.Marshaller.Free (buffer);
			}
		}
	}
}

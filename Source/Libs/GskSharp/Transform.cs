// Transform.cs - Gsk Transform class customizations
//
// This code is inserted after the automatically generated code.
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of version 2 of the Lesser GNU General
// Public License as published by the Free Software Foundation.

namespace Gsk {

	using System;
	using System.Runtime.InteropServices;

	public partial class Transform {

		// Every one of the twelve builders below takes its receiver as
		// (transfer full). GSK builds a chain: the transform that comes back
		// stores the one it was called on in its own `next` slot and keeps that
		// reference. An api.xml <method> describes the ownership of its
		// PARAMETERS and has no way to describe the instance's, so codegen
		// passed Handle and the wrapper went on owning a reference the callee
		// had already eaten. Both then unref: the second one is a free of freed
		// memory, deferred onto the main loop by the generated finalizer, so it
		// lands wherever the GC happens to run. They are hidden in
		// GskSharp.metadata; taking the reference here keeps the managed object
		// behaving like every other one in the binding - a method does not
		// destroy the object it was called on.
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_gsk_transform_ref_transfer(IntPtr raw);
		static d_gsk_transform_ref_transfer gsk_transform_ref_transfer = FuncLoader.LoadFunction<d_gsk_transform_ref_transfer>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gsk), "gsk_transform_ref"));

		IntPtr Consumed {
			get {
				gsk_transform_ref_transfer (Handle);
				return Handle;
			}
		}

		static Transform Wrap (IntPtr raw)
		{
			return raw == IntPtr.Zero ? null : (Transform) GLib.Opaque.GetOpaque (raw, typeof (Transform), true);
		}

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_gsk_transform_invert(IntPtr raw);
		static d_gsk_transform_invert gsk_transform_invert = FuncLoader.LoadFunction<d_gsk_transform_invert>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gsk), "gsk_transform_invert"));

		/// <summary>The inverse transform, or null when this one is not invertible.</summary>
		public Transform Invert ()
		{
			return Wrap (gsk_transform_invert (Consumed));
		}

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_gsk_transform_matrix(IntPtr raw, IntPtr matrix);
		static d_gsk_transform_matrix gsk_transform_matrix = FuncLoader.LoadFunction<d_gsk_transform_matrix>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gsk), "gsk_transform_matrix"));

		public Transform Matrix (Graphene.Matrix matrix)
		{
			return Wrap (gsk_transform_matrix (Consumed, matrix == null ? IntPtr.Zero : matrix.Handle));
		}

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_gsk_transform_matrix_2d(IntPtr raw, float xx, float yx, float xy, float yy, float dx, float dy);
		static d_gsk_transform_matrix_2d gsk_transform_matrix_2d = FuncLoader.LoadFunction<d_gsk_transform_matrix_2d>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gsk), "gsk_transform_matrix_2d"));

		public Transform Matrix2d (float xx, float yx, float xy, float yy, float dx, float dy)
		{
			return Wrap (gsk_transform_matrix_2d (Consumed, xx, yx, xy, yy, dx, dy));
		}

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_gsk_transform_perspective(IntPtr raw, float depth);
		static d_gsk_transform_perspective gsk_transform_perspective = FuncLoader.LoadFunction<d_gsk_transform_perspective>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gsk), "gsk_transform_perspective"));

		public Transform Perspective (float depth)
		{
			return Wrap (gsk_transform_perspective (Consumed, depth));
		}

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_gsk_transform_rotate(IntPtr raw, float angle);
		static d_gsk_transform_rotate gsk_transform_rotate = FuncLoader.LoadFunction<d_gsk_transform_rotate>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gsk), "gsk_transform_rotate"));

		public Transform Rotate (float angle)
		{
			return Wrap (gsk_transform_rotate (Consumed, angle));
		}

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_gsk_transform_rotate_3d(IntPtr raw, float angle, IntPtr axis);
		static d_gsk_transform_rotate_3d gsk_transform_rotate_3d = FuncLoader.LoadFunction<d_gsk_transform_rotate_3d>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gsk), "gsk_transform_rotate_3d"));

		public Transform Rotate3d (float angle, Graphene.Vec3 axis)
		{
			return Wrap (gsk_transform_rotate_3d (Consumed, angle, axis == null ? IntPtr.Zero : axis.Handle));
		}

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_gsk_transform_scale(IntPtr raw, float factor_x, float factor_y);
		static d_gsk_transform_scale gsk_transform_scale = FuncLoader.LoadFunction<d_gsk_transform_scale>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gsk), "gsk_transform_scale"));

		public Transform Scale (float factor_x, float factor_y)
		{
			return Wrap (gsk_transform_scale (Consumed, factor_x, factor_y));
		}

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_gsk_transform_scale_3d(IntPtr raw, float factor_x, float factor_y, float factor_z);
		static d_gsk_transform_scale_3d gsk_transform_scale_3d = FuncLoader.LoadFunction<d_gsk_transform_scale_3d>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gsk), "gsk_transform_scale_3d"));

		public Transform Scale3d (float factor_x, float factor_y, float factor_z)
		{
			return Wrap (gsk_transform_scale_3d (Consumed, factor_x, factor_y, factor_z));
		}

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_gsk_transform_skew(IntPtr raw, float skew_x, float skew_y);
		static d_gsk_transform_skew gsk_transform_skew = FuncLoader.LoadFunction<d_gsk_transform_skew>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gsk), "gsk_transform_skew"));

		public Transform Skew (float skew_x, float skew_y)
		{
			return Wrap (gsk_transform_skew (Consumed, skew_x, skew_y));
		}

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_gsk_transform_transform(IntPtr raw, IntPtr other);
		static d_gsk_transform_transform gsk_transform_transform = FuncLoader.LoadFunction<d_gsk_transform_transform>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gsk), "gsk_transform_transform"));

		/// <summary>Applies <paramref name="other"/>'s operations on top of this transform's.</summary>
		public Transform With (Transform other)
		{
			return Wrap (gsk_transform_transform (Consumed, other == null ? IntPtr.Zero : other.Handle));
		}

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_gsk_transform_translate(IntPtr raw, IntPtr point);
		static d_gsk_transform_translate gsk_transform_translate = FuncLoader.LoadFunction<d_gsk_transform_translate>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gsk), "gsk_transform_translate"));

		public Transform Translate (Graphene.Point point)
		{
			return Wrap (gsk_transform_translate (Consumed, point == null ? IntPtr.Zero : point.Handle));
		}

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_gsk_transform_translate_3d(IntPtr raw, IntPtr point);
		static d_gsk_transform_translate_3d gsk_transform_translate_3d = FuncLoader.LoadFunction<d_gsk_transform_translate_3d>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gsk), "gsk_transform_translate_3d"));

		public Transform Translate3d (Graphene.Point3D point)
		{
			return Wrap (gsk_transform_translate_3d (Consumed, point == null ? IntPtr.Zero : point.Handle));
		}
	}
}

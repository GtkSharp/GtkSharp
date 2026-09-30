// Stroke.cs - Gsk Stroke class customizations
//
// This code is inserted after the automatically generated code.
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of version 2 of the Lesser GNU General
// Public License as published by the Free Software Foundation.

namespace Gsk {

	using System;
	using System.Runtime.InteropServices;

	public partial class Stroke {

		// gsk_stroke_set_dash (stroke, const float *dash, gsize n_dash) generated
		// as "float SetDash (ulong n_dash)": the array parameter became an
		// "out float", so the only way to call it was to hand GSK a pointer to
		// four bytes of the caller's stack and tell it to read n_dash floats from
		// there. There was no way to set a dash pattern, and trying corrupted the
		// frame. The getter returns (const float *, out n_dash) and had the
		// mirror-image problem. Both hidden in GskSharp.metadata.

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_gsk_stroke_set_dash(IntPtr raw, float[] dash, UIntPtr n_dash);
		static d_gsk_stroke_set_dash gsk_stroke_set_dash = FuncLoader.LoadFunction<d_gsk_stroke_set_dash>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gsk), "gsk_stroke_set_dash"));

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_gsk_stroke_get_dash(IntPtr raw, out UIntPtr n_dash);
		static d_gsk_stroke_get_dash gsk_stroke_get_dash = FuncLoader.LoadFunction<d_gsk_stroke_get_dash>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gsk), "gsk_stroke_get_dash"));

		/// <summary>
		/// The dash pattern: alternating on and off lengths, in the units the
		/// stroke is drawn in. An empty array -- or null -- means a solid line,
		/// which is what GSK stores as no dash at all.
		/// </summary>
		public float[] Dash {
			get {
				UIntPtr count;
				IntPtr raw = gsk_stroke_get_dash (Handle, out count);
				ulong n = (ulong) count;

				if (raw == IntPtr.Zero || n == 0)
					return new float [0];

				var dash = new float [n];
				Marshal.Copy (raw, dash, 0, (int) n);
				return dash;
			}
			set {
				// GSK reads the array only during the call, so nothing here has
				// to outlive it; passing null with a count of zero is how the
				// C API clears the pattern.
				if (value == null || value.Length == 0)
					gsk_stroke_set_dash (Handle, null, UIntPtr.Zero);
				else
					gsk_stroke_set_dash (Handle, value, new UIntPtr ((ulong) value.Length));
			}
		}
	}
}

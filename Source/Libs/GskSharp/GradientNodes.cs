// GradientNodes.cs - Gsk gradient node customizations
//
// This code is inserted after the automatically generated code.
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of version 2 of the Lesser GNU General
// Public License as published by the Free Software Foundation.

namespace Gsk {

	using System;
	using System.Runtime.InteropServices;

	/// <summary>Shared marshalling for the (const GskColorStop *, gsize) pair.</summary>
	/// <remarks>
	/// GskColorStop is a float offset followed by a GdkRGBA of four floats, all
	/// blittable, so an array of them can be handed straight to GSK. The count is
	/// the array's own length: it is not something a caller should be able to get
	/// wrong, and the generated constructors let them, which is the bug these
	/// files exist to fix.
	/// </remarks>
	static class ColorStopArray {

		public static ColorStop[] Check (ColorStop[] stops)
		{
			if (stops == null)
				throw new ArgumentNullException ("color_stops");
			// GSK asserts on this, and an assertion inside a constructor aborts
			// the process rather than raising anything catchable.
			if (stops.Length < 2)
				throw new ArgumentException ("a gradient needs at least two colour stops", "color_stops");

			return stops;
		}

		/// <summary>Copies a native GskColorStop array out into managed memory.</summary>
		public static ColorStop[] Read (IntPtr raw, ulong count)
		{
			if (raw == IntPtr.Zero || count == 0)
				return new ColorStop [0];

			var stops = new ColorStop [count];
			int size = Marshal.SizeOf<ColorStop> ();

			for (ulong i = 0; i < count; i++)
				stops [i] = Marshal.PtrToStructure<ColorStop> (raw + (int) (i * (ulong) size));

			return stops;
		}
	}

	public partial class LinearGradientNode {

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_gsk_linear_gradient_node_new(IntPtr bounds, IntPtr start, IntPtr end, ColorStop[] color_stops, UIntPtr n_color_stops);
		static d_gsk_linear_gradient_node_new gsk_linear_gradient_node_new = FuncLoader.LoadFunction<d_gsk_linear_gradient_node_new>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gsk), "gsk_linear_gradient_node_new"));

		/// <summary>A linear gradient over <paramref name="bounds"/>, running from
		/// <paramref name="start"/> to <paramref name="end"/>.</summary>
		public LinearGradientNode (Graphene.Rect bounds, Graphene.Point start, Graphene.Point end, ColorStop[] color_stops) : base (IntPtr.Zero)
		{
			Gsk.ColorStopArray.Check (color_stops);

			Owned = true;
			Raw = gsk_linear_gradient_node_new (
				bounds == null ? IntPtr.Zero : bounds.Handle,
				start == null ? IntPtr.Zero : start.Handle,
				end == null ? IntPtr.Zero : end.Handle,
				color_stops, new UIntPtr ((ulong) color_stops.Length));
		}

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_gsk_linear_gradient_node_get_color_stops(IntPtr raw, out UIntPtr n_stops);
		static d_gsk_linear_gradient_node_get_color_stops gsk_linear_gradient_node_get_color_stops = FuncLoader.LoadFunction<d_gsk_linear_gradient_node_get_color_stops>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gsk), "gsk_linear_gradient_node_get_color_stops"));

		/// <summary>The stops this gradient was built from, in order.</summary>
		public ColorStop[] ColorStops {
			get {
				UIntPtr count;
				IntPtr raw = gsk_linear_gradient_node_get_color_stops (Handle, out count);
				return Gsk.ColorStopArray.Read (raw, (ulong) count);
			}
		}
	}

	public partial class RepeatingLinearGradientNode {

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_gsk_repeating_linear_gradient_node_new(IntPtr bounds, IntPtr start, IntPtr end, ColorStop[] color_stops, UIntPtr n_color_stops);
		static d_gsk_repeating_linear_gradient_node_new gsk_repeating_linear_gradient_node_new = FuncLoader.LoadFunction<d_gsk_repeating_linear_gradient_node_new>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gsk), "gsk_repeating_linear_gradient_node_new"));

		/// <summary>As <see cref="LinearGradientNode"/>, but the stop pattern tiles
		/// beyond <paramref name="end"/> instead of holding the last colour.</summary>
		public RepeatingLinearGradientNode (Graphene.Rect bounds, Graphene.Point start, Graphene.Point end, ColorStop[] color_stops) : base (IntPtr.Zero)
		{
			Gsk.ColorStopArray.Check (color_stops);

			Owned = true;
			Raw = gsk_repeating_linear_gradient_node_new (
				bounds == null ? IntPtr.Zero : bounds.Handle,
				start == null ? IntPtr.Zero : start.Handle,
				end == null ? IntPtr.Zero : end.Handle,
				color_stops, new UIntPtr ((ulong) color_stops.Length));
		}
	}

	public partial class RadialGradientNode {

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_gsk_radial_gradient_node_new(IntPtr bounds, IntPtr center, float hradius, float vradius, float start, float end, ColorStop[] color_stops, UIntPtr n_color_stops);
		static d_gsk_radial_gradient_node_new gsk_radial_gradient_node_new = FuncLoader.LoadFunction<d_gsk_radial_gradient_node_new>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gsk), "gsk_radial_gradient_node_new"));

		/// <summary>An elliptical gradient centred on <paramref name="center"/>.
		/// <paramref name="start"/> and <paramref name="end"/> are fractions of the
		/// radius at which the first and last stop sit.</summary>
		public RadialGradientNode (Graphene.Rect bounds, Graphene.Point center, float hradius, float vradius, float start, float end, ColorStop[] color_stops) : base (IntPtr.Zero)
		{
			Gsk.ColorStopArray.Check (color_stops);

			Owned = true;
			Raw = gsk_radial_gradient_node_new (
				bounds == null ? IntPtr.Zero : bounds.Handle,
				center == null ? IntPtr.Zero : center.Handle,
				hradius, vradius, start, end,
				color_stops, new UIntPtr ((ulong) color_stops.Length));
		}

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_gsk_radial_gradient_node_get_color_stops(IntPtr raw, out UIntPtr n_stops);
		static d_gsk_radial_gradient_node_get_color_stops gsk_radial_gradient_node_get_color_stops = FuncLoader.LoadFunction<d_gsk_radial_gradient_node_get_color_stops>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gsk), "gsk_radial_gradient_node_get_color_stops"));

		/// <summary>The stops this gradient was built from, in order.</summary>
		public ColorStop[] ColorStops {
			get {
				UIntPtr count;
				IntPtr raw = gsk_radial_gradient_node_get_color_stops (Handle, out count);
				return Gsk.ColorStopArray.Read (raw, (ulong) count);
			}
		}
	}

	public partial class RepeatingRadialGradientNode {

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_gsk_repeating_radial_gradient_node_new(IntPtr bounds, IntPtr center, float hradius, float vradius, float start, float end, ColorStop[] color_stops, UIntPtr n_color_stops);
		static d_gsk_repeating_radial_gradient_node_new gsk_repeating_radial_gradient_node_new = FuncLoader.LoadFunction<d_gsk_repeating_radial_gradient_node_new>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gsk), "gsk_repeating_radial_gradient_node_new"));

		/// <summary>As <see cref="RadialGradientNode"/>, but the stop pattern tiles
		/// outwards instead of holding the last colour.</summary>
		public RepeatingRadialGradientNode (Graphene.Rect bounds, Graphene.Point center, float hradius, float vradius, float start, float end, ColorStop[] color_stops) : base (IntPtr.Zero)
		{
			Gsk.ColorStopArray.Check (color_stops);

			Owned = true;
			Raw = gsk_repeating_radial_gradient_node_new (
				bounds == null ? IntPtr.Zero : bounds.Handle,
				center == null ? IntPtr.Zero : center.Handle,
				hradius, vradius, start, end,
				color_stops, new UIntPtr ((ulong) color_stops.Length));
		}
	}

	public partial class ConicGradientNode {

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_gsk_conic_gradient_node_new(IntPtr bounds, IntPtr center, float rotation, ColorStop[] color_stops, UIntPtr n_color_stops);
		static d_gsk_conic_gradient_node_new gsk_conic_gradient_node_new = FuncLoader.LoadFunction<d_gsk_conic_gradient_node_new>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gsk), "gsk_conic_gradient_node_new"));

		/// <summary>A gradient that sweeps around <paramref name="center"/>,
		/// starting <paramref name="rotation"/> degrees from straight up.</summary>
		public ConicGradientNode (Graphene.Rect bounds, Graphene.Point center, float rotation, ColorStop[] color_stops) : base (IntPtr.Zero)
		{
			Gsk.ColorStopArray.Check (color_stops);

			Owned = true;
			Raw = gsk_conic_gradient_node_new (
				bounds == null ? IntPtr.Zero : bounds.Handle,
				center == null ? IntPtr.Zero : center.Handle,
				rotation,
				color_stops, new UIntPtr ((ulong) color_stops.Length));
		}

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_gsk_conic_gradient_node_get_color_stops(IntPtr raw, out UIntPtr n_stops);
		static d_gsk_conic_gradient_node_get_color_stops gsk_conic_gradient_node_get_color_stops = FuncLoader.LoadFunction<d_gsk_conic_gradient_node_get_color_stops>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gsk), "gsk_conic_gradient_node_get_color_stops"));

		/// <summary>The stops this gradient was built from, in order.</summary>
		public ColorStop[] ColorStops {
			get {
				UIntPtr count;
				IntPtr raw = gsk_conic_gradient_node_get_color_stops (Handle, out count);
				return Gsk.ColorStopArray.Read (raw, (ulong) count);
			}
		}
	}
}

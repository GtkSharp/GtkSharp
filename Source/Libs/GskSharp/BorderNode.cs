// BorderNode.cs - Gsk BorderNode class customizations
//
// This code is inserted after the automatically generated code.
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of version 2 of the Lesser GNU General
// Public License as published by the Free Software Foundation.

namespace Gsk {

	using System;
	using System.Runtime.InteropServices;

	public partial class BorderNode {

		// Both getters return a pointer to four elements -- one per edge, in the
		// order top, right, bottom, left. The api.xml records the element type
		// with no length, so codegen declared gsk_border_node_get_widths as
		// *returning a float* rather than a float*: on x86-64 the wrapper read
		// XMM0 while GSK had put the pointer in RAX, so the answer was whatever
		// the last floating-point operation had left there. get_colors had the
		// same shape and wrapped the array's first element as the whole answer.
		// Both hidden in GskSharp.metadata.

		/// <summary>The four edges, in the order top, right, bottom, left.</summary>
		/// <remarks>Named to match GSK's own ordering, which is CSS's.</remarks>
		public const int TopEdge = 0, RightEdge = 1, BottomEdge = 2, LeftEdge = 3;

		const int Edges = 4;

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_gsk_border_node_get_widths(IntPtr raw);
		static d_gsk_border_node_get_widths gsk_border_node_get_widths = FuncLoader.LoadFunction<d_gsk_border_node_get_widths>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gsk), "gsk_border_node_get_widths"));

		/// <summary>The border's four widths, top, right, bottom, left.</summary>
		public float[] Widths {
			get {
				IntPtr raw = gsk_border_node_get_widths (Handle);
				if (raw == IntPtr.Zero)
					return new float [Edges];

				var widths = new float [Edges];
				Marshal.Copy (raw, widths, 0, Edges);
				return widths;
			}
		}

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_gsk_border_node_get_colors(IntPtr raw);
		static d_gsk_border_node_get_colors gsk_border_node_get_colors = FuncLoader.LoadFunction<d_gsk_border_node_get_colors>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gsk), "gsk_border_node_get_colors"));

		/// <summary>The border's four colours, top, right, bottom, left.</summary>
		public Gdk.RGBA[] Colors {
			get {
				IntPtr raw = gsk_border_node_get_colors (Handle);
				if (raw == IntPtr.Zero)
					return new Gdk.RGBA [Edges];

				var colors = new Gdk.RGBA [Edges];
				int size = Marshal.SizeOf<Gdk.RGBA> ();

				for (int i = 0; i < Edges; i++)
					colors [i] = Marshal.PtrToStructure<Gdk.RGBA> (raw + i * size);

				return colors;
			}
		}
	}
}

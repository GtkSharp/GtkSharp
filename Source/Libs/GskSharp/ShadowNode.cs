// ShadowNode.cs - Gsk ShadowNode class customizations
//
// This code is inserted after the automatically generated code.
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of version 2 of the Lesser GNU General
// Public License as published by the Free Software Foundation.

namespace Gsk {

	using System;
	using System.Runtime.InteropServices;

	public partial class ShadowNode {

		// gsk_shadow_node_new (child, const GskShadow *shadows, gsize n_shadows).
		// The generated constructor was ShadowNode (RenderNode, Shadow, ulong):
		// one shadow copied to the heap, and a count the caller chose
		// independently of it. Hidden in GskSharp.metadata.
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_gsk_shadow_node_new(IntPtr child, Shadow[] shadows, UIntPtr n_shadows);
		static d_gsk_shadow_node_new gsk_shadow_node_new = FuncLoader.LoadFunction<d_gsk_shadow_node_new>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gsk), "gsk_shadow_node_new"));

		/// <summary>Draws <paramref name="child"/> with each of
		/// <paramref name="shadows"/> beneath it.</summary>
		/// <remarks>GskShadow is a GdkRGBA followed by three floats -- dx, dy and
		/// blur radius -- so the array is blittable and goes to GSK unchanged.</remarks>
		public ShadowNode (RenderNode child, Shadow[] shadows) : base (IntPtr.Zero)
		{
			if (child == null)
				throw new ArgumentNullException ("child");
			if (shadows == null)
				throw new ArgumentNullException ("shadows");
			// gsk_shadow_node_new asserts n_shadows > 0, and an assertion here
			// aborts the process rather than raising anything catchable.
			if (shadows.Length == 0)
				throw new ArgumentException ("a shadow node needs at least one shadow", "shadows");

			Owned = true;
			Raw = gsk_shadow_node_new (child.Handle, shadows, new UIntPtr ((ulong) shadows.Length));

			GC.KeepAlive (child);
		}

		/// <summary>The shadows this node was built from, in order.</summary>
		/// <remarks>A convenience over <see cref="GetShadow"/> and
		/// <see cref="NShadows"/>, which are correct individually but leave the
		/// caller to write the loop every time.</remarks>
		public Shadow[] Shadows {
			get {
				ulong count = NShadows;
				var shadows = new Shadow [count];

				for (ulong i = 0; i < count; i++)
					shadows [i] = GetShadow (i);

				return shadows;
			}
		}
	}
}

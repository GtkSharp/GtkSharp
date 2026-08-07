// RenderNode.cs - Gsk RenderNode class customizations
//
// This code is inserted after the automatically generated code.
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of version 2 of the Lesser GNU General
// Public License as published by the Free Software Foundation.

namespace Gsk {

	using System;
	using System.Runtime.InteropServices;

	public partial class RenderNode {

		// gsk_render_node_get_children returns a GskRenderNode** plus a count.
		// The api.xml carries the pointer-to-pointer type but the length is a
		// separate out-parameter rather than a NULL terminator, and codegen has
		// no rule for that shape - so the generated GetChildren wrapped the
		// *array* address in a single RenderNode, whose every use then read a
		// GskRenderNode header out of an array of pointers. Hidden in
		// GskSharp.metadata; this is the call the C API describes.
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_gsk_render_node_get_children(IntPtr raw, out UIntPtr n_children);
		static d_gsk_render_node_get_children gsk_render_node_get_children = FuncLoader.LoadFunction<d_gsk_render_node_get_children>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gsk), "gsk_render_node_get_children"));

		/// <summary>
		/// The node's children, in the order GSK stores them. The array belongs
		/// to the node, so the wrappers returned here reference the children
		/// rather than taking them over. Empty for a node that has none.
		/// </summary>
		public RenderNode[] Children {
			get {
				UIntPtr count;
				IntPtr raw = gsk_render_node_get_children (Handle, out count);
				ulong n = (ulong) count;

				if (raw == IntPtr.Zero || n == 0)
					return new RenderNode [0];

				var children = new RenderNode [n];
				for (ulong i = 0; i < n; i++) {
					IntPtr child = Marshal.ReadIntPtr (raw, (int) (i * (ulong) IntPtr.Size));
					children [i] = GLib.Opaque.GetOpaque (child, typeof (RenderNode), false) as RenderNode;
				}

				return children;
			}
		}
	}
}

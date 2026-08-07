// ContainerNode.cs - Gsk ContainerNode class customizations
//
// This code is inserted after the automatically generated code.
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of version 2 of the Lesser GNU General
// Public License as published by the Free Software Foundation.

namespace Gsk {

	using System;
	using System.Runtime.InteropServices;

	public partial class ContainerNode {

		// gsk_container_node_new takes a GskRenderNode** plus a count. The
		// generated constructor was ContainerNode (RenderNode, uint), which
		// passed one node's handle where an array of handles was expected: GSK
		// read the node's own first word - its GskRenderNodeClass pointer - as
		// children[0] and reffed it. Hidden in GskSharp.metadata.
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_gsk_container_node_new(IntPtr[] children, uint n_children);
		static d_gsk_container_node_new gsk_container_node_new = FuncLoader.LoadFunction<d_gsk_container_node_new>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gsk), "gsk_container_node_new"));

		/// <summary>Groups <paramref name="children"/> into one node, drawn in order.</summary>
		public ContainerNode (RenderNode[] children) : base (IntPtr.Zero)
		{
			if (children == null)
				throw new ArgumentNullException ("children");

			var handles = new IntPtr [children.Length];
			for (int i = 0; i < children.Length; i++) {
				if (children [i] == null)
					throw new ArgumentException ("a container node cannot hold a null child", "children");
				handles [i] = children [i].Handle;
			}

			Owned = true;
			Raw = gsk_container_node_new (handles, (uint) children.Length);

			GC.KeepAlive (children);
		}
	}
}

// PathBuilder.cs - Gsk PathBuilder class customizations
//
// This code is inserted after the automatically generated code.
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of version 2 of the Lesser GNU General
// Public License as published by the Free Software Foundation.

namespace Gsk {

	using System;
	using System.Runtime.InteropServices;

	public partial class PathBuilder {

		// gsk_path_builder_free_to_path takes the builder as (transfer full) and
		// frees it, exactly the way gdk_content_formats_builder_free_to_formats
		// does. Codegen cannot see that - an api.xml <method> never describes
		// its instance's ownership - so the generated FreeToPath left a live
		// wrapper holding a freed builder, which unreffed it again when it was
		// collected. Hidden in GskSharp.metadata; the reference the callee eats
		// is taken here.
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_gsk_path_builder_ref_transfer(IntPtr raw);
		static d_gsk_path_builder_ref_transfer gsk_path_builder_ref_transfer = FuncLoader.LoadFunction<d_gsk_path_builder_ref_transfer>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gsk), "gsk_path_builder_ref"));

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_gsk_path_builder_free_to_path(IntPtr raw);
		static d_gsk_path_builder_free_to_path gsk_path_builder_free_to_path = FuncLoader.LoadFunction<d_gsk_path_builder_free_to_path>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gsk), "gsk_path_builder_free_to_path"));

		/// <summary>
		/// The path built so far, clearing the builder's operations. Unlike the C
		/// function it is named after, this does not destroy the builder: the
		/// wrapper stays usable, and the builder is freed when it is.
		/// </summary>
		public Path FreeToPath ()
		{
			gsk_path_builder_ref_transfer (Handle);
			IntPtr raw = gsk_path_builder_free_to_path (Handle);
			return raw == IntPtr.Zero ? null : (Path) GLib.Opaque.GetOpaque (raw, typeof (Path), true);
		}
	}
}

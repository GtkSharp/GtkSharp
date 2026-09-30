// ContentFormatsBuilder.cs - Gdk ContentFormatsBuilder class customizations
//
// This code is inserted after the automatically generated code.
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of version 2 of the Lesser GNU General
// Public License as published by the Free Software Foundation.

namespace Gdk {

	using System;
	using System.Runtime.InteropServices;

	public partial class ContentFormatsBuilder {

		// gdk_content_formats_builder_free_to_formats takes the builder as
		// (transfer full) -- the name says so, and the api.xml has no way to.
		// The generated wrapper kept owning the freed builder and unreffed it
		// again on dispose. Taking a reference first leaves the managed object
		// alive and usable, which is what ToFormats already does.
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_gdk_content_formats_builder_ref_transfer(IntPtr raw);
		static d_gdk_content_formats_builder_ref_transfer gdk_content_formats_builder_ref_transfer = FuncLoader.LoadFunction<d_gdk_content_formats_builder_ref_transfer>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gdk), "gdk_content_formats_builder_ref"));

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_gdk_content_formats_builder_free_to_formats(IntPtr raw);
		static d_gdk_content_formats_builder_free_to_formats gdk_content_formats_builder_free_to_formats = FuncLoader.LoadFunction<d_gdk_content_formats_builder_free_to_formats>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gdk), "gdk_content_formats_builder_free_to_formats"));

		public Gdk.ContentFormats FreeToFormats ()
		{
			gdk_content_formats_builder_ref_transfer (Handle);
			IntPtr raw = gdk_content_formats_builder_free_to_formats (Handle);
			return raw == IntPtr.Zero ? null : (Gdk.ContentFormats) GLib.Opaque.GetOpaque (raw, typeof (Gdk.ContentFormats), true);
		}
	}
}

// ContentFormats.cs - Gdk ContentFormats class customizations
//
// This code is inserted after the automatically generated code.
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of version 2 of the Lesser GNU General
// Public License as published by the Free Software Foundation.

namespace Gdk {

	using System;
	using System.Runtime.InteropServices;

	public partial class ContentFormats {

		// gdk_content_formats_new takes an array of n_mime_types strings. The
		// api.xml describes it as const-char** with an explicit length rather
		// than a null-terminated array, and codegen has no rule for that shape,
		// so it emitted ContentFormats(string, uint) and passed ONE strdup'd
		// string -- which Gdk then read as an array of pointers, using the
		// characters of the string as addresses. Hidden in GdkSharp.metadata.
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_gdk_content_formats_new(IntPtr[] mime_types, uint n_mime_types);
		static d_gdk_content_formats_new gdk_content_formats_new = FuncLoader.LoadFunction<d_gdk_content_formats_new>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gdk), "gdk_content_formats_new"));

		public ContentFormats (string[] mime_types)
		{
			int count = mime_types == null ? 0 : mime_types.Length;
			IntPtr[] native = new IntPtr [count + 1];
			for (int i = 0; i < count; i++)
				native [i] = GLib.Marshaller.StringToPtrGStrdup (mime_types [i]);
			native [count] = IntPtr.Zero;

			Raw = gdk_content_formats_new (native, (uint) count);

			for (int i = 0; i < count; i++)
				GLib.Marshaller.Free (native [i]);
		}

		// gdk_content_formats_get_gtypes returns a borrowed GType array plus its
		// length. Codegen turned the array pointer itself into a single
		// GLib.GType, which is a GType whose value is an address.
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_gdk_content_formats_get_gtypes(IntPtr raw, out UIntPtr n_gtypes);
		static d_gdk_content_formats_get_gtypes gdk_content_formats_get_gtypes = FuncLoader.LoadFunction<d_gdk_content_formats_get_gtypes>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gdk), "gdk_content_formats_get_gtypes"));

		public GLib.GType[] GetGtypes ()
		{
			UIntPtr count;
			IntPtr array = gdk_content_formats_get_gtypes (Handle, out count);
			if (array == IntPtr.Zero)
				return new GLib.GType [0];

			GLib.GType[] result = new GLib.GType [(int) count];
			for (int i = 0; i < result.Length; i++)
				result [i] = new GLib.GType (Marshal.ReadIntPtr (array, i * IntPtr.Size));
			return result;
		}

		// Every gdk_content_formats_union* takes its receiver as (transfer full):
		// the callee eats a reference. The api.xml has no way to say so -- a
		// <method> describes its parameters' ownership but never the instance's
		// -- so the generated wrappers handed over Handle and went on owning it.
		// The formats were then freed underneath a live wrapper, which unreffed
		// them a second time when it was disposed or finalized, and the crash
		// landed on whatever touched the reused address next.
		//
		// Taking a reference before the call is what keeps the managed object
		// behaving like every other one here: a method does not destroy the
		// object it was called on.
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_gdk_content_formats_ref_transfer(IntPtr raw);
		static d_gdk_content_formats_ref_transfer gdk_content_formats_ref_transfer = FuncLoader.LoadFunction<d_gdk_content_formats_ref_transfer>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gdk), "gdk_content_formats_ref"));

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_gdk_content_formats_union(IntPtr raw, IntPtr second);
		static d_gdk_content_formats_union gdk_content_formats_union = FuncLoader.LoadFunction<d_gdk_content_formats_union>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gdk), "gdk_content_formats_union"));

		public Gdk.ContentFormats Union (Gdk.ContentFormats second)
		{
			gdk_content_formats_ref_transfer (Handle);
			IntPtr raw = gdk_content_formats_union (Handle, second == null ? IntPtr.Zero : second.Handle);
			return raw == IntPtr.Zero ? null : (Gdk.ContentFormats) GLib.Opaque.GetOpaque (raw, typeof (Gdk.ContentFormats), true);
		}

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_gdk_content_formats_union_step(IntPtr raw);

		static d_gdk_content_formats_union_step gdk_content_formats_union_deserialize_gtypes = FuncLoader.LoadFunction<d_gdk_content_formats_union_step>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gdk), "gdk_content_formats_union_deserialize_gtypes"));
		static d_gdk_content_formats_union_step gdk_content_formats_union_deserialize_mime_types = FuncLoader.LoadFunction<d_gdk_content_formats_union_step>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gdk), "gdk_content_formats_union_deserialize_mime_types"));
		static d_gdk_content_formats_union_step gdk_content_formats_union_serialize_gtypes = FuncLoader.LoadFunction<d_gdk_content_formats_union_step>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gdk), "gdk_content_formats_union_serialize_gtypes"));
		static d_gdk_content_formats_union_step gdk_content_formats_union_serialize_mime_types = FuncLoader.LoadFunction<d_gdk_content_formats_union_step>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gdk), "gdk_content_formats_union_serialize_mime_types"));

		Gdk.ContentFormats UnionStep (d_gdk_content_formats_union_step step)
		{
			gdk_content_formats_ref_transfer (Handle);
			IntPtr raw = step (Handle);
			return raw == IntPtr.Zero ? null : (Gdk.ContentFormats) GLib.Opaque.GetOpaque (raw, typeof (Gdk.ContentFormats), true);
		}

		public Gdk.ContentFormats UnionDeserializeGtypes ()
		{
			return UnionStep (gdk_content_formats_union_deserialize_gtypes);
		}

		public Gdk.ContentFormats UnionDeserializeMimeTypes ()
		{
			return UnionStep (gdk_content_formats_union_deserialize_mime_types);
		}

		public Gdk.ContentFormats UnionSerializeGtypes ()
		{
			return UnionStep (gdk_content_formats_union_serialize_gtypes);
		}

		public Gdk.ContentFormats UnionSerializeMimeTypes ()
		{
			return UnionStep (gdk_content_formats_union_serialize_mime_types);
		}
	}
}

// ContentProvider.cs - Gdk ContentProvider class customizations
//
// This code is inserted after the automatically generated code.
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of version 2 of the Lesser GNU General
// Public License as published by the Free Software Foundation.

namespace Gdk {

	using System;
	using System.Runtime.InteropServices;

	public partial class ContentProvider {

		// gdk_content_provider_get_value fills a GValue that the CALLER has
		// already initialised to the type it is asking for -- the provider
		// answers with G_VALUE_HOLDS and refuses anything else. Codegen bound it
		// as a plain out-parameter over Marshal.AllocHGlobal, so Gdk was handed
		// a block of uninitialised memory and read a GType out of whatever
		// happened to be there. Hidden in GdkSharp.metadata; the type has to be
		// part of the managed signature, because it is an input.
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate bool d_gdk_content_provider_get_value(IntPtr raw, IntPtr value, out IntPtr error);
		static d_gdk_content_provider_get_value gdk_content_provider_get_value = FuncLoader.LoadFunction<d_gdk_content_provider_get_value>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gdk), "gdk_content_provider_get_value"));

		/// <summary>
		/// Asks the provider for its content as a <paramref name="type"/>.
		/// Raises <see cref="GLib.GException"/> when the provider does not offer
		/// that type.
		/// </summary>
		public bool GetValue (GLib.GType type, out GLib.Value value)
		{
			// new GLib.Value (GType) zeroes the struct and then g_value_inits
			// it, which is exactly the G_VALUE_INIT + g_value_init a C caller
			// writes.
			value = new GLib.Value (type);

			IntPtr native_value = GLib.Marshaller.StructureToPtrAlloc (value);
			IntPtr error = IntPtr.Zero;
			bool ret = gdk_content_provider_get_value (Handle, native_value, out error);
			value = (GLib.Value) Marshal.PtrToStructure (native_value, typeof (GLib.Value));
			Marshal.FreeHGlobal (native_value);
			if (error != IntPtr.Zero)
				throw new GLib.GException (error);
			return ret;
		}

		// gdk_content_provider_new_union takes "GdkContentProvider **providers,
		// gsize n_providers" -- the same pointer-plus-count shape codegen has no
		// rule for as gsk_container_node_new and gtk_drop_target_set_gtypes.
		// It emitted
		//
		//   public ContentProvider (Gdk.ContentProvider providers, ulong n)
		//
		// and passed a single provider's handle as the address of the array, so
		// GDK read that object's own GTypeInstance class pointer as element
		// zero and reffed it as a content provider. This is how a drag source
		// offers one thing several ways -- a file as a URI and as an image --
		// which is the whole reason the union provider exists.
		//
		// Both the array and a reference to every provider in it are
		// (transfer full): GDK keeps the block and frees it with g_free, and
		// unrefs each element when the union is disposed. So the array has to
		// come from g_malloc, and each reference has to be taken here rather
		// than surrendered, or the managed wrappers are left holding pointers
		// the union has already released.
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_gdk_content_provider_new_union(IntPtr providers, UIntPtr n_providers);
		static d_gdk_content_provider_new_union gdk_content_provider_new_union = FuncLoader.LoadFunction<d_gdk_content_provider_new_union>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gdk), "gdk_content_provider_new_union"));

		public ContentProvider (Gdk.ContentProvider[] providers) : base (IntPtr.Zero)
		{
			if (GetType () != typeof (ContentProvider)) {
				CreateNativeObject (new string [0], new GLib.Value [0]);
				return;
			}

			int count = providers == null ? 0 : providers.Length;

			IntPtr native = IntPtr.Zero;
			if (count > 0) {
				native = GLib.Marshaller.Malloc ((ulong) count * (ulong) IntPtr.Size);
				for (int i = 0; i < count; i++)
					Marshal.WriteIntPtr (native, i * IntPtr.Size, providers [i].OwnedHandle);
			}

			Raw = gdk_content_provider_new_union (native, new UIntPtr ((ulong) count));
		}
	}
}

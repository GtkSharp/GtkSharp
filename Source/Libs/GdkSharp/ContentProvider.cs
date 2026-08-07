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
	}
}

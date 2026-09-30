// Texture.cs - Gdk Texture class customizations
//
// This code is inserted after the automatically generated code.
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of version 2 of the Lesser GNU General
// Public License as published by the Free Software Foundation.

namespace Gdk {

	using System;
	using System.Runtime.InteropServices;

	public partial class Texture {

		// gdk_texture_download writes Height * stride bytes into storage the
		// CALLER provides. Codegen bound the guchar* as `out byte` and returned
		// it, so the generated Download(stride) handed Gdk the address of one
		// stack byte and let it write a whole image through it. The parameter is
		// hidden in GdkSharp.metadata; this is the buffer the call actually
		// needs.
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_gdk_texture_download(IntPtr raw, byte[] data, UIntPtr stride);
		static d_gdk_texture_download gdk_texture_download = FuncLoader.LoadFunction<d_gdk_texture_download>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gdk), "gdk_texture_download"));

		/// <summary>Bytes per pixel of the format gdk_texture_download always
		/// produces: GDK_MEMORY_DEFAULT, i.e. cairo's ARGB32.</summary>
		const int DownloadBytesPerPixel = 4;

		/// <summary>
		/// Copies the texture into <paramref name="data"/> as premultiplied
		/// 32-bit BGRA (cairo ARGB32), one row every <paramref name="stride"/>
		/// bytes.
		/// </summary>
		public void Download (byte[] data, ulong stride)
		{
			if (data == null)
				throw new ArgumentNullException ("data");

			ulong minimum = (ulong) Width * DownloadBytesPerPixel;
			if (stride < minimum)
				throw new ArgumentOutOfRangeException ("stride", "a downloaded row is 4 bytes per pixel, so stride must be at least " + minimum);
			if ((ulong) data.LongLength < stride * (ulong) Height)
				throw new ArgumentException ("buffer holds fewer than stride * Height bytes", "data");

			gdk_texture_download (Handle, data, new UIntPtr (stride));
		}

		/// <summary>
		/// Copies the texture into a freshly allocated tightly packed buffer of
		/// premultiplied 32-bit BGRA, Width * 4 bytes per row.
		/// </summary>
		public byte[] Download ()
		{
			ulong stride = (ulong) Width * DownloadBytesPerPixel;
			byte[] data = new byte [stride * (ulong) Height];
			Download (data, stride);
			return data;
		}
	}
}

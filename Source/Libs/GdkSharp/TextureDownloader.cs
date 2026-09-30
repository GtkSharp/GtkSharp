// TextureDownloader.cs - Gdk TextureDownloader class customizations
//
// This code is inserted after the automatically generated code.
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of version 2 of the Lesser GNU General
// Public License as published by the Free Software Foundation.

namespace Gdk {

	using System;
	using System.Runtime.InteropServices;

	public partial class TextureDownloader {

		// Same defect as Gdk.Texture.Download: the guchar* the caller has to
		// allocate was bound as `out byte`, so the generated DownloadInto(stride)
		// let Gdk write a whole image over one stack byte. Hidden in
		// GdkSharp.metadata and replaced here.
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_gdk_texture_downloader_download_into(IntPtr raw, byte[] data, UIntPtr stride);
		static d_gdk_texture_downloader_download_into gdk_texture_downloader_download_into = FuncLoader.LoadFunction<d_gdk_texture_downloader_download_into>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gdk), "gdk_texture_downloader_download_into"));

		/// <summary>
		/// Copies the downloader's texture into <paramref name="data"/> in the
		/// downloader's current <see cref="Format"/>, one row every
		/// <paramref name="stride"/> bytes.
		/// </summary>
		public void DownloadInto (byte[] data, ulong stride)
		{
			if (data == null)
				throw new ArgumentNullException ("data");

			var texture = Texture;
			if (texture == null)
				throw new InvalidOperationException ("the downloader has no texture");
			if ((ulong) data.LongLength < stride * (ulong) texture.Height)
				throw new ArgumentException ("buffer holds fewer than stride * Texture.Height bytes", "data");

			gdk_texture_downloader_download_into (Handle, data, new UIntPtr (stride));
		}
	}
}

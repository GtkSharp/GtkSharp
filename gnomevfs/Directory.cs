//
// Directory.cs: Bindings for gnome-vfs directory functions calls.
//
// Author:
//   Jeroen Zwartepoorte <jeroen@xs4all.nl>
//
// (C) Copyright Jeroen Zwartepoorte 2004
//

using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Gnome.Vfs {
	public class Directory {
		[DllImport ("gnomevfs-2")]
		private static extern Result gnome_vfs_directory_list_load (out IntPtr list, string uri, FileInfoOptions options);

		public static FileInfo[] GetEntries (Uri uri)
		{
			return GetEntries (uri.ToString ());
		}

		public static FileInfo[] GetEntries (Uri uri, FileInfoOptions options)
		{
			return GetEntries (uri.ToString (), options);
		}

		public static FileInfo[] GetEntries (string text_uri)
		{
			return GetEntries (text_uri, FileInfoOptions.Default);
		}

		public static FileInfo[] GetEntries (string text_uri, FileInfoOptions options)
		{
			IntPtr raw_ret;
			Result result = gnome_vfs_directory_list_load (out raw_ret, text_uri, options);
			Vfs.ThrowException (text_uri, result);
			
			GLib.List list = new GLib.List (raw_ret, typeof (FileInfo.FileInfoNative));
			FileInfo[] entries = new FileInfo [list.Count];
			for (int i = 0; i < list.Count; i++)
				entries[i] = new FileInfo ((FileInfo.FileInfoNative) list [i]);
			
			return entries;
		}
	}
}

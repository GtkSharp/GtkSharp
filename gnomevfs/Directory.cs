//  Directory.cs - Bindings for gnome-vfs directory functions calls.
//
//  Authors:  Jeroen Zwartepoorte  <jeroen@xs4all.nl>
//
//  Copyright (c) 2004 Jeroen Zwartepoorte
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of version 2 of the Lesser GNU General
// Public License as published by the Free Software Foundation.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public
// License along with this program; if not, write to the
// Free Software Foundation, Inc., 59 Temple Place - Suite 330,
// Boston, MA 02111-1307, USA.

using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Gnome.Vfs {
	public class Directory {
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

		[DllImport ("gnomevfs-2")]
		private static extern Result gnome_vfs_directory_list_load (out IntPtr list, string uri, FileInfoOptions options);

		[DllImport ("gnomevfs-2")]
		private static extern void gnome_vfs_file_info_list_free (IntPtr list);

		public static FileInfo[] GetEntries (string text_uri, FileInfoOptions options)
		{
			IntPtr raw_ret;
			Result result = gnome_vfs_directory_list_load (out raw_ret, text_uri, options);
			Vfs.ThrowException (text_uri, result);
			
			GLib.List list = new GLib.List (raw_ret, typeof (FileInfo.FileInfoNative));
			FileInfo[] entries = new FileInfo [list.Count];
			for (int i = 0; i < list.Count; i++)
				entries[i] = new FileInfo ((FileInfo.FileInfoNative) list [i]);
			gnome_vfs_file_info_list_free (raw_ret);
			
			return entries;
		}
		
		public static void GetEntries (Uri uri, FileInfoOptions options,
					       uint itemsPerNotification, int priority,
					       AsyncDirectoryLoadCallback callback)
		{
			GetEntries (uri.ToString (), options, itemsPerNotification, priority, callback);
		}
		
		[DllImport ("gnomevfs-2")]
		private static extern void gnome_vfs_async_load_directory (out IntPtr handle, string uri, FileInfoOptions options, uint items_per_notification, int priority, AsyncDirectoryLoadCallbackNative native, IntPtr data);
		
		public static void GetEntries (string uri, FileInfoOptions options,
					       uint itemsPerNotification, int priority,
					       AsyncDirectoryLoadCallback callback)
		{
			IntPtr handle = IntPtr.Zero;
			AsyncDirectoryLoadCallbackWrapper wrapper = new AsyncDirectoryLoadCallbackWrapper (callback, null);
			gnome_vfs_async_load_directory (out handle, uri, options, itemsPerNotification,
							priority, wrapper.NativeDelegate, IntPtr.Zero);
		}
		
		[DllImport ("gnomevfs-2")]
		private static extern Result gnome_vfs_make_directory (string uri, uint perm);
		
		public static Result Create (Uri uri, uint perm)
		{
			return Create (uri.ToString (), perm);
		}
		
		public static Result Create (string uri, uint perm)
		{
			return gnome_vfs_make_directory (uri, perm);
		}
		
		[DllImport ("gnomevfs-2")]
		private static extern Result gnome_vfs_remove_directory (string uri);
		
		public static Result Delete (Uri uri)
		{
			return Delete (uri.ToString ());
		}
		
		public static Result Delete (string uri)
		{
			return gnome_vfs_remove_directory (uri);
		}
	}
}

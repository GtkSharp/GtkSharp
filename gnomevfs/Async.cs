//  Async.cs - Bindings for gnome-vfs asynchronized file operations.
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
	public class Async {
		public enum Priority {
			Min = -10,
			Default = 0,
			Max = 10
		}
	
		[DllImport ("gnomevfs-2")]
		private static extern void gnome_vfs_async_cancel (IntPtr handle);
		
		public static void Cancel (Handle handle)
		{
			gnome_vfs_async_cancel (handle.Raw);
		}

		[DllImport ("gnomevfs-2")]
		private static extern void gnome_vfs_async_close (IntPtr handle, AsyncCallbackNative callback, IntPtr data);
		
		public static void Close (Handle handle, AsyncCallback callback)
		{
			AsyncCallbackWrapper wrapper = new AsyncCallbackWrapper (callback, null);
			gnome_vfs_async_close (handle.Raw, wrapper.NativeDelegate, IntPtr.Zero);
		}

		[DllImport ("gnomevfs-2")]
		private static extern void gnome_vfs_async_create (out IntPtr handle, string uri, OpenMode mode, bool exclusive, uint perm, int priority, AsyncCallbackNative callback, IntPtr data);
		
		public static Handle Create (string uri, OpenMode mode, bool exclusive, FilePermissions perm, int priority, AsyncCallback callback)
		{
			IntPtr handle = IntPtr.Zero;
			AsyncCallbackWrapper wrapper = new AsyncCallbackWrapper (callback, null);
			gnome_vfs_async_create (out handle, uri, mode, exclusive, (uint)perm, priority, wrapper.NativeDelegate, IntPtr.Zero);
			return new Handle (handle);
		}
		
		public static Handle Create (Uri uri, OpenMode mode, bool exclusive, FilePermissions perm, int priority, AsyncCallback callback)
		{
			return Create (uri.ToString (), mode, exclusive, perm, priority, callback);
		}

		[DllImport ("gnomevfs-2")]
		private static extern void gnome_vfs_async_open (out IntPtr handle, string uri, OpenMode mode, int priority, AsyncCallbackNative callback, IntPtr data);
		
		public static Handle Open (string uri, OpenMode mode, int priority, AsyncCallback callback)
		{
			IntPtr handle = IntPtr.Zero;
			AsyncCallbackWrapper wrapper = new AsyncCallbackWrapper (callback, null);
			gnome_vfs_async_open (out handle, uri, mode, priority, wrapper.NativeDelegate, IntPtr.Zero);
			return new Handle (handle);
		}
		
		public static Handle Open (Uri uri, OpenMode mode, int priority, AsyncCallback callback)
		{
			return Open (uri.ToString (), mode, priority, callback);
		}

		[DllImport ("gnomevfs-2")]
		private static extern void gnome_vfs_async_read (IntPtr handle, out byte buffer, uint bytes, AsyncReadCallbackNative callback, IntPtr data);
		
		public static void Read (Handle handle, out byte buffer, uint bytes, AsyncReadCallback callback)
		{
			AsyncReadCallbackWrapper wrapper = new AsyncReadCallbackWrapper (callback, null);
			gnome_vfs_async_read (handle.Raw, out buffer, bytes, wrapper.NativeDelegate, IntPtr.Zero);
		}
		
		[DllImport ("gnomevfs-2")]
		private static extern void gnome_vfs_async_seek (IntPtr handle, SeekPosition whence, long offset, AsyncCallbackNative callback, IntPtr data);
		
		public static void Seek (Handle handle, SeekPosition whence, long offset, AsyncCallback callback)
		{
			AsyncCallbackWrapper wrapper = new AsyncCallbackWrapper (callback, null);
			gnome_vfs_async_seek (handle.Raw, whence, offset, wrapper.NativeDelegate, IntPtr.Zero);
		}

		[DllImport ("gnomevfs-2")]
		private static extern void gnome_vfs_async_write (IntPtr handle, out byte buffer, uint bytes, AsyncWriteCallbackNative callback, IntPtr data);
		
		public static void Write (Handle handle, out byte buffer, uint bytes, AsyncWriteCallback callback)
		{
			AsyncWriteCallbackWrapper wrapper = new AsyncWriteCallbackWrapper (callback, null);
			gnome_vfs_async_write (handle.Raw, out buffer, bytes, wrapper.NativeDelegate, IntPtr.Zero);
		}
		
	}
}

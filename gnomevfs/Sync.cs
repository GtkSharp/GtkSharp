//  Sync.cs - Bindings for gnome-vfs synchronized file operations.
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
	public class Sync {
		[DllImport ("gnomevfs-2")]
		private static extern Result gnome_vfs_close (IntPtr handle);

		public static Result Close (Handle handle)
		{
			return gnome_vfs_close (handle.Raw);
		}

		[DllImport ("gnomevfs-2")]
		private static extern Result gnome_vfs_create (out IntPtr handle, string uri, OpenMode mode, bool exclusive, uint perm);
		
		public static Handle Create (string uri, OpenMode mode, bool exclusive, FilePermissions perm)
		{
			IntPtr handle = IntPtr.Zero;
			Result result = gnome_vfs_create (out handle, uri, mode, exclusive, (uint)perm);
			if (result != Result.Ok) {
				Vfs.ThrowException (uri, result);
				return null;
			} else {
				return new Handle (handle);
			}
		}

		[DllImport ("gnomevfs-2")]
		private static extern Result gnome_vfs_create_uri (out IntPtr handle, IntPtr uri, OpenMode mode, bool exclusive, uint perm);
		
		public static Handle Create (Uri uri, OpenMode mode, bool exclusive, FilePermissions perm)
		{
			IntPtr handle = IntPtr.Zero;
			Result result = gnome_vfs_create_uri (out handle, uri.Handle, mode, exclusive, (uint)perm);
			if (result != Result.Ok) {
				Vfs.ThrowException (uri, result);
				return null;
			} else {
				return new Handle (handle);
			}
		}

		[DllImport ("gnomevfs-2")]
		private static extern Result gnome_vfs_open (out IntPtr handle, string uri, OpenMode mode);
		
		public static Handle Open (string uri, OpenMode mode)
		{
			IntPtr handle = IntPtr.Zero;
			Result result = gnome_vfs_open (out handle, uri, mode);
			if (result != Result.Ok) {
				Vfs.ThrowException (uri, result);
				return null;
			} else {
				return new Handle (handle);
			}
		}

		[DllImport ("gnomevfs-2")]
		private static extern Result gnome_vfs_open_uri (out IntPtr handle, IntPtr uri, OpenMode mode);
		
		public static Handle Open (Uri uri, OpenMode mode)
		{
			IntPtr handle = IntPtr.Zero;
			Result result = gnome_vfs_open_uri (out handle, uri.Handle, mode);
			if (result != Result.Ok) {
				Vfs.ThrowException (uri, result);
				return null;
			} else {
				return new Handle (handle);
			}
		}

		[DllImport ("gnomevfs-2")]
		private static extern Result gnome_vfs_read (IntPtr handle, out byte buffer, ulong bytes, out ulong bytes_read);
		
		public static Result Read (Handle handle, out byte buffer, ulong bytes, out ulong bytes_read)
		{
			return gnome_vfs_read (handle.Raw, out buffer, bytes, out bytes_read);
		}

		[DllImport ("gnomevfs-2")]
		private static extern Result gnome_vfs_seek (IntPtr handle, SeekPosition whence, long offset);
		
		public static Result Seek (Handle handle, SeekPosition whence, long offset)
		{
			return gnome_vfs_seek (handle.Raw, whence, offset);
		}
		
		[DllImport ("gnomevfs-2")]
		private static extern Result gnome_vfs_write (IntPtr handle, out byte buffer, ulong bytes, out ulong bytes_written);
		
		public static Result Write (Handle handle, out byte buffer, ulong bytes, out ulong bytes_written)
		{
			return gnome_vfs_write (handle.Raw, out buffer, bytes, out bytes_written);
		}

		[DllImport ("gnomevfs-2")]
		private static extern Result gnome_vfs_tell (IntPtr handle, out ulong offset);
		
		public static Result Tell (Handle handle, out ulong offset)
		{
			return gnome_vfs_tell (handle.Raw, out offset);
		}

		[DllImport ("gnomevfs-2")]
		private static extern Result gnome_vfs_truncate (string uri, ulong length);
		
		public static Result Truncate (string uri, ulong length)
		{
			return gnome_vfs_truncate (uri, length);
		}
		
		[DllImport ("gnomevfs-2")]
		private static extern Result gnome_vfs_truncate_handle (IntPtr handle, ulong length);
		
		public static Result Truncate (Handle handle, ulong length)
		{
			return gnome_vfs_truncate_handle (handle.Raw, length);
		}
		
		[DllImport ("gnomevfs-2")]
		private static extern Result gnome_vfs_file_control (IntPtr handle, string operation, out string data);
		
		// TODO: data parameter only works when you want a string back,
		// like in the case of a "file:test" operation. Unknown at this
		// time what other possible uses/parameters this method has.
		public static Result FileControl (Handle handle, string operation, out string data)
		{
			return gnome_vfs_file_control (handle.Raw, operation, out data);
		}
	}
}

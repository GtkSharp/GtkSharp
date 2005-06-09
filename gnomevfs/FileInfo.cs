//  FileInfo.cs - Class wrapping the GnomeVFSFileInfo struct.
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
using System.Runtime.InteropServices;

namespace Gnome.Vfs {
	public class FileInfo {
		[StructLayout(LayoutKind.Sequential)]
		internal struct FileInfoNative {
			public IntPtr name;
			public FileInfoFields valid_fields;
			public FileType type;
			public FilePermissions permissions;
			public FileFlags flags;
			public long dev_t;
			public long inode;
			public uint link_count;
			public uint uid;
			public uint gid;
			public long size;
			public long block_count;
			public uint io_block_size;
			public IntPtr atime;
			public IntPtr mtime;
			public IntPtr ctime;
			public IntPtr symlink_name;
			public IntPtr mime_type;
			public uint refcount;
			public IntPtr reserved1;
			public IntPtr reserved2;
			public IntPtr reserved3;
			public IntPtr reserved4;
			public IntPtr reserved5;
		}
	
		IntPtr handle;
		bool needs_dispose = false;

		[DllImport ("gnomevfs-2")]
		private static extern void gnome_vfs_file_info_unref (IntPtr handle);

		~FileInfo ()
		{
			if (needs_dispose)
				gnome_vfs_file_info_unref (Handle);
		}
		
		[DllImport ("gnomevfs-2")]
		private static extern IntPtr gnome_vfs_file_info_new ();

		public FileInfo ()
		{
			needs_dispose = true;
			handle = gnome_vfs_file_info_new ();
		}

		public FileInfo (IntPtr handle)
		{
			this.handle = handle;
		}

		public FileInfo (string uri) : this (uri, FileInfoOptions.Default) {}

		public FileInfo (string uri, FileInfoOptions options) : this (new Uri (uri), options) {}

		public FileInfo (Uri uri) : this (uri, FileInfoOptions.Default) {}
		
		[DllImport ("gnomevfs-2")]
		private static extern Result gnome_vfs_get_file_info_uri (IntPtr uri, IntPtr info, int options);

		public FileInfo (Uri uri, FileInfoOptions options) : this ()
		{
			Result result = gnome_vfs_get_file_info_uri (uri.Handle, Handle, (int) options);
			Vfs.ThrowException (uri, result);
		}
		
		FileInfoNative Native {
			get {
				return (FileInfoNative) Marshal.PtrToStructure (handle, typeof (FileInfoNative));
			}
		}

		public IntPtr Handle {
			get {
				return handle;
			}
		}

		public string Name {
			get {
				FileInfoNative info = Native;
				if (info.name != IntPtr.Zero)
					return GLib.Marshaller.Utf8PtrToString (info.name);
				else
					return null;
			}
		}
		
		public FileInfoFields ValidFields {
			get {
				return Native.valid_fields;
			}
		}
		
		public FileType Type {
			get {
				if ((ValidFields & FileInfoFields.Type) != 0)
					return Native.type;
				else
					throw new ArgumentException ("Type is not set");
			}
		}
		
		public FilePermissions Permissions {
			get {
				if ((ValidFields & FileInfoFields.Permissions) != 0)
					return Native.permissions;
				else
					throw new ArgumentException ("Permissions is not set");
			}
		}
		
		public FileFlags Flags {
			get {
				if ((ValidFields & FileInfoFields.Flags) != 0)
					return Native.flags;
				else
					throw new ArgumentException ("Flags is not set");
			}
		}
		
		public long Device {
			get {
				if ((ValidFields & FileInfoFields.Device) != 0)
					return Native.dev_t;
				else
					throw new ArgumentException ("Device is not set");
			}
		}
		
		public long Inode {
			get {
				if ((ValidFields & FileInfoFields.Inode) != 0)
					return Native.inode;
				else
					throw new ArgumentException ("Inode is not set");
			}
		}
		
		public uint LinkCount {
			get {
				if ((ValidFields & FileInfoFields.LinkCount) != 0)
					return Native.link_count;
				else
					throw new ArgumentException ("LinkCount is not set");
			}
		}
		
		public uint Uid {
			get {
				return Native.uid;
			}
		}
		
		public uint Gid {
			get {
				return Native.gid;
			}
		}
		
		public long Size {
			get {
				if ((ValidFields & FileInfoFields.Size) != 0)
					return Native.size;
				else
					throw new ArgumentException ("Size is not set");
			}
		}
		
		public long BlockCount {
			get {
				if ((ValidFields & FileInfoFields.BlockCount) != 0)
					return Native.block_count;
				else
					throw new ArgumentException ("BlockCount is not set");
			}
		}
		
		public uint IoBlockSize {
			get {
				if ((ValidFields & FileInfoFields.IoBlockSize) != 0)
					return Native.io_block_size;
				else
					throw new ArgumentException ("IoBlockSize is not set");
			}
		}
		
		public System.DateTime Atime {
			get {
				if ((ValidFields & FileInfoFields.Atime) != 0)
					return GLib.Marshaller.time_tToDateTime (Native.atime);
				else
					throw new ArgumentException ("Atime is not set");
			}
		}
		
		public System.DateTime Mtime {
			get {
				if ((ValidFields & FileInfoFields.Mtime) != 0)
					return GLib.Marshaller.time_tToDateTime (Native.mtime);
				else
					throw new ArgumentException ("Mtime is not set");
			}
		}
		
		public System.DateTime Ctime  {
			get {
				if ((ValidFields & FileInfoFields.Ctime) != 0)
					return GLib.Marshaller.time_tToDateTime (Native.ctime);
				else
					throw new ArgumentException ("Ctime is not set");
			}
		}
		
		public string SymlinkName {
			get {
				FileInfoNative info = Native;
				if ((ValidFields & FileInfoFields.SymlinkName) != 0 &&
				    info.symlink_name != IntPtr.Zero)
					return GLib.Marshaller.Utf8PtrToString (info.symlink_name);
				else
					throw new ArgumentException ("SymlinkName is not set");
			}
		}
		
		public string MimeType {
			get {
				FileInfoNative info = Native;
				if ((ValidFields & FileInfoFields.MimeType) != 0 &&
				    info.mime_type != IntPtr.Zero)
					return GLib.Marshaller.Utf8PtrToString (info.mime_type);
				else
					throw new ArgumentException ("MimeType is not set");
			}
		}
		
		public bool IsSymlink {
			get {
				FileFlags flags = Flags;
				return (flags & FileFlags.Symlink) != 0;
			}
		}
		
		public bool IsLocal {
			get {
				FileFlags flags = Flags;
				return (flags & FileFlags.Local) != 0;
			}
		}
		
		public bool HasSuid {
			get {
				FilePermissions perms = Permissions;
				return (perms & FilePermissions.Suid) != 0;
			}
		}
		
		public bool HasSgid {
			get {
				FilePermissions perms = Permissions;
				return (perms & FilePermissions.Sgid) != 0;
			}
		}
		
		public bool IsSticky {
			get {
				FilePermissions perms = Permissions;
				return (perms & FilePermissions.Sticky) != 0;
			}
		}

		[DllImport ("gnomevfs-2")]
		private static extern void gnome_vfs_file_info_clear (IntPtr info);
		
		public void Clear ()
		{
			gnome_vfs_file_info_clear (Handle);
		}

		public override String ToString ()
		{
			FileInfoNative info = Native;
			string result = "Name        = " + Name + "\n" +
					"ValidFields = " + info.valid_fields + "\n";
			if ((ValidFields & FileInfoFields.Type) != 0)
				result += "Type        = " + info.type + "\n";
			if ((ValidFields & FileInfoFields.Permissions) != 0)
				result += "Permissions = " + info.permissions + "\n";
			if ((ValidFields & FileInfoFields.Flags) != 0) {
				result += "Flags       = ";
				bool flag = false;
				if (Flags == FileFlags.None) {
					result += "None";
					flag = true;
				}
				if ((Flags & FileFlags.Symlink) != 0) {
					result += flag ? ", Symlink" : "Symlink";
					flag = true;
				}
				if ((Flags & FileFlags.Local) != 0)
					result += flag ? ", Local" : "Local";
				result += "\n";
			}
			if ((ValidFields & FileInfoFields.Device) != 0)
				result += "Device      = " + info.dev_t + "\n";
			if ((ValidFields & FileInfoFields.Inode) != 0)
				result += "Inode       = " + info.inode + "\n";
			if ((ValidFields & FileInfoFields.LinkCount) != 0)
				result += "LinkCount   = " + info.link_count + "\n";
			result += "Uid         = " + info.uid + "\n";
			result += "Gid         = " + info.gid + "\n";
			if ((ValidFields & FileInfoFields.Size) != 0)
				result += "Size        = " + info.size + "\n";
			if ((ValidFields & FileInfoFields.BlockCount) != 0)
				result += "BlockCount  = " + info.block_count + "\n";
			if ((ValidFields & FileInfoFields.IoBlockSize) != 0)
				result += "IoBlockSize = " + info.io_block_size + "\n";
			if ((ValidFields & FileInfoFields.Atime) != 0)
				result += "Atime       = " + Atime + "\n";
			if ((ValidFields & FileInfoFields.Mtime) != 0)
				result += "Mtime       = " + Mtime + "\n";
			if ((ValidFields & FileInfoFields.Ctime) != 0)
				result += "Ctime       = " + Ctime + "\n";
			if ((ValidFields & FileInfoFields.SymlinkName) != 0)
				result += "SymlinkName = " + SymlinkName + "\n";
			if ((ValidFields & FileInfoFields.MimeType) != 0)
				result += "MimeType    = " + MimeType + "\n";
			return result;
		}
	}
}

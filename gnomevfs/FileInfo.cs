//
// FileInfo.cs: Class wrapping the GnomeVFSFileInfo struct.
//
// Author:
//   Jeroen Zwartepoorte <jeroen@xs4all.nl>
//
// (C) Copyright Jeroen Zwartepoorte 2004
//

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
	
		private FileInfoNative info;
		private Uri uri;
		private FileInfoOptions options;

		[DllImport ("gnomevfs-2")]
		extern static FileInfoNative gnome_vfs_file_info_new ();

		[DllImport ("gnomevfs-2")]
		private static extern Result gnome_vfs_get_file_info_uri (IntPtr uri, ref FileInfoNative info, FileInfoOptions options);

		[DllImport ("gnomevfs-2")]
		extern static void gnome_vfs_file_info_unref (ref FileInfoNative info);

		internal FileInfo (FileInfoNative info)
		{
			this.info = info;
			uri = null;
		}

		public FileInfo (string uri) : this (uri, FileInfoOptions.Default) {}

		public FileInfo (string uri, FileInfoOptions options) : this (new Uri (uri), options) {}

		public FileInfo (Uri uri) : this (uri, FileInfoOptions.Default) {}
		
		public FileInfo (Uri uri, FileInfoOptions options)
		{
			this.uri = uri;
			this.options = options;
			info = gnome_vfs_file_info_new ();

			Result result = gnome_vfs_get_file_info_uri (uri.Handle, ref info, options);
			Vfs.ThrowException (uri, result);
		}

		public string Name {
			get {
				if (info.name != IntPtr.Zero)
					return Marshal.PtrToStringAnsi (info.name);
				else
					return null;
			}
		}
		
		public FileInfoFields ValidFields {
			get {
				return info.valid_fields;
			}
		}
		
		public FileType Type {
			get {
				if ((ValidFields & FileInfoFields.Type) != 0)
					return info.type;
				else
					throw new ArgumentException ("Type is not set");
			}
		}
		
		public FilePermissions Permissions {
			get {
				if ((ValidFields & FileInfoFields.Permissions) != 0)
					return info.permissions;
				else
					throw new ArgumentException ("Permissions is not set");
			}
		}
		
		public FileFlags Flags {
			get {
				if ((ValidFields & FileInfoFields.Flags) != 0)
					return info.flags;
				else
					throw new ArgumentException ("Flags is not set");
			}
		}
		
		public long Device {
			get {
				if ((ValidFields & FileInfoFields.Device) != 0)
					return info.dev_t;
				else
					throw new ArgumentException ("Device is not set");
			}
		}
		
		public long Inode {
			get {
				if ((ValidFields & FileInfoFields.Inode) != 0)
					return info.inode;
				else
					throw new ArgumentException ("Inode is not set");
			}
		}
		
		public uint LinkCount {
			get {
				if ((ValidFields & FileInfoFields.LinkCount) != 0)
					return info.link_count;
				else
					throw new ArgumentException ("LinkCount is not set");
			}
		}
		
		public uint Uid {
			get {
				return info.uid;
			}
		}
		
		public uint Gid {
			get {
				return info.gid;
			}
		}
		
		public long Size {
			get {
				if ((ValidFields & FileInfoFields.Size) != 0)
					return info.size;
				else
					throw new ArgumentException ("Size is not set");
			}
		}
		
		public long BlockCount {
			get {
				if ((ValidFields & FileInfoFields.BlockCount) != 0)
					return info.block_count;
				else
					throw new ArgumentException ("BlockCount is not set");
			}
		}
		
		public uint IoBlockSize {
			get {
				if ((ValidFields & FileInfoFields.IoBlockSize) != 0)
					return info.io_block_size;
				else
					throw new ArgumentException ("IoBlockSize is not set");
			}
		}
		
		public System.DateTime Atime {
			get {
				if ((ValidFields & FileInfoFields.Atime) != 0)
					return GLib.Marshaller.time_tToDateTime (info.atime);
				else
					throw new ArgumentException ("Atime is not set");
			}
		}
		
		public System.DateTime Mtime {
			get {
				if ((ValidFields & FileInfoFields.Mtime) != 0)
					return GLib.Marshaller.time_tToDateTime (info.mtime);
				else
					throw new ArgumentException ("Mtime is not set");
			}
		}
		
		public System.DateTime Ctime  {
			get {
				if ((ValidFields & FileInfoFields.Ctime) != 0)
					return GLib.Marshaller.time_tToDateTime (info.ctime);
				else
					throw new ArgumentException ("Ctime is not set");
			}
		}
		
		public string SymlinkName {
			get {
				if ((ValidFields & FileInfoFields.SymlinkName) != 0 &&
				    info.symlink_name != IntPtr.Zero)
					return Marshal.PtrToStringAnsi (info.symlink_name);
				else
					throw new ArgumentException ("SymlinkName is not set");
			}
		}
		
		public string MimeType {
			get {
				if ((ValidFields & FileInfoFields.MimeType) != 0 &&
				    info.mime_type != IntPtr.Zero)
					return Marshal.PtrToStringAnsi (info.mime_type);
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

		public void Update ()
		{
			Result result = gnome_vfs_get_file_info_uri (uri.Handle, ref info, options);
			Vfs.ThrowException (uri, result);
		}

		public override String ToString ()
		{
			string result = "Name        = " + Name + "\n" +
					"ValidFields = " + info.valid_fields + "\n";
			if ((ValidFields & FileInfoFields.Type) != 0)
				result += "Type        = " + info.type + "\n";
			if ((ValidFields & FileInfoFields.Permissions) != 0)
				result += "Permissions = " + info.permissions + "\n";
			if ((ValidFields & FileInfoFields.Flags) != 0) {
				result += "Flags       = ";
				bool flag = false;
				if ((Flags & FileFlags.None) != 0) {
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

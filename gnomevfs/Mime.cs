//
// Mime.cs: Mime-type method bindings.
//
// Author:
//   Jeroen Zwartepoorte <jeroen@xs4all.nl>
//
// (C) Copyright Jeroen Zwartepoorte 2004
//

using System;
using System.Runtime.InteropServices;

namespace Gnome.Vfs {
	public class Mime {
		public static string UnknownMimeType {
			get {
				return "application/octet-stream";
			}
		}

		[DllImport ("gnomevfs-2")]
		private static extern Result gnome_vfs_mime_add_extension (string mime_type, string extension);
		
		public static Result AddExtension (string mime_type, string extension)
		{
			return gnome_vfs_mime_add_extension (mime_type, extension);
		}
		
		[DllImport ("gnomevfs-2")]
		private static extern Result gnome_vfs_mime_remove_extension (string mime_type, string extension);
		
		public static Result RemoveExtension (string mime_type, string extension)
		{
			return gnome_vfs_mime_remove_extension (mime_type, extension);
		}

		[DllImport ("gnomevfs-2")]
		private static extern string gnome_vfs_get_mime_type (string uri);
		
		public static string GetMimeType (string uri)
		{
			return gnome_vfs_get_mime_type (uri);
		}

		[DllImport ("gnomevfs-2")]
		private static extern string gnome_vfs_get_mime_type_for_data (string data, int length);
		
		public static string GetMimeTypeForData (string data, int length)
		{
			return gnome_vfs_get_mime_type_for_data (data, length);
		}
		
		[DllImport ("gnomevfs-2")]
		private static extern string gnome_vfs_mime_get_icon (string mime_type);
		
		public static string GetIcon (string mime_type)
		{
			return gnome_vfs_mime_get_icon (mime_type);
		}

		[DllImport ("gnomevfs-2")]
		private static extern string gnome_vfs_mime_get_description (string mime_type);
		
		public static string GetDescription (string mime_type)
		{
			return gnome_vfs_mime_get_description (mime_type);
		}

		[DllImport ("gnomevfs-2")]
		static extern bool gnome_vfs_mime_can_be_executable (string mime_type);
		
		public static bool IsExecutable (string mime_type)
		{
			return gnome_vfs_mime_can_be_executable (mime_type);
		}

		[DllImport ("gnomevfs-2")]
		static extern bool gnome_vfs_mime_type_is_known (string mime_type);

		public static bool IsKnown (string mime_type)
		{
			return gnome_vfs_mime_type_is_known (mime_type);
		}
	}
}

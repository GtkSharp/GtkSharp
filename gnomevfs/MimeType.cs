//  MimeType.cs - Mime-type bindings.
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
	public class MimeType {
		public static readonly string UnknownMimeType = "application/octet-stream";
		private string mimetype;

		[DllImport ("gnomevfs-2")]
		static extern IntPtr gnome_vfs_get_mime_type (IntPtr uri);
		
		public MimeType (Uri uri)
		{
			IntPtr uri_native = GLib.Marshaller.StringToPtrGStrdup (uri.ToString ());
			mimetype = GLib.Marshaller.PtrToStringGFree (gnome_vfs_get_mime_type (uri_native));
			GLib.Marshaller.Free (uri_native);
		}

		[DllImport ("gnomevfs-2")]
		static extern bool gnome_vfs_mime_type_is_known (IntPtr mime_type);

		public MimeType (string mimetype)
		{
			IntPtr mimetype_native = GLib.Marshaller.StringToPtrGStrdup (mimetype);
			if (!gnome_vfs_mime_type_is_known (mimetype_native))
				throw new ArgumentException ("Unknown mimetype");
			this.mimetype = mimetype;
			GLib.Marshaller.Free (mimetype_native);
		}
		
		[DllImport ("gnomevfs-2")]
		static extern IntPtr gnome_vfs_get_mime_type_for_data (ref byte data, int size);
		
		public MimeType (byte[] buffer, int size)
		{
			mimetype = GLib.Marshaller.Utf8PtrToString (gnome_vfs_get_mime_type_for_data (ref buffer[0], size));
		}
		
		[DllImport ("gnomevfs-2")]
		static extern MimeActionType gnome_vfs_mime_get_default_action_type (IntPtr mime_type);
		
		[DllImport ("gnomevfs-2")]
		static extern Result gnome_vfs_mime_set_default_action_type (IntPtr mime_type, MimeActionType action_type);

		public MimeActionType DefaultActionType {
			get {
				IntPtr mimetype_native = GLib.Marshaller.StringToPtrGStrdup (mimetype);
				MimeActionType result = gnome_vfs_mime_get_default_action_type (mimetype_native);
				GLib.Marshaller.Free (mimetype_native);
				return result;
			}
			set {
				IntPtr mimetype_native = GLib.Marshaller.StringToPtrGStrdup (mimetype);
				Result result = gnome_vfs_mime_set_default_action_type (mimetype_native, value);
				GLib.Marshaller.Free (mimetype_native);
				Vfs.ThrowException (result);
			}
		}
		
		[DllImport ("gnomevfs-2")]
		static extern MimeAction gnome_vfs_mime_get_default_action (IntPtr mime_type);

		public MimeAction DefaultAction {
			get {
				IntPtr mimetype_native = GLib.Marshaller.StringToPtrGStrdup (mimetype);
				MimeAction result = gnome_vfs_mime_get_default_action (mimetype_native);
				GLib.Marshaller.Free (mimetype_native);
				return result;
			}
		}
		
		[DllImport ("gnomevfs-2")]
		static extern IntPtr gnome_vfs_mime_get_description (IntPtr mime_type);

		[DllImport ("gnomevfs-2")]
		static extern Result gnome_vfs_mime_set_description (IntPtr mime_type, IntPtr description);

		public string Description {
			get {
				IntPtr mimetype_native = GLib.Marshaller.StringToPtrGStrdup (mimetype);
				string result = GLib.Marshaller.Utf8PtrToString (gnome_vfs_mime_get_description (mimetype_native));
				GLib.Marshaller.Free (mimetype_native);
				return result;
			}
			set {
				IntPtr mimetype_native = GLib.Marshaller.StringToPtrGStrdup (mimetype);
				IntPtr desc_native = GLib.Marshaller.StringToPtrGStrdup (value);
				Result result = gnome_vfs_mime_set_description (mimetype_native, desc_native);
				GLib.Marshaller.Free (mimetype_native);
				GLib.Marshaller.Free (desc_native);
				Vfs.ThrowException (result);
			}
		}
		
		[DllImport ("gnomevfs-2")]
		static extern IntPtr gnome_vfs_mime_get_icon (IntPtr mime_type);
		
		[DllImport ("gnomevfs-2")]
		static extern Result gnome_vfs_mime_set_icon (IntPtr mime_type, IntPtr filename);
		
		public string Icon {
			get {
				IntPtr mimetype_native = GLib.Marshaller.StringToPtrGStrdup (mimetype);
				string result = GLib.Marshaller.Utf8PtrToString (gnome_vfs_mime_get_icon (mimetype_native));
				GLib.Marshaller.Free (mimetype_native);
				return result;
			}
			set {
				IntPtr mimetype_native = GLib.Marshaller.StringToPtrGStrdup (mimetype);
				IntPtr icon_native = GLib.Marshaller.StringToPtrGStrdup (value);
				Result result = gnome_vfs_mime_set_icon (mimetype_native, icon_native);
				GLib.Marshaller.Free (mimetype_native);
				GLib.Marshaller.Free (icon_native);
				Vfs.ThrowException (result);
			}
		}

		[DllImport ("gnomevfs-2")]
		static extern bool gnome_vfs_mime_can_be_executable (IntPtr mime_type);

		[DllImport ("gnomevfs-2")]
		static extern Result gnome_vfs_mime_set_can_be_executable (IntPtr mime_type, bool value);
		
		public bool CanBeExecutable {
			get {
				IntPtr mimetype_native = GLib.Marshaller.StringToPtrGStrdup (mimetype);
				bool result = gnome_vfs_mime_can_be_executable (mimetype_native);
				GLib.Marshaller.Free (mimetype_native);
				return result;
			}
			set {
				IntPtr mimetype_native = GLib.Marshaller.StringToPtrGStrdup (mimetype);
				Result result = gnome_vfs_mime_set_can_be_executable (mimetype_native, value);
				GLib.Marshaller.Free (mimetype_native);
				Vfs.ThrowException (result);
			}
		}
		
		public string Name {
			get {
				return mimetype;
			}
		}

		public override string ToString ()
		{
			return mimetype;
		}
		
		public static string GetMimeTypeForUri (string uri)
		{
			IntPtr uri_native = GLib.Marshaller.StringToPtrGStrdup (uri.ToString ());
			string mimetype = GLib.Marshaller.PtrToStringGFree (gnome_vfs_get_mime_type (uri_native));
			GLib.Marshaller.Free (uri_native);
			return mimetype;
		}
	}
}

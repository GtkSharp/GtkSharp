//
// Xfer.cs: Bindings for gnome_vfs_xfer_* methods.
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
	public class Xfer {
		[DllImport ("gnomevfs-2")]
		private static extern Result gnome_vfs_xfer_uri_list (IntPtr source_uri_list,
								      IntPtr target_uri_list,
								      XferOptions xfer_options,
								      XferErrorMode error_mode,
								      XferOverwriteMode overwrite_mode,
								      XferProgressCallbackNative progress_callback,
								      IntPtr data);

		public static Result XferUriList (Uri[] sources,
						  Uri[] targets,
						  XferOptions options,
						  XferErrorMode errorMode,
						  XferOverwriteMode overwriteMode,
						  XferProgressCallback callback)
		{
			XferProgressCallbackWrapper wrapper = new XferProgressCallbackWrapper (callback, null);
			GLib.List sourcesList = new GLib.List (typeof (Uri));
			foreach (Uri uri in sources)
				sourcesList.Append (uri.Handle);
				
			GLib.List targetsList = new GLib.List (typeof (Uri));
			foreach (Uri uri in targets)
				targetsList.Append (uri.Handle);
			
			return gnome_vfs_xfer_uri_list (sourcesList.Handle,
							targetsList.Handle,
							options, errorMode,
							overwriteMode,
							wrapper.NativeDelegate,
							IntPtr.Zero);
		}

		[DllImport ("gnomevfs-2")]
		private static extern Result gnome_vfs_xfer_uri (IntPtr source_uri,
								 IntPtr target_uri,
								 XferOptions xfer_options,
								 XferErrorMode error_mode,
								 XferOverwriteMode overwrite_mode,
								 XferProgressCallbackNative progress_callback,
								 IntPtr data);

		public static Result XferUri (Uri source, Uri target,
					      XferOptions options,
					      XferErrorMode errorMode,
					      XferOverwriteMode overwriteMode,
					      XferProgressCallback callback)
		{
			XferProgressCallbackWrapper wrapper = new XferProgressCallbackWrapper (callback, null);
			return gnome_vfs_xfer_uri (source.Handle, target.Handle,
						   options, errorMode, overwriteMode,
						   wrapper.NativeDelegate, IntPtr.Zero);
		}

		[DllImport ("gnomevfs-2")]
		private static extern Result gnome_vfs_xfer_delete_list (IntPtr source_uri_list,
									 XferErrorMode error_mode,
									 XferOptions xfer_options,
									 XferProgressCallbackNative progress_callback,
									 IntPtr data);

		public static Result XferDeleteList (Uri[] sources,
						     XferErrorMode errorMode,
						     XferOptions options,
						     XferProgressCallback callback)
		{
			XferProgressCallbackWrapper wrapper = new XferProgressCallbackWrapper (callback, null);
			GLib.List sourcesList = new GLib.List (typeof (Uri));
			foreach (Uri uri in sources)
				sourcesList.Append (uri.Handle);
			
			return gnome_vfs_xfer_delete_list (sourcesList.Handle,
							   errorMode, options,
							   wrapper.NativeDelegate,
							   IntPtr.Zero);
		}
	}
}

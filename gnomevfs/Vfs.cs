//
// Vfs.cs: Bindings for gnome-vfs.
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
	public class Vfs {
		[DllImport ("gnomevfs-2")]
		static extern bool gnome_vfs_init ();
		
		[DllImport ("gnomevfs-2")]
		static extern bool gnome_vfs_initialized ();		
	
		public static bool Initialized {
			get {
				return gnome_vfs_initialized ();
			}
		}
		
		static Vfs ()
		{
			if (!gnome_vfs_initialized ())
				gnome_vfs_init ();
		}
		
		public static bool Initialize ()
		{
			return gnome_vfs_init ();
		}
		
		[DllImport ("gnomevfs-2")]
		static extern bool gnome_vfs_shutdown ();
		
		public static bool Shutdown ()
		{
			return gnome_vfs_shutdown ();
		}
		
		public static bool Exists (string textUri)
		{
			Uri uri = new Uri (textUri);
			return uri.Exists;
		}
		
		[DllImport ("gnomevfs-2")]
		private static extern Result gnome_vfs_move (string old_uri, string new_uri, bool force_replace);
		
		public static Result Move (string old_uri, string new_uri, bool force_replace)
		{
			return gnome_vfs_move (old_uri, new_uri, force_replace);
		}
		
		[DllImport ("gnomevfs-2")]
		private static extern Result gnome_vfs_unlink (string uri);
		
		public static Result Unlink (string uri)
		{
			return gnome_vfs_unlink (uri);
		}
		
		[DllImport ("gnomevfs-2")]
		private static extern IntPtr gnome_vfs_result_to_string (int result);
		
		public static string ResultToString (Result result)
		{
			return ResultToString ((int)result);
		}
		
		public static string ResultToString (int result)
		{
			IntPtr ptr = gnome_vfs_result_to_string (result);
			return Marshal.PtrToStringAnsi (ptr);
		}
		
		[DllImport ("gnomevfs-2")]
		private static extern Result gnome_vfs_truncate (string uri, ulong length);
		
		public static Result Truncate (string uri, ulong length)
		{
			return gnome_vfs_truncate (uri, length);
		}
		
		public static Result Truncate (Uri uri, ulong length)
		{
			return Truncate (uri.ToString (), length);
		}
		
		public static void ThrowException (Result result)
		{
			ThrowException ((string)null, result);
		}
		
		public static void ThrowException (Uri uri, Result result)
		{
			ThrowException (uri.ToString (), result);
		}

		public static void ThrowException (string uri, Result result)
		{
			switch (result) {
				case Result.Ok:
					return;
				case Result.ErrorNotFound:
					throw new FileNotFoundException (uri);
				default:
					throw new IOException (ResultToString (result));
			}
		}
	}
}

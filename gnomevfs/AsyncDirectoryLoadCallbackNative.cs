//
// VfsAsyncDirectoryLoadCallbackNative.cs: Utility class for accessing gnome-vfs methods.
//
// Author:
//   Jeroen Zwartepoorte <jeroen@xs4all.nl>
//
// (C) Copyright Jeroen Zwartepoorte 2004
//

using System;
using System.Runtime.InteropServices;

namespace Gnome.Vfs {
	internal delegate void AsyncDirectoryLoadCallbackNative (IntPtr handle, Result result, IntPtr list, uint entries_read, IntPtr data);

	internal class AsyncDirectoryLoadCallbackWrapper : GLib.DelegateWrapper {

		public void NativeCallback (IntPtr handle, Result result, IntPtr list, uint entries_read, IntPtr data)
		{
			GLib.List infos = new GLib.List (list, typeof (FileInfo.FileInfoNative));
			FileInfo[] entries = new FileInfo [infos.Count];
			for (int i = 0; i < infos.Count; i++)
				entries[i] = new FileInfo ((FileInfo.FileInfoNative) infos [i]);
			
			_managed (result, entries, entries_read);			
		}

		internal AsyncDirectoryLoadCallbackNative NativeDelegate;
		protected AsyncDirectoryLoadCallback _managed;

		public AsyncDirectoryLoadCallbackWrapper (AsyncDirectoryLoadCallback managed, object o) : base (o)
		{
			NativeDelegate = new AsyncDirectoryLoadCallbackNative (NativeCallback);
			_managed = managed;
		}
	}
}

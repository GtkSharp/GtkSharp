//
// AsyncWriteCallbackNative.cs: Native wrapper for GnomeVFSAsyncWriteCallback.
//
// Author:
//   Jeroen Zwartepoorte <jeroen@xs4all.nl>
//
// (C) Copyright Jeroen Zwartepoorte 2004
//

using System;
using System.Runtime.InteropServices;

namespace Gnome.Vfs {
	internal delegate void AsyncWriteCallbackNative (IntPtr handle, Result result, IntPtr buffer, ulong bytes_requested, ulong bytes_written, IntPtr data);

	internal class AsyncWriteCallbackWrapper : GLib.DelegateWrapper {

		public void NativeCallback (IntPtr handle, Result result, IntPtr buffer, ulong bytes_requested, ulong bytes_written, IntPtr data)
		{
			byte[] bytes = new byte[bytes_written];
			Marshal.Copy (buffer, bytes, 0, (int)bytes_written);			
			_managed (new Handle (handle), result, bytes, bytes_requested, bytes_written);
		}

		internal AsyncWriteCallbackNative NativeDelegate;
		protected AsyncWriteCallback _managed;

		public AsyncWriteCallbackWrapper (AsyncWriteCallback managed, object o) : base (o)
		{
			NativeDelegate = new AsyncWriteCallbackNative (NativeCallback);
			_managed = managed;
		}
	}
}

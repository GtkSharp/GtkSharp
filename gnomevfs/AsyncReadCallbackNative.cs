//
// VfsAsyncReadCallbackNative.cs: Utility class for accessing gnome-vfs methods.
//
// Author:
//   Jeroen Zwartepoorte <jeroen@xs4all.nl>
//
// (C) Copyright Jeroen Zwartepoorte 2004
//

using System;
using System.Runtime.InteropServices;

namespace Gnome.Vfs {
	internal delegate void AsyncReadCallbackNative (IntPtr handle, Result result, IntPtr buffer, ulong bytes_requested, ulong bytes_read, IntPtr data);

	internal class AsyncReadCallbackWrapper : GLib.DelegateWrapper {

		public void NativeCallback (IntPtr handle, Result result, IntPtr buffer, ulong bytes_requested, ulong bytes_read, IntPtr data)
		{
			byte[] bytes = new byte[bytes_read];
			Marshal.Copy (buffer, bytes, 0, (int)bytes_read);
			_managed (new Handle (handle), result, bytes, bytes_requested, bytes_read);
		}

		internal AsyncReadCallbackNative NativeDelegate;
		protected AsyncReadCallback _managed;

		public AsyncReadCallbackWrapper (AsyncReadCallback managed, object o) : base (o)
		{
			NativeDelegate = new AsyncReadCallbackNative (NativeCallback);
			_managed = managed;
		}
	}
}

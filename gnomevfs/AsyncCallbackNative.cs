//
// AsyncCallbackNative.cs: Native wrapper for GnomeVFSAsyncCallback.
//
// Author:
//   Jeroen Zwartepoorte <jeroen@xs4all.nl>
//
// (C) Copyright Jeroen Zwartepoorte 2004
//

using System;

namespace Gnome.Vfs {
	internal delegate void AsyncCallbackNative (IntPtr handle, Result result, IntPtr data);

	internal class AsyncCallbackWrapper : GLib.DelegateWrapper {

		public void NativeCallback (IntPtr handle, Result result, IntPtr data)
		{
			_managed (new Handle (handle), result);
		}

		internal AsyncCallbackNative NativeDelegate;
		protected AsyncCallback _managed;

		public AsyncCallbackWrapper (AsyncCallback managed, object o) : base (o)
		{
			NativeDelegate = new AsyncCallbackNative (NativeCallback);
			_managed = managed;
		}
	}
}

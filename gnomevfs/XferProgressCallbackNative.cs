//
// XferProgressCallbackNative.cs: Native wrapper for GnomeVFSXferProgressCallback.
//
// Author:
//   Jeroen Zwartepoorte <jeroen@xs4all.nl>
//
// (C) Copyright Jeroen Zwartepoorte 2004
//

using System;

namespace Gnome.Vfs {
	internal delegate int XferProgressCallbackNative (ref XferProgressInfo info, IntPtr data);

	internal class XferProgressCallbackWrapper : GLib.DelegateWrapper {

		public int NativeCallback (ref XferProgressInfo info, IntPtr data)
		{
			return _managed (info);
		}

		internal XferProgressCallbackNative NativeDelegate;
		protected XferProgressCallback _managed;

		public XferProgressCallbackWrapper (XferProgressCallback managed, object o) : base (o)
		{
			NativeDelegate = new XferProgressCallbackNative (NativeCallback);
			_managed = managed;
		}
	}
}

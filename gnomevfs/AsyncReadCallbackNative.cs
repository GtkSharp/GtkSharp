//  AsyncReadCallbackNative.cs - GnomeVFSAsyncReadCallback native wrapper.
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

//  AsyncCallbackNative.cs - GnomeVFSAsyncCallback native wrapper.
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

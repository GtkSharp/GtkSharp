//  AsyncDirectoryLoadCallbackNative.cs - GnomeVFSAsyncDirectoryLoadCallback
//  native wrapper.
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

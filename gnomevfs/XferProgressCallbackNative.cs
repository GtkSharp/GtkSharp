//  XferProgressCallbackNative.cs - GnomeVFSXferProgressCallback wrapper.
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

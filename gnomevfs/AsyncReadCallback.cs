//
// VfsAsyncReadCallback.cs: GnomeVFSAsyncReadCallback delegate.
//
// Author:
//   Jeroen Zwartepoorte <jeroen@xs4all.nl>
//
// (C) Copyright Jeroen Zwartepoorte 2004
//

using System;

namespace Gnome.Vfs {
	public delegate void AsyncReadCallback (Handle handle, Result result, byte[] buffer, ulong bytes_requested, ulong bytes_read);
}

//
// VfsAsyncWriteCallback.cs: GnomeVFSAsyncWriteCallback delegate.
//
// Author:
//   Jeroen Zwartepoorte <jeroen@xs4all.nl>
//
// (C) Copyright Jeroen Zwartepoorte 2004
//

namespace Gnome.Vfs {
	public delegate void AsyncWriteCallback (Handle handle, Result result, byte[] buffer, ulong bytes_requested, ulong bytes_written);
}

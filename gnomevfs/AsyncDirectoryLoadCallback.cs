//
// VfsAsyncDirectoryLoadCallback.cs: GnomeVFSAsyncDirectoryLoadCallback delegate.
//
// Author:
//   Jeroen Zwartepoorte <jeroen@xs4all.nl>
//
// (C) Copyright Jeroen Zwartepoorte 2004
//

using System;

namespace Gnome.Vfs {
	public delegate void AsyncDirectoryLoadCallback (Result result, FileInfo[] infos, uint entries_read);
}

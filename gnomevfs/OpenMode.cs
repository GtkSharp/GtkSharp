//
// OpenMode.cs: GnomeVFSOpenMode enum.
//
// Author:
//   Jeroen Zwartepoorte <jeroen@xs4all.nl>
//
// (C) Copyright Jeroen Zwartepoorte 2004
//

namespace Gnome.Vfs {
	public enum OpenMode {
		None = 0,
		Read = 1 << 0,
		Write = 1 << 1,
		Random = 1 << 2
	}
}

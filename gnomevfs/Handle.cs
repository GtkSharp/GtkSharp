//
// Handle.cs: Wrapper class for GnomeVFSHandle struct.
//
// Author:
//   Jeroen Zwartepoorte <jeroen@xs4all.nl>
//
// (C) Copyright Jeroen Zwartepoorte 2004
//

using System;

namespace Gnome.Vfs {
	public class Handle {
		private IntPtr raw;
		
		internal IntPtr Raw {
			get {
				return raw;
			}
		}
		
		internal Handle (IntPtr raw)
		{
			this.raw = raw;
		}
	}
}

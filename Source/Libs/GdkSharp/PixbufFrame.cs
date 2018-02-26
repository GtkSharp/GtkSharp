// Gdk.PixbufFrame.cs - Gdk PixbufFrame class customizations
//
// Author:   Mike Kestner <mkestner@novell.com>
//
// Copyright (c) 2005 Novell, Inc.
//
// This code is inserted after the automatically generated code.
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

namespace Gdk {

	using System;

	public partial struct PixbufFrame {

		[Obsolete ("Replaced by Pixbuf property.")]
		public Gdk.Pixbuf pixbuf {
			get { 
				Gdk.Pixbuf ret = (Gdk.Pixbuf) GLib.Object.GetObject(_pixbuf);
				return ret;
			}
			set { _pixbuf = value.Handle; }
		}

		[Obsolete ("Replaced by Composited property.")]
		public Gdk.Pixbuf composited {
			get { 
				Gdk.Pixbuf ret = (Gdk.Pixbuf) GLib.Object.GetObject(_composited);
				return ret;
			}
			set { _composited = value.Handle; }
		}

		[Obsolete ("Replaced by Revert property.")]
		public Gdk.Pixbuf revert {
			get { 
				Gdk.Pixbuf ret = (Gdk.Pixbuf) GLib.Object.GetObject(_revert);
				return ret;
			}
			set { _revert = value.Handle; }
		}
	}
}


// Gdk.WindowAttr.cs - Gdk Window class customizations
//
// Author: Mike Kestner <mkestner@novell.com>
//
// Copyright (c) 2004 Novell, Inc.
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

	public partial struct WindowAttr {

		public EventMask Mask {
			get {
				return (EventMask) EventMask;
			}
			set {
				EventMask = (int) value;
			}
		}

		[Obsolete ("Replaced by Visual property.")]
		public Gdk.Visual visual {
			get { 
				Gdk.Visual ret = (Gdk.Visual) GLib.Object.GetObject(_visual);
				return ret;
			}
			set { _visual = value.Handle; }
		}

		[Obsolete ("Replaced by Cursor property.")]
		public Gdk.Cursor cursor {
			get { 
				Gdk.Cursor ret = new Gdk.Cursor(_cursor);
				if (ret == null) ret = new Gdk.Cursor(_cursor);
				return ret;
			}
			set { _cursor = value.Handle; }
		}
	}
}


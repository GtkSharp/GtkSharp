// Pango.Analysis.cs - Pango Analysis class customizations
//
// Authors:  Mike Kestner  <mkestner@novell.com>
//
// Copyright (c) 2004 Novell, Inc.
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

namespace Pango {

	using System;

	public partial struct Analysis {

		public Attribute[] ExtraAttrs {
			get {
				GLib.SList list = new GLib.SList (_extra_attrs, typeof (IntPtr));
				Attribute[] result = new Attribute [list.Count];
				int i = 0;
				foreach (IntPtr attr in list)
					result [i++] = Attribute.GetAttribute (attr);
				return result;
			}
		}

		// Pango removed the shape/lang engine API; PangoEngineShape and
		// PangoEngineLang no longer exist. The struct still carries the two
		// pointers for layout, but there is nothing to wrap them in. The
		// accessors that used to sit here were already [Obsolete].

		[Obsolete ("Replaced by Font property")]
		public Pango.Font font {
			get { 
				return GLib.Object.GetObject(_font) as Pango.Font;
			}
			set { _font = value == null ? IntPtr.Zero : value.Handle; }
		}

		[Obsolete ("Replaced by Language property")]
		public Pango.Language language {
			get { 
				return _language == IntPtr.Zero ? null : new Pango.Language(_language);
			}
			set { _language = value == null ? IntPtr.Zero : value.Handle; }
		}
	}
}

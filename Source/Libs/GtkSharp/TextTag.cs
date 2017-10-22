		//
		// Gtk.TextTag.cs - Gtk TextTag class customizations
		//
		// Author: Radek Doulik  (rodo@ximian.com)
		//
		// Copyright (C) 2002 Ximian, Inc. 
		//
		// This code is inserted after the automatically generated code.
		//
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

namespace Gtk {
		
	using System;
		
	public partial class TextTag {
				
		public Pango.Weight Weight {
			get {
				GLib.Value val = GetProperty ("weight");
				Pango.Weight ret = (Pango.Weight) (int) val;
				val.Dispose ();
				return ret;
			}
			set {
				GLib.Value val = new GLib.Value ((int) value);
				SetProperty ("weight", val);
				val.Dispose ();
			}
		}
	}
}

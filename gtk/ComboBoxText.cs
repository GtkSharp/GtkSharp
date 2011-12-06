//  ComboBoxText.cs - Gtk ComboBoxText customizations
//
//  Authors:  Bertrand Lorentz  <bertrand.lorentz@gmail.com>
//
//  Copyright (c) 2011 Bertrand Lorentz
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

	public partial class ComboBoxText {

		protected ComboBoxText (bool has_entry) : base (IntPtr.Zero)
		{
			if (GetType () != typeof (ComboBoxText)) {
				CreateNativeObject (new string[] { "has-entry", "entry-text-column", "id-column" },
									new GLib.Value[] { new GLib.Value (has_entry), new GLib.Value (0), new GLib.Value (1) });
				return;
			}
				
			if (has_entry) {
				Raw = gtk_combo_box_text_new_with_entry ();
			} else {
				Raw = gtk_combo_box_text_new ();
			}
		}

		public Gtk.Entry Entry {
			get {
				return (Gtk.Entry)Child;
			}
		}
	}
}

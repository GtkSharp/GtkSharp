//
// Gtk.Entry.cs - Allow customization of values in the GtkEntry
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

	public partial class Entry {

		public int InsertText (string new_text)
		{
			int position = 0;

			InsertText (new_text, ref position);

			return position;
		}

		public Entry(string initialText): this()
		{
			Text = initialText;
		}

		[Obsolete("Replaced by IsEditable property")]
		public bool Editable {
			get { return IsEditable; }
			set { IsEditable = value; }
		}
	}
}

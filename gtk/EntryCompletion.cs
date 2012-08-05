//  Gtk.EntryCompletion.cs - Gtk EntryCompletion customizations
//
//  Authors:  Todd Berman  <tberman@off.net>
//  	      Mike Kestner  <mkestner@novell.com>
//
//  Copyright (c) 2004 Todd Berman
//  Copyright (c) 2005 Novell, Inc.
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

	public partial class EntryCompletion {

		public void SetAttributes (CellRenderer cell, params object[] attrs)
		{
			if (attrs.Length % 2 != 0)
				throw new ArgumentException ("attrs should contain pairs of attribute/col");

			ClearAttributes (cell);
			for (int i = 0; i < attrs.Length - 1; i += 2) {
				AddAttribute (cell, (string) attrs [i], (int) attrs [i + 1]);
			}
		}
	}
}

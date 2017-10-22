// StockItem.cs - customizations to Gtk.StockItem
//
// Authors: Larry Ewing	<lewing@novell.com>
//
// Copyright (c) 2005 Novell, Inc.
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

	public partial struct StockItem {

		public StockItem (string stock_id, string label, uint keyval, Gdk.ModifierType modifier, string domain)
		{
			this.StockId = stock_id;
			this.Label = label;
			this.Keyval = keyval;
			this.Modifier = modifier;
			this.TranslationDomain = domain;
		}
	}
}

// RadioActionEntry.cs - Syntactic C# sugar for easily defining RadioActions.
//
// Authors:  Jeroen Zwartepoorte  <jeroen@xs4all.nl>
//
// Copyright (c) 2004 Jeroen Zwartepoorte
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

using System;

namespace Gtk {
	public struct RadioActionEntry {
		public string name;
		public string stock_id;
		public string label;
		public string tooltip;
		public string accelerator;
		public int value;
		
		public RadioActionEntry (string name, string stock_id)
			: this (name, stock_id, null, null, null, 0) {}
		
		public RadioActionEntry (string name, string stock_id, string label,
					 string accelerator, string tooltip)
			: this (name, stock_id, label, accelerator, tooltip, 0) {}

		public RadioActionEntry (string name, string stock_id, string label,
					 string accelerator, string tooltip, int value)
		{
			this.name = name;
			this.stock_id = stock_id;
			this.label = label;
			this.accelerator = accelerator;
			this.tooltip = tooltip;
			this.value = value;
		}
	}
}

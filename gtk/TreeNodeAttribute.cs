// TreeNodeAttribute.cs - Attribute to specify TreeNode information for a class
//
// Author: Mike Kestner  <mkestner@novell.com>
//
// Copyright (c) 2003-2005 Novell, Inc.
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

	[AttributeUsage(AttributeTargets.Class)]
	public sealed class TreeNodeAttribute : Attribute {
		int col_count;
		bool list_only;

		public int ColumnCount {
			get {
				return col_count;
			}
			set {
				col_count = value;
			}
		}

		public bool ListOnly {
			get {
				return list_only;
			}
			set {
				list_only = value;
			}
		}
	}
}


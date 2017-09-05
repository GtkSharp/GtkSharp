// Gtk.TreeModel.cs - Gtk TreeModel interface customizations
//
// Author: Kristian Rietveld <kris@gtk.org>
//
// Copyright (c) 2002 Kristian Rietveld
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

	public partial interface ITreeModel {

		/// <summary>IterChildren Method</summary>
		/// <remarks>To be completed</remarks>
		bool IterChildren (out Gtk.TreeIter iter);

		/// <summary>IterNChildren Method</summary>
		/// <remarks>To be completed</remarks>
		int IterNChildren ();

		/// <summary>IterNthChild Method</summary>
		/// <remarks>To be completed</remarks>
		bool IterNthChild (out Gtk.TreeIter iter, int n);

		void SetValue (Gtk.TreeIter iter, int column, bool value);
		void SetValue (Gtk.TreeIter iter, int column, double value);
		void SetValue (Gtk.TreeIter iter, int column, int value);
		void SetValue (Gtk.TreeIter iter, int column, string value);
		void SetValue (Gtk.TreeIter iter, int column, float value);
		void SetValue (Gtk.TreeIter iter, int column, uint value);
		void SetValue (Gtk.TreeIter iter, int column, object value);
		object GetValue(Gtk.TreeIter iter, int column);

		event RowsReorderedHandler RowsReordered;
	}
}

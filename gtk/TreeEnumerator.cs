// TreeEnumerator.cs - .NET-style Enumerator for TreeModel classes
//
// Author: Eric Butler <eric@extremeboredom.net>
//
// Copyright (c) 2005 Eric Butler
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
using System.Collections;

namespace Gtk
{
	internal class TreeEnumerator : IEnumerator
	{
		private Gtk.TreeIter iter;
		private Gtk.TreeModel model;
		private bool reset = true;
		private bool changed = false;
		
		public TreeEnumerator (TreeModel model)
		{
			this.model = model;
			
			model.RowChanged += model_changed;
			model.RowDeleted += model_changed;
			model.RowInserted += model_changed;
			model.RowsReordered += model_changed;
		}
		
		public object Current
		{
			get {
				if (reset == false) {
					object[] row = new object[model.NColumns];
					for (int x = 0; x < model.NColumns; x++) {
						row[x] = model.GetValue(iter, x);
					}
					return row;
				} else {
					throw new InvalidOperationException("Enumerator not started.");
				}
			}
		}
		
		public bool MoveNext()
		{
			if (changed == false) {
				if (reset == true) {
					reset = false;
					return model.GetIterFirst(out iter);
				} else {
					return model.IterNext(ref iter);
				}
			} else {
				throw new InvalidOperationException("List has changed.");
			}
		}
		
		public void Reset()
		{
			reset = true;
			changed = false;
		}
		
		private void model_changed(object o, EventArgs args)
		{
			changed = true;
		}
	}
}


// Object.cs - Atk Object class customizations
//
// Author: Andres G. Aragoneses <aaragoneses@novell.com>
//
// Copyright (c) 2008 Novell, Inc.
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

namespace Atk {

	public partial class Object {

		protected void EmitChildrenChanged (ChildrenChangedDetail detail, uint child_index, Atk.Object child)
		{
			GLib.Signal.Emit (this, 
				"children-changed::" + detail.ToString ().ToLower (), 
				child_index, child.Handle);
		}
		
		protected enum ChildrenChangedDetail
		{
			Add,
			Remove
		}

		protected void EmitVisibleDataChanged ()
		{
			GLib.Signal.Emit (this, "visible-data-changed");
		}
		
		public void NotifyStateChange (Atk.StateType state, bool value) {
			NotifyStateChange ((ulong)state, value);
		}

		protected void EmitFocusEvent (bool gained)
		{
			GLib.Signal.Emit (this, "focus-event", gained);
		}
	}
}

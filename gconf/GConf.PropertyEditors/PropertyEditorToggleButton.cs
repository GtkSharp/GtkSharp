// PropertyEditorToggleButton.cs -
//
// Author: Rachel Hestilow  <hestilow@nullenvoid.com>
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

namespace GConf.PropertyEditors
{
	using Gtk;
	using System;

	public class PropertyEditorToggleButton : PropertyEditorBool
	{
		protected override void ValueChanged (object sender, NotifyEventArgs args)
		{
			object val = args.Value;
			ToggleButton checkbox = (ToggleButton) Control;
			checkbox.Active = (bool) val;
		}

		void Toggled (object obj, EventArgs args)
		{
			ToggleButton checkbox = (ToggleButton) Control;
			Set (checkbox.Active);
		}
		
		protected override void ConnectHandlers ()
		{
			ToggleButton checkbox = (ToggleButton) Control;
			checkbox.Toggled += new EventHandler (Toggled);
		}

		public PropertyEditorToggleButton (string key, ToggleButton checkbox) : base (key, checkbox)
		{
		}
	}
}

// PropertyEditorSpinButton.cs -
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
	using GtkSharp;
	using System;

	public class PropertyEditorSpinButton : PropertyEditor
	{
		bool is_int;
		
		protected override void ValueChanged (object sender, NotifyEventArgs args)
		{
			object val = args.Value;
			SpinButton spin = (SpinButton) Control;
			is_int = val is int;

			if (is_int)
				spin.Value = (double) (int) val;
			else
				spin.Value = (double) val;
		}

		void Changed (object obj, EventArgs args)
		{
			Adjustment adj = (Adjustment) obj;
			if (is_int)
				Set ((int) adj.Value);
			else
				Set (adj.Value);
		}
		
		protected override void ConnectHandlers ()
		{
			SpinButton spin = (SpinButton) Control;
			spin.Adjustment.ValueChanged += new EventHandler (Changed);
		}	

		public PropertyEditorSpinButton (string key, SpinButton spin) : base (key, spin)
		{
		}
	}
}

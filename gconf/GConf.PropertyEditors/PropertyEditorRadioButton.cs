// PropertyEditorRadioButton.cs -
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

	public class PropertyEditorRadioButton : PropertyEditorEnum
	{
		GLib.SList group = null;

		protected override void ValueChanged (object sender, NotifyEventArgs args)
		{
			object val = args.Value;
			int selected = ValueToInt (val);
			int i = group.Count - 1;
			foreach (RadioButton button in group)
			{
				if (i == selected)
				{
					button.Active = true;
					break;
				}
				i--;
			}
		}

		void Changed (object obj, EventArgs args)
		{
			int i = group.Count - 1;
			foreach (RadioButton button in group)
			{
				if (button.Active)
				{
					Set (i);
					break;
				}
				i--;
			}
		}
		
		protected override void ConnectHandlers ()
		{
			foreach (RadioButton button in group)
				button.Toggled += new EventHandler (Changed);
		}

		public PropertyEditorRadioButton (string key, RadioButton button) : base (key, button)
		{
			group = button.Group;
		}

		public PropertyEditorRadioButton (string key, RadioButton button, Type enum_type, int[] enum_values) : base (key, button, enum_type, enum_values)
		{
			group = button.Group;
		}

		public PropertyEditorRadioButton (string key, RadioButton button, Type enum_type) : base (key, button, enum_type)
		{
			group = button.Group;
		}
	}
}

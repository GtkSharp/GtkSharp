// PropertyEditorOptionMenu.cs -
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

	public class PropertyEditorOptionMenu : PropertyEditorEnum
	{
		protected override void ValueChanged (object sender, NotifyEventArgs args)
		{
			object val = args.Value;
			OptionMenu menu = (OptionMenu) Control;
			menu.SetHistory ((uint) ValueToInt (val));
		}

		void Changed (object obj, EventArgs args)
		{
			OptionMenu menu = (OptionMenu) Control;
			int history = menu.History;
			Set (history);
		}
		
		protected override void ConnectHandlers ()
		{
			OptionMenu menu = (OptionMenu) Control;
			menu.Changed += new EventHandler (Changed);
		}	

		public PropertyEditorOptionMenu (string key, OptionMenu menu) : base (key, menu)
		{
		}

		public PropertyEditorOptionMenu (string key, OptionMenu menu, Type enum_type, int[] enum_values) : base (key, menu, enum_type, enum_values)
		{
		}

		public PropertyEditorOptionMenu (string key, OptionMenu menu, Type enum_type) : base (key, menu, enum_type)
		{
		}
	}
}

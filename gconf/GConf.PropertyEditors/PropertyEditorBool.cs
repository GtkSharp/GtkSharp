// PropertyEditorBool.cs -
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
	using System.Collections;

	public abstract class PropertyEditorBool : PropertyEditor
	{
		ArrayList guards = new ArrayList ();

		public void AddGuard (Widget control)
		{
			guards.Add (control);
			control.Sensitive = (bool) Get ();
		}

		protected override void Set (object val)
		{
			bool val_bool = (bool) val;
			
			foreach (Widget control in guards)
				control.Sensitive = val_bool;

			base.Set (val);
		}

		public PropertyEditorBool (string key, Widget control) : base (key, control)
		{
		}
	}
}

// PropertyEditorColorPicker.cs -
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
	using Gnome;
	using System;
	using System.Drawing;

	public class PropertyEditorColorPicker : PropertyEditor
	{
		protected override void ValueChanged (object sender, NotifyEventArgs args)
		{
			object val = args.Value;
			if (val == null)
				return;

			ColorPicker picker = (ColorPicker) Control;
			Color color = ColorTranslator.FromHtml ((string) val);
			picker.SetI8 (color.R, color.G, color.B, color.A);
		}

		byte ToByte (uint val)
		{
			return (byte) (val >> 8);
		}

		void Changed (object obj, Gnome.ColorSetArgs args)
		{
			ColorPicker picker = (ColorPicker) Control;
			Color color = Color.FromArgb (ToByte (picker.Red), ToByte (picker.Green), ToByte (picker.Blue));
			Set (ColorTranslator.ToHtml (color));
		}
		
		protected override void ConnectHandlers ()
		{
			ColorPicker picker = (ColorPicker) Control;
			picker.ColorSet += new Gnome.ColorSetHandler (Changed);
		}

		public PropertyEditorColorPicker (string key, ColorPicker picker) : base (key, picker)
		{
		}
	}
}

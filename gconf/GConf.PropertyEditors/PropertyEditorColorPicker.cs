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

		void Changed (object obj, GnomeSharp.ColorSetArgs args)
		{
			ColorPicker picker = (ColorPicker) Control;
			Color color = Color.FromArgb (ToByte (picker.Red), ToByte (picker.Green), ToByte (picker.Blue));
			Set (ColorTranslator.ToHtml (color));
		}
		
		protected override void ConnectHandlers ()
		{
			ColorPicker picker = (ColorPicker) Control;
			picker.ColorSet += new GnomeSharp.ColorSetHandler (Changed);
		}

		public PropertyEditorColorPicker (string key, ColorPicker picker) : base (key, picker)
		{
		}
	}
}

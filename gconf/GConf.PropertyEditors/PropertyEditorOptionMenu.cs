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

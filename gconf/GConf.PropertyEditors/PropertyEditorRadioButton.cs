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

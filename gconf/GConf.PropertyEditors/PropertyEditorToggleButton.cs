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

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

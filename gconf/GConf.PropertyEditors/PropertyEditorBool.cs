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

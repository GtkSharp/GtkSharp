namespace GConf.PropertyEditors
{
	using Gtk;
	using System;

	public class PropertyEditorEntry : PropertyEditor
	{
		protected override void ValueChanged (object sender, NotifyEventArgs args)
		{
			object val = args.Value;
			if (val == null)
				return;
			Entry entry = (Entry) Control;
			entry.Text = (string) val;
		}

		void Changed (object obj, EventArgs args)
		{
			Entry entry = (Entry) Control;
			Set (entry.Text);
		}
		
		protected override void ConnectHandlers ()
		{
			Entry entry = (Entry) Control;
			entry.Changed += new EventHandler (Changed);
		}

		public PropertyEditorEntry (string key, Entry entry) : base (key, entry)
		{
		}
	}
}

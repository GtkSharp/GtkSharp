namespace GConf.PropertyEditors
{
	using Gtk;
	using Gnome;
	using System;

	public class PropertyEditorFileEntry : PropertyEditor
	{
		protected override void ValueChanged (object sender, NotifyEventArgs args)
		{
			object val = args.Value;
			if (val == null)
				return;
			FileEntry entry = (FileEntry) Control;
			entry.Filename = (string) val;
		}

		void Changed (object obj, EventArgs args)
		{
			FileEntry entry = (FileEntry) Control;
			string filename = entry.Filename;
			if (filename == null)
				filename = String.Empty;
			Set (filename);
		}
		
		protected override void ConnectHandlers ()
		{
			FileEntry entry = (FileEntry) Control;
			entry.Changed += new EventHandler (Changed);
		}

		public PropertyEditorFileEntry (string key, FileEntry entry) : base (key, entry)
		{
		}
	}
}

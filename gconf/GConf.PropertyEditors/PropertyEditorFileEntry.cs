// PropertyEditorFileEntry.cs -
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

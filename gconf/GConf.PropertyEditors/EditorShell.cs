// EditorShell.cs -
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
	using System;
	using System.Collections;

	public class EditorNotSupportedException : Exception
	{
	}
			
	public class InvalidGladeKeyException : Exception
	{
		public InvalidGladeKeyException (string control_name) : base ("No such glade entry \"" + control_name + "\"")
		{
		}
	}

	public class EditorShell
	{
		ArrayList editors = new ArrayList ();
		Hashtable by_key = new Hashtable ();
		Glade.XML gxml;
		GConf.ChangeSet cs = null;
		
		public EditorShell (Glade.XML gxml)
		{
			this.gxml = gxml;
		}

		public EditorShell (Glade.XML gxml, GConf.ChangeSet cs)
		{
			this.gxml = gxml;
			this.cs = cs;
		}

		public void Add (PropertyEditor editor)
		{
			editors.Add (editor);
			if (cs != null)
				editor.ChangeSet = cs;
			editor.Setup ();
		}

		public void Add (string key, string control_name)
		{
			Add (key, control_name, null, null);
		}

		public void Add (string key, string control_name, Type enum_type, int[] enum_values)
		{
			PropertyEditor editor;
			Gtk.Widget control = gxml[control_name];

			if (control == null)
				throw new InvalidGladeKeyException (control_name);

			//if (control is Gnome.ColorPicker)
				//editor = new PropertyEditorColorPicker (key, (Gnome.ColorPicker) control);
			else if (control is Gnome.FileEntry)
				editor = new PropertyEditorFileEntry (key, (Gnome.FileEntry) control);
			else if (control is Gtk.SpinButton)
				editor = new PropertyEditorSpinButton (key, (Gtk.SpinButton) control);
			else if (control is Gtk.RadioButton)
				editor = new PropertyEditorRadioButton (key, (Gtk.RadioButton) control, enum_type, enum_values);
			else if (control is Gtk.ToggleButton)
				editor = new PropertyEditorToggleButton (key, (Gtk.ToggleButton) control);
			else if (control is Gtk.Entry)
				editor = new PropertyEditorEntry (key, (Gtk.Entry) control);
			/*else if (control is Gtk.OptionMenu)
				editor = new PropertyEditorOptionMenu (key, (Gtk.OptionMenu) control, enum_type, enum_values);*/
			else
				throw new EditorNotSupportedException ();

			by_key.Add (key, editor);
			Add (editor);
		}

		public void Add (string key, string control_name, Type enum_type)
		{
			Add (key, control_name, enum_type, null);
		}

		public void AddGuard (string key, string control_name)
		{
			if (!by_key.Contains (key))
				return;
			Gtk.Widget control = gxml[control_name];
			if (control == null)
				throw new InvalidGladeKeyException (control_name);
			
			PropertyEditorBool editor = by_key[key] as PropertyEditorBool;
			if (editor == null)
				throw new EditorNotSupportedException ();
			
			editor.AddGuard (control);
		}
	}
}

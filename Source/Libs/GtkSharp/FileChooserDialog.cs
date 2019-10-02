//  Gtk.FileChooserDialog.cs - Gtk FileChooserDialog customizations
//
//  Authors:  Todd Berman  <tberman@off.net>
//            Jeroen Zwartepoorte  <jeroen@xs4all.nl>
//            Mike Kestner  <mkestner@novell.com>
//
//  Copyright (c) 2004 Todd Berman, Jeroen Zwartepoorte
//  Copyright (c) 2005 Novell, Inc.
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

namespace Gtk {

	using System;
	using System.Runtime.InteropServices;

	public partial class FileChooserDialog {
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_gtk_file_chooser_dialog_new(IntPtr title, IntPtr parent, int action, IntPtr nil);
		static d_gtk_file_chooser_dialog_new gtk_file_chooser_dialog_new = FuncLoader.LoadFunction<d_gtk_file_chooser_dialog_new>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_file_chooser_dialog_new"));

		public FileChooserDialog (string title, Window parent, FileChooserAction action, params object[] button_data) : base (IntPtr.Zero)
		{
			if (GetType () != typeof (FileChooserDialog)) {
				CreateNativeObject (new string[0], new GLib.Value[0]);
				Title = title;
				if (parent != null)
					TransientFor = parent;
				Action = action;
			} else {
				IntPtr native = GLib.Marshaller.StringToPtrGStrdup (title);
				Raw = gtk_file_chooser_dialog_new (native, parent == null ? IntPtr.Zero : parent.Handle, (int)action, IntPtr.Zero);
				GLib.Marshaller.Free (native);
			}

			for (int i = 0; i < button_data.Length - 1; i += 2)
				AddButton ((string) button_data [i], (int) button_data [i + 1]);
		}
	}
}


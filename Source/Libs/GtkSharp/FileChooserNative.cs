// Gtk.FileChooserNative.cs - Gtk FileChooserNative class customizations
//
// Author: Mikkel Kruse Johnsen <mikkel@xmedicus.com>
//
// Copyright (c) 2016 XMedicus ApS
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

	public partial class FileChooserNative : Gtk.NativeDialog, Gtk.IFileChooser {

		public FileChooserNative (IntPtr raw) : base(raw) {}

		delegate IntPtr d_gtk_file_chooser_native_new(IntPtr title, IntPtr parent, int action, IntPtr accept_label, IntPtr cancel_label);
		static d_gtk_file_chooser_native_new gtk_file_chooser_native_new = FuncLoader.LoadFunction<d_gtk_file_chooser_native_new>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_file_chooser_native_new"));

		delegate string d_gtk_file_chooser_native_get_accept_label(IntPtr self);
		static d_gtk_file_chooser_native_get_accept_label gtk_file_chooser_native_get_accept_label = FuncLoader.LoadFunction<d_gtk_file_chooser_native_get_accept_label>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_file_chooser_native_get_accept_label"));

		delegate string d_gtk_file_chooser_native_set_accept_label(IntPtr self, string accept_label);
		static d_gtk_file_chooser_native_set_accept_label gtk_file_chooser_native_set_accept_label = FuncLoader.LoadFunction<d_gtk_file_chooser_native_set_accept_label>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_file_chooser_native_set_accept_label"));

		delegate string d_gtk_file_chooser_native_get_cancel_label(IntPtr self);
		static d_gtk_file_chooser_native_get_cancel_label gtk_file_chooser_native_get_cancel_label = FuncLoader.LoadFunction<d_gtk_file_chooser_native_get_cancel_label>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_file_chooser_native_get_cancel_label"));

		delegate string d_gtk_file_chooser_native_set_cancel_label(IntPtr self, string cancel_label);
		static d_gtk_file_chooser_native_set_cancel_label gtk_file_chooser_native_set_cancel_label = FuncLoader.LoadFunction<d_gtk_file_chooser_native_set_cancel_label>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_file_chooser_native_set_cancel_label"));

                public FileChooserNative (string title, Gtk.Window parent, Gtk.FileChooserAction action, string accept_label, string cancel_label) : base(FileChooserNativeCreate(title, parent, action, accept_label, cancel_label))
                {
			/*
                        if (GetType () != typeof (FileChooserNative)) {
                                var vals = new List<GLib.Value> ();
                                var names = new List<string> ();
                                names.Add ("title");
                                vals.Add (new GLib.Value (title));
                                names.Add ("parent");
                                vals.Add (new GLib.Value (parent));
                                names.Add ("action");
                                vals.Add (new GLib.Value (action));
                                names.Add ("accept_label");
                                vals.Add (new GLib.Value (accept_label));
                                names.Add ("cancel_label");
                                vals.Add (new GLib.Value (cancel_label));
                                CreateNativeObject (names.ToArray (), vals.ToArray ());
                                return;
                        }
			*/
		}

		static IntPtr FileChooserNativeCreate (string title, Gtk.Window parent, Gtk.FileChooserAction action, string accept_label, string cancel_label)
		{
                        IntPtr native_title = GLib.Marshaller.StringToPtrGStrdup (title);
			IntPtr native_accept_label = IntPtr.Zero;
			if (accept_label != null)
	                        native_accept_label = GLib.Marshaller.StringToPtrGStrdup (accept_label);
			IntPtr native_cancel_label = IntPtr.Zero;
			if (cancel_label != null)
	                        native_cancel_label = GLib.Marshaller.StringToPtrGStrdup (cancel_label);

                        IntPtr raw = gtk_file_chooser_native_new(native_title, parent.Handle, (int) action, native_accept_label, native_cancel_label);

                        /*GLib.Marshaller.Free (native_title);
			if (accept_label != null)
                        	GLib.Marshaller.Free (native_accept_label);
			if (cancel_label != null)
                        	GLib.Marshaller.Free (native_cancel_label);*/

			return raw;
                }
	}
}

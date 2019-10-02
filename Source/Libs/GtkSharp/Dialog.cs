//
// Gtk.Dialog.cs - Gtk Dialog class customizations
//
// Author: Duncan Mak  (duncan@ximian.com)
//	   Mike Kestner (mkestner@speakeasy.net)
//
// Copyright (C) 2002 Ximian, Inc. and Mike Kestner 
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

	public partial class Dialog {
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_gtk_dialog_new_with_buttons(IntPtr title, IntPtr i, int flags, IntPtr dummy);
		static d_gtk_dialog_new_with_buttons gtk_dialog_new_with_buttons = FuncLoader.LoadFunction<d_gtk_dialog_new_with_buttons>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_dialog_new_with_buttons"));
		public Dialog (string title, Gtk.Window parent, Gtk.DialogFlags flags, params object[] button_data) : base(IntPtr.Zero)
		{
			IntPtr native = GLib.Marshaller.StringToPtrGStrdup (title);
			Raw = gtk_dialog_new_with_buttons (native, parent == null ? IntPtr.Zero : parent.Handle, (int) flags, IntPtr.Zero);
			GLib.Marshaller.Free (native);

			for (int i = 0; i < button_data.Length - 1; i += 2)
				AddButton ((string) button_data [i], (int) button_data [i + 1]);
		}

		public void AddActionWidget (Widget child, ResponseType response)
		{
			this.AddActionWidget (child, (int) response);
		}

		public Gtk.Widget AddButton (string button_text, ResponseType response)
		{
			return this.AddButton (button_text, (int) response);
		}

		public void Respond (ResponseType response)
		{
			this.Respond ((int) response);
		}

		[Obsolete ("Replaced by AlternativeButtonOrder property")]
		public int SetAlternativeButtonOrderFromArray (int n_params)
		{
			return -1;
		}
	}
}


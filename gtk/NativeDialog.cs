// Gtk.NativeDialog.cs - Gtk NativeDialog class customizations
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

	public partial class NativeDialog : Gtk.FileChooserAdapter {

		public NativeDialog (IntPtr raw) : base(raw) { }

		[DllImport (Global.GtkNativeDll, CallingConvention = CallingConvention.Cdecl)]
                static extern void gtk_native_dialog_show (IntPtr self);

		public void Show ()
		{
			gtk_native_dialog_show (Handle);
		}

		[DllImport (Global.GtkNativeDll, CallingConvention = CallingConvention.Cdecl)]
                static extern void gtk_native_dialog_hide (IntPtr self);

                public void Hide ()
                {
                        gtk_native_dialog_hide (Handle);
                }

		[DllImport (Global.GtkNativeDll, CallingConvention = CallingConvention.Cdecl)]
                static extern void gtk_native_dialog_destroy (IntPtr self);

                public void Destroy ()
                {
                        gtk_native_dialog_destroy (Handle);
                }

		[DllImport (Global.GtkNativeDll, CallingConvention = CallingConvention.Cdecl)]
                static extern bool gtk_native_dialog_get_visible (IntPtr self);

		public bool Visible {
			get {
				return gtk_native_dialog_get_visible (Handle);
			}
		}

		[DllImport (Global.GtkNativeDll, CallingConvention = CallingConvention.Cdecl)]
                static extern void gtk_native_dialog_set_modal (IntPtr self, bool modal);

		[DllImport (Global.GtkNativeDll, CallingConvention = CallingConvention.Cdecl)]
                static extern bool gtk_native_dialog_get_modal (IntPtr self);

		public bool Modal {
			set {
				gtk_native_dialog_set_modal (Handle, value);
			}
                        get {
                                return gtk_native_dialog_get_modal (Handle);
                        }
                }

		[DllImport (Global.GtkNativeDll, CallingConvention = CallingConvention.Cdecl)]
                static extern void gtk_native_dialog_set_title (IntPtr self, string title);

		[DllImport (Global.GtkNativeDll, CallingConvention = CallingConvention.Cdecl)]
                static extern string gtk_native_dialog_get_title (IntPtr self);

                public string Title {
                        set {
                                gtk_native_dialog_set_title (Handle, value);
                        }
                        get {
                                return gtk_native_dialog_get_title (Handle);
                        }
                }

		[DllImport (Global.GtkNativeDll, CallingConvention = CallingConvention.Cdecl)]
                static extern void gtk_native_dialog_set_transient_for (IntPtr self, IntPtr parent);

		[DllImport (Global.GtkNativeDll, CallingConvention = CallingConvention.Cdecl)]
                static extern IntPtr gtk_native_dialog_get_transient_for (IntPtr self);

		[DllImport (Global.GtkNativeDll, CallingConvention = CallingConvention.Cdecl)]
                static extern int gtk_native_dialog_run (IntPtr self);

		public int Run ()
                {
                        return gtk_native_dialog_run (Handle);
                }
	}
}

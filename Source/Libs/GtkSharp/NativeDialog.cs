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

	public partial class NativeDialog : GLib.Object {

		public NativeDialog (IntPtr raw) : base(raw) { }
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_gtk_native_dialog_show(IntPtr self);
		static d_gtk_native_dialog_show gtk_native_dialog_show = FuncLoader.LoadFunction<d_gtk_native_dialog_show>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_native_dialog_show"));

		public void Show ()
		{
			gtk_native_dialog_show (Handle);
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_gtk_native_dialog_hide(IntPtr self);
		static d_gtk_native_dialog_hide gtk_native_dialog_hide = FuncLoader.LoadFunction<d_gtk_native_dialog_hide>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_native_dialog_hide"));

                public void Hide ()
                {
                        gtk_native_dialog_hide (Handle);
                }
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_gtk_native_dialog_destroy(IntPtr self);
		static d_gtk_native_dialog_destroy gtk_native_dialog_destroy = FuncLoader.LoadFunction<d_gtk_native_dialog_destroy>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_native_dialog_destroy"));

                public void Destroy ()
                {
                        gtk_native_dialog_destroy (Handle);
                }
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate bool d_gtk_native_dialog_get_visible(IntPtr self);
		static d_gtk_native_dialog_get_visible gtk_native_dialog_get_visible = FuncLoader.LoadFunction<d_gtk_native_dialog_get_visible>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_native_dialog_get_visible"));

		public bool Visible {
			get {
				return gtk_native_dialog_get_visible (Handle);
			}
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_gtk_native_dialog_set_modal(IntPtr self, bool modal);
		static d_gtk_native_dialog_set_modal gtk_native_dialog_set_modal = FuncLoader.LoadFunction<d_gtk_native_dialog_set_modal>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_native_dialog_set_modal"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate bool d_gtk_native_dialog_get_modal(IntPtr self);
		static d_gtk_native_dialog_get_modal gtk_native_dialog_get_modal = FuncLoader.LoadFunction<d_gtk_native_dialog_get_modal>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_native_dialog_get_modal"));

		public bool Modal {
			set {
				gtk_native_dialog_set_modal (Handle, value);
			}
                        get {
                                return gtk_native_dialog_get_modal (Handle);
                        }
                }
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_gtk_native_dialog_set_title(IntPtr self, string title);
		static d_gtk_native_dialog_set_title gtk_native_dialog_set_title = FuncLoader.LoadFunction<d_gtk_native_dialog_set_title>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_native_dialog_set_title"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate string d_gtk_native_dialog_get_title(IntPtr self);
		static d_gtk_native_dialog_get_title gtk_native_dialog_get_title = FuncLoader.LoadFunction<d_gtk_native_dialog_get_title>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_native_dialog_get_title"));

                public string Title {
                        set {
                                gtk_native_dialog_set_title (Handle, value);
                        }
                        get {
                                return gtk_native_dialog_get_title (Handle);
                        }
                }
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_gtk_native_dialog_set_transient_for(IntPtr self, IntPtr parent);
		static d_gtk_native_dialog_set_transient_for gtk_native_dialog_set_transient_for = FuncLoader.LoadFunction<d_gtk_native_dialog_set_transient_for>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_native_dialog_set_transient_for"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_gtk_native_dialog_get_transient_for(IntPtr self);
		static d_gtk_native_dialog_get_transient_for gtk_native_dialog_get_transient_for = FuncLoader.LoadFunction<d_gtk_native_dialog_get_transient_for>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_native_dialog_get_transient_for"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate int d_gtk_native_dialog_run(IntPtr self);
		static d_gtk_native_dialog_run gtk_native_dialog_run = FuncLoader.LoadFunction<d_gtk_native_dialog_run>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_native_dialog_run"));

		public int Run ()
                {
                        return gtk_native_dialog_run (Handle);
                }
	}
}


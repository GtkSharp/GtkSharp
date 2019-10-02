// Gtk.Window.cs - Gtk Window class customizations
//
// Author: Mike Kestner <mkestner@ximian.com>
//
// Copyright (c) 2001 Mike Kestner
// Copyright (c) 2004 Novell, Inc.
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

	public partial class Window {

		public Window (String title) : this (WindowType.Toplevel)
		{
			this.Title = title;
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_gtk_window_get_default_icon_list();
		static d_gtk_window_get_default_icon_list gtk_window_get_default_icon_list = FuncLoader.LoadFunction<d_gtk_window_get_default_icon_list>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_window_get_default_icon_list"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_gtk_window_set_default_icon_list(IntPtr list);
		static d_gtk_window_set_default_icon_list gtk_window_set_default_icon_list = FuncLoader.LoadFunction<d_gtk_window_set_default_icon_list>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_window_set_default_icon_list"));

		public static Gdk.Pixbuf[] DefaultIconList {
			get {
				IntPtr raw_ret = gtk_window_get_default_icon_list();
				if (raw_ret == IntPtr.Zero)
					return new Gdk.Pixbuf [0];
				GLib.List list = new GLib.List(raw_ret);
				Gdk.Pixbuf[] result = new Gdk.Pixbuf [list.Count];
				for (int i = 0; i < list.Count; i++)
					result [i] = list [i] as Gdk.Pixbuf;
				return result;
			}
			set {
				GLib.List list = new GLib.List(IntPtr.Zero);
				foreach (Gdk.Pixbuf val in value)
					list.Append (val.Handle);
				gtk_window_set_default_icon_list(list.Handle);
			}
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_gtk_window_get_icon_list(IntPtr raw);
		static d_gtk_window_get_icon_list gtk_window_get_icon_list = FuncLoader.LoadFunction<d_gtk_window_get_icon_list>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_window_get_icon_list"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_gtk_window_set_icon_list(IntPtr raw, IntPtr list);
		static d_gtk_window_set_icon_list gtk_window_set_icon_list = FuncLoader.LoadFunction<d_gtk_window_set_icon_list>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_window_set_icon_list"));

		public Gdk.Pixbuf[] IconList {
			get {
				IntPtr raw_ret = gtk_window_get_icon_list(Handle);
				if (raw_ret == IntPtr.Zero)
					return new Gdk.Pixbuf [0];
				GLib.List list = new GLib.List(raw_ret);
				Gdk.Pixbuf[] result = new Gdk.Pixbuf [list.Count];
				for (int i = 0; i < list.Count; i++)
					result [i] = list [i] as Gdk.Pixbuf;
				return result;
			}
			set {
				GLib.List list = new GLib.List(IntPtr.Zero);
				foreach (Gdk.Pixbuf val in value)
					list.Append (val.Handle);
				gtk_window_set_icon_list(Handle, list.Handle);
			}
		}

		public Gdk.Size DefaultSize {
			get {
				return new Gdk.Size (DefaultWidth, DefaultHeight);
			}
			set {
				DefaultWidth = value.Width;
				DefaultHeight = value.Height;
			}
		}
	}
}


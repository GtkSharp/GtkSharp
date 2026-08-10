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

		// IconList: gtk_window_[gs]et_icon_list is gone in Gtk 4; a window is identified by icon name.

		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		delegate void d_gtk_window_destroy (IntPtr raw);
		static d_gtk_window_destroy gtk_window_destroy = FuncLoader.LoadFunction<d_gtk_window_destroy> (FuncLoader.GetProcAddress (GLibrary.Load (Library.Gtk), "gtk_window_destroy"));

		/// <summary>
		/// Destroys the window. This override (the generated one is hidden via GtkSharp.metadata) is
		/// idempotent: calling gtk_window_destroy on an already-destroyed window trips
		/// "gtk_window_destroy: assertion 'GTK_IS_WINDOW (window)' failed" and can crash the process, and
		/// because the toplevel reference is gone the wrapper may otherwise re-destroy it from a second
		/// close path or at finalization. It skips when the handle is already cleared, then Dispose()s to
		/// clear it (Dispose is designed to be safe on an object torn down behind the wrapper's back — see
		/// GLib.Object.Dispose).
		/// </summary>
		public void Destroy ()
		{
			if (Handle == IntPtr.Zero)
				return;
			gtk_window_destroy (Handle);
			Dispose ();
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


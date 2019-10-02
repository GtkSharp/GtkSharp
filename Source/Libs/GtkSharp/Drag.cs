// Drag.cs - Gtk.Drag class customizations
//
// Copyright (c) 2005 Novell, Inc.
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

	public partial class Drag {
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_gtk_drag_set_icon_default(IntPtr context);
		static d_gtk_drag_set_icon_default gtk_drag_set_icon_default = FuncLoader.LoadFunction<d_gtk_drag_set_icon_default>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_drag_set_icon_default"));

		public static void SetIconDefault(Gdk.DragContext context)
		{
			gtk_drag_set_icon_default(context == null ? IntPtr.Zero : context.Handle);
		}

		[Obsolete("Replaced by SetIconDefault(ctx)")]
		public static Gdk.DragContext IconDefault { 
			set {
				gtk_drag_set_icon_default(value == null ? IntPtr.Zero : value.Handle);
			}
		}
	}
}


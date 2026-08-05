// Gdk.Global.cs - Gdk global customizations
//
// Copyright (c) 2008 Novell, Inc.
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

namespace Gdk {

	using System;

	public partial class Global {

		// Everything else this file used to hold is gone from Gtk 4.
		//
		// gdk_init_check, gdk_parse_args: GDK is no longer initialised or
		//   argument-parsed separately from GTK; Gtk.Application.Init does it.
		// gdk_list_visuals, gdk_query_depths, gdk_query_visual_types: GdkVisual
		//   is gone, along with the notion of choosing one.
		// The window-manager helpers (supported hints, client windows, desktop
		//   count and workareas, active window): built on GdkAtom, GdkWindow and
		//   GdkScreen, none of which survive. There is no Gtk 4 equivalent -- a
		//   compositor is not required to expose any of it.

		public static bool IsSupported => GLibrary.IsSupported(Library.Gdk);
	}
}

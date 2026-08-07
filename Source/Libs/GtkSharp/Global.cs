//  Authors:  Aaron Bockover <abockover@novell.com>
// 
//  Copyright 2007-2010 Novell, Inc.
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

	public partial class Global {

		// Gtk 4's gtk_show_uri takes a parent window and a timestamp, and returns
		// void: it launches asynchronously and reports nothing back. Gtk 4.10 adds
		// GtkUriLauncher, which does report completion.
		public static void ShowUri (string uri)
		{
			ShowUri (null, uri, 0);
		}

		// Screen-based helpers are gone with GdkScreen.

		public static bool IsSupported => GLibrary.IsSupported(Library.Gtk);

	}
}

